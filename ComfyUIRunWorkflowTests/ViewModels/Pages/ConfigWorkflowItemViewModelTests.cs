using ComfyUILibs.Models;
using ComfyUIRunWorkflow.ViewModels.Pages;

namespace ComfyUIRunWorkflowTests.ViewModels.Pages
{
    public class ConfigWorkflowItemViewModelTests
    {
        [Fact]
        public void CreateDefault_SetsNameAndAllSizesTo1024()
        {
            var vm = ConfigWorkflowItemViewModel.CreateDefault("new_workflow");

            Assert.Equal("new_workflow", vm.Name);
            Assert.Equal(1024, vm.DefaultWidth);
            Assert.Equal(1024, vm.DefaultHeight);
            Assert.Equal(1024, vm.VerticalWidth);
            Assert.Equal(1024, vm.VerticalHeight);
            Assert.Equal(1024, vm.HorizontalWidth);
            Assert.Equal(1024, vm.HorizontalHeight);
            Assert.Equal(1024, vm.SquareWidth);
            Assert.Equal(1024, vm.SquareHeight);
            Assert.Empty(vm.Loras);
        }

        [Fact]
        public void FromSettings_MapsAllFields()
        {
            var settings = new WorkflowSettings
            {
                DefaultImageSize = new ImageSize { Width = 832, Height = 1216 },
                ImageSize = new Dictionary<string, ImageSize>
                {
                    ["vertical"] = new ImageSize { Width = 832, Height = 1216 },
                    ["horizontal"] = new ImageSize { Width = 1216, Height = 832 },
                    ["square"] = new ImageSize { Width = 1024, Height = 1024 },
                },
                Loras = new Dictionary<string, LoraEntry>
                {
                    ["my_lora"] = new LoraEntry { File = "my_lora.safetensors", Strength = 0.8 },
                },
            };

            var vm = ConfigWorkflowItemViewModel.FromSettings("sdxl", settings);

            Assert.Equal("sdxl", vm.Name);
            Assert.Equal(832, vm.DefaultWidth);
            Assert.Equal(1216, vm.DefaultHeight);
            Assert.Equal(832, vm.VerticalWidth);
            Assert.Equal(1216, vm.VerticalHeight);
            Assert.Equal(1216, vm.HorizontalWidth);
            Assert.Equal(832, vm.HorizontalHeight);
            Assert.Equal(1024, vm.SquareWidth);
            Assert.Equal(1024, vm.SquareHeight);

            var lora = Assert.Single(vm.Loras);
            Assert.Equal("my_lora", lora.Name);
            Assert.Equal("my_lora.safetensors", lora.File);
            Assert.Equal(0.8, lora.Strength);
        }

        [Fact]
        public void ToSettings_RoundTripsFromSettings()
        {
            var settings = new WorkflowSettings
            {
                DefaultImageSize = new ImageSize { Width = 832, Height = 1216 },
                ImageSize = new Dictionary<string, ImageSize>
                {
                    ["vertical"] = new ImageSize { Width = 832, Height = 1216 },
                    ["horizontal"] = new ImageSize { Width = 1216, Height = 832 },
                    ["square"] = new ImageSize { Width = 1024, Height = 1024 },
                },
                Loras = new Dictionary<string, LoraEntry>
                {
                    ["my_lora"] = new LoraEntry { File = "my_lora.safetensors", Strength = 0.8 },
                },
            };
            var vm = ConfigWorkflowItemViewModel.FromSettings("sdxl", settings);

            var result = vm.ToSettings();

            Assert.Equal(832, result.DefaultImageSize!.Width);
            Assert.Equal(1216, result.DefaultImageSize.Height);
            Assert.Equal(832, result.ImageSize!["vertical"].Width);
            Assert.Equal(1216, result.ImageSize["horizontal"].Width);
            Assert.Equal(1024, result.ImageSize["square"].Width);
            Assert.Equal("my_lora.safetensors", result.Loras!["my_lora"].File);
            Assert.Equal(0.8, result.Loras["my_lora"].Strength);
        }

        [Fact]
        public void AddLoraCommand_Execute_AddsEmptyLoraItem()
        {
            var vm = ConfigWorkflowItemViewModel.CreateDefault("sdxl");

            vm.AddLoraCommand.Execute(null);

            var lora = Assert.Single(vm.Loras);
            Assert.Equal("", lora.Name);
        }

        [Fact]
        public void RemoveLoraCommand_Execute_RemovesItem()
        {
            var vm = ConfigWorkflowItemViewModel.CreateDefault("sdxl");
            vm.AddLoraCommand.Execute(null);
            var lora = vm.Loras[0];

            vm.RemoveLoraCommand.Execute(lora);

            Assert.Empty(vm.Loras);
        }

