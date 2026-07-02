using System.Windows.Media.Imaging;
using ComfyUILibs.Models;
using ComfyUILibs.Services;

namespace ComfyUIRunWorkflow.Models
{
    /// <summary>
    /// 出力ファイル 1 件分のプレビュー表示状態を保持するラッパー。
    /// <see cref="Services.PreviewImageLoader"/> がサムネイル取得の進行に合わせて
    /// <see cref="Thumbnail"/>・<see cref="IsLoading"/>・<see cref="HasError"/> を更新する。
    /// </summary>
    public partial class OutputFilePreview : ObservableObject
    {
        /// <summary>元の出力ファイル情報。</summary>
        public OutputFile Output { get; }

        /// <summary>拡張子から判定した、プレビュー対象の画像ファイルかどうか。</summary>
        public bool IsImage { get; }

        /// <summary>取得済みキャッシュファイルのローカルパス。拡大表示時に原寸画像を読み込むために使用する。</summary>
        public string? CachedFilePath { get; internal set; }

        /// <summary>サムネイル用に縮小した画像。未取得または非画像の場合は null。</summary>
        [ObservableProperty]
        private BitmapImage? _thumbnail;

        /// <summary>サムネイル取得中かどうか。</summary>
        [ObservableProperty]
        private bool _isLoading;

        /// <summary>サムネイル取得に失敗したかどうか（プレースホルダー表示に使用）。</summary>
        [ObservableProperty]
        private bool _hasError;

        public OutputFilePreview(OutputFile output)
        {
            Output = output;
            IsImage = PreviewImageCacheService.IsImageFile(output.Filename);
            IsLoading = IsImage;
        }
    }
}
