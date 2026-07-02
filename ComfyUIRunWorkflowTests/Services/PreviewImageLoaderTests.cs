using System.IO;
using System.Runtime.ExceptionServices;
using System.Text.Json;
using System.Text.Json.Nodes;
using ComfyUILibs.Models;
using ComfyUILibs.Services;
using ComfyUIRunWorkflow.Models;
using ComfyUIRunWorkflow.Services;

namespace ComfyUIRunWorkflowTests.Services
{
    /// <summary>テスト用の IComfyUIClient モック（画像取得は使用しない。キャッシュ解決は Fake で差し替える）</summary>
    internal class NoopComfyUIClient : IComfyUIClient
    {
        public Task<string> SubmitAsync(JsonObject workflow, string clientId) => Task.FromResult("pid");
        public Task MonitorAsync(string promptId, string clientId) => Task.CompletedTask;
        public Task<string> UploadImageAsync(byte[] imageData, string filename = "image.png") => Task.FromResult("uploaded.png");
        public Task<JsonElement> GetHistoryAsync(string promptId) => Task.FromResult(JsonDocument.Parse("{}").RootElement);
        public Task<List<OutputFile>> GetOutputsAsync(string promptId) => Task.FromResult(new List<OutputFile>());
        public Task<byte[]> GetImageAsync(string filename, string subfolder, string type) => Task.FromResult(Array.Empty<byte>());
    }

    /// <summary>テスト用の IPreviewImageCacheService モック。返すパスを外部から指定できる。</summary>
    internal class FakePreviewImageCacheService : IPreviewImageCacheService
    {
        public string? PathToReturn { get; set; }

        public Task<string?> GetOrFetchAsync(IComfyUIClient client, string? promptId, OutputFile output, string cacheDirectory)
            => Task.FromResult(PathToReturn);
    }

    public class PreviewImageLoaderTests : IDisposable
    {
        // 1x1 の最小有効 PNG（透過赤ピクセル）
        private static readonly byte[] MinimalPngBytes = Convert.FromBase64String(
            "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mNk+A8AAQUBAScY42YAAAAASUVORK5CYII=");

        private readonly string _tempDir;

        public PreviewImageLoaderTests()
        {
            _tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            Directory.CreateDirectory(_tempDir);
        }

        public void Dispose() => Directory.Delete(_tempDir, recursive: true);

        /// <summary>BitmapImage の生成など WPF 固有処理を STA スレッドで実行するヘルパー。</summary>
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

        private string WriteMinimalPng()
        {
            var path = Path.Combine(_tempDir, "cached.png");
            File.WriteAllBytes(path, MinimalPngBytes);
            return path;
        }

        [Fact]
        public void LoadAsync_NonImageOutput_DoesNotCallCacheServiceAndClearsIsLoading()
        {
            var output = new OutputFile { Filename = "video.mp4", Subfolder = "", Type = "output" };
            var preview = new OutputFilePreview(output);
            var cache = new FakePreviewImageCacheService();
            var loader = new PreviewImageLoader(cache);

            RunOnSta(() => loader.LoadAsync(preview, new NoopComfyUIClient(), "pid", _tempDir).Wait());

            Assert.False(preview.IsLoading);
            Assert.False(preview.HasError);
            Assert.Null(preview.Thumbnail);
        }

        [Fact]
        public void LoadAsync_CacheReturnsPath_SetsThumbnailAndCachedFilePath()
        {
            var pngPath = WriteMinimalPng();
            var output = new OutputFile { Filename = "img.png", Subfolder = "", Type = "output" };
            var preview = new OutputFilePreview(output);
            var cache = new FakePreviewImageCacheService { PathToReturn = pngPath };
            var loader = new PreviewImageLoader(cache);

            RunOnSta(() => loader.LoadAsync(preview, new NoopComfyUIClient(), "pid", _tempDir).Wait());

            Assert.False(preview.IsLoading);
            Assert.False(preview.HasError);
            Assert.NotNull(preview.Thumbnail);
            Assert.Equal(pngPath, preview.CachedFilePath);
        }

        [Fact]
        public void LoadAsync_CacheReturnsNull_SetsHasError()
        {
            var output = new OutputFile { Filename = "img.png", Subfolder = "", Type = "output" };
            var preview = new OutputFilePreview(output);
            var cache = new FakePreviewImageCacheService { PathToReturn = null };
            var loader = new PreviewImageLoader(cache);

            RunOnSta(() => loader.LoadAsync(preview, new NoopComfyUIClient(), "pid", _tempDir).Wait());

            Assert.False(preview.IsLoading);
            Assert.True(preview.HasError);
            Assert.Null(preview.Thumbnail);
        }

        [Fact]
        public void LoadFullSize_ValidImagePath_ReturnsBitmap()
        {
            var pngPath = WriteMinimalPng();

            RunOnSta(() =>
            {
                var bitmap = PreviewImageLoader.LoadFullSize(pngPath);
                Assert.NotNull(bitmap);
                Assert.Equal(1, bitmap.PixelWidth);
            });
        }
    }
}
