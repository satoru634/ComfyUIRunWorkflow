using System.IO;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;
using System.Windows;
using ComfyUILibs.Common;
using ComfyUILibs.Exceptions;
using ComfyUILibs.Models;
using ComfyUILibs.Services;
using ComfyUIRunWorkflow.Helpers;
using ComfyUIRunWorkflow.Models;
using ComfyUIRunWorkflow.Services;
using Wpf.Ui;
using Wpf.Ui.Abstractions.Controls;
using Wpf.Ui.Controls;

namespace ComfyUIRunWorkflow.ViewModels.Pages
{
    /// <summary>
    /// WD14 Tagger ページの ViewModel。画像 1 枚を選択してタグ付けを実行する。
    /// </summary>
    public partial class TaggerViewModel : ObservableObject, INavigationAware
    {
        /// <summary>アプリケーション設定。</summary>
        public Setting<AppConfig> Config { get; }

        /// <summary>スナックバー通知サービス。</summary>
        private readonly ISnackbarService _snackbarService;

        /// <summary>config が正常に読み込まれ、Wd14TaggerRunner が使用可能かどうか。</summary>
        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(TagImageCommand))]
        private bool _isConfigLoaded = false;

        /// <summary>選択中の画像ファイルパス。</summary>
        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(TagImageCommand))]
        private string? _selectedImagePath;

        /// <summary>選択中の画像のプレビュー表示用ビットマップ。</summary>
        [ObservableProperty]
        private System.Windows.Media.Imaging.BitmapImage? _selectedImagePreview;

        /// <summary>タグ付け実行中かどうか。</summary>
        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(TagImageCommand))]
        private bool _isRunning = false;

        /// <summary>タグ付け結果（カンマ区切り文字列）。未実行時は空文字。</summary>
        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(CopyTagsCommand))]
        private string _resultTags = "";

        /// <summary>結果を表示すべきかどうか（ResultTags が 1 件以上あるか）。</summary>
        public bool HasResult => !string.IsNullOrEmpty(ResultTags);

        /// <summary>ResultTags が差し替えられたとき、派生プロパティ HasResult を通知する。</summary>
        partial void OnResultTagsChanged(string value)
            => OnPropertyChanged(nameof(HasResult));

        private Wd14TaggerRunner? _runner;

        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            WriteIndented = true,
            Encoder = JavaScriptEncoder.Create(UnicodeRanges.All),
        };

        /// <summary>DI コンテナから設定を受け取って初期化する。</summary>
        public TaggerViewModel(Setting<AppConfig> config, ISnackbarService snackbarService)
        {
            Config = config;
            _snackbarService = snackbarService;
        }

        // ── INavigationAware ─────────────────────────────────────────────────

        /// <summary>ページへ遷移するたびに workflow_config.json を再読み込みし、Runner を初期化する。</summary>
        public Task OnNavigatedToAsync()
        {
            TryLoadRunner();
            return Task.CompletedTask;
        }

        /// <summary>ページから離れるときは何もしない。</summary>
        public Task OnNavigatedFromAsync() => Task.CompletedTask;

        /// <summary>
        /// 設定ページで指定された ConfigPath から Wd14TaggerRunner を初期化する。
        /// 失敗した場合はスナックバーでエラーメッセージを表示し、実行ボタンを無効化する。
        /// </summary>
        private void TryLoadRunner()
        {
            var path = Config.Data.ConfigPath;
            if (string.IsNullOrWhiteSpace(path))
            {
                _runner = null;
                IsConfigLoaded = false;

                _snackbarService.Show(
                    "Error",
                    LocalizationManager.Instance["Common_ConfigPathNotSet"],
                    ControlAppearance.Danger,
                    new SymbolIcon(SymbolRegular.ErrorCircle24),
                    TimeSpan.FromSeconds(3.0)
                );
                return;
            }

            try
            {
                _runner = new Wd14TaggerRunner(path);
                IsConfigLoaded = true;
            }
            catch (ComfyUIException ex)
            {
                _runner = null;
                IsConfigLoaded = false;

                _snackbarService.Show(
                    "Error",
                    string.Format(LocalizationManager.Instance["Tagger_ConfigLoadError_Format"], ex.Message),
                    ControlAppearance.Danger,
                    new SymbolIcon(SymbolRegular.ErrorCircle24),
                    TimeSpan.FromSeconds(3.0)
                );
            }
        }

        // ── 画像選択 ───────────────────────────────────────────────────────────

        /// <summary>ファイル選択ダイアログを開いて画像を選択する。</summary>
        [RelayCommand]
        private void BrowseImage()
        {
            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                Title = LocalizationManager.Instance["Tagger_ImageFileDialogTitle"],
                Filter = LocalizationManager.Instance["Tagger_ImageFileDialogFilter"],
            };

            if (dialog.ShowDialog() == true)
                SetSelectedImage(dialog.FileName);
        }

        /// <summary>
        /// 指定したパスの画像を選択状態にする。ドラッグ&amp;ドロップからも呼び出される。
        /// 画像ファイルとして認識できない場合は何もしない。
        /// </summary>
        public void SetSelectedImage(string path)
        {
            if (!PreviewImageCacheService.IsImageFile(path))
                return;

            SelectedImagePath = path;
            ResultTags = "";

            try
            {
                SelectedImagePreview = PreviewImageLoader.LoadFullSize(path);
            }
            catch
            {
                SelectedImagePreview = null;
            }
        }

        // ── タグ付け実行 ──────────────────────────────────────────────────────

        private bool CanTagImage() => _runner != null && !string.IsNullOrWhiteSpace(SelectedImagePath) && !IsRunning;

        /// <summary>選択中の画像をタグ付けし、結果を ResultTags に反映して tag_result_*.json に保存する。</summary>
        [RelayCommand(CanExecute = nameof(CanTagImage))]
        private async Task TagImageAsync()
        {
            var imagePath = SelectedImagePath!;
            var filename = Path.GetFileName(imagePath);

            IsRunning = true;
            TagResult result;

            try
            {
                var imageData = await File.ReadAllBytesAsync(imagePath);
                var tags = await _runner!.TagAsync(imageData, filename);

                ResultTags = tags;
                result = new TagResult
                {
                    Status = "success",
                    Timestamp = DateTime.Now.ToString("s"),
                    InputFilename = filename,
                    Tags = tags,
                };

                _snackbarService.Show(
                    LocalizationManager.Instance["Common_Completed"],
                    LocalizationManager.Instance["Tagger_TaggingCompleted"],
                    ControlAppearance.Success,
                    new SymbolIcon(SymbolRegular.CheckmarkCircle24),
                    TimeSpan.FromSeconds(4.0)
                );
            }
            catch (ComfyUIException ex)
            {
                result = new TagResult
                {
                    Status = "error",
                    Timestamp = DateTime.Now.ToString("s"),
                    InputFilename = filename,
                    Error = ex.Message,
                };

                _snackbarService.Show(
                    LocalizationManager.Instance["Common_Error"],
                    ex.Message,
                    ControlAppearance.Danger,
                    new SymbolIcon(SymbolRegular.ErrorCircle24),
                    TimeSpan.FromSeconds(5.0)
                );
            }
            finally
            {
                IsRunning = false;
            }

            await TrySaveResultAsync(result);
        }

        /// <summary>
        /// タグ付け結果を ResultsFolder に tag_result_{timestamp}.json として保存する。
        /// ResultsFolder が未設定の場合は何もしない。
        /// </summary>
        private async Task TrySaveResultAsync(TagResult result)
        {
            var folder = Config.Data.ResultsFolder;
            if (string.IsNullOrWhiteSpace(folder)) return;

            try
            {
                Directory.CreateDirectory(folder);
                var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                var outputPath = Path.Combine(folder, $"tag_result_{timestamp}.json");
                var json = JsonSerializer.Serialize(result, _jsonOptions);
                await File.WriteAllTextAsync(outputPath, json);
            }
            catch
            {
                // 保存失敗は実行結果に影響させない
            }
        }

        // ── 結果コピー ────────────────────────────────────────────────────────

        private bool CanCopyTags() => HasResult;

        /// <summary>タグ結果をクリップボードにコピーする。</summary>
        [RelayCommand(CanExecute = nameof(CanCopyTags))]
        private void CopyTags()
        {
            if (!string.IsNullOrEmpty(ResultTags))
                Clipboard.SetText(ResultTags);
        }
    }
}
