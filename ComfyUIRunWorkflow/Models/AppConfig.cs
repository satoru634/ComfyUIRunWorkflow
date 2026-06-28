using ComfyUILibs.Base;
using System.IO;
using Wpf.Ui.Appearance;

namespace ComfyUIRunWorkflow.Models
{
    /// <summary>
    /// ウィンドウの表示状態（位置・サイズ・テーマ・ペイン開閉）を保持するデータクラス。
    /// <see cref="AppConfig"/> の一部として JSON に永続化される。
    /// </summary>
    public partial class WindowSettingData : ObservableObject
    {
        /// <summary>ウィンドウの左上座標（スクリーン座標系）。</summary>
        [ObservableProperty]
        private ObservablePoint _windowPos;

        /// <summary>ウィンドウの幅と高さ（ピクセル）。</summary>
        [ObservableProperty]
        private ObservableSize _windowSize;

        /// <summary>ウィンドウの表示状態（Normal / Minimized / Maximized）。</summary>
        [ObservableProperty]
        private WindowState _state;

        /// <summary>アプリケーションのテーマ（Light / Dark）。</summary>
        [ObservableProperty]
        private ApplicationTheme _theme;

        /// <summary>ナビゲーションペインが開いているかどうか。</summary>
        [ObservableProperty]
        private bool _isPaneOpen;

        /// <summary>
        /// フィールドを既定値で初期化する。JSON デシリアライズ時にも呼ばれるため、
        /// ここでは暫定の値のみを設定し、実際のデフォルト値は <see cref="AppConfig"/> のコンストラクターで上書きする。
        /// </summary>
        public WindowSettingData()
        {
            _windowPos = new ObservablePoint(0, 0);
            _windowSize = new ObservableSize(0, 0);
            _state = WindowState.Normal;
            _theme = ApplicationTheme.Light;
            _isPaneOpen = false;
        }
    }

    /// <summary>
    /// アプリケーション全体の設定を保持するルートクラス。
    /// <c>Setting&lt;AppConfig&gt;</c> 経由で <c>ComfyUIRunWorkflow_setting.json</c> に永続化される。
    /// </summary>
    public partial class AppConfig : ObservableObject
    {
        /// <summary>ウィンドウ状態の設定グループ。</summary>
        [ObservableProperty]
        private WindowSettingData _windowSetting;

        /// <summary>ComfyUI サーバーの接続先 URL。</summary>
        [ObservableProperty]
        private string _comfyUIUrl = "http://127.0.0.1:8188";

        /// <summary>ワークフロー設定ファイル（config.json）のパス。</summary>
        [ObservableProperty]
        private string _configPath = "";

        /// <summary>実行結果 JSON ファイルの出力先フォルダパス。</summary>
        [ObservableProperty]
        private string _resultsFolder = "";

        /// <summary>
        /// 初回起動時のデフォルト値を設定する。
        /// 設定ファイルが存在する場合は JSON デシリアライズ後に上書きされる。
        /// </summary>
        public AppConfig()
        {
            _windowSetting = new WindowSettingData();

            _windowSetting.WindowPos = new ObservablePoint(100, 100);
            _windowSetting.WindowSize = new ObservableSize(1000, 640);
            _windowSetting.State = WindowState.Normal;
            _windowSetting.Theme = ApplicationTheme.Light;
            _windowSetting.IsPaneOpen = false;

            _resultsFolder = Path.Combine(Directory.GetCurrentDirectory(), "Results");
        }
    }
}
