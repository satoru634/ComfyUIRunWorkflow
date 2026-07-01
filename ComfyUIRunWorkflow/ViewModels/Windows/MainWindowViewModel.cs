using ComfyUILibs.Common;
using ComfyUIRunWorkflow.Models;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
        private ObservableCollection<object> _menuItems = new()
        {
            new NavigationViewItem()
            {
                Content = "Run workflow",
                Icon = new SymbolIcon { Symbol = SymbolRegular.WindowPlay20 },
                TargetPageType = typeof(Views.Pages.DashboardPage)
            },
            new NavigationViewItem()
            {
                Content = "Results",
                Icon = new SymbolIcon { Symbol = SymbolRegular.TaskListLtr20 },
                TargetPageType = typeof(Views.Pages.DataPage)
            }
        };

        /// <summary>ナビゲーションペイン下部のフッターメニュー項目リスト（設定ページなど）。</summary>
        [ObservableProperty]
        private ObservableCollection<object> _footerMenuItems = new()
        {
            new NavigationViewItem()
            {
                Content = "Settings",
                Icon = new SymbolIcon { Symbol = SymbolRegular.Settings20 },
                TargetPageType = typeof(Views.Pages.SettingsPage)
            }
        };

        /// <summary>タスクトレイアイコンの右クリックメニュー項目リスト。</summary>
        [ObservableProperty]
        private ObservableCollection<MenuItem> _trayMenuItems = new()
        {
            new MenuItem { Header = "Home", Tag = "tray_home" }
        };

        /// <summary>アプリケーション設定（ウィンドウ状態・テーマ等）。</summary>
        public Setting<AppConfig> Config { get; }

        /// <summary>
        /// DI コンテナから設定を受け取って初期化する。
        /// </summary>
        public MainWindowViewModel(Setting<AppConfig> config)
        {
            Config = config;
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
