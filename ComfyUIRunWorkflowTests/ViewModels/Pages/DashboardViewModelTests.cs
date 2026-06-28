using System.ComponentModel;
using System.IO;
using System.Text.Json;
using ComfyUILibs.Common;
using ComfyUIRunWorkflow.Models;
using ComfyUIRunWorkflow.ViewModels.Pages;

namespace ComfyUIRunWorkflowTests.ViewModels.Pages
{
    public class DashboardViewModelTests : IDisposable
    {
        private readonly string _tempDir;

        public DashboardViewModelTests()
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
            var vm = new DashboardViewModel(setting);
            Assert.Same(setting, vm.Config);
        }

        [Fact]
        public void Constructor_WorkflowNames_IsEmpty()
        {
            var vm = new DashboardViewModel(CreateSetting());
            Assert.Empty(vm.WorkflowNames);
        }

        [Fact]
        public void Constructor_AvailableLoras_IsEmpty()
        {
            var vm = new DashboardViewModel(CreateSetting());
            Assert.Empty(vm.AvailableLoras);
        }

        [Fact]
        public void Constructor_IsRunning_IsFalse()
        {
            var vm = new DashboardViewModel(CreateSetting());
            Assert.False(vm.IsRunning);
        }

        [Fact]
        public void Constructor_LoraSlots_IsEmpty()
        {
            var vm = new DashboardViewModel(CreateSetting());
            Assert.Empty(vm.LoraSlots);
        }

        [Fact]
        public void Constructor_IsConfigLoaded_IsFalse()
        {
            var vm = new DashboardViewModel(CreateSetting());
            Assert.False(vm.IsConfigLoaded);
        }

        [Fact]
        public void Constructor_IsCustomSize_IsFalse()
        {
            var vm = new DashboardViewModel(CreateSetting());
            Assert.False(vm.IsCustomSize);
        }

        [Fact]
        public void Constructor_CustomWidth_IsDefault()
        {
            var vm = new DashboardViewModel(CreateSetting());
            Assert.Equal(832, vm.CustomWidth);
        }

        [Fact]
        public void Constructor_CustomHeight_IsDefault()
        {
            var vm = new DashboardViewModel(CreateSetting());
            Assert.Equal(1216, vm.CustomHeight);
        }

        // ── 画像サイズ向き ─────────────────────────────────────────────────────

        [Fact]
        public void IsVertical_Initial_IsTrue()
        {
            var vm = new DashboardViewModel(CreateSetting());
            Assert.True(vm.IsVertical);
        }

        [Fact]
        public void IsHorizontal_Initial_IsFalse()
        {
            var vm = new DashboardViewModel(CreateSetting());
            Assert.False(vm.IsHorizontal);
        }

        [Fact]
        public void IsSquare_Initial_IsFalse()
        {
            var vm = new DashboardViewModel(CreateSetting());
            Assert.False(vm.IsSquare);
        }

        [Fact]
        public void IsHorizontal_SetTrue_ChangesOrientation()
        {
            var vm = new DashboardViewModel(CreateSetting());
            vm.IsHorizontal = true;
            Assert.Equal("horizontal", vm.ImageSizeOrientation);
            Assert.True(vm.IsHorizontal);
            Assert.False(vm.IsVertical);
        }

        [Fact]
        public void IsSquare_SetTrue_ChangesOrientation()
        {
            var vm = new DashboardViewModel(CreateSetting());
            vm.IsSquare = true;
            Assert.Equal("square", vm.ImageSizeOrientation);
            Assert.True(vm.IsSquare);
            Assert.False(vm.IsVertical);
        }

        [Fact]
        public void ImageSizeOrientation_Change_NotifiesIsVertical()
        {
            var vm = new DashboardViewModel(CreateSetting());
            var changed = new List<string?>();
            ((INotifyPropertyChanged)vm).PropertyChanged += (_, e) => changed.Add(e.PropertyName);

            vm.ImageSizeOrientation = "horizontal";

            Assert.Contains("IsVertical", changed);
            Assert.Contains("IsHorizontal", changed);
            Assert.Contains("IsSquare", changed);
        }

        // ── OnNavigatedToAsync ────────────────────────────────────────────────

        [Fact]
        public async Task OnNavigatedToAsync_EmptyConfigPath_ShowsGuideMessage()
        {
            var setting = CreateSetting();
            setting.Data.ConfigPath = "";
            var vm = new DashboardViewModel(setting);

            await vm.OnNavigatedToAsync();

            Assert.False(vm.IsConfigLoaded);
            Assert.NotEmpty(vm.ConfigStatus);
        }

        [Fact]
        public async Task OnNavigatedToAsync_InvalidConfigPath_ShowsErrorMessage()
        {
            var setting = CreateSetting();
            setting.Data.ConfigPath = Path.Combine(_tempDir, "nonexistent.json");
            var vm = new DashboardViewModel(setting);

            await vm.OnNavigatedToAsync();

            Assert.False(vm.IsConfigLoaded);
            Assert.NotEmpty(vm.ConfigStatus);
        }

