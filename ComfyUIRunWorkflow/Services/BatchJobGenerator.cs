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
        /// ファイルごとに1件生成する。生成される各ジョブの JobName にはベースプロンプトのファイル名（拡張子なし）を
        /// 設定する（ジョブテンプレート側の値は使用しない）。
        /// </summary>
        /// <param name="onLog">
        /// 処理の進捗を1行ずつ通知するコールバック（GeneratePage のログ表示用）。不要な場合は null を指定する。
        /// </param>
        /// <exception cref="InvalidOperationException">
        /// いずれかの入力ファイルの JSON 形式が期待するスキーマと異なる場合、baseDirectory 内に *.json が
        /// 1件もない場合、またはベースプロンプト内のプレースホルダーが置換リストに存在しない場合。
        /// 元の例外（<see cref="System.Text.Json.JsonException"/> 等）は <see cref="Exception.InnerException"/> に保持する。
        /// </exception>
        public List<QueueJobData> Generate(
            string baseDirectory, string replacementListPath, string jobTemplatePath, Action<string>? onLog = null)
        {
            var template = ReadJsonOrThrow<QueueJobData>(jobTemplatePath, "Generate_JobTemplateReadError_Format");
            onLog?.Invoke(string.Format(LocalizationManager.Instance["Generate_Log_LoadedTemplate_Format"], jobTemplatePath));

            var replacements = ReadJsonOrThrow<Dictionary<string, string>>(replacementListPath, "Generate_ReplacementListReadError_Format");
            onLog?.Invoke(string.Format(
                LocalizationManager.Instance["Generate_Log_LoadedReplacements_Format"], replacementListPath, replacements.Count));

            var basePromptFiles = Directory.GetFiles(baseDirectory, "*.json")
                .OrderBy(f => f, StringComparer.Ordinal)
                .ToList();
            onLog?.Invoke(string.Format(
                LocalizationManager.Instance["Generate_Log_FoundBasePrompts_Format"], basePromptFiles.Count, baseDirectory));

            if (basePromptFiles.Count == 0)
            {
                throw new InvalidOperationException(
                    string.Format(LocalizationManager.Instance["Generate_NoBasePromptFiles_Format"], baseDirectory));
            }

            var jobs = new List<QueueJobData>();
            foreach (var file in basePromptFiles)
            {
                var basePrompt = ReadJsonOrThrow<PromptPair>(file, "Generate_BasePromptReadError_Format");

                jobs.Add(new QueueJobData
                {
                    JobName = Path.GetFileNameWithoutExtension(file),
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

                onLog?.Invoke(string.Format(
                    LocalizationManager.Instance["Generate_Log_JobGenerated_Format"], Path.GetFileName(file)));
            }

            return jobs;
        }

        /// <summary>
        /// <see cref="JsonLoader.ReadJson{T}(string)"/> をラップし、失敗時に「どのファイルの読み込みで失敗したか」が
        /// わかる <see cref="InvalidOperationException"/> に変換する（元の例外は InnerException に保持）。
        /// 期待するスキーマと異なる JSON（例: 置換リストファイルにジョブテンプレート形式の JSON を誤って指定した場合）を
        /// 選択した際に、原因ファイルが特定できないエラーメッセージしか出ない問題への対処。
        /// </summary>
        private static T ReadJsonOrThrow<T>(string path, string errorMessageKey) where T : new()
        {
            try
            {
                return JsonLoader.ReadJson<T>(path);
            }
            catch (Exception ex) when (ex is not InvalidOperationException)
            {
                throw new InvalidOperationException(
                    string.Format(LocalizationManager.Instance[errorMessageKey], path, ex.Message), ex);
            }
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
