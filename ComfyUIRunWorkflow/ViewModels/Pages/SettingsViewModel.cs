using ComfyUILibs.Common;
using ComfyUIRunWorkflow.Models;
using Microsoft.Win32;
using Wpf.Ui.Abstractions.Controls;
using Wpf.Ui.Appearance;

namespace ComfyUIRunWorkflow.ViewModels.Pages
{
    /// <summary>
    /// 設定ページの ViewModel。テーマ切り替え・接続設定・出力フォルダの管理を担当する。
    /// </summary>
    public partial class SettingsViewModel : ObservableObject, INavigationAware
    {
        /// <summary>アプリケーション設定。</summary>
        public Setting<AppConfig> Config { get; }

        private bool _isInitialized = false;

        /// <summary>アプリバージョン文字列。</summary>
        [ObservableProperty]
        private string _appVersion = String.Empty;

        /// <summary>選択中のテーマ。変更時に即時適用される。</summary>
        [ObservableProperty]
        private ApplicationTheme _selectedTheme;

        /// <summary>テーマ選択コンボボックスに表示する選択肢。</summary>
        public List<ApplicationTheme> ThemeList { get; } = new List<ApplicationTheme>
        {
            ApplicationTheme.Light,
            ApplicationTheme.Dark
        };

        /// <summary>DI コンテナから設定を受け取って初期化する。</summary>
        public SettingsViewModel(Setting<AppConfig> config)
        {
            Config = config;
        }

        /// <summary>ページへナビゲートされたときに呼び出される。初回のみ初期化する。</summary>
        public Task OnNavigatedToAsync()
        {
            if (!_isInitialized)
                InitializeViewModel();

            return Task.CompletedTask;
        }

        /// <summary>ページから離れるときに設定を保存する。</summary>
        public Task OnNavigatedFromAsync()
        {
            Config.Save();
            return Task.CompletedTask;
        }

        private void InitializeViewModel()
        {
            AppVersion = $"ComfyUIRunWorkflow - {GetAssemblyVersion()}";
            SelectedTheme = Config.Data.WindowSetting.Theme;
            _isInitialized = true;
        }

        private string GetAssemblyVersion()
            => System.Reflection.Assembly.GetExecutingAssembly().GetName().Version?.ToString()
                ?? String.Empty;

        /// <summary>テーマが変更されたとき、設定へ保存してアプリに即時適用する。</summary>
        partial void OnSelectedThemeChanged(ApplicationTheme value)
        {
            Config.Data.WindowSetting.Theme = value;
            ApplicationThemeManager.Apply(value);
        }

        // ── ファイル参照コマンド ──────────────────────────────────────────────

        /// <summary>config.json のファイル選択ダイアログを開く。</summary>
        [RelayCommand]
        private void BrowseConfigPath()
        {
            var dialog = new OpenFileDialog
            {
                Title = "config.json を選択",
                Filter = "JSON ファイル|*.json|すべてのファイル|*.*",
            };

            if (!string.IsNullOrWhiteSpace(Config.Data.ConfigPath))
            {
                var dir = System.IO.Path.GetDirectoryName(Config.Data.ConfigPath);
                if (!string.IsNullOrEmpty(dir))
                    dialog.InitialDirectory = dir;
            }

            if (dialog.ShowDialog() == true)
                Config.Data.ConfigPath = dialog.FileName;
        }

        /// <summary>結果出力フォルダの選択ダイアログを開く。</summary>
        [RelayCommand]
        private void BrowseResultsFolder()
        {
            var dialog = new OpenFolderDialog
            {
                Title = "結果出力フォルダを選択",
            };

            if (!string.IsNullOrWhiteSpace(Config.Data.ResultsFolder))
                dialog.InitialDirectory = Config.Data.ResultsFolder;

            if (dialog.ShowDialog() == true)
                Config.Data.ResultsFolder = dialog.FolderName;
        }
    }
}
