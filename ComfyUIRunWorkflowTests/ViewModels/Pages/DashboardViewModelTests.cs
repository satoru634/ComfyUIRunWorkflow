using System.ComponentModel;
using System.IO;
using ComfyUILibs.Common;
using ComfyUIRunWorkflow.Models;
using ComfyUIRunWorkflow.ViewModels.Pages;

namespace ComfyUIRunWorkflowTests.ViewModels.Pages
{
    public class DashboardViewModelTests : IDisposable
    {
        private readonly string _tempDir;

        public DashboardViewModelTests()
        {
            _tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            Directory.CreateDirectory(_tempDir);
        }

        public void Dispose()
        {
            Directory.Delete(_tempDir, recursive: true);
        }

        private Setting<AppConfig> CreateSetting()
            => new Setting<AppConfig>(Path.Combine(_tempDir, "setting.json"), onLoad: false);

        // ── コンストラクター ───────────────────────────────────────────────────

        [Fact]
        public void Constructor_Config_IsSet()
        {
            var setting = CreateSetting();

            var vm = new DashboardViewModel(setting);

            Assert.Same(setting, vm.Config);
        }

        // ── Counter ────────────────────────────────────────────────────────────

        [Fact]
        public void Counter_InitialValue_IsZero()
        {
            var vm = new DashboardViewModel(CreateSetting());

            Assert.Equal(0, vm.Counter);
        }

        [Fact]
        public void Counter_Set_RaisesPropertyChanged()
        {
            var vm = new DashboardViewModel(CreateSetting());
            var changed = new List<string?>();
            ((INotifyPropertyChanged)vm).PropertyChanged += (_, e) => changed.Add(e.PropertyName);

            vm.Counter = 5;

            Assert.Contains("Counter", changed);
        }

        // ── CounterIncrementCommand ────────────────────────────────────────────

        [Fact]
        public void CounterIncrementCommand_Execute_IncrementsCounter()
        {
            var vm = new DashboardViewModel(CreateSetting());

            vm.CounterIncrementCommand.Execute(null);

            Assert.Equal(1, vm.Counter);
        }

        [Fact]
        public void CounterIncrementCommand_ExecuteMultipleTimes_AccumulatesCount()
        {
            var vm = new DashboardViewModel(CreateSetting());

            vm.CounterIncrementCommand.Execute(null);
            vm.CounterIncrementCommand.Execute(null);
            vm.CounterIncrementCommand.Execute(null);

            Assert.Equal(3, vm.Counter);
        }
    }
}