        [Fact]
        public async Task OnNavigatedToAsync_ValidConfig_LoadsWorkflowNames()
        {
            var setting = CreateSetting();
            setting.Data.ConfigPath = CreateConfigJson();
            var vm = new DashboardViewModel(setting);

            await vm.OnNavigatedToAsync();

            Assert.True(vm.IsConfigLoaded);
            Assert.Contains("sdxl", vm.WorkflowNames);
        }

        [Fact]
        public async Task OnNavigatedToAsync_ValidConfig_SetsDefaultWorkflow()
        {
            var setting = CreateSetting();
            setting.Data.ConfigPath = CreateConfigJson();
            var vm = new DashboardViewModel(setting);

            await vm.OnNavigatedToAsync();

            Assert.Equal("sdxl", vm.SelectedWorkflow);
        }

        [Fact]
        public async Task OnNavigatedToAsync_ValidConfig_PopulatesAvailableLoras()
        {
            var setting = CreateSetting();
            setting.Data.ConfigPath = CreateConfigJson();
            var vm = new DashboardViewModel(setting);

            await vm.OnNavigatedToAsync();

            Assert.Contains("my_lora", vm.AvailableLoras);
        }

        [Fact]
        public async Task OnNavigatedToAsync_ValidConfig_UpdatesPresetLabels()
        {
            var setting = CreateSetting();
            setting.Data.ConfigPath = CreateConfigJson();
            var vm = new DashboardViewModel(setting);

            await vm.OnNavigatedToAsync();

            Assert.Contains("832", vm.VerticalLabel);
            Assert.Contains("1216", vm.VerticalLabel);
        }

        // ── LoRA 操作 ─────────────────────────────────────────────────────────

        [Fact]
        public void AddLoraCommand_Execute_AddsSlot()
        {
            var vm = new DashboardViewModel(CreateSetting());
            vm.AddLoraCommand.Execute(null);
            Assert.Single(vm.LoraSlots);
        }

        [Fact]
        public void AddLoraCommand_ExecuteThreeTimes_AddThreeSlots()
        {
            var vm = new DashboardViewModel(CreateSetting());
            vm.AddLoraCommand.Execute(null);
            vm.AddLoraCommand.Execute(null);
            vm.AddLoraCommand.Execute(null);
            Assert.Equal(3, vm.LoraSlots.Count);
        }

        [Fact]
        public void AddLoraCommand_AtMaxFour_DoesNotAddMore()
        {
            var vm = new DashboardViewModel(CreateSetting());
            for (int i = 0; i < 5; i++)
                vm.AddLoraCommand.Execute(null);
            Assert.Equal(4, vm.LoraSlots.Count);
        }

        [Fact]
        public void RemoveLoraCommand_Execute_RemovesSlot()
        {
            var vm = new DashboardViewModel(CreateSetting());
            vm.AddLoraCommand.Execute(null);
            var slot = vm.LoraSlots[0];

            vm.RemoveLoraCommand.Execute(slot);

            Assert.Empty(vm.LoraSlots);
        }

        [Fact]
        public void RemoveLoraCommand_MiddleSlot_RemovesCorrectSlot()
        {
            var vm = new DashboardViewModel(CreateSetting());
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
            var vm = new DashboardViewModel(CreateSetting());
            vm.PositivePrompt = "test";
            Assert.False(vm.RunWorkflowCommand.CanExecute(null));
        }

        [Fact]
        public async Task RunWorkflowCommand_WithConfigAndEmptyPrompt_CannotExecute()
        {
            var setting = CreateSetting();
            setting.Data.ConfigPath = CreateConfigJson();
            var vm = new DashboardViewModel(setting);
            await vm.OnNavigatedToAsync();

            Assert.False(vm.RunWorkflowCommand.CanExecute(null));
        }

        [Fact]
        public async Task RunWorkflowCommand_WithConfigAndPrompt_CanExecute()
        {
            var setting = CreateSetting();
            setting.Data.ConfigPath = CreateConfigJson();
            var vm = new DashboardViewModel(setting);
            await vm.OnNavigatedToAsync();
            vm.PositivePrompt = "masterpiece";

            Assert.True(vm.RunWorkflowCommand.CanExecute(null));
        }

        // ── IsSuccess / IsError 初期値 ────────────────────────────────────────

        [Fact]
        public void IsSuccess_Initial_IsFalse()
        {
            var vm = new DashboardViewModel(CreateSetting());
            Assert.False(vm.IsSuccess);
        }

        [Fact]
        public void IsError_Initial_IsFalse()
        {
            var vm = new DashboardViewModel(CreateSetting());
            Assert.False(vm.IsError);
        }
    }
}
