using ComfyUILibs.Models;
using ComfyUIRunWorkflow.Helpers;
using ComfyUIRunWorkflow.Models;

namespace ComfyUIRunWorkflow.Services
{
    /// <summary>
    /// ワークフロー設定（画像サイズプリセット）から画像サイズ選択肢（vertical/horizontal/square/custom）を
    /// 組み立てる共通ロジック。DashboardViewModel・QueueJobViewModel で共用する。
    /// </summary>
    internal static class WorkflowSizeOptionBuilder
    {
        private static readonly List<string> OrientationKeys = new() { "vertical", "horizontal", "square", "custom" };

        /// <summary>
        /// 選択中ワークフローの設定から、画像サイズ選択肢一覧（常に4件）とプリセットサイズ辞書を組み立てる。
        /// </summary>
        /// <param name="workflowSettings">選択中ワークフローの設定。null の場合はプリセットサイズなしの選択肢のみ返す。</param>
        public static (List<SizeOption> Options, Dictionary<string, ImageSize> PresetSizes) Build(WorkflowSettings? workflowSettings)
        {
            var presetSizes = workflowSettings?.ImageSize ?? new Dictionary<string, ImageSize>();
            var options = OrientationKeys.Select(key => new SizeOption(key, FormatLabel(key, presetSizes))).ToList();
            return (options, presetSizes);
        }

        /// <summary>ラベル文字列を生成する。プリセットサイズが存在する場合は "(width×height)" を付加する。</summary>
        private static string FormatLabel(string orientation, Dictionary<string, ImageSize> presetSizes)
        {
            var label = OrientationLabel(orientation);
            return presetSizes.TryGetValue(orientation, out var size)
                ? $"{label} ({size.Width}×{size.Height})"
                : label;
        }

        /// <summary>向きキー（"vertical"/"horizontal"/"square"/"custom"）を現在の言語の表示名に変換する。</summary>
        public static string OrientationLabel(string orientation) => orientation switch
        {
            "vertical" => LocalizationManager.Instance["Dashboard_OrientationVertical"],
            "horizontal" => LocalizationManager.Instance["Dashboard_OrientationHorizontal"],
            "square" => LocalizationManager.Instance["Dashboard_OrientationSquare"],
            "custom" => LocalizationManager.Instance["Dashboard_OrientationCustom"],
            _ => orientation,
        };
    }
}
