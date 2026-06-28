using ComfyUIRunWorkflow.ViewModels.Windows;
using Wpf.Ui;
using Wpf.Ui.Abstractions;
using Wpf.Ui.Appearance;
using Wpf.Ui.Controls;

namespace ComfyUIRunWorkflow.Views.Windows
{
    /// <summary>
    /// アプリケーションのメインウィンドウ。
    /// <see cref="INavigationWindow"/> を実装し、ナビゲーションの起点となる。
    /// </summary>
    public partial class MainWindow : INavigationWindow
    {
        /// <summary>このウィンドウに対応する ViewModel。</summary>
        public MainWindowViewModel ViewModel { get; }

        /// <summary>
        /// DI コンテナから依存サービスを受け取って初期化する。
        /// OS のテーマ変更を自動検知するために <see cref="SystemThemeWatcher"/> を登録する。
        /// </summary>
        public MainWindow(
            MainWindowViewModel viewModel,
            INavigationViewPageProvider navigationViewPageProvider,
            INavigationService navigationService
        )
        {
            ViewModel = viewModel;
            DataContext = this;

            // OS のテーマ変更（ライト/ダーク切り替え）を自動追従する
            SystemThemeWatcher.Watch(this);

            InitializeComponent();
            SetPageService(navigationViewPageProvider);

            navigationService.SetNavigationControl(RootNavigation);
        }

        #region INavigationWindow methods

        /// <summary>ナビゲーションビューコントロールを返す。</summary>
        public INavigationView GetNavigation() => RootNavigation;

        /// <summary>指定したページ型へ遷移する。</summary>
        public bool Navigate(Type pageType) => RootNavigation.Navigate(pageType);

        /// <summary>ページプロバイダーサービスをナビゲーションビューに設定する。</summary>
        public void SetPageService(INavigationViewPageProvider navigationViewPageProvider) => RootNavigation.SetPageProviderService(navigationViewPageProvider);

        /// <summary>ウィンドウを表示する。</summary>
        public void ShowWindow() => Show();

        /// <summary>ウィンドウを閉じる。</summary>
        public void CloseWindow() => Close();

        #endregion INavigationWindow methods

        /// <summary>
        /// ウィンドウが閉じた後に呼び出される。アプリケーション全体を終了する。
        /// </summary>
        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);

            // このウィンドウが唯一のメインウィンドウのため、閉じたらアプリを終了する
            Application.Current.Shutdown();
        }

        INavigationView INavigationWindow.GetNavigation()
        {
            throw new NotImplementedException();
        }

        public void SetServiceProvider(IServiceProvider serviceProvider)
        {
            throw new NotImplementedException();
        }
    }
}
