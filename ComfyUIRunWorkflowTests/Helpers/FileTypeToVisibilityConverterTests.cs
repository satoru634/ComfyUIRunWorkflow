using System.Globalization;
using System.Windows;
using ComfyUIRunWorkflow.Helpers;

namespace ComfyUIRunWorkflowTests.Helpers
{
    public class FileTypeToVisibilityConverterTests
    {
        private readonly FileTypeToVisibilityConverter _converter = new();

        // ── Convert ────────────────────────────────────

        [Fact]
        public void Convert_TypeIsOutput_ReturnsVisible()
        {
            var actual = _converter.Convert("output", typeof(Visibility), null!, CultureInfo.InvariantCulture);

            Assert.Equal(Visibility.Visible, actual);
        }

        [Fact]
        public void Convert_TypeIsTemp_ReturnsCollapsed()
        {
            var actual = _converter.Convert("temp", typeof(Visibility), null!, CultureInfo.InvariantCulture);

            Assert.Equal(Visibility.Collapsed, actual);
        }

        [Fact]
        public void Convert_EmptyString_ReturnsCollapsed()
        {
            var actual = _converter.Convert("", typeof(Visibility), null!, CultureInfo.InvariantCulture);

            Assert.Equal(Visibility.Collapsed, actual);
        }

        [Fact]
        public void Convert_NonStringValue_ReturnsCollapsed()
        {
            var actual = _converter.Convert(123, typeof(Visibility), null!, CultureInfo.InvariantCulture);

            Assert.Equal(Visibility.Collapsed, actual);
        }

        [Fact]
        public void Convert_NullValue_ReturnsCollapsed()
        {
            var actual = _converter.Convert(null!, typeof(Visibility), null!, CultureInfo.InvariantCulture);

            Assert.Equal(Visibility.Collapsed, actual);
        }

        // ── ConvertBack ────────────────────────────────────

        [Fact]
        public void ConvertBack_Always_ThrowsNotImplementedException()
        {
            Assert.Throws<NotImplementedException>(
                () => _converter.ConvertBack(Visibility.Visible, typeof(string), null!, CultureInfo.InvariantCulture));
        }
    }
}
