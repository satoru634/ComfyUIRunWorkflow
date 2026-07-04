using System.IO;
using System.Runtime.ExceptionServices;
using ComfyUILibs.Common;
using ComfyUIRunWorkflow.Models;
using ComfyUIRunWorkflow.ViewModels.Pages;
using ComfyUIRunWorkflowTests.Fakes;
using Wpf.Ui.Controls;

namespace ComfyUIRunWorkflowTests.ViewModels.Pages
{
    public class TaggerViewModelTests : IDisposable
    {
        private readonly string _tempDir;
        private readonly FakeSnackbarService _fakeSnackbar;

        public TaggerViewModelTests()
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

        private TaggerViewModel CreateVm(Setting<AppConfig>? setting = null)
            => new TaggerViewModel(setting ?? CreateSetting(), _fakeSnackbar);

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

        private string CreateTaggerConfigJson()
        {
            var configPath = Path.Combine(_tempDir, "workflow_config.json");
            var json = """
                {
                  "comfyui_url": "http://127.0.0.1:8188",
                  "wd14_tagger": {
                    "model_name": "wd-eva02-large-tagger-v3",
                    "general_threshold": 0.35,
                    "character_threshold": 0.85
                  }
                }
                """;
            File.WriteAllText(configPath, json);
            return configPath;
        }

        private string CreateConfigJsonWithoutTaggerSection()
        {
            var configPath = Path.Combine(_tempDir, "workflow_config.json");
            var json = """
                {
                  "comfyui_url": "http://127.0.0.1:8188"
                }
                """;
            File.WriteAllText(configPath, json);
            return configPath;
        }

        // 1x1 の透明 PNG（BitmapImage が正常にデコードできる最小の有効な画像データ）
        private const string ValidPngBase64 =
            "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mNk+A8AAQUBAScY42YAAAAASUVORK5CYII=";

        private string CreateDummyImageFile(string extension = ".png")
        {
            var path = Path.Combine(_tempDir, $"input{extension}");
            File.WriteAllBytes(path, Convert.FromBase64String(ValidPngBase64));
            return path;
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
        public void Constructor_IsConfigLoaded_IsFalse()
        {
            var vm = CreateVm();
            Assert.False(vm.IsConfigLoaded);
        }

        [Fact]
        public void Constructor_SelectedImagePath_IsNull()
        {
            var vm = CreateVm();
            Assert.Null(vm.SelectedImagePath);
        }

        [Fact]
        public void Constructor_IsRunning_IsFalse()
        {
            var vm = CreateVm();
            Assert.False(vm.IsRunning);
        }

        [Fact]
        public void Constructor_ResultTags_IsEmpty()
        {
            var vm = CreateVm();
            Assert.Equal("", vm.ResultTags);
        }

        [Fact]
        public void Constructor_HasResult_IsFalse()
        {
            var vm = CreateVm();
            Assert.False(vm.HasResult);
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
        public void OnNavigatedToAsync_ConfigWithoutTaggerSection_IsConfigLoadedFalse()
        {
            var setting = CreateSetting();
            setting.Data.ConfigPath = CreateConfigJsonWithoutTaggerSection();
            var vm = CreateVm(setting);

            RunOnSta(() => vm.OnNavigatedToAsync().Wait());

            Assert.False(vm.IsConfigLoaded);
            Assert.Single(_fakeSnackbar.Calls);
        }

        [Fact]
        public void OnNavigatedToAsync_ValidTaggerConfig_IsConfigLoadedTrue()
        {
            var setting = CreateSetting();
            setting.Data.ConfigPath = CreateTaggerConfigJson();
            var vm = CreateVm(setting);

            RunOnSta(() => vm.OnNavigatedToAsync().Wait());

            Assert.True(vm.IsConfigLoaded);
            Assert.Empty(_fakeSnackbar.Calls);
        }

        // ── SetSelectedImage ──────────────────────────────────────────────────

        [Fact]
        public void SetSelectedImage_ImageFile_SetsSelectedImagePath()
        {
            var vm = CreateVm();
            var path = CreateDummyImageFile(".png");

            vm.SetSelectedImage(path);

            Assert.Equal(path, vm.SelectedImagePath);
        }

        [Fact]
        public void SetSelectedImage_NonImageFile_DoesNotSetSelectedImagePath()
        {
            var vm = CreateVm();
            var path = Path.Combine(_tempDir, "input.txt");
            File.WriteAllText(path, "not an image");

            vm.SetSelectedImage(path);

            Assert.Null(vm.SelectedImagePath);
        }

        [Fact]
        public void SetSelectedImage_ClearsPreviousResultTags()
        {
            var vm = CreateVm();
            vm.ResultTags = "1girl, solo";
            var path = CreateDummyImageFile(".jpg");

            vm.SetSelectedImage(path);

            Assert.Equal("", vm.ResultTags);
        }

        // ── TagImageCommand CanExecute ────────────────────────────────────────

        [Fact]
        public void TagImageCommand_NoConfigNoImage_CannotExecute()
        {
            var vm = CreateVm();
            Assert.False(vm.TagImageCommand.CanExecute(null));
        }

        [Fact]
        public void TagImageCommand_ConfigLoadedButNoImage_CannotExecute()
        {
            var setting = CreateSetting();
            setting.Data.ConfigPath = CreateTaggerConfigJson();
            var vm = CreateVm(setting);
            RunOnSta(() => vm.OnNavigatedToAsync().Wait());

            Assert.False(vm.TagImageCommand.CanExecute(null));
        }

        [Fact]
        public void TagImageCommand_ImageSelectedButConfigNotLoaded_CannotExecute()
        {
            var vm = CreateVm();
            vm.SetSelectedImage(CreateDummyImageFile());

            Assert.False(vm.TagImageCommand.CanExecute(null));
        }

        [Fact]
        public void TagImageCommand_ConfigLoadedAndImageSelected_CanExecute()
        {
            var setting = CreateSetting();
            setting.Data.ConfigPath = CreateTaggerConfigJson();
            var vm = CreateVm(setting);
            RunOnSta(() => vm.OnNavigatedToAsync().Wait());
            vm.SetSelectedImage(CreateDummyImageFile());

            Assert.True(vm.TagImageCommand.CanExecute(null));
        }

        [Fact]
        public void TagImageCommand_WhileRunning_CannotExecute()
        {
            var setting = CreateSetting();
            setting.Data.ConfigPath = CreateTaggerConfigJson();
            var vm = CreateVm(setting);
            RunOnSta(() => vm.OnNavigatedToAsync().Wait());
            vm.SetSelectedImage(CreateDummyImageFile());
            vm.IsRunning = true;

            Assert.False(vm.TagImageCommand.CanExecute(null));
        }

        // ── CopyTagsCommand CanExecute ────────────────────────────────────────

        [Fact]
        public void CopyTagsCommand_NoResult_CannotExecute()
        {
            var vm = CreateVm();
            Assert.False(vm.CopyTagsCommand.CanExecute(null));
        }

        [Fact]
        public void CopyTagsCommand_HasResult_CanExecute()
        {
            var vm = CreateVm();
            vm.ResultTags = "1girl, solo";

            Assert.True(vm.CopyTagsCommand.CanExecute(null));
        }
    }
}
