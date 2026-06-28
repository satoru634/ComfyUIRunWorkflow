using CommunityToolkit.Mvvm.ComponentModel;

namespace ComfyUIRunWorkflow.Models
{
    /// <summary>
    /// DashboardPage の LoRA 選択 ComboBox 一つ分のデータを保持する ViewModel。
    /// ObservableCollection&lt;LoraSlot&gt; の要素として使用する。
    /// </summary>
    public partial class LoraSlot : ObservableObject
    {
        /// <summary>選択中の LoRA 論理名。未選択の場合は空文字。</summary>
        [ObservableProperty]
        private string _selectedLora = "";
    }
}
