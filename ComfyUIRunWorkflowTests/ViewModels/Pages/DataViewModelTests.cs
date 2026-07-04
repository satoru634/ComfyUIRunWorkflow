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

        private void WriteTagResultFile(string folder, TagResult result, string filename)
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

        [Fact]
        public void Constructor_TagResults_IsEmpty()
        {
            var vm = new DataViewModel(CreateSetting());
            Assert.Empty(vm.TagResults);
        }

        [Fact]
        public void Constructor_IsTagHistorySelected_IsFalse()
        {
            var vm = new DataViewModel(CreateSetting());
            Assert.False(vm.IsTagHistorySelected);
        }

        // ── タブ切り替え ──────────────────────────────────────────────────────

        [Fact]
        public void ShowTagHistoryTabCommand_Execute_SetsIsTagHistorySelectedTrue()
        {
            var vm = new DataViewModel(CreateSetting());
            vm.ShowTagHistoryTabCommand.Execute(null);
            Assert.True(vm.IsTagHistorySelected);
        }

        [Fact]
        public void ShowResultsTabCommand_Execute_SetsIsTagHistorySelectedFalse()
        {
            var vm = new DataViewModel(CreateSetting());
            vm.ShowTagHistoryTabCommand.Execute(null);
            vm.ShowResultsTabCommand.Execute(null);
            Assert.False(vm.IsTagHistorySelected);
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
            Assert.Equal("success", vm.Results[0].Result.Status);
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

        // ── タグ付け履歴の読み込み ────────────────────────────────────────────

        [Fact]
        public async Task OnNavigatedToAsync_WithTagResultFiles_LoadsTagResults()
        {
            var folder = Path.Combine(_tempDir, "results");
            Directory.CreateDirectory(folder);
            WriteTagResultFile(
                folder,
                new TagResult { Status = "success", Timestamp = "2026-07-04T12:00:00", InputFilename = "photo.jpg", Tags = "1girl, solo" },
                "tag_result_20260704_120000.json");

            var setting = CreateSetting();
            setting.Data.ResultsFolder = folder;
            var vm = new DataViewModel(setting);

            await vm.OnNavigatedToAsync();

            Assert.Single(vm.TagResults);
            Assert.Equal("1girl, solo", vm.TagResults[0].Tags);
        }

        [Fact]
        public async Task OnNavigatedToAsync_TagResultFilesDoNotAffectResults()
        {
            var folder = Path.Combine(_tempDir, "results");
            Directory.CreateDirectory(folder);
            WriteTagResultFile(folder, new TagResult { Status = "success" }, "tag_result_20260704_120000.json");

            var setting = CreateSetting();
            setting.Data.ResultsFolder = folder;
            var vm = new DataViewModel(setting);

            await vm.OnNavigatedToAsync();

            Assert.Empty(vm.Results);
            Assert.Single(vm.TagResults);
        }

        [Fact]
        public async Task OnNavigatedToAsync_EmptyFolder_ShowsTagStatusMessage()
        {
            var folder = Path.Combine(_tempDir, "results");
            Directory.CreateDirectory(folder);
            var setting = CreateSetting();
            setting.Data.ResultsFolder = folder;
            var vm = new DataViewModel(setting);

            await vm.OnNavigatedToAsync();

            Assert.NotEmpty(vm.TagStatusMessage);
            Assert.Empty(vm.TagResults);
        }

        [Fact]
        public async Task OnNavigatedToAsync_WithInvalidTagResultJson_SkipsInvalidFiles()
        {
            var folder = Path.Combine(_tempDir, "results");
            Directory.CreateDirectory(folder);
            WriteTagResultFile(folder, new TagResult { Status = "success" }, "tag_result_20260704_120000.json");
            File.WriteAllText(Path.Combine(folder, "tag_result_20260704_110000.json"), "invalid json {{{");

            var setting = CreateSetting();
            setting.Data.ResultsFolder = folder;
            var vm = new DataViewModel(setting);

            await vm.OnNavigatedToAsync();

            Assert.Single(vm.TagResults);
        }

        [Fact]
        public async Task RefreshAsyncCommand_AfterAddingTagResultFile_ReloadsTagResults()
        {
            var folder = Path.Combine(_tempDir, "results");
            Directory.CreateDirectory(folder);

            var setting = CreateSetting();
            setting.Data.ResultsFolder = folder;
            var vm = new DataViewModel(setting);
            await vm.OnNavigatedToAsync();

            WriteTagResultFile(folder, new TagResult { Status = "success" }, "tag_result_20260704_120000.json");
            await vm.RefreshCommand.ExecuteAsync(null);

            Assert.Single(vm.TagResults);
        }

        // ── CopyTagsCommand ──────────────────────────────────────────────────

        [Fact]
        public void CopyTagsCommand_WithEmptyTags_DoesNotThrow()
        {
            var vm = new DataViewModel(CreateSetting());
            var result = new TagResult { Status = "error", Error = "失敗" };

            var ex = Record.Exception(() => vm.CopyTagsCommand.Execute(result));

            Assert.Null(ex);
        }
    }
}
