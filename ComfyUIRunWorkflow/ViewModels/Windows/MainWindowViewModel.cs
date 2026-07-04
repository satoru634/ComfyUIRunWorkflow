using ComfyUILibs.Common;
using ComfyUIRunWorkflow.Helpers;
using ComfyUIRunWorkflow.Models;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading;
using Wpf.Ui.Controls;

namespace ComfyUIRunWorkflow.ViewModels.Windows
{
    /// <summary>
    /// メインウィンドウの ViewModel。
    /// ナビゲーション項目の定義とウィンドウクローズ時の設定保存を担当する。
    /// </summary>
    public partial class MainWindowViewModel : ObservableObject
    {
        /// <summary>タイトルバーに表示するアプリケーション名。</summary>
        [ObservableProperty]
        private string _applicationTitle = "ComfyUIRunWorkflow";

        /// <summary>ナビゲーションペインのメニュー項目リスト。</summary>
        [ObservableProperty]
        private ObservableCollection<object> _menuItems = new();

        /// <summary>ナビゲーションペイン下部のフッターメニュー項目リスト（設定ページなど）。</summary>
        [ObservableProperty]
        private ObservableCollection<object> _footerMenuItems = new();

        /// <summary>タスクトレイアイコンの右クリックメニュー項目リスト。</summary>
        [ObservableProperty]
        private ObservableCollection<MenuItem> _trayMenuItems = new();

        /// <summary>アプリケーション設定（ウィンドウ状態・テーマ等）。</summary>
        public Setting<AppConfig> Config { get; }

        /// <summary>
        /// DI コンテナから設定を受け取って初期化する。
        /// 本 ViewModel はシングルトン登録されているため、言語切替時にメニュー項目を
        /// 再生成できるよう <see cref="LocalizationManager"/> の変更通知を購読し続ける。
        /// </summary>
        public MainWindowViewModel(Setting<AppConfig> config)
        {
            Config = config;
            BuildMenuItems();
            // NavigationViewItem/MenuItem の生成には STA スレッドが必要なため、
            // 言語切替以外の要因（テスト等で他スレッドから CurrentCulture が変更される場合）で
            // 誤って非 STA スレッドから呼び出されないようガードする。
            LocalizationManager.Instance.PropertyChanged += (_, _) =>
            {
                if (Thread.CurrentThread.GetApartmentState() == ApartmentState.STA)
                    BuildMenuItems();
            };
        }

        /// <summary>ナビゲーション項目・トレイメニュー項目を現在の言語で構築（再構築）する。</summary>
        private void BuildMenuItems()
        {
            MenuItems = new ObservableCollection<object>
            {
                new NavigationViewItem()
                {
                    Content = LocalizationManager.Instance["MainWindow_MenuDashboard"],
                    Icon = new SymbolIcon { Symbol = SymbolRegular.WindowPlay20 },
                    TargetPageType = typeof(Views.Pages.DashboardPage)
                },
                new NavigationViewItem()
                {
                    Content = LocalizationManager.Instance["MainWindow_MenuResults"],
                    Icon = new SymbolIcon { Symbol = SymbolRegular.TaskListLtr20 },
                    TargetPageType = typeof(Views.Pages.DataPage)
                },
                new NavigationViewItem()
                {
                    Content = LocalizationManager.Instance["MainWindow_MenuTagger"],
                    Icon = new SymbolIcon { Symbol = SymbolRegular.TagSearch20 },
                    TargetPageType = typeof(Views.Pages.TaggerPage)
                }
            };

            FooterMenuItems = new ObservableCollection<object>
            {
                new NavigationViewItem()
                {
                    Content = LocalizationManager.Instance["MainWindow_MenuSettings"],
                    Icon = new SymbolIcon { Symbol = SymbolRegular.Settings20 },
                    TargetPageType = typeof(Views.Pages.SettingsPage)
                }
            };

            TrayMenuItems = new ObservableCollection<MenuItem>
            {
                new MenuItem { Header = LocalizationManager.Instance["MainWindow_TrayHome"], Tag = "tray_home" }
            };
        }

        /// <summary>
        /// ウィンドウが閉じられる直前に呼び出されるコマンドハンドラー。
        /// 現在のウィンドウ状態（位置・サイズ・テーマ等）を設定ファイルに保存する。
        /// </summary>
        /// <param name="e">キャンセル可能な場合は <c>e.Cancel = true</c> で終了を中断できる。</param>
        [RelayCommand]
        private void OnWindowClosing(CancelEventArgs? e)
        {
            Config.Save();
        }
    }
}
