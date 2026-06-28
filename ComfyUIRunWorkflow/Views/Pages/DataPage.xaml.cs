using ComfyUIRunWorkflow.ViewModels.Pages;
using Wpf.Ui.Abstractions.Controls;

namespace ComfyUIRunWorkflow.Views.Pages
{
    /// <summary>
    /// データページの View。
    /// DataContext に自身を設定することで XAML から ViewModel へアクセスする。
    /// </summary>
    public partial class DataPage : INavigableView<DataViewModel>
    {
        /// <summary>このページに対応する ViewModel。</summary>
        public DataViewModel ViewModel { get; }

        /// <summary>
        /// DI コンテナから ViewModel を受け取って初期化する。
        /// </summary>
        public DataPage(DataViewModel viewModel)
        {
            ViewModel = viewModel;
            DataContext = this;

            InitializeComponent();
        }
    }
}
