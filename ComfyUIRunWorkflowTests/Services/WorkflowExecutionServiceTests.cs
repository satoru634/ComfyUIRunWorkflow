using System.IO;
using System.Text.Json;
using ComfyUILibs.Models;
using ComfyUIRunWorkflow.Services;

namespace ComfyUIRunWorkflowTests.Services
{
    public class WorkflowExecutionServiceTests : IDisposable
    {
        private readonly string _tempDir;

        public WorkflowExecutionServiceTests()
        {
            _tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            Directory.CreateDirectory(_tempDir);
        }

        public void Dispose() => Directory.Delete(_tempDir, recursive: true);

        private static WorkflowResult CreateResult(string status = "success") => new()
        {
            Status = status,
            PromptId = "abc-123",
            Timestamp = DateTime.Now.ToString("s"),
            Parameters = new WorkflowParameters(),
            Outputs = new List<OutputFile>
            {
                new() { Filename = "output.png", Subfolder = "", Type = "output" },
            },
        };

        [Fact]
        public async Task SaveResultAsync_EmptyFolder_DoesNotThrow()
        {
            await WorkflowExecutionService.SaveResultAsync(CreateResult(), "");
        }

        [Fact]
        public async Task SaveResultAsync_ValidFolder_WritesResultJsonFile()
        {
            await WorkflowExecutionService.SaveResultAsync(CreateResult(), _tempDir);

            var files = Directory.GetFiles(_tempDir, "result_*.json");
            Assert.Single(files);
        }

        [Fact]
        public async Task SaveResultAsync_ValidFolder_WrittenContentRoundTrips()
        {
            var result = CreateResult();

            await WorkflowExecutionService.SaveResultAsync(result, _tempDir);

            var file = Directory.GetFiles(_tempDir, "result_*.json").Single();
            var json = await File.ReadAllTextAsync(file);
            var loaded = JsonSerializer.Deserialize<WorkflowResult>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            Assert.NotNull(loaded);
            Assert.Equal(result.Status, loaded!.Status);
            Assert.Equal(result.PromptId, loaded.PromptId);
            Assert.Single(loaded.Outputs);
        }

        [Fact]
        public async Task SaveResultAsync_FolderDoesNotExist_CreatesFolder()
        {
            var newFolder = Path.Combine(_tempDir, "nested");

            await WorkflowExecutionService.SaveResultAsync(CreateResult(), newFolder);

            Assert.True(Directory.Exists(newFolder));
        }
    }
}
