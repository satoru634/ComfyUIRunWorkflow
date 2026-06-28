using System.ComponentModel;
using System.IO;
using System.Runtime.ExceptionServices;
using ComfyUILibs.Common;
using ComfyUIRunWorkflow.Models;
using ComfyUIRunWorkflow.ViewModels.Windows;
using ComfyUIRunWorkflow.Views.Pages;
using Wpf.Ui.Controls;

namespace ComfyUIRunWorkflowTests.ViewModels.Windows
{
    public class MainWindowViewModelTests : IDisposable
    {
        private readonly string _tempDir;

        public MainWindowViewModelTests()
        {
            _tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            Directory.CreateDirectory(_tempDir);
        }

        public void Dispose()
        {
            Directory.Delete(_tempDir, recursive: true);
        }

        private string TempPath(string name) => Path.Combine(_tempDir, name);

        private Setting<AppConfig> CreateSetting(string fileName = "setting.json")
            => new Setting<AppConfig>(TempPath(fileName), onLoad: false);

        /// <summary>
        /// WPF コントロール生成に必要な STA スレッドでアクションを実行するヘルパー。
        /// NavigationViewItem などの WPF 要素はコンストラクターで STA を要求するため必要。
        /// </summary>
        private static void RunOnSta(Action action)
        {
            Exception? caught = null;
            var thread = new Thread(() =>
            {
                try { action(); }
                catch (Exception ex) { caught = ex; }
            });
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join();
            if (caught is not null)
                ExceptionDispatchInfo.Capture(caught).Throw();
        }

        // ── コンストラクター ───────────────────────────────────────────────────

        [Fact]
        public void Constructor_Config_IsSet()
        {
            var setting = CreateSetting();
            MainWindowViewModel? vm = null;

            RunOnSta(() => vm = new MainWindowViewModel(setting));

            Assert.Same(setting, vm!.Config);
        }

        // ── ApplicationTitle ──────────────────────────────────────────────────

        [Fact]
        public void ApplicationTitle_InitialValue_IsNotEmpty()
        {
            RunOnSta(() =>
            {
                var vm = new MainWindowViewModel(CreateSetting());

                Assert.False(string.IsNullOrEmpty(vm.ApplicationTitle));
            });
        }

        [Fact]
        public void ApplicationTitle_Set_RaisesPropertyChanged()
        {
            RunOnSta(() =>
            {
                var vm = new MainWindowViewModel(CreateSetting());
                var changed = new List<string?>();
                ((INotifyPropertyChanged)vm).PropertyChanged += (_, e) => changed.Add(e.PropertyName);

                vm.ApplicationTitle = "Test Title";

                Assert.Contains("ApplicationTitle", changed);
            });
        }

        // ── MenuItems ─────────────────────────────────────────────────────────

        [Fact]
        public void MenuItems_Contains_HomeItem()
        {
            RunOnSta(() =>
            {
                var vm = new MainWindowViewModel(CreateSetting());

                var items = vm.MenuItems.OfType<NavigationViewItem>().ToList();
                Assert.Contains(items, item => item.Content?.ToString() == "Home");
            });
        }

        [Fact]
        public void MenuItems_Contains_DataItem()
        {
            RunOnSta(() =>
            {
                var vm = new MainWindowViewModel(CreateSetting());

                var items = vm.MenuItems.OfType<NavigationViewItem>().ToList();
                Assert.Contains(items, item => item.Content?.ToString() == "Data");
            });
        }

        [Fact]
        public void MenuItems_HomeItem_TargetPage_IsDashboardPage()
        {
            RunOnSta(() =>
            {
                var vm = new MainWindowViewModel(CreateSetting());

                var home = vm.MenuItems.OfType<NavigationViewItem>()
                    .FirstOrDefault(item => item.Content?.ToString() == "Home");

                Assert.Equal(typeof(DashboardPage), home?.TargetPageType);
            });
        }

        [Fact]
        public void MenuItems_DataItem_TargetPage_IsDataPage()
        {
            RunOnSta(() =>
            {
                var vm = new MainWindowViewModel(CreateSetting());

                var data = vm.MenuItems.OfType<NavigationViewItem>()
                    .FirstOrDefault(item => item.Content?.ToString() == "Data");

                Assert.Equal(typeof(DataPage), data?.TargetPageType);
            });
        }

        // ── FooterMenuItems ────────────────────────────────────────────────────

        [Fact]
        public void FooterMenuItems_Contains_SettingsItem()
        {
            RunOnSta(() =>
            {
                var vm = new MainWindowViewModel(CreateSetting());

                var items = vm.FooterMenuItems.OfType<NavigationViewItem>().ToList();
                Assert.Contains(items, item => item.Content?.ToString() == "Settings");
            });
        }

        [Fact]
        public void FooterMenuItems_SettingsItem_TargetPage_IsSettingsPage()
        {
            RunOnSta(() =>
            {
                var vm = new MainWindowViewModel(CreateSetting());

                var settings = vm.FooterMenuItems.OfType<NavigationViewItem>()
                    .FirstOrDefault(item => item.Content?.ToString() == "Settings");

                Assert.Equal(typeof(SettingsPage), settings?.TargetPageType);
            });
        }

        // ── TrayMenuItems ─────────────────────────────────────────────────────

        [Fact]
        public void TrayMenuItems_Contains_HomeItem()
        {
            RunOnSta(() =>
            {
                var vm = new MainWindowViewModel(CreateSetting());

                Assert.Contains(vm.TrayMenuItems, item => item.Header?.ToString() == "Home");
            });
        }

        // ── WindowClosingCommand ──────────────────────────────────────────────

        [Fact]
        public void WindowClosingCommand_Execute_SavesConfigToFile()
        {
            var path = TempPath("save_test.json");
            var setting = new Setting<AppConfig>(path, onLoad: false);

            RunOnSta(() =>
            {
                var vm = new MainWindowViewModel(setting);
                vm.WindowClosingCommand.Execute(null);
            });

            Assert.True(File.Exists(path));
        }

        [Fact]
        public void WindowClosingCommand_Execute_WithCancelEventArgs_SavesConfig()
        {
            var path = TempPath("cancel_args_test.json");
            var setting = new Setting<AppConfig>(path, onLoad: false);

            RunOnSta(() =>
            {
                var vm = new MainWindowViewModel(setting);
                vm.WindowClosingCommand.Execute(new CancelEventArgs());
            });

            Assert.True(File.Exists(path));
        }
    }
}
