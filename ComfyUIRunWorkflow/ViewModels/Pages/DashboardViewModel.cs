using ComfyUILibs.Common;
using ComfyUIRunWorkflow.Models;

namespace ComfyUIRunWorkflow.ViewModels.Pages
{
    public partial class DashboardViewModel : ObservableObject
    {
        public Setting<AppConfig> Config { get; }

        [ObservableProperty]
        private int _counter = 0;

        public DashboardViewModel(Setting<AppConfig> config)
        {
            Config = config;
        }

        [RelayCommand]
        private void OnCounterIncrement()
        {
            Counter++;
        }
    }
}
