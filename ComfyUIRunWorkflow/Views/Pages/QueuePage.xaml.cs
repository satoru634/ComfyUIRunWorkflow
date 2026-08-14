using ComfyUIRunWorkflow.ViewModels.Pages;
using Wpf.Ui.Abstractions.Controls;

namespace ComfyUIRunWorkflow.Views.Pages
{
    /// <summary>
    /// 複数ワークフロー連続実行ページ（Queue）の View。
    /// DataContext に自身を設定することで XAML から ViewModel へアクセスする。
    /// </summary>
    public partial class QueuePage : INavigableView<QueueViewModel>
    {
        /// <summary>このページに対応する ViewModel。</summary>
        public QueueViewModel ViewModel { get; }

        /// <summary>
        /// DI コンテナから ViewModel を受け取って初期化する。
        /// </summary>
        public QueuePage(QueueViewModel viewModel)
        {
            ViewModel = viewModel;
            DataContext = this;

            InitializeComponent();
        }
    }
}
