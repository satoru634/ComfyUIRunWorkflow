using ComfyUILibs.Common;
using ComfyUIRunWorkflow.Models;
using System.Windows.Media;
using Wpf.Ui.Abstractions.Controls;

namespace ComfyUIRunWorkflow.ViewModels.Pages
{
    /// <summary>
    /// データページの ViewModel。
    /// 現在はランダムカラーのデモ表示のみ。将来的に実行結果一覧に置き換える。
    /// </summary>
    public partial class DataViewModel : ObservableObject, INavigationAware
    {
        /// <summary>アプリケーション設定。</summary>
        public Setting<AppConfig> Config { get; }

        /// <summary>初期化済みかどうか。ページ遷移のたびに再初期化しないためのフラグ。</summary>
        private bool _isInitialized = false;

        /// <summary>表示するカラーコレクション。</summary>
        [ObservableProperty]
        private IEnumerable<DataColor> _colors;

        /// <summary>
        /// DI コンテナから設定を受け取って初期化する。
        /// </summary>
        public DataViewModel(Setting<AppConfig> config)
        {
            Config = config;
            _colors = new List<DataColor>();
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
        /// デモ用のランダムカラーデータを 8192 件生成して <see cref="Colors"/> に設定する。
        /// </summary>
        private void InitializeViewModel()
        {
            var random = new Random();
            var colorCollection = new List<DataColor>();

            for (int i = 0; i < 8192; i++)
                colorCollection.Add(
                    new DataColor
                    {
                        Color = new SolidColorBrush(
                            Color.FromArgb(
                                (byte)200,
                                (byte)random.Next(0, 250),
                                (byte)random.Next(0, 250),
                                (byte)random.Next(0, 250)
                            )
                        )
                    }
                );

            Colors = colorCollection;

            _isInitialized = true;
        }
    }
}
