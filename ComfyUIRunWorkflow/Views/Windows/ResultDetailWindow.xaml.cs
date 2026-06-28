using ComfyUILibs.Models;

namespace ComfyUIRunWorkflow.Views.Windows
{
    /// <summary>
    /// ワークフロー実行結果の詳細を表示するダイアログウィンドウ。
    /// DataContext に <see cref="WorkflowResult"/> を設定して使用する。
    /// </summary>
    public partial class ResultDetailWindow
    {
        /// <summary>
        /// 表示する実行結果を受け取って初期化する。
        /// </summary>
        public ResultDetailWindow(WorkflowResult result)
        {
            DataContext = result;
            InitializeComponent();
        }

        private void OnCloseClick(object sender, System.Windows.RoutedEventArgs e)
            => Close();
    }
}
