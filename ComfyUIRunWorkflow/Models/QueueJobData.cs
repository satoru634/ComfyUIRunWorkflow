namespace ComfyUIRunWorkflow.Models
{
    /// <summary>
    /// QueuePage の1ジョブ分の設定（永続化対象）。
    /// <see cref="QueueJobListData.Jobs"/> の要素として <c>queue_jobs.json</c> に保存される。
    /// 実行ステータス・実行結果（<see cref="QueueJobStatus"/>・<see cref="ComfyUILibs.Models.WorkflowResult"/>）は
    /// セッション限りのため、本クラスには含まれない。
    /// </summary>
    public partial class QueueJobData : ObservableObject
    {
        /// <summary>使用するワークフロー名。</summary>
        [ObservableProperty]
        private string _workflowName = "";

        /// <summary>ポジティブプロンプト。</summary>
        [ObservableProperty]
        private string _positivePrompt = "";

        /// <summary>ネガティブプロンプト。</summary>
        [ObservableProperty]
        private string _negativePrompt = "";

        /// <summary>選択済み LoRA 論理名の一覧。</summary>
        [ObservableProperty]
        private List<string> _loraFiles = new();

        /// <summary>選択中の画像サイズ向き（"vertical" / "horizontal" / "square"）。カスタムサイズ選択時は参照されない。</summary>
        [ObservableProperty]
        private string _imageSizeOrientation = "vertical";

        /// <summary>カスタムサイズ入力モードかどうか。</summary>
        [ObservableProperty]
        private bool _isCustomSize = false;

        /// <summary>カスタムサイズの幅（ピクセル）。</summary>
        [ObservableProperty]
        private int _customWidth = 832;

        /// <summary>カスタムサイズの高さ（ピクセル）。</summary>
        [ObservableProperty]
        private int _customHeight = 1216;

        /// <summary>バッチ数（1〜10）。</summary>
        [ObservableProperty]
        private int _batchCount = 1;
    }
}
