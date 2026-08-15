using ComfyUILibs.Common;
using ComfyUILibs.Models;
using ComfyUIRunWorkflow.Helpers;
using ComfyUIRunWorkflow.Models;
using System.IO;
using System.Text.RegularExpressions;

namespace ComfyUIRunWorkflow.Services
{
    /// <summary>
    /// ベースプロンプトディレクトリ・置換リスト・ジョブテンプレートから、QueuePage 用ジョブ一覧（<see cref="QueueJobData"/>）を
    /// 一括生成する Generate ページ専用サービス。
    /// </summary>
    public class BatchJobGenerator
    {
        private static readonly Regex PlaceholderPattern = new(@"<[^<>]+>", RegexOptions.Compiled);

        /// <summary>
        /// baseDirectory 直下の全 *.json をベースプロンプト（<see cref="PromptPair"/> 形式）として読み込み、
        /// 置換リストでプレースホルダー（&lt;CHARACTER&gt; 等）を置換したうえで、ジョブテンプレートの各項目
        /// （ワークフロー・LoRA・画像サイズ・バッチ数・ファイル名プレフィックス）を複製した <see cref="QueueJobData"/> を
        /// ファイルごとに1件生成する。
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// baseDirectory 内に *.json が1件もない場合、またはベースプロンプト内のプレースホルダーが
        /// 置換リストに存在しない場合。
        /// </exception>
        public List<QueueJobData> Generate(string baseDirectory, string replacementListPath, string jobTemplatePath)
        {
            var template = JsonLoader.ReadJson<QueueJobData>(jobTemplatePath);
            var replacements = JsonLoader.ReadJson<Dictionary<string, string>>(replacementListPath);

            var basePromptFiles = Directory.GetFiles(baseDirectory, "*.json")
                .OrderBy(f => f, StringComparer.Ordinal)
                .ToList();

            if (basePromptFiles.Count == 0)
            {
                throw new InvalidOperationException(
                    string.Format(LocalizationManager.Instance["Generate_NoBasePromptFiles_Format"], baseDirectory));
            }

            var jobs = new List<QueueJobData>();
            foreach (var file in basePromptFiles)
            {
                var basePrompt = JsonLoader.ReadJson<PromptPair>(file);

                jobs.Add(new QueueJobData
                {
                    WorkflowName = template.WorkflowName,
                    PositivePrompt = ReplacePlaceholders(basePrompt.Positive, replacements, file),
                    NegativePrompt = ReplacePlaceholders(basePrompt.Negative, replacements, file),
                    FilenamePrefix = template.FilenamePrefix,
                    LoraFiles = new List<string>(template.LoraFiles),
                    ImageSizeOrientation = template.ImageSizeOrientation,
                    IsCustomSize = template.IsCustomSize,
                    CustomWidth = template.CustomWidth,
                    CustomHeight = template.CustomHeight,
                    BatchCount = template.BatchCount,
                });
            }

            return jobs;
        }

        /// <summary>
        /// テキスト内の &lt;KEYWORD&gt; 形式のプレースホルダーを置換リストの値で置換する。
        /// 置換リストに存在しないキーワードがあった場合は例外を投げて処理全体を中断する。
        /// </summary>
        private static string ReplacePlaceholders(string text, Dictionary<string, string> replacements, string sourceFile)
        {
            return PlaceholderPattern.Replace(text, match =>
            {
                if (replacements.TryGetValue(match.Value, out var replacement))
                    return replacement;

                throw new InvalidOperationException(string.Format(
                    LocalizationManager.Instance["Generate_UndefinedKeyword_Format"],
                    Path.GetFileName(sourceFile),
                    match.Value));
            });
        }
    }
}
