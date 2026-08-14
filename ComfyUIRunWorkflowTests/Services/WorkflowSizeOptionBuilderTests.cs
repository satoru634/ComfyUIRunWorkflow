using System.Globalization;
using ComfyUILibs.Models;
using ComfyUIRunWorkflow.Helpers;
using ComfyUIRunWorkflow.Services;

namespace ComfyUIRunWorkflowTests.Services
{
    [Collection("Culture")]
    public class WorkflowSizeOptionBuilderTests : IDisposable
    {
        private readonly CultureInfo _originalCulture;

        public WorkflowSizeOptionBuilderTests()
        {
            _originalCulture = LocalizationManager.Instance.CurrentCulture;
            LocalizationManager.Instance.CurrentCulture = new CultureInfo("ja");
        }

        public void Dispose() => LocalizationManager.Instance.CurrentCulture = _originalCulture;

        [Fact]
        public void Build_NullWorkflowSettings_ReturnsFourOptions()
        {
            var (options, presetSizes) = WorkflowSizeOptionBuilder.Build(null);

            Assert.Equal(4, options.Count);
            Assert.Empty(presetSizes);
        }

        [Fact]
        public void Build_NullWorkflowSettings_LabelsHaveNoDimensions()
        {
            var (options, _) = WorkflowSizeOptionBuilder.Build(null);

            var vertical = options.Single(o => o.Key == "vertical");
            Assert.DoesNotContain("×", vertical.Label);
        }

        [Fact]
        public void Build_WithImageSize_LabelsContainDimensions()
        {
            var ws = new WorkflowSettings
            {
                ImageSize = new Dictionary<string, ImageSize>
                {
                    ["vertical"] = new ImageSize { Width = 832, Height = 1216 },
                    ["horizontal"] = new ImageSize { Width = 1216, Height = 832 },
                    ["square"] = new ImageSize { Width = 1024, Height = 1024 },
                }
            };

            var (options, presetSizes) = WorkflowSizeOptionBuilder.Build(ws);

            var vertical = options.Single(o => o.Key == "vertical");
            Assert.Contains("832", vertical.Label);
            Assert.Contains("1216", vertical.Label);
            Assert.Equal(3, presetSizes.Count);
        }

        [Fact]
        public void Build_WithImageSize_CustomOptionHasNoDimensions()
        {
            var ws = new WorkflowSettings
            {
                ImageSize = new Dictionary<string, ImageSize>
                {
                    ["vertical"] = new ImageSize { Width = 832, Height = 1216 },
                }
            };

            var (options, _) = WorkflowSizeOptionBuilder.Build(ws);

            var custom = options.Single(o => o.Key == "custom");
            Assert.DoesNotContain("×", custom.Label);
        }

        [Theory]
        [InlineData("vertical", "縦")]
        [InlineData("horizontal", "横")]
        [InlineData("square", "正方形")]
        [InlineData("custom", "カスタム")]
        public void OrientationLabel_Japanese_ReturnsExpectedLabel(string orientation, string expected)
        {
            Assert.Equal(expected, WorkflowSizeOptionBuilder.OrientationLabel(orientation));
        }
    }
}
