using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json;
using System.Windows;
using ComfyUILibs.Common;
using ComfyUILibs.Models;
using ComfyUILibs.Services;
using ComfyUIRunWorkflow.Helpers;
using ComfyUIRunWorkflow.Models;
using ComfyUIRunWorkflow.Services;
using ComfyUIRunWorkflow.Views.Windows;
using Wpf.Ui.Abstractions.Controls;

namespace ComfyUIRunWorkflow.ViewModels.Pages
{
    /// <summary>
    /// 実行結果一覧ページの ViewModel。
    /// ResultsFolder 内の result_*.json（生成結果）・tag_result_*.json（タグ付け履歴）を読み込んで一覧表示する。
    /// </summary>
    public partial class DataViewModel : ObservableObject, INavigationAware
    {
        /// <summary>プレビュー画像のキャッシュ先サブフォルダ名。</summary>
        private const string PreviewCacheDirectoryName = "preview_cache";

        /// <summary>アプリケーション設定。</summary>
        public Setting<AppConfig> Config { get; }

        /// <summary>読み込んだ実行結果のリスト（新しい順）。</summary>
        [ObservableProperty]
        private ObservableCollection<WorkflowResultPreview> _results = new();

        /// <summary>現在の状態メッセージ（空ならメッセージなし）。「生成結果」タブに対応する。</summary>
        [ObservableProperty]
        private string _statusMessage = "";

        /// <summary>読み込んだタグ付け履歴のリスト（新しい順）。</summary>
        [ObservableProperty]
        private ObservableCollection<TagResult> _tagResults = new();

        /// <summary>タグ付け履歴タブの状態メッセージ（空ならメッセージなし）。</summary>
        [ObservableProperty]
        private string _tagStatusMessage = "";

        /// <summary>「タグ付け履歴」タブを表示中かどうか（false の場合は「生成結果」タブ）。</summary>
        [ObservableProperty]
        private bool _isTagHistorySelected = false;

        /// <summary>読み込み中かどうか。</summary>
        [ObservableProperty]
        private bool _isLoading = false;

        private readonly PreviewImageLoader _previewLoader = new();

        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true,
        };

        /// <summary>DI コンテナから設定を受け取って初期化する。</summary>
        public DataViewModel(Setting<AppConfig> config)
        {
            Config = config;
        }

        /// <summary>ページへ遷移するたびに結果を再読み込みする。</summary>
        public Task OnNavigatedToAsync() => LoadResultsAsync();

        /// <summary>ページから離れるときは何もしない。</summary>
        public Task OnNavigatedFromAsync() => Task.CompletedTask;

        /// <summary>結果フォルダを再スキャンして一覧を更新する。</summary>
        [RelayCommand]
        private Task RefreshAsync() => LoadResultsAsync();

        /// <summary>「生成結果」タブを表示する。</summary>
        [RelayCommand]
        private void ShowResultsTab() => IsTagHistorySelected = false;

        /// <summary>「タグ付け履歴」タブを表示する。</summary>
        [RelayCommand]
        private void ShowTagHistoryTab() => IsTagHistorySelected = true;

        /// <summary>
        /// ResultsFolder 内の result_*.json（生成結果）と tag_result_*.json（タグ付け履歴）を読み込んで更新する。
        /// フォルダが未設定または存在しない場合は両タブにエラーメッセージを表示する。
        /// </summary>
        private async Task LoadResultsAsync()
        {
            var folder = Config.Data.ResultsFolder;

            if (string.IsNullOrWhiteSpace(folder))
            {
                var message = LocalizationManager.Instance["Data_ResultsFolderNotSet"];
                StatusMessage = message;
                TagStatusMessage = message;
                Results = new ObservableCollection<WorkflowResultPreview>();
                TagResults = new ObservableCollection<TagResult>();
                return;
            }

            if (!Directory.Exists(folder))
            {
                var message = string.Format(LocalizationManager.Instance["Data_FolderNotFound_Format"], folder);
                StatusMessage = message;
                TagStatusMessage = message;
                Results = new ObservableCollection<WorkflowResultPreview>();
                TagResults = new ObservableCollection<TagResult>();
                return;
            }

            IsLoading = true;
            StatusMessage = "";
            TagStatusMessage = "";

            try
            {
                await LoadWorkflowResultsAsync(folder);
                await LoadTagHistoryAsync(folder);
            }
            finally
            {
                IsLoading = false;
            }

            _ = LoadThumbnailsAsync(Results, folder);
        }

