using System.ComponentModel;
using System.IO;
using System.Runtime.ExceptionServices;
using System.Text.Json;
using ComfyUILibs.Common;
using ComfyUIRunWorkflow.Models;
using ComfyUIRunWorkflow.ViewModels.Pages;
using ComfyUIRunWorkflowTests.Fakes;
using Wpf.Ui.Controls;

namespace ComfyUIRunWorkflowTests.ViewModels.Pages
{
    public class DashboardViewModelTests : IDisposable
    {
        private readonly string _tempDir;
        private readonly FakeSnackbarService _fakeSnackbar;

        public DashboardViewModelTests()
        {
            _tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            Directory.CreateDirectory(_tempDir);
            _fakeSnackbar = new FakeSnackbarService();
        }

        public void Dispose()
        {
            Directory.Delete(_tempDir, recursive: true);
        }

        private Setting<AppConfig> CreateSetting()
            => new Setting<AppConfig>(Path.Combine(_tempDir, "setting.json"), onLoad: false);

        private DashboardViewModel CreateVm(Setting<AppConfig>? setting = null)
            => new DashboardViewModel(setting ?? CreateSetting(), _fakeSnackbar);

        /// <summary>
        /// SymbolIcon など WPF コントロールの生成を含む処理を STA スレッドで実行するヘルパー。
        /// </summary>
        private static void RunOnSta(Action action)
        {
            Exception? caught = null;
            var thread = new Thread(() =>
            {
                try { action(); }
                catch (Exception ex) { caught = ex; }
            });
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join();
            if (caught is not null)
                ExceptionDispatchInfo.Capture(caught).Throw();
        }

        private string CreateConfigJson()
        {
            var configPath = Path.Combine(_tempDir, "config.json");
            var json = """
                {
                  "comfyui_url": "http://127.0.0.1:8188",
                  "default_workflow": "sdxl",
                  "workflows": {
                    "sdxl": {
                      "default_image_size": {"width": 832, "height": 1216},
                      "image_size": {
                        "vertical":   {"width": 832,  "height": 1216},
                        "horizontal": {"width": 1216, "height": 832},
                        "square":     {"width": 1024, "height": 1024}
                      },
                      "loras": {
                        "my_lora": {"file": "my_lora.safetensors", "strength": 0.8}
                      }
                    }
                  }
                }
                """;
            File.WriteAllText(configPath, json);
            return configPath;
        }

        // ── コンストラクター ───────────────────────────────────────────────────

        [Fact]
        public void Constructor_Config_IsSet()
        {
            var setting = CreateSetting();
            var vm = CreateVm(setting);
            Assert.Same(setting, vm.Config);
        }

        [Fact]
        public void Constructor_WorkflowNames_IsEmpty()
        {
            var vm = CreateVm();
            Assert.Empty(vm.WorkflowNames);
        }

        [Fact]
        public void Constructor_AvailableLoras_IsEmpty()
        {
            var vm = CreateVm();
            Assert.Empty(vm.AvailableLoras);
        }

        [Fact]
        public void Constructor_IsRunning_IsFalse()
        {
            var vm = CreateVm();
            Assert.False(vm.IsRunning);
        }

        [Fact]
        public void Constructor_LoraSlots_IsEmpty()
        {
            var vm = CreateVm();
            Assert.Empty(vm.LoraSlots);
        }

        [Fact]
        public void Constructor_IsConfigLoaded_IsFalse()
        {
            var vm = CreateVm();
            Assert.False(vm.IsConfigLoaded);
        }

        [Fact]
        public void Constructor_IsCustomSize_IsFalse()
        {
            var vm = CreateVm();
            Assert.False(vm.IsCustomSize);
        }

        [Fact]
        public void Constructor_CustomWidth_IsDefault()
        {
            var vm = CreateVm();
            Assert.Equal(832, vm.CustomWidth);
        }

        [Fact]
        public void Constructor_CustomHeight_IsDefault()
        {
            var vm = CreateVm();
            Assert.Equal(1216, vm.CustomHeight);
        }

