using System.IO;
using System.Text.Json;
using ComfyUILibs.Common;
using ComfyUILibs.Models;
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

        private void WriteResultFile(string folder, WorkflowResult result, string filename)
        {
            var path = Path.Combine(folder, filename);
            var json = JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(path, json);
        }

        // ── コンストラクター ───────────────────────────────────────────────────

        [Fact]
        public void Constructor_Config_IsSet()
        {
            var setting = CreateSetting();
            var vm = new DataViewModel(setting);
            Assert.Same(setting, vm.Config);
        }

        [Fact]
        public void Constructor_Results_IsEmpty()
        {
            var vm = new DataViewModel(CreateSetting());
            Assert.Empty(vm.Results);
        }

        [Fact]
        public void Constructor_IsLoading_IsFalse()
        {
            var vm = new DataViewModel(CreateSetting());
            Assert.False(vm.IsLoading);
        }

        // ── OnNavigatedToAsync ────────────────────────────────────────────────

        [Fact]
        public async Task OnNavigatedToAsync_EmptyResultsFolder_ShowsGuideMessage()
        {
            var setting = CreateSetting();
            setting.Data.ResultsFolder = "";
            var vm = new DataViewModel(setting);

            await vm.OnNavigatedToAsync();

            Assert.NotEmpty(vm.StatusMessage);
            Assert.Empty(vm.Results);
        }

        [Fact]
        public async Task OnNavigatedToAsync_NonExistentFolder_ShowsErrorMessage()
        {
            var setting = CreateSetting();
            setting.Data.ResultsFolder = Path.Combine(_tempDir, "nonexistent");
            var vm = new DataViewModel(setting);

            await vm.OnNavigatedToAsync();

            Assert.NotEmpty(vm.StatusMessage);
            Assert.Empty(vm.Results);
        }

        [Fact]
        public async Task OnNavigatedToAsync_EmptyFolder_ShowsNoResultsMessage()
        {
            var folder = Path.Combine(_tempDir, "results");
            Directory.CreateDirectory(folder);
            var setting = CreateSetting();
            setting.Data.ResultsFolder = folder;
            var vm = new DataViewModel(setting);

            await vm.OnNavigatedToAsync();

            Assert.NotEmpty(vm.StatusMessage);
            Assert.Empty(vm.Results);
        }

        [Fact]
        public async Task OnNavigatedToAsync_WithResultFiles_LoadsResults()
        {
            var folder = Path.Combine(_tempDir, "results");
            Directory.CreateDirectory(folder);
            WriteResultFile(folder, new WorkflowResult { Status = "success", Timestamp = "2026-06-29T12:00:00" }, "result_20260629_120000.json");

            var setting = CreateSetting();
            setting.Data.ResultsFolder = folder;
            var vm = new DataViewModel(setting);

            await vm.OnNavigatedToAsync();

            Assert.Single(vm.Results);
            Assert.Equal("success", vm.Results[0].Status);
        }

        [Fact]
        public async Task OnNavigatedToAsync_WithMultipleResultFiles_LoadsAll()
        {
            var folder = Path.Combine(_tempDir, "results");
            Directory.CreateDirectory(folder);
            WriteResultFile(folder, new WorkflowResult { Status = "success" }, "result_20260629_120000.json");
            WriteResultFile(folder, new WorkflowResult { Status = "error", Error = "接続失敗" }, "result_20260629_110000.json");

            var setting = CreateSetting();
            setting.Data.ResultsFolder = folder;
            var vm = new DataViewModel(setting);

            await vm.OnNavigatedToAsync();

            Assert.Equal(2, vm.Results.Count);
        }

        [Fact]
        public async Task OnNavigatedToAsync_WithInvalidJson_SkipsInvalidFiles()
        {
            var folder = Path.Combine(_tempDir, "results");
            Directory.CreateDirectory(folder);
            WriteResultFile(folder, new WorkflowResult { Status = "success" }, "result_20260629_120000.json");
            File.WriteAllText(Path.Combine(folder, "result_20260629_110000.json"), "invalid json {{{");

            var setting = CreateSetting();
            setting.Data.ResultsFolder = folder;
            var vm = new DataViewModel(setting);

            await vm.OnNavigatedToAsync();

            Assert.Single(vm.Results);
        }

        [Fact]
        public async Task OnNavigatedToAsync_IgnoresNonResultFiles()
        {
            var folder = Path.Combine(_tempDir, "results");
            Directory.CreateDirectory(folder);
            WriteResultFile(folder, new WorkflowResult { Status = "success" }, "result_20260629_120000.json");
            File.WriteAllText(Path.Combine(folder, "other_file.json"), "{}");

            var setting = CreateSetting();
            setting.Data.ResultsFolder = folder;
            var vm = new DataViewModel(setting);

            await vm.OnNavigatedToAsync();

            Assert.Single(vm.Results);
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

        // ── RefreshAsyncCommand ────────────────────────────────────────────────

        [Fact]
        public async Task RefreshAsyncCommand_AfterAddingFile_ReloadsResults()
        {
            var folder = Path.Combine(_tempDir, "results");
            Directory.CreateDirectory(folder);

            var setting = CreateSetting();
            setting.Data.ResultsFolder = folder;
            var vm = new DataViewModel(setting);
            await vm.OnNavigatedToAsync();

            WriteResultFile(folder, new WorkflowResult { Status = "success" }, "result_20260629_120000.json");
            await vm.RefreshCommand.ExecuteAsync(null);

            Assert.Single(vm.Results);
        }
    }
}