        /// <summary>result_*.json を読み込んで Results を更新する。</summary>
        private async Task LoadWorkflowResultsAsync(string folder)
        {
            var files = await Task.Run(() =>
                Directory.GetFiles(folder, "result_*.json")
                    .OrderByDescending(f => f)
                    .ToArray());

            var loaded = new ObservableCollection<WorkflowResultPreview>();
            foreach (var file in files)
            {
                try
                {
                    var json = await File.ReadAllTextAsync(file);
                    var result = JsonSerializer.Deserialize<WorkflowResult>(json, _jsonOptions);
                    if (result != null)
                        loaded.Add(new WorkflowResultPreview(result));
                }
                catch
                {
                    // 読み込めないファイルはスキップする
                }
            }

            Results = loaded;

            if (loaded.Count == 0)
                StatusMessage = LocalizationManager.Instance["Data_NoResults"];
        }

        /// <summary>tag_result_*.json を読み込んで TagResults を更新する。</summary>
        private async Task LoadTagHistoryAsync(string folder)
        {
            var files = await Task.Run(() =>
                Directory.GetFiles(folder, "tag_result_*.json")
                    .OrderByDescending(f => f)
                    .ToArray());

            var loaded = new ObservableCollection<TagResult>();
            foreach (var file in files)
            {
                try
                {
                    var json = await File.ReadAllTextAsync(file);
                    var result = JsonSerializer.Deserialize<TagResult>(json, _jsonOptions);
                    if (result != null)
                        loaded.Add(result);
                }
                catch
                {
                    // 読み込めないファイルはスキップする
                }
            }

            TagResults = loaded;

            if (loaded.Count == 0)
                TagStatusMessage = LocalizationManager.Instance["Data_NoTagHistory"];
        }

        /// <summary>
        /// 一覧の各カードに表示するサムネイルを非同期に取得する。
        /// ComfyUIUrl が未設定の場合は何もしない（一覧はテキストのみで表示される）。
        /// </summary>
        private async Task LoadThumbnailsAsync(ObservableCollection<WorkflowResultPreview> items, string resultsFolder)
        {
            var url = Config.Data.ComfyUIUrl;
            if (string.IsNullOrWhiteSpace(url))
                return;

            var client = new ComfyUIClient(url);
            var cacheDirectory = Path.Combine(resultsFolder, PreviewCacheDirectoryName);

            var tasks = items
                .Where(item => item.Result.Status == "success")
                .Select(item => LoadThumbnailAsync(item, client, cacheDirectory));

            try
            {
                await Task.WhenAll(tasks);
            }
            catch
            {
                // サムネイル取得失敗は一覧表示に影響させない
            }
        }

        /// <summary>1 件分の結果からサムネイル対象の出力を選び、プレビューを読み込む。</summary>
        private async Task LoadThumbnailAsync(WorkflowResultPreview item, IComfyUIClient client, string cacheDirectory)
        {
            var output = item.Result.Outputs.Find(o => o.Type == "output" && PreviewImageCacheService.IsImageFile(o.Filename));
            if (output == null)
                return;

            var preview = new OutputFilePreview(output);
            item.Preview = preview;
            await _previewLoader.LoadAsync(preview, client, item.Result.PromptId, cacheDirectory);
        }

        /// <summary>指定した結果の詳細ダイアログを開く。</summary>
        [RelayCommand]
        private void OpenDetail(WorkflowResult result)
        {
            var window = new ResultDetailWindow(result, Config)
            {
                Owner = System.Windows.Application.Current.MainWindow
            };
            window.ShowDialog();
        }

        /// <summary>タグ付け履歴のタグ文字列をクリップボードにコピーする。</summary>
        [RelayCommand]
        private void CopyTags(TagResult result)
        {
            if (!string.IsNullOrEmpty(result.Tags))
                Clipboard.SetText(result.Tags);
        }
    }
}
