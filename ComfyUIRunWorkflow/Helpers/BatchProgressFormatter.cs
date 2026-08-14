namespace ComfyUIRunWorkflow.Helpers
{
    /// <summary>
    /// バッチ実行中の進捗テキスト（例: "2/5件目を実行中"）を組み立てる共通ヘルパー。
    /// DashboardViewModel・QueueViewModel で共用する。
    /// </summary>
    internal static class BatchProgressFormatter
    {
        /// <summary>現在の言語で進捗テキストを組み立てる。</summary>
        public static string Format(int current, int total) =>
            string.Format(LocalizationManager.Instance["Dashboard_BatchProgress_Format"], current, total);
    }
}
