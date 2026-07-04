using System.ComponentModel;
using System.Globalization;
using ComfyUIRunWorkflow.Resources;

namespace ComfyUIRunWorkflow.Helpers
{
    /// <summary>
    /// GUI の表示文言を Strings.resx（既定: 日本語）/ Strings.en.resx（英語）から解決するシングルトン。
    /// XAML から <c>{Binding Source={x:Static helpers:LocalizationManager.Instance}, Path=[キー]}</c>
    /// のようにインデクサーへバインドすることで、<see cref="CurrentCulture"/> 変更時に全画面の文言が即座に切り替わる。
    /// </summary>
    public sealed class LocalizationManager : INotifyPropertyChanged
    {
        /// <summary>アプリ全体で共有するシングルトンインスタンス。</summary>
        public static LocalizationManager Instance { get; } = new();

        private CultureInfo _currentCulture = CultureInfo.CurrentUICulture;

        private LocalizationManager() { }

        /// <inheritdoc/>
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// 現在の表示言語。設定すると <see cref="CultureInfo.CurrentUICulture"/> /
        /// <see cref="CultureInfo.DefaultThreadCurrentUICulture"/> も更新し、インデクサーの変更通知を発行する。
        /// </summary>
        public CultureInfo CurrentCulture
        {
            get => _currentCulture;
            set
            {
                if (_currentCulture.Equals(value))
                    return;

                _currentCulture = value;
                CultureInfo.CurrentUICulture = value;
                CultureInfo.DefaultThreadCurrentUICulture = value;

                // WPF のインデクサーバインディング規約に従い "Item[]" で変更を通知する
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Item[]"));
            }
        }

        /// <summary>指定キーの表示文言を現在の言語で取得する。</summary>
        public string this[string key] => Strings.ResourceManager.GetString(key, _currentCulture) ?? key;
    }
}
