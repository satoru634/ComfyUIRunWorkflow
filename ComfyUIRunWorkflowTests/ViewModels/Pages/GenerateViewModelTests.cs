using System.IO;
using System.Runtime.ExceptionServices;
using ComfyUILibs.Common;
using ComfyUIRunWorkflow.Models;
using ComfyUIRunWorkflow.ViewModels.Pages;
using ComfyUIRunWorkflowTests.Fakes;
using Wpf.Ui.Controls;

namespace ComfyUIRunWorkflowTests.ViewModels.Pages
{
    public class GenerateViewModelTests : IDisposable
    {
        private readonly string _tempDir;
        private readonly string _baseDir;
        private readonly FakeSnackbarService _fakeSnackbar;

        public GenerateViewModelTests()
        {
            _tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            _baseDir = Path.Combine(_tempDir, "base_prompts");
            Directory.CreateDirectory(_baseDir);
            _fakeSnackbar = new FakeSnackbarService();
        }

        public void Dispose() => Directory.Delete(_tempDir, recursive: true);

        /// <summary>
        /// GenerateCommand は失敗・成功いずれの場合も SymbolIcon など WPF コントロールの生成を伴うため、
        /// STA スレッドで実行するヘルパー（QueueViewModelTests と同様のパターン）。
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

        private Setting<AppConfig> CreateSetting()
            => new Setting<AppConfig>(Path.Combine(_tempDir, "setting.json"), onLoad: false);

        private GenerateViewModel CreateVm(Setting<AppConfig>? setting = null)
            => new GenerateViewModel(setting ?? CreateSetting(), _fakeSnackbar);

        private string WriteBasePrompt(string fileName, string positive, string negative)
        {
            var path = Path.Combine(_baseDir, fileName);
            JsonLoader.WriteJson(path, new ComfyUILibs.Models.PromptPair { Positive = positive, Negative = negative });
            return path;
        }

        private string WriteReplacements(Dictionary<string, string> replacements)
        {
            var path = Path.Combine(_tempDir, "replacements.json");
            JsonLoader.WriteJson(path, replacements);
            return path;
        }

        private string WriteTemplate(QueueJobData template)
        {
            var path = Path.Combine(_tempDir, "template.json");
            JsonLoader.WriteJson(path, template);
            return path;
        }

        // ── コンストラクター ─────────────────────────────────────────────────────

        [Fact]
        public void Constructor_Config_IsSet()
        {
            var setting = CreateSetting();
            var vm = CreateVm(setting);
            Assert.Same(setting, vm.Config);
        }

        // ── GenerateCommand の CanExecute ────────────────────────────────────────

        [Fact]
        public void GenerateCommand_AllPathsEmpty_CannotExecute()
        {
            var vm = CreateVm();
            Assert.False(vm.GenerateCommand.CanExecute(null));
        }

        [Fact]
        public void GenerateCommand_OnlySomePathsSet_CannotExecute()
        {
            var setting = CreateSetting();
            setting.Data.GenerateBasePromptDirectory = _baseDir;
            setting.Data.GenerateReplacementListPath = "replacements.json";
            var vm = CreateVm(setting);

            Assert.False(vm.GenerateCommand.CanExecute(null));
        }

        [Fact]
        public void GenerateCommand_AllPathsSet_CanExecute()
        {
            var setting = CreateSetting();
            setting.Data.GenerateBasePromptDirectory = _baseDir;
            setting.Data.GenerateReplacementListPath = "replacements.json";
            setting.Data.GenerateJobTemplatePath = "template.json";
            setting.Data.GenerateOutputPath = "output.json";
            var vm = CreateVm(setting);

            Assert.True(vm.GenerateCommand.CanExecute(null));
        }

        // ── GenerateCommand の実行 ───────────────────────────────────────────────

        [Fact]
        public void GenerateCommand_Execute_WritesOutputFile()
        {
            WriteBasePrompt("a.json", "1girl, <CHARACTER>", "bad");
            var replacementsPath = WriteReplacements(new() { ["<CHARACTER>"] = "alice" });
            var templatePath = WriteTemplate(new QueueJobData { WorkflowName = "sdxl" });
            var outputPath = Path.Combine(_tempDir, "output.json");

            var setting = CreateSetting();
            setting.Data.GenerateBasePromptDirectory = _baseDir;
            setting.Data.GenerateReplacementListPath = replacementsPath;
            setting.Data.GenerateJobTemplatePath = templatePath;
            setting.Data.GenerateOutputPath = outputPath;
            var vm = CreateVm(setting);

            RunOnSta(() => vm.GenerateCommand.Execute(null));

            Assert.True(File.Exists(outputPath));
            var listData = JsonLoader.ReadJson<QueueJobListData>(outputPath);
            Assert.Single(listData.Jobs);
            Assert.Equal("1girl, alice", listData.Jobs[0].PositivePrompt);
        }

        [Fact]
        public void GenerateCommand_Execute_Success_ShowsSuccessSnackbar()
        {
            WriteBasePrompt("a.json", "1girl", "bad");
            var replacementsPath = WriteReplacements(new());
            var templatePath = WriteTemplate(new QueueJobData { WorkflowName = "sdxl" });
            var outputPath = Path.Combine(_tempDir, "output.json");

            var setting = CreateSetting();
            setting.Data.GenerateBasePromptDirectory = _baseDir;
            setting.Data.GenerateReplacementListPath = replacementsPath;
            setting.Data.GenerateJobTemplatePath = templatePath;
            setting.Data.GenerateOutputPath = outputPath;
            var vm = CreateVm(setting);

            RunOnSta(() => vm.GenerateCommand.Execute(null));

            Assert.Single(_fakeSnackbar.Calls);
            Assert.Equal(ControlAppearance.Success, _fakeSnackbar.Calls[0].Appearance);
        }

