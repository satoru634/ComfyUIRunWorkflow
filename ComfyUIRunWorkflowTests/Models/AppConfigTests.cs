using System.ComponentModel;
using System.IO;
using System.Windows;
using ComfyUILibs.Base;
using ComfyUIRunWorkflow.Models;
using Wpf.Ui.Appearance;

namespace ComfyUIRunWorkflowTests.Models
{
    public class AppConfigTests
    {
        // ── WindowSettingData コンストラクター ────────────────────────────────────

        [Fact]
        public void WindowSettingData_Constructor_WindowPos_IsZero()
        {
            var data = new WindowSettingData();

            Assert.Equal(0, data.WindowPos.X);
            Assert.Equal(0, data.WindowPos.Y);
        }

        [Fact]
        public void WindowSettingData_Constructor_WindowSize_IsZero()
        {
            var data = new WindowSettingData();

            Assert.Equal(0, data.WindowSize.Width);
            Assert.Equal(0, data.WindowSize.Height);
        }

        [Fact]
        public void WindowSettingData_Constructor_State_IsNormal()
        {
            var data = new WindowSettingData();

            Assert.Equal(WindowState.Normal, data.State);
        }

        [Fact]
        public void WindowSettingData_Constructor_Theme_IsLight()
        {
            var data = new WindowSettingData();

            Assert.Equal(ApplicationTheme.Light, data.Theme);
        }

        [Fact]
        public void WindowSettingData_Constructor_IsPaneOpen_IsFalse()
        {
            var data = new WindowSettingData();

            Assert.False(data.IsPaneOpen);
        }

        // ── WindowSettingData PropertyChanged ─────────────────────────────────────

        [Fact]
        public void WindowSettingData_WindowPos_Set_RaisesPropertyChanged()
        {
            var data = new WindowSettingData();
            var changed = new List<string?>();
            ((INotifyPropertyChanged)data).PropertyChanged += (_, e) => changed.Add(e.PropertyName);

            data.WindowPos = new ObservablePoint(10, 20);

            Assert.Contains("WindowPos", changed);
        }

        [Fact]
        public void WindowSettingData_WindowSize_Set_RaisesPropertyChanged()
        {
            var data = new WindowSettingData();
            var changed = new List<string?>();
            ((INotifyPropertyChanged)data).PropertyChanged += (_, e) => changed.Add(e.PropertyName);

            data.WindowSize = new ObservableSize(800, 600);

            Assert.Contains("WindowSize", changed);
        }

        [Fact]
        public void WindowSettingData_State_Set_RaisesPropertyChanged()
        {
            var data = new WindowSettingData();
            var changed = new List<string?>();
            ((INotifyPropertyChanged)data).PropertyChanged += (_, e) => changed.Add(e.PropertyName);

            data.State = WindowState.Maximized;

            Assert.Contains("State", changed);
        }

        [Fact]
        public void WindowSettingData_Theme_Set_RaisesPropertyChanged()
        {
            var data = new WindowSettingData();
            var changed = new List<string?>();
            ((INotifyPropertyChanged)data).PropertyChanged += (_, e) => changed.Add(e.PropertyName);

            data.Theme = ApplicationTheme.Dark;

            Assert.Contains("Theme", changed);
        }

        [Fact]
        public void WindowSettingData_IsPaneOpen_Set_RaisesPropertyChanged()
        {
            var data = new WindowSettingData();
            var changed = new List<string?>();
            ((INotifyPropertyChanged)data).PropertyChanged += (_, e) => changed.Add(e.PropertyName);

            data.IsPaneOpen = true;

            Assert.Contains("IsPaneOpen", changed);
        }

        // ── AppConfig コンストラクター ─────────────────────────────────────────────

        [Fact]
        public void AppConfig_Constructor_WindowSetting_IsNotNull()
        {
            var config = new AppConfig();

            Assert.NotNull(config.WindowSetting);
        }

