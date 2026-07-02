using ComfyUILibs.Models;

namespace ComfyUIRunWorkflow.Models
{
    /// <summary>
    /// 実行結果一覧（DataPage）の 1 行分を表すラッパー。
    /// <see cref="WorkflowResult"/> 本体に加え、カードに表示するサムネイル 1 枚分の
    /// プレビュー状態（<see cref="Preview"/>）を保持する。
    /// </summary>
    public partial class WorkflowResultPreview : ObservableObject
    {
        /// <summary>元の実行結果。</summary>
        public WorkflowResult Result { get; }

        /// <summary>
        /// 一覧カードに表示するサムネイル（最初の出力画像）。
        /// 出力に画像が含まれない場合は null のまま。
        /// </summary>
        [ObservableProperty]
        private OutputFilePreview? _preview;

        public WorkflowResultPreview(WorkflowResult result)
        {
            Result = result;
        }
    }
}
