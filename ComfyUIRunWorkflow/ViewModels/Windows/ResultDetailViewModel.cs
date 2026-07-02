using System.Collections.ObjectModel;
using System.IO;
using ComfyUILibs.Common;
using ComfyUILibs.Models;
using ComfyUILibs.Services;
using ComfyUIRunWorkflow.Models;
using ComfyUIRunWorkflow.Services;
using ComfyUIRunWorkflow.Views.Windows;

namespace ComfyUIRunWorkflow.ViewModels.Windows
{
    /// <summary>
    /// 実行結果詳細ダイアログ（<see cref="ResultDetailWindow"/>）の ViewModel。
    /// 出力ファイルごとのサムネイルを非同期に読み込み、クリックで拡大表示できるようにする。
    /// </summary>
    public partial class ResultDetailViewModel : ObservableObject
    {
        /// <summary>プレビュー画像のキャッシュ先サブフォルダ名。</summary>
        private const string PreviewCacheDirectoryName = "preview_cache";

        /// <summary>表示対象の実行結果。</summary>
        public WorkflowResult Result { get; }

        /// <summary>出力ファイル（type=="output"）ごとのプレビュー一覧。</summary>
        public ObservableCollection<OutputFilePreview> Previews { get; } = new();

        private readonly PreviewImageLoader _previewLoader = new();

        public ResultDetailViewModel(WorkflowResult result, Setting<AppConfig> config)
        {
            Result = result;

            foreach (var output in result.Outputs.Where(o => o.Type == "output"))
                Previews.Add(new OutputFilePreview(output));

            _ = LoadPreviewsAsync(config);
        }

        /// <summary>各出力のサムネイルを非同期に取得する。</summary>
        private async Task LoadPreviewsAsync(Setting<AppConfig> config)
        {
            var url = config.Data.ComfyUIUrl;
            var resultsFolder = config.Data.ResultsFolder;
            if (string.IsNullOrWhiteSpace(url) || string.IsNullOrWhiteSpace(resultsFolder))
                return;

            var client = new ComfyUIClient(url);
            var cacheDirectory = Path.Combine(resultsFolder, PreviewCacheDirectoryName);

            try
            {
                await Task.WhenAll(Previews.Select(p =>
                    _previewLoader.LoadAsync(p, client, Result.PromptId, cacheDirectory)));
            }
            catch
            {
                // サムネイル取得失敗はダイアログ表示に影響させない
            }
        }

        /// <summary>クリックされたサムネイルを拡大表示するウィンドウを開く。</summary>
        [RelayCommand]
        private void OpenEnlarged(OutputFilePreview preview)
        {
            if (preview.CachedFilePath == null)
                return;

            var window = new ImagePreviewWindow(preview.CachedFilePath)
            {
                Owner = System.Windows.Application.Current.MainWindow
            };
            window.Show();
        }
    }
}
