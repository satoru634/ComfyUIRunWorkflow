using ComfyUILibs.Common;
using ComfyUIRunWorkflow.Models;

namespace ComfyUIRunWorkflow.ViewModels.Pages
{
    /// <summary>
    /// ダッシュボードページの ViewModel。
    /// 将来的にワークフロー実行 UI に置き換える。
    /// </summary>
    public partial class DashboardViewModel : ObservableObject
    {
        /// <summary>アプリケーション設定。</summary>
        public Setting<AppConfig> Config { get; }

        /// <summary>デモ用カウンター値。</summary>
        [ObservableProperty]
        private int _counter = 0;

        /// <summary>
        /// DI コンテナから設定を受け取って初期化する。
        /// </summary>
        public DashboardViewModel(Setting<AppConfig> config)
        {
            Config = config;
        }

        /// <summary>
        /// カウンターをインクリメントするコマンドハンドラー。
        /// </summary>
        [RelayCommand]
        private void OnCounterIncrement()
        {
            Counter++;
        }
    }
}
