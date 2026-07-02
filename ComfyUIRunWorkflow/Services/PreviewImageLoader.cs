using System.Windows.Media.Imaging;
using ComfyUILibs.Services;
using ComfyUIRunWorkflow.Models;

namespace ComfyUIRunWorkflow.Services
{
    /// <summary>
    /// <see cref="OutputFilePreview"/> のサムネイル画像を非同期で読み込むヘルパー。
    /// キャッシュファイルの解決（取得・保存）は <see cref="IPreviewImageCacheService"/> に委譲し、
    /// このクラスは BitmapImage への変換など WPF 固有の処理のみを担う。
    /// </summary>
    public class PreviewImageLoader
    {
        /// <summary>サムネイル表示用に画像をデコードする際の幅（ピクセル）。</summary>
        private const int ThumbnailDecodePixelWidth = 512;

        private readonly IPreviewImageCacheService _cacheService;

        public PreviewImageLoader() : this(new PreviewImageCacheService())
        {
        }

        internal PreviewImageLoader(IPreviewImageCacheService cacheService)
        {
            _cacheService = cacheService;
        }

        /// <summary>
        /// <paramref name="preview"/> のサムネイルを読み込み、<see cref="OutputFilePreview.Thumbnail"/> 等を更新する。
        /// 非画像ファイルの場合は何もせず <see cref="OutputFilePreview.IsLoading"/> を false にする。
        /// 取得に失敗した場合は例外を送出せず <see cref="OutputFilePreview.HasError"/> を true にする。
        /// </summary>
        /// <param name="preview">更新対象のプレビュー状態。</param>
        /// <param name="client">画像取得に使用する ComfyUI クライアント。</param>
        /// <param name="promptId">実行時の prompt_id（キャッシュキーに使用）。</param>
        /// <param name="cacheDirectory">キャッシュ保存先ディレクトリ。</param>
        public async Task LoadAsync(
            OutputFilePreview preview,
            IComfyUIClient client,
            string? promptId,
            string cacheDirectory)
        {
            if (!preview.IsImage)
            {
                preview.IsLoading = false;
                return;
            }

            try
            {
                var path = await _cacheService.GetOrFetchAsync(client, promptId, preview.Output, cacheDirectory);
                if (path == null)
                {
                    preview.HasError = true;
                    return;
                }

                preview.CachedFilePath = path;
                preview.Thumbnail = LoadBitmap(path, ThumbnailDecodePixelWidth);
            }
            catch
            {
                // 予期しない例外（破損ファイル等）もプレビュー取得失敗として扱う
                preview.HasError = true;
            }
            finally
            {
                preview.IsLoading = false;
            }
        }

        /// <summary>
        /// キャッシュファイルパスから原寸の <see cref="BitmapImage"/> を読み込む。拡大表示用。
        /// </summary>
        public static BitmapImage LoadFullSize(string cachedFilePath)
            => LoadBitmap(cachedFilePath, decodePixelWidth: 0);

        /// <summary>
        /// ファイルパスから <see cref="BitmapImage"/> を生成する。
        /// バックグラウンドスレッドから呼ばれても安全なよう OnLoad でデコードし Freeze する。
        /// </summary>
        private static BitmapImage LoadBitmap(string path, int decodePixelWidth)
        {
            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            if (decodePixelWidth > 0)
                bitmap.DecodePixelWidth = decodePixelWidth;
            bitmap.UriSource = new Uri(path, UriKind.Absolute);
            bitmap.EndInit();
            bitmap.Freeze();
            return bitmap;
        }
    }
}
