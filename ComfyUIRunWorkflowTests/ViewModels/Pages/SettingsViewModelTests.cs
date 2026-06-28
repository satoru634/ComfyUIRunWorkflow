using System.ComponentModel;
using System.IO;
using ComfyUILibs.Common;
using ComfyUIRunWorkflow.Models;
using ComfyUIRunWorkflow.ViewModels.Pages;
using Wpf.Ui.Appearance;

namespace ComfyUIRunWorkflowTests.ViewModels.Pages
{
    public class SettingsViewModelTests : IDisposable
    {
        private readonly string _tempDir;

        public SettingsViewModelTests()
        {
            _tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            Directory.CreateDirectory(_tempDir);
        }

        public void Dispose()
        {
            Directory.Delete(_tempDir, recursive: true);
        }

        private Setting<AppConfig> CreateSetting()
            => new Setting<AppConfig>(Path.Combine(_tempDir, "setting.json"), onLoad: false);

        // ── コンストラクター ───────────────────────────────────────────────────

        [Fact]
        public void Constructor_Config_IsSet()
        {
            var setting = CreateSetting();

            var vm = new SettingsViewModel(setting);

            Assert.Same(setting, vm.Config);
        }

        // ── ThemeList ─────────────────────────────────────────────────────────

        [Fact]
        public void ThemeList_Count_IsTwo()
        {
            var vm = new SettingsViewModel(CreateSetting());

            Assert.Equal(2, vm.ThemeList.Count);
        }

        [Fact]
        public void ThemeList_ContainsLight()
        {
            var vm = new SettingsViewModel(CreateSetting());

            Assert.Contains(ApplicationTheme.Light, vm.ThemeList);
        }

        [Fact]
        public void ThemeList_ContainsDark()
        {
            var vm = new SettingsViewModel(CreateSetting());

            Assert.Contains(ApplicationTheme.Dark, vm.ThemeList);
        }

        // ── OnNavigatedToAsync ────────────────────────────────────────────────

        [Fact]
        public async Task OnNavigatedToAsync_AppVersion_StartsWithAppName()
        {
            var vm = new SettingsViewModel(CreateSetting());

            await vm.OnNavigatedToAsync();

            Assert.StartsWith("ComfyUIRunWorkflow - ", vm.AppVersion);
        }

        [Fact]
        public async Task OnNavigatedToAsync_SelectedTheme_LoadedFromConfig()
        {
            var setting = CreateSetting();
            setting.Data.WindowSetting.Theme = ApplicationTheme.Dark;
            var vm = new SettingsViewModel(setting);

            await vm.OnNavigatedToAsync();

            Assert.Equal(ApplicationTheme.Dark, vm.SelectedTheme);
        }

        [Fact]
        public async Task OnNavigatedToAsync_CalledTwice_DoesNotReset()
        {
            var setting = CreateSetting();
            var vm = new SettingsViewModel(setting);
            await vm.OnNavigatedToAsync();
            var versionAfterFirst = vm.AppVersion;

            await vm.OnNavigatedToAsync();

            Assert.Equal(versionAfterFirst, vm.AppVersion);
        }

        // ── SelectedTheme ──────────────────────────────────────────────────────

        [Fact]
        public void SelectedTheme_Set_UpdatesConfigTheme()
        {
            var setting = CreateSetting();
            var vm = new SettingsViewModel(setting);

            vm.SelectedTheme = ApplicationTheme.Dark;

            Assert.Equal(ApplicationTheme.Dark, setting.Data.WindowSetting.Theme);
        }

        [Fact]
        public void SelectedTheme_SetToLight_UpdatesConfigTheme()
        {
            var setting = CreateSetting();
            setting.Data.WindowSetting.Theme = ApplicationTheme.Dark;
            var vm = new SettingsViewModel(setting);

            vm.SelectedTheme = ApplicationTheme.Light;

            Assert.Equal(ApplicationTheme.Light, setting.Data.WindowSetting.Theme);
        }

        [Fact]
        public void SelectedTheme_Set_RaisesPropertyChanged()
        {
            var vm = new SettingsViewModel(CreateSetting());
            var changed = new List<string?>();
            ((INotifyPropertyChanged)vm).PropertyChanged += (_, e) => changed.Add(e.PropertyName);

            vm.SelectedTheme = ApplicationTheme.Dark;

            Assert.Contains("SelectedTheme", changed);
        }

        // ── OnNavigatedFromAsync ──────────────────────────────────────────────

        [Fact]
        public async Task OnNavigatedFromAsync_ReturnsCompletedTask()
        {
            var vm = new SettingsViewModel(CreateSetting());

            var task = vm.OnNavigatedFromAsync();
            await task;

            Assert.True(task.IsCompletedSuccessfully);
        }
    }
}
