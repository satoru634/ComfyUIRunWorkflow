using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ComfyUIRunWorkflow.ViewModels.Pages;
using Wpf.Ui.Abstractions.Controls;

namespace ComfyUIRunWorkflow.Views.Pages
{
    /// <summary>
    /// ワークフロー設定編集ページ（Config）の View。
    /// DataContext に自身を設定することで XAML から ViewModel へアクセスする。
    /// </summary>
    public partial class ConfigEditorPage : INavigableView<ConfigEditorViewModel>
    {
        /// <summary>このページに対応する ViewModel。</summary>
        public ConfigEditorViewModel ViewModel { get; }

        /// <summary>
        /// DI コンテナから ViewModel を受け取って初期化する。
        /// </summary>
        public ConfigEditorPage(ConfigEditorViewModel viewModel)
        {
            ViewModel = viewModel;
            DataContext = this;

            InitializeComponent();
        }

        /// <summary>ワークフロー名テキストをダブルクリックしたら、そのワークフローをインライン編集モードにする。</summary>
        private void WorkflowNameText_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2 && sender is FrameworkElement { DataContext: ConfigWorkflowItemViewModel item })
            {
                ViewModel.BeginEditingWorkflowName(item);
                e.Handled = true;
            }
        }

        /// <summary>編集用テキストボックスが表示されたタイミングでフォーカスし、既存の文字列を選択状態にする。</summary>
        private void WorkflowNameEditBox_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (sender is TextBox { IsVisible: true } textBox)
            {
                textBox.Focus();
                textBox.SelectAll();
            }
        }

        /// <summary>編集用テキストボックスからフォーカスが外れたら編集を確定する。</summary>
        private void WorkflowNameEditBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement { DataContext: ConfigWorkflowItemViewModel item })
                ViewModel.FinishEditingWorkflowName(item);
        }

        /// <summary>Enter キーで編集を確定する。</summary>
        private void WorkflowNameEditBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && sender is FrameworkElement { DataContext: ConfigWorkflowItemViewModel item })
            {
                ViewModel.FinishEditingWorkflowName(item);
                e.Handled = true;
            }
        }
    }
}
