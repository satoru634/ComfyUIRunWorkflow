using ComfyUIRunWorkflow.Services;
using Wpf.Ui.Controls;

namespace ComfyUIRunWorkflow.Views.Controls
{
    /// <summary>
    /// 生成画像を原寸で拡大表示するダイアログ。
    /// </summary>
    public partial class ImagePreviewWindow : ContentDialog
    {
        /// <summary>キャッシュ済み画像ファイルのパスとダイアログホストを受け取って初期化する。</summary>
        public ImagePreviewWindow(string cachedFilePath, ContentDialogHost? contentDialogHost) : base(contentDialogHost)
        {
            InitializeComponent();
            PreviewImage.Source = PreviewImageLoader.LoadFullSize(cachedFilePath);
        }
    }
}
