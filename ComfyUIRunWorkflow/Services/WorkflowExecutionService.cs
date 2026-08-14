using ComfyUILibs.Exceptions;
using ComfyUILibs.Models;
using ComfyUILibs.Services;
using ComfyUIRunWorkflow.Models;
using System.IO;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;

namespace ComfyUIRunWorkflow.Services
{
    /// <summary>
    /// 「指定回数分バッチ実行して1件の <see cref="WorkflowResult"/> にまとめる」処理を担う共通サービス。
    /// DashboardViewModel（単一ワークフロー実行）・QueueViewModel（複数ワークフロー連続実行）で共用する。
    /// </summary>
    public class WorkflowExecutionService
    {
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            WriteIndented = true,
            Encoder = JavaScriptEncoder.Create(UnicodeRanges.All),
        };

        /// <summary>
        /// LoRA・プロンプト・画像サイズを指定回数分（<paramref name="batchCount"/>）繰り返し実行し、
        /// 結果を1件の <see cref="WorkflowResult"/> にまとめる。各回シードは <see cref="WorkflowRunner"/> 側で自動採番される。
        /// 途中で <see cref="ComfyUIException"/>（またはその他の例外）が発生した場合はその時点で中断し、
        /// 成功済み分の出力を含めたエラー結果を返す（例外は再送出しない）。
        /// </summary>
        /// <param name="configPath">workflow_config.json のパス。</param>
        /// <param name="workflowName">実行するワークフロー名。</param>
        /// <param name="loras">LoRA 論理名のリスト。</param>
        /// <param name="prompts">正・負プロンプトのペア。</param>
        /// <param name="imageSize">画像サイズ。null の場合はワークフローの既定サイズを使用する。</param>
        /// <param name="batchCount">バッチ数（1未満は1として扱う）。</param>
        /// <param name="onBatchStart">各バッチ開始直前に呼ばれるコールバック（現在の回数, 総回数）。</param>
        /// <param name="onBatchCompleted">各バッチ完了直後に呼ばれるコールバック（出力ファイル一覧, prompt_id）。</param>
        public async Task<WorkflowBatchOutcome> RunBatchAsync(
            string configPath,
            string workflowName,
            List<string> loras,
            PromptPair prompts,
            ImageSize? imageSize,
            int batchCount,
            Action<int, int>? onBatchStart = null,
            Action<List<OutputFile>, string?>? onBatchCompleted = null)
        {
            var totalBatches = Math.Max(1, batchCount);
            var allOutputs = new List<OutputFile>();
            string? lastSuccessPromptId = null;
            string? lastSuccessTemplatePath = null;
            WorkflowParameters? lastSuccessParameters = null;

            WorkflowRunner? runner = null;

            try
            {
                runner = new WorkflowRunner(configPath, workflowName);

                for (int i = 1; i <= totalBatches; i++)
                {
                    onBatchStart?.Invoke(i, totalBatches);

                    var outputs = await runner.ExecuteAsync(loras, prompts, imageSize);

                    allOutputs.AddRange(outputs);
                    lastSuccessPromptId = runner.PromptId;
                    lastSuccessTemplatePath = runner.TemplatePath;
                    lastSuccessParameters = runner.Parameters;

                    onBatchCompleted?.Invoke(outputs, runner.PromptId);
                }

                var result = new WorkflowResult
                {
                    Status = "success",
                    PromptId = lastSuccessPromptId,
                    Timestamp = DateTime.Now.ToString("s"),
                    Template = lastSuccessTemplatePath,
                    Parameters = lastSuccessParameters ?? new WorkflowParameters(),
                    Outputs = allOutputs,
                };

                return new WorkflowBatchOutcome { Result = result, Error = null };
            }
            catch (ComfyUIException ex)
            {
                var result = new WorkflowResult
                {
                    Status = "error",
                    PromptId = lastSuccessPromptId,
                    Timestamp = DateTime.Now.ToString("s"),
                    Template = lastSuccessTemplatePath ?? runner?.TemplatePath,
                    Parameters = lastSuccessParameters ?? runner?.Parameters ?? new WorkflowParameters(),
                    Outputs = allOutputs,
                    Error = ex.Message,
                };

                return new WorkflowBatchOutcome { Result = result, Error = ex };
            }
            catch (Exception ex)
            {
                var result = new WorkflowResult
                {
                    Status = "error",
                    PromptId = lastSuccessPromptId,
                    Timestamp = DateTime.Now.ToString("s"),
                    Template = lastSuccessTemplatePath,
                    Parameters = lastSuccessParameters ?? new WorkflowParameters(),
                    Outputs = allOutputs,
                    Error = ex.Message,
                };

                return new WorkflowBatchOutcome { Result = result, Error = ex };
            }
        }

        /// <summary>
        /// 実行結果を resultsFolder に result_{timestamp}.json として保存する。
        /// resultsFolder が未設定の場合、または保存に失敗した場合は何もしない（呼び出し元の結果表示に影響させない）。
        /// </summary>
        public static async Task SaveResultAsync(WorkflowResult result, string resultsFolder)
        {
            if (string.IsNullOrWhiteSpace(resultsFolder)) return;

            try
            {
                Directory.CreateDirectory(resultsFolder);
                var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                var outputPath = Path.Combine(resultsFolder, $"result_{timestamp}.json");
                var json = JsonSerializer.Serialize(result, JsonOptions);
                await File.WriteAllTextAsync(outputPath, json);
            }
            catch
            {
                // 保存失敗は実行結果に影響させない
            }
        }
    }
}