        // ── LoRA 一覧のソート ─────────────────────────────────────────────────

        private static ConfigWorkflowItemViewModel CreateVmWithUnsortedLoras()
        {
            var vm = ConfigWorkflowItemViewModel.CreateDefault("sdxl");
            vm.Loras.Add(new ConfigLoraItemViewModel { Name = "charlie" });
            vm.Loras.Add(new ConfigLoraItemViewModel { Name = "alpha" });
            vm.Loras.Add(new ConfigLoraItemViewModel { Name = "Bravo" });
            return vm;
        }

        [Fact]
        public void SortLorasAscendingCommand_Execute_SortsByNameAscendingCaseInsensitive()
        {
            var vm = CreateVmWithUnsortedLoras();

            vm.SortLorasAscendingCommand.Execute(null);

            Assert.Equal(new[] { "alpha", "Bravo", "charlie" }, vm.Loras.Select(l => l.Name));
        }

        [Fact]
        public void SortLorasDescendingCommand_Execute_SortsByNameDescendingCaseInsensitive()
        {
            var vm = CreateVmWithUnsortedLoras();

            vm.SortLorasDescendingCommand.Execute(null);

            Assert.Equal(new[] { "charlie", "Bravo", "alpha" }, vm.Loras.Select(l => l.Name));
        }

        [Fact]
        public void SortLorasAscendingCommand_Execute_PreservesItemInstances()
        {
            var vm = CreateVmWithUnsortedLoras();
            var alpha = vm.Loras.Single(l => l.Name == "alpha");

            vm.SortLorasAscendingCommand.Execute(null);

            Assert.Same(alpha, vm.Loras[0]);
        }

        [Fact]
        public void SortLorasAscendingCommand_Execute_EmptyList_DoesNotThrow()
        {
            var vm = ConfigWorkflowItemViewModel.CreateDefault("sdxl");

            vm.SortLorasAscendingCommand.Execute(null);

            Assert.Empty(vm.Loras);
        }

        // ── SelectedSizeKind / Width / Height 委譲プロパティ ─────────────────────

        [Fact]
        public void SelectedSizeKind_DefaultsToDefault()
        {
            var vm = ConfigWorkflowItemViewModel.CreateDefault("sdxl");
            Assert.Equal("default", vm.SelectedSizeKind);
        }

        [Theory]
        [InlineData("default")]
        [InlineData("vertical")]
        [InlineData("horizontal")]
        [InlineData("square")]
        public void WidthHeight_ReflectsSelectedSizeKind(string kind)
        {
            var settings = new WorkflowSettings
            {
                DefaultImageSize = new ImageSize { Width = 512, Height = 520 },
                ImageSize = new Dictionary<string, ImageSize>
                {
                    ["vertical"] = new ImageSize { Width = 832, Height = 1216 },
                    ["horizontal"] = new ImageSize { Width = 1216, Height = 832 },
                    ["square"] = new ImageSize { Width = 1024, Height = 1024 },
                },
                Loras = new Dictionary<string, LoraEntry>(),
            };
            var vm = ConfigWorkflowItemViewModel.FromSettings("sdxl", settings);

            vm.SelectedSizeKind = kind;

            var (expectedWidth, expectedHeight) = kind switch
            {
                "vertical" => (832, 1216),
                "horizontal" => (1216, 832),
                "square" => (1024, 1024),
                _ => (512, 520),
            };
            Assert.Equal(expectedWidth, vm.Width);
            Assert.Equal(expectedHeight, vm.Height);
        }

        [Fact]
        public void Width_SetWhileVerticalSelected_UpdatesVerticalWidthOnly()
        {
            var vm = ConfigWorkflowItemViewModel.CreateDefault("sdxl");
            vm.SelectedSizeKind = "vertical";

            vm.Width = 900;
            vm.Height = 1300;

            Assert.Equal(900, vm.VerticalWidth);
            Assert.Equal(1300, vm.VerticalHeight);
            Assert.Equal(1024, vm.DefaultWidth);
            Assert.Equal(1024, vm.HorizontalWidth);
            Assert.Equal(1024, vm.SquareWidth);
        }

        [Fact]
        public void SelectedSizeKindChanged_RaisesPropertyChangedForWidthAndHeight()
        {
            var vm = ConfigWorkflowItemViewModel.CreateDefault("sdxl");
            vm.SquareWidth = 777;
            var raised = new List<string?>();
            vm.PropertyChanged += (_, e) => raised.Add(e.PropertyName);

            vm.SelectedSizeKind = "square";

            Assert.Contains(nameof(ConfigWorkflowItemViewModel.Width), raised);
            Assert.Contains(nameof(ConfigWorkflowItemViewModel.Height), raised);
            Assert.Equal(777, vm.Width);
        }
    }
}
