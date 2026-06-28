using ComfyUILibs.Common;
using ComfyUIRunWorkflow.Models;
using Wpf.Ui.Abstractions.Controls;
using Wpf.Ui.Appearance;

namespace ComfyUIRunWorkflow.ViewModels.Pages
{
    /// <summary>
    /// 設定ページの ViewModel。テーマ切り替えとバージョン表示を担当する。
    /// </summary>
    public partial class SettingsViewModel : ObservableObject, INavigationAware
    {
        /// <summary>アプリケーション設定。</summary>
        public Setting<AppConfig> Config { get; }

        /// <summary>初期化済みかどうか。ページ遷移のたびに再初期化しないためのフラグ。</summary>
        private bool _isInitialized = false;

        /// <summary>アプリバージョン文字列（"ComfyUIRunWorkflow - x.x.x.x" 形式）。</summary>
        [ObservableProperty]
        private string _appVersion = String.Empty;

        /// <summary>コンボボックスで選択中のテーマ。変更時に即時適用される。</summary>
        [ObservableProperty]
        private ApplicationTheme _selectedTheme;

        /// <summary>テーマ選択コンボボックスに表示する選択肢。</summary>
        public List<ApplicationTheme> ThemeList { get; } = new List<ApplicationTheme>
        {
            ApplicationTheme.Light,
            ApplicationTheme.Dark
        };

        /// <summary>
        /// DI コンテナから設定を受け取って初期化する。
        /// </summary>
        public SettingsViewModel(Setting<AppConfig> config)
        {
            Config = config;
        }

        /// <summary>
        /// ページへナビゲートされたときに呼び出される。初回のみ <see cref="InitializeViewModel"/> を実行する。
        /// </summary>
        public Task OnNavigatedToAsync()
        {
            if (!_isInitialized)
                InitializeViewModel();

            return Task.CompletedTask;
        }

        /// <summary>
        /// ページから離れるときに呼び出される。現在は何もしない。
        /// </summary>
        public Task OnNavigatedFromAsync() => Task.CompletedTask;

        /// <summary>
        /// バージョン文字列と保存済みテーマを UI に反映する。
        /// </summary>
        private void InitializeViewModel()
        {
            AppVersion = $"ComfyUIRunWorkflow - {GetAssemblyVersion()}";
            SelectedTheme = Config.Data.WindowSetting.Theme;

            _isInitialized = true;
        }

        /// <summary>
        /// 実行中アセンブリのバージョン文字列を返す。
        /// </summary>
        private string GetAssemblyVersion()
        {
            return System.Reflection.Assembly.GetExecutingAssembly().GetName().Version?.ToString()
                ?? String.Empty;
        }

        /// <summary>
        /// テーマが変更されたときに設定へ保存してアプリに即時適用する。
        /// CommunityToolkit.Mvvm が <see cref="SelectedTheme"/> のセッターから自動呼び出しする。
        /// </summary>
        partial void OnSelectedThemeChanged(ApplicationTheme value)
        {
            Config.Data.WindowSetting.Theme = value;
            ApplicationThemeManager.Apply(value);
        }
    }
}
