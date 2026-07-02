using ComfyUIRunWorkflow.Services;

namespace ComfyUIRunWorkflow.Views.Windows
{
    /// <summary>
    /// 生成画像を原寸で拡大表示するウィンドウ。
    /// </summary>
    public partial class ImagePreviewWindow
    {
        /// <summary>キャッシュ済み画像ファイルのパスを受け取って初期化する。</summary>
        public ImagePreviewWindow(string cachedFilePath)
        {
            InitializeComponent();
            PreviewImage.Source = PreviewImageLoader.LoadFullSize(cachedFilePath);
        }
    }
}