        // ── 画像サイズ向き ─────────────────────────────────────────────────────

        [Fact]
        public void IsVertical_Initial_IsTrue()
        {
            var vm = CreateVm();
            Assert.True(vm.IsVertical);
        }

        [Fact]
        public void IsHorizontal_Initial_IsFalse()
        {
            var vm = CreateVm();
            Assert.False(vm.IsHorizontal);
        }

        [Fact]
        public void IsSquare_Initial_IsFalse()
        {
            var vm = CreateVm();
            Assert.False(vm.IsSquare);
        }

        [Fact]
        public void IsHorizontal_SetTrue_ChangesOrientation()
        {
            var vm = CreateVm();
            vm.IsHorizontal = true;
            Assert.Equal("horizontal", vm.ImageSizeOrientation);
            Assert.True(vm.IsHorizontal);
            Assert.False(vm.IsVertical);
        }

        [Fact]
        public void IsSquare_SetTrue_ChangesOrientation()
        {
            var vm = CreateVm();
            vm.IsSquare = true;
            Assert.Equal("square", vm.ImageSizeOrientation);
            Assert.True(vm.IsSquare);
            Assert.False(vm.IsVertical);
        }

        [Fact]
        public void ImageSizeOrientation_Change_NotifiesIsVertical()
        {
            var vm = CreateVm();
            var changed = new List<string?>();
            ((INotifyPropertyChanged)vm).PropertyChanged += (_, e) => changed.Add(e.PropertyName);

            vm.ImageSizeOrientation = "horizontal";

            Assert.Contains("IsVertical", changed);
            Assert.Contains("IsHorizontal", changed);
            Assert.Contains("IsSquare", changed);
        }

        // ── OnNavigatedToAsync ────────────────────────────────────────────────

        [Fact]
        public void OnNavigatedToAsync_EmptyConfigPath_IsConfigLoadedFalse()
        {
            var setting = CreateSetting();
            setting.Data.ConfigPath = "";
            var vm = CreateVm(setting);

            RunOnSta(() => vm.OnNavigatedToAsync().Wait());

            Assert.False(vm.IsConfigLoaded);
        }

        [Fact]
        public void OnNavigatedToAsync_EmptyConfigPath_ShowsSnackbar()
        {
            var setting = CreateSetting();
            setting.Data.ConfigPath = "";
            var vm = CreateVm(setting);

            RunOnSta(() => vm.OnNavigatedToAsync().Wait());

            Assert.Single(_fakeSnackbar.Calls);
            Assert.Equal(ControlAppearance.Danger, _fakeSnackbar.Calls[0].Appearance);
        }

        [Fact]
        public void OnNavigatedToAsync_InvalidConfigPath_IsConfigLoadedFalse()
        {
            var setting = CreateSetting();
            setting.Data.ConfigPath = Path.Combine(_tempDir, "nonexistent.json");
            var vm = CreateVm(setting);

            RunOnSta(() => vm.OnNavigatedToAsync().Wait());

            Assert.False(vm.IsConfigLoaded);
        }

        [Fact]
        public void OnNavigatedToAsync_InvalidConfigPath_ShowsSnackbar()
        {
            var setting = CreateSetting();
            setting.Data.ConfigPath = Path.Combine(_tempDir, "nonexistent.json");
            var vm = CreateVm(setting);

            RunOnSta(() => vm.OnNavigatedToAsync().Wait());

            Assert.Single(_fakeSnackbar.Calls);
            Assert.Equal(ControlAppearance.Danger, _fakeSnackbar.Calls[0].Appearance);
        }

        [Fact]
        public async Task OnNavigatedToAsync_ValidConfig_LoadsWorkflowNames()
        {
            var setting = CreateSetting();
            setting.Data.ConfigPath = CreateConfigJson();
            var vm = CreateVm(setting);

            await vm.OnNavigatedToAsync();

            Assert.True(vm.IsConfigLoaded);
            Assert.Contains("sdxl", vm.WorkflowNames);
        }

