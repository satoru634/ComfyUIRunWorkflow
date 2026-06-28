using ComfyUILibs.Base;
using Wpf.Ui.Appearance;

namespace ComfyUIRunWorkflow.Models
{
    public partial class WindowSettingData : ObservableObject
    {
        [ObservableProperty]
        private ObservablePoint _windowPos;

        [ObservableProperty]
        private ObservableSize _windowSize;

        [ObservableProperty]
        private WindowState _state;

        [ObservableProperty]
        private ApplicationTheme _theme;

        [ObservableProperty]
        private bool _isPaneOpen;

        public WindowSettingData()
        {
            _windowPos = new ObservablePoint(0, 0);
            _windowSize = new ObservableSize(0, 0);
            _state = WindowState.Normal;
            _theme = ApplicationTheme.Light;
            _isPaneOpen = false;
        }
    }

    public partial class AppConfig : ObservableObject
    {
        [ObservableProperty]
        private WindowSettingData _windowSetting;

        public AppConfig()
        {
            _windowSetting = new WindowSettingData();

            _windowSetting.WindowPos = new ObservablePoint(100, 100);
            _windowSetting.WindowSize = new ObservableSize(1000, 640);
            _windowSetting.State = WindowState.Normal;
            _windowSetting.Theme = ApplicationTheme.Light;
            _windowSetting.IsPaneOpen = false;
        }
    }
}
