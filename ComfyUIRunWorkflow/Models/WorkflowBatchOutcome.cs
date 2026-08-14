using ComfyUILibs.Models;

namespace ComfyUIRunWorkflow.Models
{
    /// <summary>
    /// <see cref="ComfyUIRunWorkflow.Services.WorkflowExecutionService.RunBatchAsync"/> の戻り値。
    /// 保存・一覧表示用の <see cref="WorkflowResult"/> に加え、発生した例外（成功時は null）を保持する。
    /// 呼び出し側は例外の種類（<see cref="ComfyUILibs.Exceptions.ComfyUIException"/> かどうか）に応じて
    /// 通知メッセージの出し分けを行える。
    /// </summary>
    public sealed class WorkflowBatchOutcome
    {
        /// <summary>保存・表示用の実行結果。</summary>
        public required WorkflowResult Result { get; init; }

        /// <summary>実行中に発生した例外。成功時は null。</summary>
        public Exception? Error { get; init; }
    }
}
