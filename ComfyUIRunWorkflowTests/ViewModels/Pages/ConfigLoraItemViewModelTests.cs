using ComfyUIRunWorkflow.ViewModels.Pages;

namespace ComfyUIRunWorkflowTests.ViewModels.Pages
{
    public class ConfigLoraItemViewModelTests
    {
        [Fact]
        public void Constructor_DefaultValues()
        {
            var vm = new ConfigLoraItemViewModel();

            Assert.Equal("", vm.Name);
            Assert.Equal("", vm.File);
            Assert.Equal(0.8, vm.Strength);
        }

        [Fact]
        public void Properties_CanBeSet()
        {
            var vm = new ConfigLoraItemViewModel
            {
                Name = "my_lora",
                File = "my_lora.safetensors",
                Strength = 0.6,
            };

            Assert.Equal("my_lora", vm.Name);
            Assert.Equal("my_lora.safetensors", vm.File);
            Assert.Equal(0.6, vm.Strength);
        }
    }
}
