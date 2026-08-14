using ComfyUILibs.Common;
using ComfyUILibs.Models;
using ComfyUIRunWorkflow.Models;
using ComfyUIRunWorkflow.ViewModels.Windows;
using Wpf.Ui.Controls;

namespace ComfyUIRunWorkflow.Views.Windows
{
    /// <summary>
    /// ワークフロー実行結果の詳細を表示するダイアログウィンドウ。
    /// DataContext に <see cref="ResultDetailViewModel"/> を設定して使用する。
    /// </summary>
    public partial class ResultDetailWindow : ContentDialog
    {
        public ResultDetailViewModel ViewModel { get; }

        /// <summary>
        /// 表示する実行結果とアプリケーション設定を受け取って初期化する。
        /// </summary>
        public ResultDetailWindow(
            ResultDetailViewModel viewModel,
            ContentDialogHost? contentDialogHost) : base(contentDialogHost)
        {
            ViewModel = viewModel;
            DataContext = viewModel;
            InitializeComponent();
        }
    }
}
