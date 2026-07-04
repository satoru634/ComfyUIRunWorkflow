namespace ComfyUIRunWorkflow.Models
{
    /// <summary>
    /// 言語選択コンボボックスの1項目（言語コード＋現地語表記のラベル）。
    /// ラベルは翻訳せず常にその言語自身の呼称（"日本語"／"English"）を表示する。
    /// </summary>
    public record LanguageOption(string Key, string Label);
}
