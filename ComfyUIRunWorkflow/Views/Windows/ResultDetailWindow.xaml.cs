using ComfyUILibs.Common;
using ComfyUILibs.Models;
using ComfyUIRunWorkflow.Models;
using ComfyUIRunWorkflow.ViewModels.Windows;

namespace ComfyUIRunWorkflow.Views.Windows
{
    /// <summary>
    /// ワークフロー実行結果の詳細を表示するダイアログウィンドウ。
    /// DataContext に <see cref="ResultDetailViewModel"/> を設定して使用する。
    /// </summary>
    public partial class ResultDetailWindow
    {
        /// <summary>
        /// 表示する実行結果とアプリケーション設定を受け取って初期化する。
        /// </summary>
        public ResultDetailWindow(WorkflowResult result, Setting<AppConfig> config)
        {
            DataContext = new ResultDetailViewModel(result, config);
            InitializeComponent();
            MaxHeight = SystemParameters.WorkArea.Height;
        }

        private void OnCloseClick(object sender, System.Windows.RoutedEventArgs e)
            => Close();
    }
}
