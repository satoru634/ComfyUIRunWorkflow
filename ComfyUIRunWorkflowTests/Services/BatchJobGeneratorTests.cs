using System.IO;
using System.Linq;
using ComfyUILibs.Common;
using ComfyUILibs.Models;
using ComfyUIRunWorkflow.Models;
using ComfyUIRunWorkflow.Services;

namespace ComfyUIRunWorkflowTests.Services
{
    public class BatchJobGeneratorTests : IDisposable
    {
        private readonly string _tempDir;
        private readonly string _baseDir;

        public BatchJobGeneratorTests()
        {
            _tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            _baseDir = Path.Combine(_tempDir, "base_prompts");
            Directory.CreateDirectory(_baseDir);
        }

        public void Dispose() => Directory.Delete(_tempDir, recursive: true);

        private string WriteBasePrompt(string fileName, string positive, string negative)
        {
            var path = Path.Combine(_baseDir, fileName);
            JsonLoader.WriteJson(path, new PromptPair { Positive = positive, Negative = negative });
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

        [Fact]
        public void Generate_ReplacesPlaceholdersInPositiveAndNegative()
        {
            WriteBasePrompt("a.json", "1girl, <CHARACTER>, <OUTFIT>", "worst quality, <BAD>");
            var replacementsPath = WriteReplacements(new()
            {
                ["<CHARACTER>"] = "alice",
                ["<OUTFIT>"] = "school uniform",
                ["<BAD>"] = "blurry",
            });
            var templatePath = WriteTemplate(new QueueJobData { WorkflowName = "sdxl" });

            var jobs = new BatchJobGenerator().Generate(_baseDir, replacementsPath, templatePath);

            Assert.Single(jobs);
            Assert.Equal("1girl, alice, school uniform", jobs[0].PositivePrompt);
            Assert.Equal("worst quality, blurry", jobs[0].NegativePrompt);
        }

        [Fact]
        public void Generate_OneJobPerBasePromptFile()
        {
            WriteBasePrompt("a.json", "1girl", "bad");
            WriteBasePrompt("b.json", "1boy", "bad");
            var replacementsPath = WriteReplacements(new());
            var templatePath = WriteTemplate(new QueueJobData { WorkflowName = "sdxl" });

            var jobs = new BatchJobGenerator().Generate(_baseDir, replacementsPath, templatePath);

            Assert.Equal(2, jobs.Count);
        }

        [Fact]
        public void Generate_JobNameIsBasePromptFileNameWithoutExtension()
        {
            WriteBasePrompt("kk_alice_shiraishi.json", "1girl", "bad");
            var replacementsPath = WriteReplacements(new());
            var templatePath = WriteTemplate(new QueueJobData { WorkflowName = "sdxl", JobName = "template job name" });

            var jobs = new BatchJobGenerator().Generate(_baseDir, replacementsPath, templatePath);

            Assert.Equal("kk_alice_shiraishi", jobs[0].JobName);
        }

        [Fact]
        public void Generate_JobNameDiffersPerBasePromptFile()
        {
            WriteBasePrompt("a.json", "1girl", "bad");
            WriteBasePrompt("b.json", "1boy", "bad");
            var replacementsPath = WriteReplacements(new());
            var templatePath = WriteTemplate(new QueueJobData { WorkflowName = "sdxl" });

            var jobs = new BatchJobGenerator().Generate(_baseDir, replacementsPath, templatePath);

            Assert.Equal("a", jobs.Single(j => j.PositivePrompt == "1girl").JobName);
            Assert.Equal("b", jobs.Single(j => j.PositivePrompt == "1boy").JobName);
        }

        [Fact]
        public void Generate_CopiesTemplateFieldsExceptPrompts()
        {
            WriteBasePrompt("a.json", "1girl", "bad");
            var replacementsPath = WriteReplacements(new());
            var templatePath = WriteTemplate(new QueueJobData
            {
                WorkflowName = "sdxl",
                FilenamePrefix = "my_batch",
                LoraFiles = new List<string> { "my_lora" },
                ImageSizeOrientation = "square",
                IsCustomSize = true,
                CustomWidth = 999,
                CustomHeight = 777,
                BatchCount = 3,
            });

            var jobs = new BatchJobGenerator().Generate(_baseDir, replacementsPath, templatePath);

            var job = jobs[0];
            Assert.Equal("sdxl", job.WorkflowName);
            Assert.Equal("my_batch", job.FilenamePrefix);
            Assert.Equal(new List<string> { "my_lora" }, job.LoraFiles);
            Assert.Equal("square", job.ImageSizeOrientation);
            Assert.True(job.IsCustomSize);
            Assert.Equal(999, job.CustomWidth);
            Assert.Equal(777, job.CustomHeight);
            Assert.Equal(3, job.BatchCount);
        }

        [Fact]
        public void Generate_NoPlaceholders_DoesNotRequireReplacements()
        {
            WriteBasePrompt("a.json", "1girl, solo", "worst quality");
            var replacementsPath = WriteReplacements(new());
            var templatePath = WriteTemplate(new QueueJobData { WorkflowName = "sdxl" });

            var jobs = new BatchJobGenerator().Generate(_baseDir, replacementsPath, templatePath);

            Assert.Equal("1girl, solo", jobs[0].PositivePrompt);
        }

        [Fact]
        public void Generate_UndefinedKeyword_ThrowsInvalidOperationException()
        {
            WriteBasePrompt("a.json", "1girl, <CHARACTER>", "worst quality");
            var replacementsPath = WriteReplacements(new());
            var templatePath = WriteTemplate(new QueueJobData { WorkflowName = "sdxl" });

            var ex = Assert.Throws<InvalidOperationException>(
                () => new BatchJobGenerator().Generate(_baseDir, replacementsPath, templatePath));

            Assert.Contains("<CHARACTER>", ex.Message);
            Assert.Contains("a.json", ex.Message);
        }

        [Fact]
        public void Generate_UndefinedKeywordInSecondFile_DoesNotProduceAnyJobs()
        {
            WriteBasePrompt("a.json", "1girl", "bad");
            WriteBasePrompt("b.json", "1girl, <MISSING>", "bad");
            var replacementsPath = WriteReplacements(new());
            var templatePath = WriteTemplate(new QueueJobData { WorkflowName = "sdxl" });

            Assert.Throws<InvalidOperationException>(
                () => new BatchJobGenerator().Generate(_baseDir, replacementsPath, templatePath));
        }

        [Fact]
        public void Generate_UnusedReplacementListEntries_AreIgnored()
        {
            WriteBasePrompt("a.json", "1girl", "bad");
            var replacementsPath = WriteReplacements(new() { ["<UNUSED>"] = "value" });
            var templatePath = WriteTemplate(new QueueJobData { WorkflowName = "sdxl" });

            var jobs = new BatchJobGenerator().Generate(_baseDir, replacementsPath, templatePath);

            Assert.Equal("1girl", jobs[0].PositivePrompt);
        }

        [Fact]
        public void Generate_EmptyDirectory_ThrowsInvalidOperationException()
        {
            var replacementsPath = WriteReplacements(new());
            var templatePath = WriteTemplate(new QueueJobData { WorkflowName = "sdxl" });

            Assert.Throws<InvalidOperationException>(
                () => new BatchJobGenerator().Generate(_baseDir, replacementsPath, templatePath));
        }

        [Fact]
        public void Generate_IgnoresNonJsonFilesInBaseDirectory()
        {
            WriteBasePrompt("a.json", "1girl", "bad");
            File.WriteAllText(Path.Combine(_baseDir, "readme.txt"), "not a base prompt");
            var replacementsPath = WriteReplacements(new());
            var templatePath = WriteTemplate(new QueueJobData { WorkflowName = "sdxl" });

            var jobs = new BatchJobGenerator().Generate(_baseDir, replacementsPath, templatePath);

            Assert.Single(jobs);
        }

        // ── 入力ファイルのスキーマ不正（原因ファイルの特定） ────────────────────────

        [Fact]
        public void Generate_ReplacementListIsJobTemplateShaped_ThrowsWithReplacementListPathInMessage()
        {
            // 実際にユーザーが遭遇した不具合の再現: 置換リストファイルに、誤ってジョブテンプレート形式の
            // JSON（LoraFiles が配列）を指定すると、Dictionary<string,string> への変換に失敗する。
            WriteBasePrompt("a.json", "1girl", "bad");
            var replacementsPath = Path.Combine(_tempDir, "replacements.json");
            File.WriteAllText(replacementsPath, """
                {
                  "WorkflowName": "sdxl",
                  "LoraFiles": ["my_lora"]
                }
                """);
            var templatePath = WriteTemplate(new QueueJobData { WorkflowName = "sdxl" });

            var ex = Assert.Throws<InvalidOperationException>(
                () => new BatchJobGenerator().Generate(_baseDir, replacementsPath, templatePath));

            Assert.Contains(replacementsPath, ex.Message);
            Assert.NotNull(ex.InnerException);
        }

        [Fact]
        public void Generate_JobTemplateHasInvalidJson_ThrowsWithJobTemplatePathInMessage()
        {
            WriteBasePrompt("a.json", "1girl", "bad");
            var replacementsPath = WriteReplacements(new());
            var templatePath = Path.Combine(_tempDir, "template.json");
            File.WriteAllText(templatePath, """{ "BatchCount": "not-a-number" }""");

            var ex = Assert.Throws<InvalidOperationException>(
                () => new BatchJobGenerator().Generate(_baseDir, replacementsPath, templatePath));

            Assert.Contains(templatePath, ex.Message);
            Assert.NotNull(ex.InnerException);
        }

        [Fact]
        public void Generate_BasePromptHasInvalidJson_ThrowsWithBasePromptPathInMessage()
        {
            var basePromptPath = Path.Combine(_baseDir, "a.json");
            File.WriteAllText(basePromptPath, """{ "positive": ["not", "a", "string"] }""");
            var replacementsPath = WriteReplacements(new());
            var templatePath = WriteTemplate(new QueueJobData { WorkflowName = "sdxl" });

            var ex = Assert.Throws<InvalidOperationException>(
                () => new BatchJobGenerator().Generate(_baseDir, replacementsPath, templatePath));

            Assert.Contains(basePromptPath, ex.Message);
            Assert.NotNull(ex.InnerException);
        }

        // ── onLog コールバック ───────────────────────────────────────────────────

        [Fact]
        public void Generate_OnLog_ReportsOneLinePerGeneratedJob()
        {
            WriteBasePrompt("a.json", "1girl", "bad");
            WriteBasePrompt("b.json", "1boy", "bad");
            var replacementsPath = WriteReplacements(new());
            var templatePath = WriteTemplate(new QueueJobData { WorkflowName = "sdxl" });
            var logLines = new List<string>();

            new BatchJobGenerator().Generate(_baseDir, replacementsPath, templatePath, onLog: logLines.Add);

            Assert.Contains(logLines, l => l.Contains("a.json"));
            Assert.Contains(logLines, l => l.Contains("b.json"));
        }

        [Fact]
        public void Generate_OnLog_ReportsFoundFileCount()
        {
            WriteBasePrompt("a.json", "1girl", "bad");
            WriteBasePrompt("b.json", "1boy", "bad");
            var replacementsPath = WriteReplacements(new());
            var templatePath = WriteTemplate(new QueueJobData { WorkflowName = "sdxl" });
            var logLines = new List<string>();

            new BatchJobGenerator().Generate(_baseDir, replacementsPath, templatePath, onLog: logLines.Add);

            Assert.Contains(logLines, l => l.Contains("2"));
        }

        [Fact]
        public void Generate_OnLog_NotProvided_DoesNotThrow()
        {
            WriteBasePrompt("a.json", "1girl", "bad");
            var replacementsPath = WriteReplacements(new());
            var templatePath = WriteTemplate(new QueueJobData { WorkflowName = "sdxl" });

            var jobs = new BatchJobGenerator().Generate(_baseDir, replacementsPath, templatePath, onLog: null);

            Assert.Single(jobs);
        }

        [Fact]
        public void Generate_OnLog_UndefinedKeyword_ReportsFoundCountBeforeThrowing()
        {
            WriteBasePrompt("a.json", "1girl, <MISSING>", "bad");
            var replacementsPath = WriteReplacements(new());
            var templatePath = WriteTemplate(new QueueJobData { WorkflowName = "sdxl" });
            var logLines = new List<string>();

            Assert.Throws<InvalidOperationException>(
                () => new BatchJobGenerator().Generate(_baseDir, replacementsPath, templatePath, onLog: logLines.Add));

            Assert.Contains(logLines, l => l.Contains("1"));
        }
    }
}
