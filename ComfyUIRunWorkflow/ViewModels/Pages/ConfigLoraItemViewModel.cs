namespace ComfyUIRunWorkflow.ViewModels.Pages
{
    /// <summary>
    /// ConfigEditorPage で編集する LoRA エントリ1件分（workflow_config.json の workflows[name].loras[論理名]）。
    /// </summary>
    public partial class ConfigLoraItemViewModel : ObservableObject
    {
        /// <summary>LoRA の論理名（キー）。入力 JSON の loras リストで指定される名前と一致させる。</summary>
        [ObservableProperty]
        private string _name = "";

        /// <summary>LoRA の実ファイル名（例: my_lora.safetensors）。</summary>
        [ObservableProperty]
        private string _file = "";

        /// <summary>LoRA の適用強度。</summary>
        [ObservableProperty]
        private double _strength = 0.8;
    }
}
