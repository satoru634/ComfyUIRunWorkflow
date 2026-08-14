namespace ComfyUIRunWorkflowTests.Helpers
{
    /// <summary>
    /// <see cref="ComfyUIRunWorkflow.Helpers.LocalizationManager.Instance"/>.CurrentCulture を書き換えるテストは
    /// プロセス全体で共有される静的状態を操作するため、並列実行すると互いに干渉して flaky になる。
    /// このコレクションに所属するテストクラスは互いに並列実行されないことを保証する。
    /// </summary>
    [CollectionDefinition("Culture", DisableParallelization = true)]
    public class CultureCollection
    {
    }
}
