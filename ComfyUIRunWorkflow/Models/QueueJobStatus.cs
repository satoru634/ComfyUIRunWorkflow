namespace ComfyUIRunWorkflow.Models
{
    /// <summary>
    /// QueuePage の1ジョブの実行状態。
    /// </summary>
    public enum QueueJobStatus
    {
        /// <summary>未実行。</summary>
        Pending,

        /// <summary>実行中。</summary>
        Running,

        /// <summary>成功。</summary>
        Success,

        /// <summary>失敗（ComfyUI エラー等）。</summary>
        Error,

        /// <summary>中断によりスキップされた（未実行のまま）。</summary>
        Cancelled,
    }
}
