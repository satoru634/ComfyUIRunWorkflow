using ComfyUIRunWorkflow.ViewModels.Pages;
using Wpf.Ui.Abstractions.Controls;

namespace ComfyUIRunWorkflow.Views.Pages
{
    /// <summary>
    /// 設定ページの View。テーマ切り替え UI を提供する。
    /// DataContext に自身を設定することで XAML から ViewModel へアクセスする。
    /// </summary>
    public partial class SettingsPage : INavigableView<SettingsViewModel>
    {
        /// <summary>このページに対応する ViewModel。</summary>
        public SettingsViewModel ViewModel { get; }

        /// <summary>
        /// DI コンテナから ViewModel を受け取って初期化する。
        /// </summary>
        public SettingsPage(SettingsViewModel viewModel)
        {
            ViewModel = viewModel;
            DataContext = this;

            InitializeComponent();
        }
    }
}
