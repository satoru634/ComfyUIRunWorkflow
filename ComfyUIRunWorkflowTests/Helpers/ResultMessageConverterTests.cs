using System.Globalization;
using ComfyUILibs.Models;
using ComfyUIRunWorkflow.Helpers;

namespace ComfyUIRunWorkflowTests.Helpers
{
    public class ResultMessageConverterTests
    {
        private readonly ResultMessageConverter _converter = new();

        // ── Convert: 成功時 ────────────────────────────────────

        [Fact]
        public void Convert_SuccessWithOutputTypeOutput_ReturnsFilename()
        {
            var result = new WorkflowResult
            {
                Status = "success",
                Outputs =
                [
                    new OutputFile { Filename = "ComfyUI_00001_.png", Type = "output" },
                ],
            };

            var actual = _converter.Convert(result, typeof(string), null!, CultureInfo.InvariantCulture);

            Assert.Equal("ComfyUI_00001_.png", actual);
        }

        [Fact]
        public void Convert_SuccessSkipsNonOutputTypeEntries_ReturnsMatchingFilename()
        {
            var result = new WorkflowResult
            {
                Status = "success",
                Outputs =
                [
                    new OutputFile { Filename = "preview.png", Type = "temp" },
                    new OutputFile { Filename = "ComfyUI_00002_.png", Type = "output" },
                ],
            };

            var actual = _converter.Convert(result, typeof(string), null!, CultureInfo.InvariantCulture);

            Assert.Equal("ComfyUI_00002_.png", actual);
        }

        [Fact]
        public void Convert_SuccessWithNoOutputTypeEntry_ReturnsEmptyString()
        {
            var result = new WorkflowResult
            {
                Status = "success",
                Outputs = [new OutputFile { Filename = "preview.png", Type = "temp" }],
            };

            var actual = _converter.Convert(result, typeof(string), null!, CultureInfo.InvariantCulture);

            Assert.Equal("", actual);
        }

        [Fact]
        public void Convert_SuccessWithEmptyOutputs_ReturnsEmptyString()
        {
            var result = new WorkflowResult { Status = "success", Outputs = [] };

            var actual = _converter.Convert(result, typeof(string), null!, CultureInfo.InvariantCulture);

            Assert.Equal("", actual);
        }

        // ── Convert: エラー時 ────────────────────────────────────

        [Fact]
        public void Convert_ErrorWithMessage_ReturnsErrorMessage()
        {
            var result = new WorkflowResult { Status = "error", Error = "WebSocket 接続エラー" };

            var actual = _converter.Convert(result, typeof(string), null!, CultureInfo.InvariantCulture);

            Assert.Equal("WebSocket 接続エラー", actual);
        }

        [Fact]
        public void Convert_ErrorWithNullMessage_ReturnsEmptyString()
        {
            var result = new WorkflowResult { Status = "error", Error = null };

            var actual = _converter.Convert(result, typeof(string), null!, CultureInfo.InvariantCulture);

            Assert.Equal("", actual);
        }

        // ── Convert: その他 ────────────────────────────────────

        [Fact]
        public void Convert_UnknownStatus_ReturnsEmptyString()
        {
            var result = new WorkflowResult { Status = "running" };

            var actual = _converter.Convert(result, typeof(string), null!, CultureInfo.InvariantCulture);

            Assert.Equal("", actual);
        }

        [Fact]
        public void Convert_NonWorkflowResultValue_ReturnsEmptyString()
        {
            var actual = _converter.Convert("not a result", typeof(string), null!, CultureInfo.InvariantCulture);

            Assert.Equal("", actual);
        }

        [Fact]
        public void Convert_NullValue_ReturnsEmptyString()
        {
            var actual = _converter.Convert(null!, typeof(string), null!, CultureInfo.InvariantCulture);

            Assert.Equal("", actual);
        }

        // ── ConvertBack ────────────────────────────────────

        [Fact]
        public void ConvertBack_Always_ThrowsNotImplementedException()
        {
            Assert.Throws<NotImplementedException>(
                () => _converter.ConvertBack("value", typeof(WorkflowResult), null!, CultureInfo.InvariantCulture));
        }
    }
}
