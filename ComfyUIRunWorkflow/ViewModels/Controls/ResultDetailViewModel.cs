using System.Collections.ObjectModel;
using System.IO;
using ComfyUILibs.Common;
using ComfyUILibs.Models;
using ComfyUILibs.Services;
using ComfyUIRunWorkflow.Models;
using ComfyUIRunWorkflow.Services;
using ComfyUIRunWorkflow.Views.Controls;
using Wpf.Ui;
using Wpf.Ui.Controls;

namespace ComfyUIRunWorkflow.ViewModels.Controls
{
    /// <summary>
    /// 実行結果詳細ダイアログ（<see cref="ResultDetailWindow"/>）の ViewModel。
    /// 出力ファイルごとのサムネイルを非同期に読み込み、クリックで拡大表示できるようにする。
    /// </summary>
    public partial class ResultDetailViewModel : ObservableObject
    {
        /// <summary>プレビュー画像のキャッシュ先サブフォルダ名。</summary>
        private const string PreviewCacheDirectoryName = "preview_cache";

        /// <summary>
        /// 自身を表示している ContentDialog（<see cref="ResultDetailWindow"/>）。
        /// ImagePreviewWindow は同じ ContentDialogHost を共有しており、表示時にこのダイアログの
        /// Content が入れ替わって閉じられてしまうため、ImagePreviewWindow を閉じた後の再表示に使用する。
        /// </summary>
        public ContentDialog? OwnerDialog { get; set; }

        /// <summary>表示対象の実行結果。</summary>
        public WorkflowResult Result { get; }

        /// <summary>出力ファイル（type=="output"）ごとのプレビュー一覧。</summary>
        public ObservableCollection<OutputFilePreview> Previews { get; } = new();

        private readonly PreviewImageLoader _previewLoader = new();
        private readonly IContentDialogService _contentDialogService;

        public ResultDetailViewModel(WorkflowResult result, Setting<AppConfig> config, IContentDialogService contentDialogService)
        {
            Result = result;
            _contentDialogService = contentDialogService;

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

        /// <summary>クリックされたサムネイルを拡大表示するダイアログを開く。</summary>
        [RelayCommand]
        private async Task OpenEnlarged(OutputFilePreview preview)
        {
            if (preview.CachedFilePath == null)
                return;

            var dialog = new ImagePreviewWindow(preview.CachedFilePath, _contentDialogService.GetDialogHostEx());
            await dialog.ShowAsync();

            // ImagePreviewWindow の表示によって ContentDialogHost の Content が入れ替わり、
            // 呼び出し元の ResultDetailWindow が閉じられてしまっているため再表示する。
            // ここで await してしまうと ResultDetailWindow が実際に閉じられるまで
            // OpenEnlargedCommand の実行が完了せず（AsyncRelayCommand は既定で多重実行を許可しないため）、
            // サムネイルを再度クリックしても次のダイアログが起動しなくなるため、await せず呼び出す
            if (OwnerDialog != null)
                _ = OwnerDialog.ShowAsync();
        }
    }
}