        [Fact]
        public void GenerateCommand_Execute_UndefinedKeyword_ShowsErrorSnackbarAndDoesNotWriteFile()
        {
            WriteBasePrompt("a.json", "1girl, <MISSING>", "bad");
            var replacementsPath = WriteReplacements(new());
            var templatePath = WriteTemplate(new QueueJobData { WorkflowName = "sdxl" });
            var outputPath = Path.Combine(_tempDir, "output.json");

            var setting = CreateSetting();
            setting.Data.GenerateBasePromptDirectory = _baseDir;
            setting.Data.GenerateReplacementListPath = replacementsPath;
            setting.Data.GenerateJobTemplatePath = templatePath;
            setting.Data.GenerateOutputPath = outputPath;
            var vm = CreateVm(setting);

            RunOnSta(() => vm.GenerateCommand.Execute(null));

            Assert.False(File.Exists(outputPath));
            Assert.Single(_fakeSnackbar.Calls);
            Assert.Equal(ControlAppearance.Danger, _fakeSnackbar.Calls[0].Appearance);
        }

        [Fact]
        public void GenerateCommand_Execute_EmptyBaseDirectory_ShowsErrorSnackbar()
        {
            var replacementsPath = WriteReplacements(new());
            var templatePath = WriteTemplate(new QueueJobData { WorkflowName = "sdxl" });
            var outputPath = Path.Combine(_tempDir, "output.json");

            var setting = CreateSetting();
            setting.Data.GenerateBasePromptDirectory = _baseDir;
            setting.Data.GenerateReplacementListPath = replacementsPath;
            setting.Data.GenerateJobTemplatePath = templatePath;
            setting.Data.GenerateOutputPath = outputPath;
            var vm = CreateVm(setting);

            RunOnSta(() => vm.GenerateCommand.Execute(null));

            Assert.False(File.Exists(outputPath));
            Assert.Single(_fakeSnackbar.Calls);
            Assert.Equal(ControlAppearance.Danger, _fakeSnackbar.Calls[0].Appearance);
        }

        // ── Log ──────────────────────────────────────────────────────────────────

        [Fact]
        public void GenerateCommand_Execute_Success_LogContainsGeneratedFileAndOutputPath()
        {
            WriteBasePrompt("a.json", "1girl", "bad");
            var replacementsPath = WriteReplacements(new());
            var templatePath = WriteTemplate(new QueueJobData { WorkflowName = "sdxl" });
            var outputPath = Path.Combine(_tempDir, "output.json");

            var setting = CreateSetting();
            setting.Data.GenerateBasePromptDirectory = _baseDir;
            setting.Data.GenerateReplacementListPath = replacementsPath;
            setting.Data.GenerateJobTemplatePath = templatePath;
            setting.Data.GenerateOutputPath = outputPath;
            var vm = CreateVm(setting);

            RunOnSta(() => vm.GenerateCommand.Execute(null));

            Assert.Contains("a.json", vm.Log);
            Assert.Contains(outputPath, vm.Log);
        }

        [Fact]
        public void GenerateCommand_Execute_Error_LogContainsErrorMessage()
        {
            WriteBasePrompt("a.json", "1girl, <MISSING>", "bad");
            var replacementsPath = WriteReplacements(new());
            var templatePath = WriteTemplate(new QueueJobData { WorkflowName = "sdxl" });
            var outputPath = Path.Combine(_tempDir, "output.json");

            var setting = CreateSetting();
            setting.Data.GenerateBasePromptDirectory = _baseDir;
            setting.Data.GenerateReplacementListPath = replacementsPath;
            setting.Data.GenerateJobTemplatePath = templatePath;
            setting.Data.GenerateOutputPath = outputPath;
            var vm = CreateVm(setting);

            RunOnSta(() => vm.GenerateCommand.Execute(null));

            Assert.Contains("<MISSING>", vm.Log);
        }

        [Fact]
        public void GenerateCommand_Execute_Twice_LogIsResetOnEachRun()
        {
            WriteBasePrompt("a.json", "1girl", "bad");
            var replacementsPath = WriteReplacements(new());
            var templatePath = WriteTemplate(new QueueJobData { WorkflowName = "sdxl" });
            var outputPath = Path.Combine(_tempDir, "output.json");

            var setting = CreateSetting();
            setting.Data.GenerateBasePromptDirectory = _baseDir;
            setting.Data.GenerateReplacementListPath = replacementsPath;
            setting.Data.GenerateJobTemplatePath = templatePath;
            setting.Data.GenerateOutputPath = outputPath;
            var vm = CreateVm(setting);

            RunOnSta(() => vm.GenerateCommand.Execute(null));
            var firstRunLineCount = vm.Log.Split(Environment.NewLine).Length;
            RunOnSta(() => vm.GenerateCommand.Execute(null));
            var secondRunLineCount = vm.Log.Split(Environment.NewLine).Length;

            Assert.Equal(firstRunLineCount, secondRunLineCount);
        }

        // ── OnNavigatedFromAsync ─────────────────────────────────────────────────

        [Fact]
        public async Task OnNavigatedFromAsync_SavesConfig()
        {
            var settingPath = Path.Combine(_tempDir, "setting_saved.json");
            var setting = new Setting<AppConfig>(settingPath, onLoad: false);
            setting.Data.GenerateBasePromptDirectory = _baseDir;
            var vm = CreateVm(setting);

            await vm.OnNavigatedFromAsync();

            Assert.True(File.Exists(settingPath));
        }
    }
}
