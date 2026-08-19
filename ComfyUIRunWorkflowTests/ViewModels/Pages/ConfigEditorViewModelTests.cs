using System.IO;
using System.Runtime.ExceptionServices;
using ComfyUILibs.Common;
using ComfyUILibs.Models;
using ComfyUIRunWorkflow.Models;
using ComfyUIRunWorkflow.ViewModels.Pages;
using ComfyUIRunWorkflowTests.Fakes;
using Wpf.Ui.Controls;

namespace ComfyUIRunWorkflowTests.ViewModels.Pages
{
    public class ConfigEditorViewModelTests : IDisposable
    {
        private readonly string _tempDir;
        private readonly FakeSnackbarService _fakeSnackbar;
        private readonly FakeContentDialogService _fakeContentDialogService;

        public ConfigEditorViewModelTests()
        {
            _tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            Directory.CreateDirectory(_tempDir);
            _fakeSnackbar = new FakeSnackbarService();
            _fakeContentDialogService = new FakeContentDialogService();
        }

        public void Dispose() => Directory.Delete(_tempDir, recursive: true);

        /// <summary>SymbolIcon など WPF コントロールの生成を含む処理を STA スレッドで実行するヘルパー。</summary>
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

        private ConfigEditorViewModel CreateVm(Setting<AppConfig>? setting = null)
            => new ConfigEditorViewModel(setting ?? CreateSetting(), _fakeSnackbar, _fakeContentDialogService);

