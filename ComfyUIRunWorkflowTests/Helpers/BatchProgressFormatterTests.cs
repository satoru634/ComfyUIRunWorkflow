using System.Globalization;
using ComfyUIRunWorkflow.Helpers;

namespace ComfyUIRunWorkflowTests.Helpers
{
    [Collection("Culture")]
    public class BatchProgressFormatterTests
    {
        [Theory]
        [InlineData(1, 5, "1/5件目を実行中")]
        [InlineData(3, 5, "3/5件目を実行中")]
        [InlineData(1, 1, "1/1件目を実行中")]
        public void Format_Japanese_ReturnsExpectedText(int current, int total, string expected)
        {
            var original = LocalizationManager.Instance.CurrentCulture;
            try
            {
                LocalizationManager.Instance.CurrentCulture = new CultureInfo("ja");
                Assert.Equal(expected, BatchProgressFormatter.Format(current, total));
            }
            finally
            {
                LocalizationManager.Instance.CurrentCulture = original;
            }
        }

        [Fact]
        public void Format_English_ReturnsExpectedText()
        {
            var original = LocalizationManager.Instance.CurrentCulture;
            try
            {
                LocalizationManager.Instance.CurrentCulture = new CultureInfo("en");
                Assert.Equal("Running 2/4", BatchProgressFormatter.Format(2, 4));
            }
            finally
            {
                LocalizationManager.Instance.CurrentCulture = original;
            }
        }
    }
}
