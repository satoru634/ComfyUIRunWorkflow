using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
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

        /// <summary>ジョブ名テキストをダブルクリックしたら、そのジョブをインライン編集モードにする。</summary>
        private void JobNameText_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2 && sender is FrameworkElement { DataContext: QueueJobViewModel job })
            {
                job.IsEditingName = true;
                e.Handled = true;
            }
        }

        /// <summary>編集用テキストボックスが表示されたタイミングでフォーカスし、既存の文字列を選択状態にする。</summary>
        private void JobNameEditBox_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (sender is TextBox { IsVisible: true } textBox)
            {
                textBox.Focus();
                textBox.SelectAll();
            }
        }

        /// <summary>編集用テキストボックスからフォーカスが外れたら編集モードを終了する。</summary>
        private void JobNameEditBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement { DataContext: QueueJobViewModel job })
                job.IsEditingName = false;
        }

        /// <summary>Enter キーで編集モードを終了する。</summary>
        private void JobNameEditBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && sender is FrameworkElement { DataContext: QueueJobViewModel job })
            {
                job.IsEditingName = false;
                e.Handled = true;
            }
        }
    }
}
