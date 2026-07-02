using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json;
using ComfyUILibs.Common;
using ComfyUILibs.Models;
using ComfyUIRunWorkflow.Models;
using ComfyUIRunWorkflow.Views.Windows;
using Wpf.Ui.Abstractions.Controls;

namespace ComfyUIRunWorkflow.ViewModels.Pages
{
    /// <summary>
    /// 実行結果一覧ページの ViewModel。
    /// ResultsFolder 内の result_*.json を読み込んで一覧表示する。
    /// </summary>
    public partial class DataViewModel : ObservableObject, INavigationAware
    {
        /// <summary>アプリケーション設定。</summary>
        public Setting<AppConfig> Config { get; }

        /// <summary>読み込んだ実行結果のリスト（新しい順）。</summary>
        [ObservableProperty]
        private ObservableCollection<WorkflowResult> _results = new();

        /// <summary>現在の状態メッセージ（空ならメッセージなし）。</summary>
        [ObservableProperty]
        private string _statusMessage = "";

        /// <summary>読み込み中かどうか。</summary>
        [ObservableProperty]
        private bool _isLoading = false;

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

        /// <summary>
        /// ResultsFolder 内の result_*.json を読み込んで Results を更新する。
        /// フォルダが未設定または存在しない場合はエラーメッセージを表示する。
        /// </summary>
        private async Task LoadResultsAsync()
        {
            var folder = Config.Data.ResultsFolder;

            if (string.IsNullOrWhiteSpace(folder))
            {
                StatusMessage = "設定ページで結果出力フォルダを指定してください";
                Results = new ObservableCollection<WorkflowResult>();
                return;
            }

            if (!Directory.Exists(folder))
            {
                StatusMessage = $"フォルダが見つかりません:\n{folder}";
                Results = new ObservableCollection<WorkflowResult>();
                return;
            }

            IsLoading = true;
            StatusMessage = "";

            try
            {
                var files = await Task.Run(() =>
                    Directory.GetFiles(folder, "result_*.json")
                        .OrderByDescending(f => f)
                        .ToArray());

                var loaded = new ObservableCollection<WorkflowResult>();
                foreach (var file in files)
                {
                    try
                    {
                        var json = await File.ReadAllTextAsync(file);
                        var result = JsonSerializer.Deserialize<WorkflowResult>(json, _jsonOptions);
                        if (result != null)
                            loaded.Add(result);
                    }
                    catch
                    {
                        // 読み込めないファイルはスキップする
                    }
                }

                Results = loaded;

                if (loaded.Count == 0)
                    StatusMessage = "結果がありません";
            }
            finally
            {
                IsLoading = false;
            }
        }

        /// <summary>指定した結果の詳細ダイアログを開く。</summary>
        [RelayCommand]
        private void OpenDetail(WorkflowResult result)
        {
            var window = new ResultDetailWindow(result)
            {
                Owner = System.Windows.Application.Current.MainWindow
            };
            window.ShowDialog();
        }
    }
}
