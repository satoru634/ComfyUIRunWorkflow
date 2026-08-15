using ComfyUIRunWorkflow.ViewModels.Pages;
using Wpf.Ui.Abstractions.Controls;

namespace ComfyUIRunWorkflow.Views.Pages
{
    /// <summary>
    /// バッチジョブ生成ページ（Generate）の View。
    /// DataContext に自身を設定することで XAML から ViewModel へアクセスする。
    /// </summary>
    public partial class GeneratePage : INavigableView<GenerateViewModel>
    {
        /// <summary>このページに対応する ViewModel。</summary>
        public GenerateViewModel ViewModel { get; }

        /// <summary>
        /// DI コンテナから ViewModel を受け取って初期化する。
        /// </summary>
        public GeneratePage(GenerateViewModel viewModel)
        {
            ViewModel = viewModel;
            DataContext = this;

            InitializeComponent();
        }
    }
}
