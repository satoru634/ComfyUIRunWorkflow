using ComfyUILibs.Common;
using ComfyUIRunWorkflow.Models;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Wpf.Ui.Controls;

namespace ComfyUIRunWorkflow.ViewModels.Windows
{
    public partial class MainWindowViewModel : ObservableObject
    {
        [ObservableProperty]
        private string _applicationTitle = "WPF UI - ComfyUIRunWorkflow";

        [ObservableProperty]
        private ObservableCollection<object> _menuItems = new()
        {
            new NavigationViewItem()
            {
                Content = "Home",
                Icon = new SymbolIcon { Symbol = SymbolRegular.Home24 },
                TargetPageType = typeof(Views.Pages.DashboardPage)
            },
            new NavigationViewItem()
            {
                Content = "Data",
                Icon = new SymbolIcon { Symbol = SymbolRegular.DataHistogram24 },
                TargetPageType = typeof(Views.Pages.DataPage)
            }
        };

        [ObservableProperty]
        private ObservableCollection<object> _footerMenuItems = new()
        {
            new NavigationViewItem()
            {
                Content = "Settings",
                Icon = new SymbolIcon { Symbol = SymbolRegular.Settings24 },
                TargetPageType = typeof(Views.Pages.SettingsPage)
            }
        };

        [ObservableProperty]
        private ObservableCollection<MenuItem> _trayMenuItems = new()
        {
            new MenuItem { Header = "Home", Tag = "tray_home" }
        };

        public Setting<AppConfig> Config { get; }

        public MainWindowViewModel(Setting<AppConfig> config)
        {
            Config = config;
        }

        [RelayCommand]
        private void OnWindowClosing(CancelEventArgs? e)
        {
            // 終了前に確認ダイアログを出す例
            // もし終了をキャンセルしたい場合は e.Cancel = true; とする
            if (e != null)
            {
                // ここに保存確認などのロジックを記述
                // e.Cancel = true; 
            }

            Config.Save();
            return;
        }
    }
}
