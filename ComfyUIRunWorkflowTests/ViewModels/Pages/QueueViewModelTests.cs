using System.IO;
using System.Runtime.ExceptionServices;
using ComfyUILibs.Common;
using ComfyUIRunWorkflow.Models;
using ComfyUIRunWorkflow.ViewModels.Pages;
using ComfyUIRunWorkflowTests.Fakes;
using Wpf.Ui.Controls;

namespace ComfyUIRunWorkflowTests.ViewModels.Pages
{
    public class QueueViewModelTests : IDisposable
    {
        private readonly string _tempDir;
        private readonly FakeSnackbarService _fakeSnackbar;
        private readonly FakeContentDialogService _fakeContentDialogService;

        public QueueViewModelTests()
        {
            _tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            Directory.CreateDirectory(_tempDir);
            _fakeSnackbar = new FakeSnackbarService();
            _fakeContentDialogService = new FakeContentDialogService();
        }

        public void Dispose() => Directory.Delete(_tempDir, recursive: true);

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

        private Setting<AppConfig> CreateSetting()
            => new Setting<AppConfig>(Path.Combine(_tempDir, "setting.json"), onLoad: false);

        private Setting<QueueJobListData> CreateQueueJobsSetting()
            => new Setting<QueueJobListData>(Path.Combine(_tempDir, "queue_jobs.json"), onLoad: false);

        private QueueViewModel CreateVm(
            Setting<AppConfig>? setting = null,
            Setting<QueueJobListData>? queueJobsSetting = null)
            => new QueueViewModel(
                setting ?? CreateSetting(),
                queueJobsSetting ?? CreateQueueJobsSetting(),
                _fakeSnackbar,
                _fakeContentDialogService);

