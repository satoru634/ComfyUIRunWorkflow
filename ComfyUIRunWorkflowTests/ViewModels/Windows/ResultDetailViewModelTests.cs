using System.IO;
using ComfyUILibs.Common;
using ComfyUILibs.Models;
using ComfyUIRunWorkflow.Models;
using ComfyUIRunWorkflow.ViewModels.Windows;

namespace ComfyUIRunWorkflowTests.ViewModels.Windows
{
    public class ResultDetailViewModelTests : IDisposable
    {
        private readonly string _tempDir;

        public ResultDetailViewModelTests()
        {
            _tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            Directory.CreateDirectory(_tempDir);
        }

        public void Dispose() => Directory.Delete(_tempDir, recursive: true);

        // ComfyUIUrl/ResultsFolder を空にしておくと、サムネイル取得（ネットワークアクセス）が
        // 起動されずコンストラクター直後の同期的な状態のみを検証できる
        private Setting<AppConfig> CreateSettingWithoutPreviewFetch()
        {
            var setting = new Setting<AppConfig>(Path.Combine(_tempDir, "setting.json"), onLoad: false);
            setting.Data.ComfyUIUrl = "";
            setting.Data.ResultsFolder = "";
            return setting;
        }

        [Fact]
        public void Constructor_ExposesResult()
        {
            var result = new WorkflowResult { Status = "success" };

            var vm = new ResultDetailViewModel(result, CreateSettingWithoutPreviewFetch());

            Assert.Same(result, vm.Result);
        }

        [Fact]
        public void Constructor_FiltersOutputsByTypeOutput()
        {
            var result = new WorkflowResult
            {
                Status = "success",
                Outputs = new List<OutputFile>
                {
                    new() { Filename = "a.png", Type = "output" },
                    new() { Filename = "b_temp.png", Type = "temp" },
                    new() { Filename = "c.png", Type = "output" },
                }
            };

            var vm = new ResultDetailViewModel(result, CreateSettingWithoutPreviewFetch());

            Assert.Equal(2, vm.Previews.Count);
            Assert.All(vm.Previews, p => Assert.Equal("output", p.Output.Type));
        }

        [Fact]
        public void Constructor_NoOutputs_PreviewsEmpty()
        {
            var result = new WorkflowResult { Status = "error", Error = "失敗" };

            var vm = new ResultDetailViewModel(result, CreateSettingWithoutPreviewFetch());

            Assert.Empty(vm.Previews);
        }

        [Fact]
        public void OpenEnlargedCommand_NoCachedFilePath_DoesNotThrow()
        {
            var result = new WorkflowResult
            {
                Status = "success",
                Outputs = new List<OutputFile> { new() { Filename = "a.png", Type = "output" } }
            };
            var vm = new ResultDetailViewModel(result, CreateSettingWithoutPreviewFetch());
            var preview = vm.Previews[0];

            var exception = Record.Exception(() => vm.OpenEnlargedCommand.Execute(preview));

            Assert.Null(exception);
        }
    }
}
