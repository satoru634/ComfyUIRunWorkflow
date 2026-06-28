using System.IO;
using ComfyUILibs.Common;
using ComfyUIRunWorkflow.Models;
using ComfyUIRunWorkflow.ViewModels.Pages;

namespace ComfyUIRunWorkflowTests.ViewModels.Pages
{
    public class DataViewModelTests : IDisposable
    {
        private readonly string _tempDir;

        public DataViewModelTests()
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

            var vm = new DataViewModel(setting);

            Assert.Same(setting, vm.Config);
        }

        [Fact]
        public void Colors_InitialValue_IsEmpty()
        {
            var vm = new DataViewModel(CreateSetting());

            Assert.Empty(vm.Colors);
        }

        // ── OnNavigatedToAsync ────────────────────────────────────────────────

        [Fact]
        public async Task OnNavigatedToAsync_FirstCall_PopulatesColors()
        {
            var vm = new DataViewModel(CreateSetting());

            await vm.OnNavigatedToAsync();

            Assert.NotEmpty(vm.Colors);
        }

        [Fact]
        public async Task OnNavigatedToAsync_FirstCall_GeneratesExpectedCount()
        {
            var vm = new DataViewModel(CreateSetting());

            await vm.OnNavigatedToAsync();

            Assert.Equal(8192, vm.Colors.Count());
        }

        [Fact]
        public async Task OnNavigatedToAsync_SecondCall_ColorsCountUnchanged()
        {
            var vm = new DataViewModel(CreateSetting());
            await vm.OnNavigatedToAsync();
            var countAfterFirst = vm.Colors.Count();

            await vm.OnNavigatedToAsync();

            Assert.Equal(countAfterFirst, vm.Colors.Count());
        }

        // ── OnNavigatedFromAsync ──────────────────────────────────────────────

        [Fact]
        public async Task OnNavigatedFromAsync_ReturnsCompletedTask()
        {
            var vm = new DataViewModel(CreateSetting());

            var task = vm.OnNavigatedFromAsync();
            await task;

            Assert.True(task.IsCompletedSuccessfully);
        }
    }
}