        private string CreateMultiWorkflowConfigJson()
        {
            var configPath = Path.Combine(_tempDir, "workflow_config.json");
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
                    },
                    "anima": {
                      "default_image_size": {"width": 896, "height": 1152},
                      "image_size": {
                        "vertical":   {"width": 896,  "height": 1152},
                        "horizontal": {"width": 1152, "height": 896},
                        "square":     {"width": 1024, "height": 1024}
                      },
                      "loras": {}
                    }
                  }
                }
                """;
            File.WriteAllText(configPath, json);
            return configPath;
        }

        // ── コンストラクター既定値 ─────────────────────────────────────────────

        [Fact]
        public void Constructor_Config_IsSet()
        {
            var setting = CreateSetting();
            var vm = CreateVm(setting);
            Assert.Same(setting, vm.Config);
        }

        [Fact]
        public void Constructor_JobsIsEmpty()
        {
            var vm = CreateVm();
            Assert.Empty(vm.Jobs);
        }

        [Fact]
        public void Constructor_IsRunningIsFalse()
        {
            var vm = CreateVm();
            Assert.False(vm.IsRunning);
            Assert.True(vm.CanEditJobs);
        }

        [Fact]
        public void Constructor_HasJobsIsFalse()
        {
            var vm = CreateVm();
            Assert.False(vm.HasJobs);
        }

        // ── OnNavigatedToAsync（config 読み込み） ────────────────────────────

        [Fact]
        public void OnNavigatedToAsync_EmptyConfigPath_IsConfigLoadedFalse()
        {
            var setting = CreateSetting();
            setting.Data.ConfigPath = "";
            var vm = CreateVm(setting);

            RunOnSta(() => vm.OnNavigatedToAsync().Wait());

            Assert.False(vm.IsConfigLoaded);
            Assert.Single(_fakeSnackbar.Calls);
            Assert.Equal(ControlAppearance.Danger, _fakeSnackbar.Calls[0].Appearance);
        }

        [Fact]
        public async Task OnNavigatedToAsync_ValidConfig_LoadsWorkflowNames()
        {
            var setting = CreateSetting();
            setting.Data.ConfigPath = CreateMultiWorkflowConfigJson();
            var vm = CreateVm(setting);

            await vm.OnNavigatedToAsync();

            Assert.True(vm.IsConfigLoaded);
            Assert.Contains("sdxl", vm.WorkflowNames);
            Assert.Contains("anima", vm.WorkflowNames);
        }

        [Fact]
        public void OnNavigatedToAsync_InvalidConfigPath_ShowsSnackbar()
        {
            var setting = CreateSetting();
            setting.Data.ConfigPath = Path.Combine(_tempDir, "nonexistent.json");
            var vm = CreateVm(setting);

            RunOnSta(() => vm.OnNavigatedToAsync().Wait());

            Assert.False(vm.IsConfigLoaded);
            Assert.Single(_fakeSnackbar.Calls);
        }

        [Fact]
        public async Task OnNavigatedToAsync_CalledAgain_PreservesExistingJobCustomSize()
        {
            var setting = CreateSetting();
            setting.Data.ConfigPath = CreateMultiWorkflowConfigJson();
            var vm = CreateVm(setting);
            await vm.OnNavigatedToAsync();

            vm.AddJobCommand.Execute(null);
            var job = vm.Jobs[0];
            job.SelectedSizeOption = "custom";
            job.CustomWidth = 999;
            job.CustomHeight = 777;

            // ページの再訪問（config の再読み込み）を模擬する
            await vm.OnNavigatedToAsync();

            Assert.True(job.IsCustomSize);
            Assert.Equal("custom", job.SelectedSizeOption);
            Assert.Equal(999, job.CustomWidth);
            Assert.Equal(777, job.CustomHeight);
        }

        // ── ジョブ追加・削除 ───────────────────────────────────────────────────

        [Fact]
        public async Task AddJobCommand_Execute_AddsJobAndSelectsIt()
        {
            var setting = CreateSetting();
            setting.Data.ConfigPath = CreateMultiWorkflowConfigJson();
            var vm = CreateVm(setting);
            await vm.OnNavigatedToAsync();

            vm.AddJobCommand.Execute(null);

            Assert.Single(vm.Jobs);
            Assert.Same(vm.Jobs[0], vm.SelectedJob);
            Assert.True(vm.HasJobs);
        }

        [Fact]
        public async Task AddJobCommand_Execute_SelectsFirstWorkflowName()
        {
            var setting = CreateSetting();
            setting.Data.ConfigPath = CreateMultiWorkflowConfigJson();
            var vm = CreateVm(setting);
            await vm.OnNavigatedToAsync();

            vm.AddJobCommand.Execute(null);

            Assert.Equal(vm.WorkflowNames[0], vm.Jobs[0].WorkflowName);
        }

        [Fact]
        public async Task AddJobCommand_Execute_PersistsToQueueJobsFile()
        {
            var setting = CreateSetting();
            setting.Data.ConfigPath = CreateMultiWorkflowConfigJson();
            var queueJobsSetting = CreateQueueJobsSetting();
            var vm = CreateVm(setting, queueJobsSetting);
            await vm.OnNavigatedToAsync();

            vm.AddJobCommand.Execute(null);

            Assert.Single(queueJobsSetting.Data.Jobs);
        }

        [Fact]
        public void RemoveJobCommand_Execute_RemovesJob()
        {
            var vm = CreateVm();
            vm.AddJobCommand.Execute(null);
            var job = vm.Jobs[0];

            vm.RemoveJobCommand.Execute(job);

            Assert.Empty(vm.Jobs);
            Assert.False(vm.HasJobs);
        }

        [Fact]
        public void RemoveJobCommand_RemovingSelectedJob_ClearsSelection()
        {
            var vm = CreateVm();
            vm.AddJobCommand.Execute(null);
            var job = vm.Jobs[0];

            vm.RemoveJobCommand.Execute(job);

            Assert.Null(vm.SelectedJob);
        }

        // ── RunAllCommand / CancelQueueCommand の CanExecute ────────────────

        [Fact]
        public void RunAllCommand_NoJobs_CannotExecute()
        {
            var vm = CreateVm();
            Assert.False(vm.RunAllCommand.CanExecute(null));
        }

        [Fact]
        public void RunAllCommand_WithJobs_CanExecute()
        {
            var vm = CreateVm();
            vm.AddJobCommand.Execute(null);
            Assert.True(vm.RunAllCommand.CanExecute(null));
        }

        [Fact]
        public void RunAllCommand_WhileRunning_CannotExecute()
        {
            var vm = CreateVm();
            vm.AddJobCommand.Execute(null);
            vm.IsRunning = true;
            Assert.False(vm.RunAllCommand.CanExecute(null));
        }

        [Fact]
        public void CancelQueueCommand_NotRunning_CannotExecute()
        {
            var vm = CreateVm();
            Assert.False(vm.CancelQueueCommand.CanExecute(null));
        }

        [Fact]
        public void CancelQueueCommand_WhileRunning_CanExecute()
        {
            var vm = CreateVm();
            vm.IsRunning = true;
            Assert.True(vm.CancelQueueCommand.CanExecute(null));
        }

        [Fact]
        public void IsRunning_True_CanEditJobsIsFalse()
        {
            var vm = CreateVm();
            vm.IsRunning = true;
            Assert.False(vm.CanEditJobs);
        }

        // ── 永続化データからの復元 ─────────────────────────────────────────────

        [Fact]
        public async Task OnNavigatedToAsync_WithPersistedJobs_RestoresJobs()
        {
            var setting = CreateSetting();
            setting.Data.ConfigPath = CreateMultiWorkflowConfigJson();
            var queueJobsSetting = CreateQueueJobsSetting();
            queueJobsSetting.Data.Jobs.Add(new QueueJobData
            {
                WorkflowName = "sdxl",
                PositivePrompt = "1girl",
                BatchCount = 2,
            });
            var vm = CreateVm(setting, queueJobsSetting);

            await vm.OnNavigatedToAsync();

            Assert.Single(vm.Jobs);
            Assert.Equal("sdxl", vm.Jobs[0].WorkflowName);
            Assert.Equal("1girl", vm.Jobs[0].PositivePrompt);
            Assert.Equal(2, vm.Jobs[0].BatchCount);
            Assert.Equal(QueueJobStatus.Pending, vm.Jobs[0].Status);
        }

        [Fact]
        public async Task OnNavigatedToAsync_NoPersistedJobsFile_StartsEmpty()
        {
            var setting = CreateSetting();
            setting.Data.ConfigPath = CreateMultiWorkflowConfigJson();
            var queueJobsSetting = CreateQueueJobsSetting();
            var vm = CreateVm(setting, queueJobsSetting);

            await vm.OnNavigatedToAsync();

            Assert.Empty(vm.Jobs);
            Assert.False(vm.HasJobs);
        }

        [Fact]
        public async Task OnNavigatedToAsync_CalledTwice_DoesNotDuplicateRestoredJobs()
        {
            var setting = CreateSetting();
            setting.Data.ConfigPath = CreateMultiWorkflowConfigJson();
            var queueJobsSetting = CreateQueueJobsSetting();
            queueJobsSetting.Data.Jobs.Add(new QueueJobData { WorkflowName = "sdxl" });
            var vm = CreateVm(setting, queueJobsSetting);

            await vm.OnNavigatedToAsync();
            await vm.OnNavigatedToAsync();

            Assert.Single(vm.Jobs);
        }

        // ── OnNavigatedFromAsync（永続化） ────────────────────────────────────

        [Fact]
        public async Task OnNavigatedFromAsync_PersistsCurrentJobs()
        {
            var queueJobsSetting = CreateQueueJobsSetting();
            var vm = CreateVm(queueJobsSetting: queueJobsSetting);
            vm.AddJobCommand.Execute(null);
            vm.Jobs[0].PositivePrompt = "edited prompt";

            await vm.OnNavigatedFromAsync();

            Assert.Single(queueJobsSetting.Data.Jobs);
            Assert.Equal("edited prompt", queueJobsSetting.Data.Jobs[0].PositivePrompt);
        }

        // ── OpenJobDetailCommand ─────────────────────────────────────────────

        [Fact]
        public async Task OpenJobDetailCommand_NoLastResult_DoesNotThrow()
        {
            var vm = CreateVm();
            vm.AddJobCommand.Execute(null);
            var job = vm.Jobs[0];

            await vm.OpenJobDetailCommand.ExecuteAsync(job);
        }

        // ImportJobListCommand はファイルダイアログを直接開くため、ExportJobCommand/ImportJobCommand と同様に
        // 単体テスト対象外とする（フェーズ13の実装方針を踏襲）。
    }
}
