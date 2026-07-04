using System.Resources;

namespace ComfyUIRunWorkflow.Resources
{
    /// <summary>
    /// Strings.resx（既定・日本語）/ Strings.en.resx（英語）を参照する <see cref="ResourceManager"/> を公開する静的クラス。
    /// <see cref="Helpers.LocalizationManager"/> から利用される。
    /// </summary>
    internal static class Strings
    {
        public static readonly ResourceManager ResourceManager =
            new("ComfyUIRunWorkflow.Resources.Strings", typeof(Strings).Assembly);
    }
}