        [Fact]
        public void AppConfig_Constructor_WindowPos_IsDefault()
        {
            var config = new AppConfig();

            Assert.Equal(100, config.WindowSetting.WindowPos.X);
            Assert.Equal(100, config.WindowSetting.WindowPos.Y);
        }

        [Fact]
        public void AppConfig_Constructor_WindowSize_IsDefault()
        {
            var config = new AppConfig();

            Assert.Equal(1000, config.WindowSetting.WindowSize.Width);
            Assert.Equal(640, config.WindowSetting.WindowSize.Height);
        }

        [Fact]
        public void AppConfig_Constructor_State_IsNormal()
        {
            var config = new AppConfig();

            Assert.Equal(WindowState.Normal, config.WindowSetting.State);
        }

        [Fact]
        public void AppConfig_Constructor_Theme_IsLight()
        {
            var config = new AppConfig();

            Assert.Equal(ApplicationTheme.Light, config.WindowSetting.Theme);
        }

        [Fact]
        public void AppConfig_Constructor_IsPaneOpen_IsFalse()
        {
            var config = new AppConfig();

            Assert.False(config.WindowSetting.IsPaneOpen);
        }

        // ── AppConfig PropertyChanged ──────────────────────────────────────────────

        [Fact]
        public void AppConfig_WindowSetting_Set_RaisesPropertyChanged()
        {
            var config = new AppConfig();
            var changed = new List<string?>();
            ((INotifyPropertyChanged)config).PropertyChanged += (_, e) => changed.Add(e.PropertyName);

            config.WindowSetting = new WindowSettingData();

            Assert.Contains("WindowSetting", changed);
        }

        // ── AppConfig 新フィールド ────────────────────────────────────────────────

        [Fact]
        public void AppConfig_ComfyUIUrl_DefaultValue()
        {
            var config = new AppConfig();
            Assert.Equal("http://127.0.0.1:8188", config.ComfyUIUrl);
        }

        [Fact]
        public void AppConfig_ConfigPath_DefaultValue()
        {
            var config = new AppConfig();
            Assert.Equal("", config.ConfigPath);
        }

        [Fact]
        public void AppConfig_ResultsFolder_DefaultValue()
        {
            var config = new AppConfig();
            var expected = Path.Combine(Directory.GetCurrentDirectory(), "Results");
            Assert.Equal(expected, config.ResultsFolder);
        }

        [Fact]
        public void AppConfig_ComfyUIUrl_Set_RaisesPropertyChanged()
        {
            var config = new AppConfig();
            var changed = new List<string?>();
            ((INotifyPropertyChanged)config).PropertyChanged += (_, e) => changed.Add(e.PropertyName);

            config.ComfyUIUrl = "http://192.168.1.100:8188";

            Assert.Contains("ComfyUIUrl", changed);
        }

        [Fact]
        public void AppConfig_ConfigPath_Set_RaisesPropertyChanged()
        {
            var config = new AppConfig();
            var changed = new List<string?>();
            ((INotifyPropertyChanged)config).PropertyChanged += (_, e) => changed.Add(e.PropertyName);

            config.ConfigPath = "C:\\workflow_config.json";

            Assert.Contains("ConfigPath", changed);
        }

        [Fact]
        public void AppConfig_ResultsFolder_Set_RaisesPropertyChanged()
        {
            var config = new AppConfig();
            var changed = new List<string?>();
            ((INotifyPropertyChanged)config).PropertyChanged += (_, e) => changed.Add(e.PropertyName);

            config.ResultsFolder = "C:\\results";

            Assert.Contains("ResultsFolder", changed);
        }

        [Fact]
        public void AppConfig_Language_DefaultValue_IsJa()
        {
            var config = new AppConfig();
            Assert.Equal("ja", config.Language);
        }

        [Fact]
        public void AppConfig_Language_Set_RaisesPropertyChanged()
        {
            var config = new AppConfig();
            var changed = new List<string?>();
            ((INotifyPropertyChanged)config).PropertyChanged += (_, e) => changed.Add(e.PropertyName);

            config.Language = "en";

            Assert.Contains("Language", changed);
        }
    }
}
