using ComfyUILibs.Models;
using ComfyUIRunWorkflow.Models;

namespace ComfyUIRunWorkflowTests.Models
{
    public class OutputFilePreviewTests
    {
        [Fact]
        public void Constructor_ImageFile_IsImageTrueAndIsLoadingTrue()
        {
            var output = new OutputFile { Filename = "img.png", Subfolder = "", Type = "output" };

            var preview = new OutputFilePreview(output);

            Assert.True(preview.IsImage);
            Assert.True(preview.IsLoading);
            Assert.Null(preview.Thumbnail);
            Assert.False(preview.HasError);
        }

        [Fact]
        public void Constructor_NonImageFile_IsImageFalseAndIsLoadingFalse()
        {
            var output = new OutputFile { Filename = "video.mp4", Subfolder = "", Type = "output" };

            var preview = new OutputFilePreview(output);

            Assert.False(preview.IsImage);
            Assert.False(preview.IsLoading);
        }

        [Fact]
        public void Constructor_ExposesOriginalOutput()
        {
            var output = new OutputFile { Filename = "img.png", Subfolder = "sub", Type = "output" };

            var preview = new OutputFilePreview(output);

            Assert.Same(output, preview.Output);
        }
    }
}