        [Fact]
        public async Task OnNavigatedToAsync_ValidConfig_SetsDefaultWorkflow()
        {
            var setting = CreateSetting();
            setting.Data.ConfigPath = CreateConfigJson();
            var vm = CreateVm(setting);

            await vm.OnNavigatedToAsync();

            Assert.Equal("sdxl", vm.SelectedWorkflow);
        }

        [Fact]
        public async Task OnNavigatedToAsync_ValidConfig_PopulatesAvailableLoras()
        {
            var setting = CreateSetting();
            setting.Data.ConfigPath = CreateConfigJson();
            var vm = CreateVm(setting);

            await vm.OnNavigatedToAsync();

            Assert.Contains("my_lora", vm.AvailableLoras);
        }

        [Fact]
        public async Task OnNavigatedToAsync_ValidConfig_UpdatesPresetLabels()
        {
            var setting = CreateSetting();
            setting.Data.ConfigPath = CreateConfigJson();
            var vm = CreateVm(setting);

            await vm.OnNavigatedToAsync();

            Assert.Contains("832", vm.VerticalLabel);
            Assert.Contains("1216", vm.VerticalLabel);
        }

        // ── LoRA 操作 ─────────────────────────────────────────────────────────

        [Fact]
        public void AddLoraCommand_Execute_AddsSlot()
        {
            var vm = CreateVm();
            vm.AddLoraCommand.Execute(null);
            Assert.Single(vm.LoraSlots);
        }

        [Fact]
        public void AddLoraCommand_ExecuteThreeTimes_AddThreeSlots()
        {
            var vm = CreateVm();
            vm.AddLoraCommand.Execute(null);
            vm.AddLoraCommand.Execute(null);
            vm.AddLoraCommand.Execute(null);
            Assert.Equal(3, vm.LoraSlots.Count);
        }

        [Fact]
        public void AddLoraCommand_AtMaxFour_DoesNotAddMore()
        {
            var vm = CreateVm();
            for (int i = 0; i < 5; i++)
                vm.AddLoraCommand.Execute(null);
            Assert.Equal(4, vm.LoraSlots.Count);
        }

        [Fact]
        public void RemoveLoraCommand_Execute_RemovesSlot()
        {
            var vm = CreateVm();
            vm.AddLoraCommand.Execute(null);
            var slot = vm.LoraSlots[0];

            vm.RemoveLoraCommand.Execute(slot);

            Assert.Empty(vm.LoraSlots);
        }

        [Fact]
        public void RemoveLoraCommand_MiddleSlot_RemovesCorrectSlot()
        {
            var vm = CreateVm();
            vm.AddLoraCommand.Execute(null);
            vm.AddLoraCommand.Execute(null);
            vm.AddLoraCommand.Execute(null);
            var middle = vm.LoraSlots[1];

            vm.RemoveLoraCommand.Execute(middle);

            Assert.Equal(2, vm.LoraSlots.Count);
            Assert.DoesNotContain(middle, vm.LoraSlots);
        }

        // ── RunWorkflowCommand CanExecute ─────────────────────────────────────

        [Fact]
        public void RunWorkflowCommand_WithoutConfig_CannotExecute()
        {
            var vm = CreateVm();
            vm.PositivePrompt = "test";
            Assert.False(vm.RunWorkflowCommand.CanExecute(null));
        }

        [Fact]
        public async Task RunWorkflowCommand_WithConfigAndEmptyPrompt_CannotExecute()
        {
            var setting = CreateSetting();
            setting.Data.ConfigPath = CreateConfigJson();
            var vm = CreateVm(setting);
            await vm.OnNavigatedToAsync();

            Assert.False(vm.RunWorkflowCommand.CanExecute(null));
        }

        [Fact]
        public async Task RunWorkflowCommand_WithConfigAndPrompt_CanExecute()
        {
            var setting = CreateSetting();
            setting.Data.ConfigPath = CreateConfigJson();
            var vm = CreateVm(setting);
            await vm.OnNavigatedToAsync();
            vm.PositivePrompt = "masterpiece";

            Assert.True(vm.RunWorkflowCommand.CanExecute(null));
        }
    }
}
