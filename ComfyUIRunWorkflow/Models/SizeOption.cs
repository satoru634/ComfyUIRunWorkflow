namespace ComfyUIRunWorkflow.Models
{
    /// <summary>
    /// 画像サイズ選択コンボボックスの1項目（論理キーと表示ラベルの組）。
    /// </summary>
    /// <param name="Key">プリセットキー（"vertical" / "horizontal" / "square" / "custom"）。</param>
    /// <param name="Label">画面表示用ラベル（例: "vertical (832×1216)"）。</param>
    public record SizeOption(string Key, string Label);
}