        private string CreateMultiWorkflowConfigJson(string? extraTopLevelJson = null)
        {
            var configPath = Path.Combine(_tempDir, "workflow_config.json");
            var extra = extraTopLevelJson == null ? "" : "," + extraTopLevelJson;
            var json = $$"""
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
                  }{{extra}}
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
        public void Constructor_IsConfigLoadedFalse_SaveCommandDisabled()
        {
            var vm = CreateVm();
            Assert.False(vm.IsConfigLoaded);
            Assert.False(vm.SaveCommand.CanExecute(null));
        }

        // ── OnNavigatedToAsync（読み込み） ────────────────────────────────────

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
        public async Task OnNavigatedToAsync_ValidConfig_LoadsWorkflowsAndSelectsFirst()
        {
            var setting = CreateSetting();
            setting.Data.ConfigPath = CreateMultiWorkflowConfigJson();
            var vm = CreateVm(setting);

            await vm.OnNavigatedToAsync();

            Assert.True(vm.IsConfigLoaded);
            Assert.Equal("http://127.0.0.1:8188", vm.ComfyUIUrl);
            Assert.Equal("sdxl", vm.DefaultWorkflow);
            Assert.Equal(2, vm.Workflows.Count);
            Assert.Contains("sdxl", vm.WorkflowNameList);
            Assert.Contains("anima", vm.WorkflowNameList);
            Assert.NotNull(vm.SelectedWorkflow);

            var sdxl = vm.Workflows.Single(w => w.Name == "sdxl");
            Assert.Equal(832, sdxl.DefaultWidth);
            var lora = Assert.Single(sdxl.Loras);
            Assert.Equal("my_lora", lora.Name);
        }

        [Fact]
        public async Task OnNavigatedToAsync_NoWd14TaggerSection_UsesBlankDefaults()
        {
            var setting = CreateSetting();
            setting.Data.ConfigPath = CreateMultiWorkflowConfigJson();
            var vm = CreateVm(setting);

            await vm.OnNavigatedToAsync();

            Assert.Equal("", vm.Wd14ModelName);
            Assert.Equal(0.35, vm.Wd14GeneralThreshold);
            Assert.Equal(0.85, vm.Wd14CharacterThreshold);
            Assert.Equal("", vm.PrependTagsText);
            Assert.Equal("", vm.ExcludeTagsText);
        }

        [Fact]
        public async Task OnNavigatedToAsync_WithWd14TaggerAndTags_LoadsValues()
        {
            var setting = CreateSetting();
            setting.Data.ConfigPath = CreateMultiWorkflowConfigJson(
                """
                "wd14_tagger": {"model_name": "wd-eva02-large-tagger-v3", "general_threshold": 0.4, "character_threshold": 0.9},
                "prepend_tags": ["chara_a", "chara_b"],
                "exclude_tags": ["rating:general"]
                """);
            var vm = CreateVm(setting);

            await vm.OnNavigatedToAsync();

            Assert.Equal("wd-eva02-large-tagger-v3", vm.Wd14ModelName);
            Assert.Equal(0.4, vm.Wd14GeneralThreshold);
            Assert.Equal(0.9, vm.Wd14CharacterThreshold);
            Assert.Equal("chara_a, chara_b", vm.PrependTagsText);
            Assert.Equal("rating:general", vm.ExcludeTagsText);
        }

        // ── ワークフロー追加 ───────────────────────────────────────────────────

        [Fact]
        public async Task AddWorkflowCommand_Execute_AddsUniqueNameAndSelectsIt()
        {
            var setting = CreateSetting();
            setting.Data.ConfigPath = CreateMultiWorkflowConfigJson();
            var vm = CreateVm(setting);
            await vm.OnNavigatedToAsync();

            vm.AddWorkflowCommand.Execute(null);

            var added = Assert.Single(vm.Workflows, w => w.Name.StartsWith("new_workflow"));
            Assert.Equal("new_workflow", added.Name);
            Assert.Same(added, vm.SelectedWorkflow);
            Assert.True(added.IsEditingName);
            Assert.Contains("new_workflow", vm.WorkflowNameList);
        }

        [Fact]
        public async Task AddWorkflowCommand_ExecuteTwice_GeneratesUniqueNames()
        {
            var setting = CreateSetting();
            setting.Data.ConfigPath = CreateMultiWorkflowConfigJson();
            var vm = CreateVm(setting);
            await vm.OnNavigatedToAsync();

            vm.AddWorkflowCommand.Execute(null);
            vm.AddWorkflowCommand.Execute(null);

            Assert.Contains(vm.Workflows, w => w.Name == "new_workflow");
            Assert.Contains(vm.Workflows, w => w.Name == "new_workflow_2");
        }

        // ── ワークフロー削除 ───────────────────────────────────────────────────

        [Fact]
        public async Task RemoveWorkflowAsync_DefaultWorkflow_ShowsErrorAndDoesNotRemove()
        {
            var setting = CreateSetting();
            setting.Data.ConfigPath = CreateMultiWorkflowConfigJson();
            var vm = CreateVm(setting);
            await vm.OnNavigatedToAsync();
            var sdxl = vm.Workflows.Single(w => w.Name == "sdxl");

            RunOnSta(() => vm.RemoveWorkflowCommand.ExecuteAsync(sdxl).Wait());

            Assert.Contains(vm.Workflows, w => w.Name == "sdxl");
            Assert.Single(_fakeSnackbar.Calls);
            Assert.Equal(ControlAppearance.Danger, _fakeSnackbar.Calls[0].Appearance);
        }

        [Fact]
        public async Task RemoveWorkflowAsync_ConfirmedNonDefault_RemovesItem()
        {
            var setting = CreateSetting();
            setting.Data.ConfigPath = CreateMultiWorkflowConfigJson();
            var vm = CreateVm(setting);
            await vm.OnNavigatedToAsync();
            var anima = vm.Workflows.Single(w => w.Name == "anima");
            vm.SelectedWorkflow = anima;
            _fakeContentDialogService.NextShowResult = ContentDialogResult.Primary;

            RunOnSta(() => vm.RemoveWorkflowCommand.ExecuteAsync(anima).Wait());

            Assert.DoesNotContain(vm.Workflows, w => w.Name == "anima");
            Assert.DoesNotContain("anima", vm.WorkflowNameList);
            Assert.Null(vm.SelectedWorkflow);
        }

        [Fact]
        public async Task RemoveWorkflowAsync_CancelledNonDefault_DoesNotRemove()
        {
            var setting = CreateSetting();
            setting.Data.ConfigPath = CreateMultiWorkflowConfigJson();
            var vm = CreateVm(setting);
            await vm.OnNavigatedToAsync();
            var anima = vm.Workflows.Single(w => w.Name == "anima");
            _fakeContentDialogService.NextShowResult = ContentDialogResult.None;

            RunOnSta(() => vm.RemoveWorkflowCommand.ExecuteAsync(anima).Wait());

            Assert.Contains(vm.Workflows, w => w.Name == "anima");
        }

        // ── ワークフロー名のインライン編集 ─────────────────────────────────────

        [Fact]
        public async Task BeginEditingWorkflowName_SetsIsEditingNameAndCapturesOriginal()
        {
            var setting = CreateSetting();
            setting.Data.ConfigPath = CreateMultiWorkflowConfigJson();
            var vm = CreateVm(setting);
            await vm.OnNavigatedToAsync();
            var sdxl = vm.Workflows.Single(w => w.Name == "sdxl");

            vm.BeginEditingWorkflowName(sdxl);

            Assert.True(sdxl.IsEditingName);
            Assert.Equal("sdxl", sdxl.NameBeforeEdit);
        }

        [Fact]
        public async Task FinishEditingWorkflowName_ValidRename_UpdatesNameAndList()
        {
            var setting = CreateSetting();
            setting.Data.ConfigPath = CreateMultiWorkflowConfigJson();
            var vm = CreateVm(setting);
            await vm.OnNavigatedToAsync();
            var sdxl = vm.Workflows.Single(w => w.Name == "sdxl");
            vm.BeginEditingWorkflowName(sdxl);
            sdxl.Name = "sdxl_renamed";

            vm.FinishEditingWorkflowName(sdxl);

            Assert.Equal("sdxl_renamed", sdxl.Name);
            Assert.False(sdxl.IsEditingName);
            Assert.Contains("sdxl_renamed", vm.WorkflowNameList);
            Assert.Equal("sdxl_renamed", vm.DefaultWorkflow);
        }

        [Fact]
        public async Task FinishEditingWorkflowName_EmptyName_RevertsAndShowsError()
        {
            var setting = CreateSetting();
            setting.Data.ConfigPath = CreateMultiWorkflowConfigJson();
            var vm = CreateVm(setting);
            await vm.OnNavigatedToAsync();
            var sdxl = vm.Workflows.Single(w => w.Name == "sdxl");
            vm.BeginEditingWorkflowName(sdxl);
            sdxl.Name = "   ";

            RunOnSta(() => vm.FinishEditingWorkflowName(sdxl));

            Assert.Equal("sdxl", sdxl.Name);
            Assert.Single(_fakeSnackbar.Calls);
        }

        [Fact]
        public async Task FinishEditingWorkflowName_DuplicateName_RevertsAndShowsError()
        {
            var setting = CreateSetting();
            setting.Data.ConfigPath = CreateMultiWorkflowConfigJson();
            var vm = CreateVm(setting);
            await vm.OnNavigatedToAsync();
            var anima = vm.Workflows.Single(w => w.Name == "anima");
            vm.BeginEditingWorkflowName(anima);
            anima.Name = "sdxl";

            RunOnSta(() => vm.FinishEditingWorkflowName(anima));

            Assert.Equal("anima", anima.Name);
            Assert.Single(_fakeSnackbar.Calls);
        }

        // ── 保存 ──────────────────────────────────────────────────────────────

        [Fact]
        public async Task Save_ValidConfig_WritesFileAndShowsSuccessSnackbar()
        {
            var setting = CreateSetting();
            var configPath = CreateMultiWorkflowConfigJson();
            setting.Data.ConfigPath = configPath;
            var vm = CreateVm(setting);
            await vm.OnNavigatedToAsync();
            vm.ComfyUIUrl = "http://localhost:9999";

            RunOnSta(() => vm.SaveCommand.Execute(null));

            var saved = JsonLoader.ReadJson<WorkflowConfig>(configPath);
            Assert.Equal("http://localhost:9999", saved.ComfyuiUrl);
            Assert.Single(_fakeSnackbar.Calls);
            // sdxl/anima はテスト実行ディレクトリの templates/ 配下に存在するため、警告なしの成功になる
            Assert.Equal(ControlAppearance.Success, _fakeSnackbar.Calls[0].Appearance);
        }

        [Fact]
        public async Task Save_WorkflowWithoutTemplatesFolder_ShowsCautionSnackbar()
        {
            var setting = CreateSetting();
            var configPath = CreateMultiWorkflowConfigJson();
            setting.Data.ConfigPath = configPath;
            var vm = CreateVm(setting);
            await vm.OnNavigatedToAsync();
            // "new_workflow" にはテスト実行ディレクトリの templates/ フォルダが存在しないため、警告付きの成功になるはず
            vm.AddWorkflowCommand.Execute(null);
            vm.DefaultWorkflow = "sdxl";

            RunOnSta(() => vm.SaveCommand.Execute(null));

            Assert.Single(_fakeSnackbar.Calls);
            Assert.Equal(ControlAppearance.Caution, _fakeSnackbar.Calls[0].Appearance);
        }

        [Fact]
        public async Task Save_BlankWd14ModelName_OmitsWd14TaggerSection()
        {
            var setting = CreateSetting();
            var configPath = CreateMultiWorkflowConfigJson(
                """
                "wd14_tagger": {"model_name": "wd-eva02-large-tagger-v3", "general_threshold": 0.4, "character_threshold": 0.9}
                """);
            setting.Data.ConfigPath = configPath;
            var vm = CreateVm(setting);
            await vm.OnNavigatedToAsync();
            vm.Wd14ModelName = "";

            RunOnSta(() => vm.SaveCommand.Execute(null));

            var saved = JsonLoader.ReadJson<WorkflowConfig>(configPath);
            Assert.Null(saved.Wd14Tagger);
        }

        [Fact]
        public async Task Save_TagsText_SplitsOnCommaAndTrims()
        {
            var setting = CreateSetting();
            var configPath = CreateMultiWorkflowConfigJson();
            setting.Data.ConfigPath = configPath;
            var vm = CreateVm(setting);
            await vm.OnNavigatedToAsync();
            vm.PrependTagsText = "chara_a,  chara_b ,chara_c";
            vm.ExcludeTagsText = "";

            RunOnSta(() => vm.SaveCommand.Execute(null));

            var saved = JsonLoader.ReadJson<WorkflowConfig>(configPath);
            Assert.Equal(new[] { "chara_a", "chara_b", "chara_c" }, saved.PrependTags);
            Assert.Null(saved.ExcludeTags);
        }

        [Fact]
        public async Task Save_EmptyDefaultWorkflow_ShowsValidationErrorAndDoesNotOverwriteFile()
        {
            var setting = CreateSetting();
            var configPath = CreateMultiWorkflowConfigJson();
            setting.Data.ConfigPath = configPath;
            var originalContent = File.ReadAllText(configPath);
            var vm = CreateVm(setting);
            await vm.OnNavigatedToAsync();
            vm.DefaultWorkflow = "";

            RunOnSta(() => vm.SaveCommand.Execute(null));

            Assert.Single(_fakeSnackbar.Calls);
            Assert.Equal(ControlAppearance.Danger, _fakeSnackbar.Calls[0].Appearance);
            Assert.Equal(originalContent, File.ReadAllText(configPath));
        }

        [Fact]
        public async Task Save_DuplicateLoraName_ShowsErrorAndDoesNotWrite()
        {
            var setting = CreateSetting();
            var configPath = CreateMultiWorkflowConfigJson();
            setting.Data.ConfigPath = configPath;
            var originalContent = File.ReadAllText(configPath);
            var vm = CreateVm(setting);
            await vm.OnNavigatedToAsync();
            var sdxl = vm.Workflows.Single(w => w.Name == "sdxl");
            sdxl.AddLoraCommand.Execute(null);
            sdxl.Loras[^1].Name = "my_lora";

            RunOnSta(() => vm.SaveCommand.Execute(null));

            Assert.Single(_fakeSnackbar.Calls);
            Assert.Equal(ControlAppearance.Danger, _fakeSnackbar.Calls[0].Appearance);
            Assert.Equal(originalContent, File.ReadAllText(configPath));
        }

        [Fact]
        public async Task Save_EmptyLoraName_ShowsErrorAndDoesNotWrite()
        {
            var setting = CreateSetting();
            var configPath = CreateMultiWorkflowConfigJson();
            setting.Data.ConfigPath = configPath;
            var originalContent = File.ReadAllText(configPath);
            var vm = CreateVm(setting);
            await vm.OnNavigatedToAsync();
            var sdxl = vm.Workflows.Single(w => w.Name == "sdxl");
            sdxl.AddLoraCommand.Execute(null);

            RunOnSta(() => vm.SaveCommand.Execute(null));

            Assert.Single(_fakeSnackbar.Calls);
            Assert.Equal(ControlAppearance.Danger, _fakeSnackbar.Calls[0].Appearance);
            Assert.Equal(originalContent, File.ReadAllText(configPath));
        }
    }
}
