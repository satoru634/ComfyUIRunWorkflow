using System.Globalization;
using System.Runtime.ExceptionServices;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using ComfyUILibs.Models;
using ComfyUIRunWorkflow.Helpers;
using ComfyUIRunWorkflow.Models;
using ComfyUIRunWorkflow.ViewModels.Pages;

namespace ComfyUIRunWorkflowTests.ViewModels.Pages
{
    [Collection("Culture")]
    public class QueueJobViewModelTests : IDisposable
    {
        private readonly CultureInfo _originalCulture;

        public QueueJobViewModelTests()
        {
            _originalCulture = LocalizationManager.Instance.CurrentCulture;
            LocalizationManager.Instance.CurrentCulture = new CultureInfo("ja");
        }

        public void Dispose() => LocalizationManager.Instance.CurrentCulture = _originalCulture;

        private static WorkflowConfig CreateMultiWorkflowConfig() => new()
        {
            ComfyuiUrl = "http://127.0.0.1:8188",
            DefaultWorkflow = "sdxl",
            Workflows = new Dictionary<string, WorkflowSettings>
            {
                ["sdxl"] = new WorkflowSettings
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
                    }
                },
                ["anima"] = new WorkflowSettings
                {
                    DefaultImageSize = new ImageSize { Width = 896, Height = 1152 },
                    ImageSize = new Dictionary<string, ImageSize>
                    {
                        ["vertical"] = new ImageSize { Width = 896, Height = 1152 },
                        ["horizontal"] = new ImageSize { Width = 1152, Height = 896 },
                        ["square"] = new ImageSize { Width = 1024, Height = 1024 },
                    },
                    Loras = new Dictionary<string, LoraEntry>()
                }
            }
        };

        // ── コンストラクター既定値 ─────────────────────────────────────────────

        [Fact]
        public void Constructor_DefaultsAreExpected()
        {
            var job = new QueueJobViewModel();

            Assert.Equal("", job.WorkflowName);
            Assert.Equal(QueueJobStatus.Pending, job.Status);
            Assert.Empty(job.LoraSlots);
            Assert.Equal(1, job.BatchCount);
            Assert.Equal("", job.FilenamePrefix);
            Assert.False(job.IsCustomSize);
            Assert.Equal(832, job.CustomWidth);
            Assert.Equal(1216, job.CustomHeight);
            Assert.False(job.HasLastResult);
            Assert.Null(job.LastResult);
        }

        [Fact]
        public void StatusLabel_Pending_ReturnsJapaneseLabel()
        {
            var job = new QueueJobViewModel();
            Assert.Equal("未実行", job.StatusLabel);
        }

        [Theory]
        [InlineData(QueueJobStatus.Running, "実行中")]
        [InlineData(QueueJobStatus.Success, "成功")]
        [InlineData(QueueJobStatus.Error, "失敗")]
        [InlineData(QueueJobStatus.Cancelled, "中断")]
        public void StatusLabel_ReflectsStatus(QueueJobStatus status, string expected)
        {
            var job = new QueueJobViewModel { Status = status };
            Assert.Equal(expected, job.StatusLabel);
        }

        [Fact]
        public void LastResult_Set_HasLastResultBecomesTrue()
        {
            var job = new QueueJobViewModel
            {
                LastResult = new WorkflowResult { Status = "success", Timestamp = "", Parameters = new WorkflowParameters() }
            };

            Assert.True(job.HasLastResult);
        }

        // ── 画像サイズ向き ─────────────────────────────────────────────────────

        [Fact]
        public void SelectedSizeOption_SetCustom_SetsIsCustomSizeTrue()
        {
            var job = new QueueJobViewModel();
            job.SelectedSizeOption = "custom";
            Assert.True(job.IsCustomSize);
            Assert.Equal("custom", job.SelectedSizeOption);
        }

        [Fact]
        public void SelectedSizeOption_SetHorizontal_ChangesOrientation()
        {
            var job = new QueueJobViewModel();
            job.SelectedSizeOption = "horizontal";
            Assert.Equal("horizontal", job.ImageSizeOrientation);
            Assert.False(job.IsCustomSize);
        }

        // ── ApplyWorkflowConfig ───────────────────────────────────────────────

        [Fact]
        public void ApplyWorkflowConfig_UnknownWorkflow_UsesFallbackSizeOptions()
        {
            var job = new QueueJobViewModel { WorkflowName = "unknown" };
            job.ApplyWorkflowConfig(CreateMultiWorkflowConfig());

            Assert.Empty(job.AvailableLoras);
            Assert.Equal(4, job.SizeLabelList.ItemList.Count);
            Assert.DoesNotContain("×", job.SizeLabelList.ItemList.Single(o => o.Key == "vertical").Label);
        }

        [Fact]
        public void ApplyWorkflowConfig_KnownWorkflow_PopulatesAvailableLoras()
        {
            var job = new QueueJobViewModel { WorkflowName = "sdxl" };
            job.ApplyWorkflowConfig(CreateMultiWorkflowConfig());

            Assert.Contains("my_lora", job.AvailableLoras);
        }

        [Fact]
        public void ApplyWorkflowConfig_KnownWorkflow_SizeLabelListHasDimensions()
        {
            var job = new QueueJobViewModel { WorkflowName = "sdxl" };
            job.ApplyWorkflowConfig(CreateMultiWorkflowConfig());

            var vertical = job.SizeLabelList.ItemList.Single(o => o.Key == "vertical");
            Assert.Contains("832", vertical.Label);
        }

        [Fact]
        public void WorkflowNameChanged_TwiceAfterConfigApplied_SizeLabelListDoesNotAccumulate()
        {
            var job = new QueueJobViewModel();
            job.ApplyWorkflowConfig(CreateMultiWorkflowConfig());

            job.WorkflowName = "anima";
            job.WorkflowName = "sdxl";

            Assert.Equal(4, job.SizeLabelList.ItemList.Count);
        }

        [Fact]
        public void WorkflowNameChanged_LoraNotInNewWorkflow_ResetsSlot()
        {
            var job = new QueueJobViewModel();
            job.ApplyWorkflowConfig(CreateMultiWorkflowConfig());
            job.WorkflowName = "sdxl";
            job.LoraSlots.Add(new LoraSlot { SelectedLora = "my_lora" });

            job.WorkflowName = "anima";

            Assert.Equal("", job.LoraSlots[0].SelectedLora);
        }

        [Fact]
        public void WorkflowNameChanged_ActualSwitch_ResetsSizeSelectionToPreset()
        {
            var job = new QueueJobViewModel();
            job.ApplyWorkflowConfig(CreateMultiWorkflowConfig());
            job.WorkflowName = "sdxl";
            job.SelectedSizeOption = "custom";

            job.WorkflowName = "anima";

            Assert.False(job.IsCustomSize);
            Assert.Equal("vertical", job.SelectedSizeOption);
        }

        [Fact]
        public void ApplyWorkflowConfig_CalledAgainForSameWorkflow_PreservesCustomSizeSelection()
        {
            var job = new QueueJobViewModel { WorkflowName = "sdxl" };
            job.ApplyWorkflowConfig(CreateMultiWorkflowConfig());
            job.SelectedSizeOption = "custom";
            job.CustomWidth = 999;
            job.CustomHeight = 777;

            // ページ再訪問等で config が再適用されても、ユーザーが選択済みのカスタムサイズは保持される
            job.ApplyWorkflowConfig(CreateMultiWorkflowConfig());

            Assert.True(job.IsCustomSize);
            Assert.Equal("custom", job.SelectedSizeOption);
            Assert.Equal(999, job.CustomWidth);
            Assert.Equal(777, job.CustomHeight);
        }

        [Fact]
        public void ApplyWorkflowConfig_CalledAgainForSameWorkflow_PreservesPresetOrientation()
        {
            var job = new QueueJobViewModel { WorkflowName = "sdxl" };
            job.ApplyWorkflowConfig(CreateMultiWorkflowConfig());
            job.SelectedSizeOption = "horizontal";

            job.ApplyWorkflowConfig(CreateMultiWorkflowConfig());

            Assert.Equal("horizontal", job.SelectedSizeOption);
        }

        /// <summary>
        /// QueuePage.xaml と同じ ComboBox バインディング（ItemsSource=SizeLabelList.ItemList,
        /// SelectedValue={Binding SelectedSizeOption, TwoWay}, SelectedValuePath=Key）を実際に構築し、
        /// ApplyWorkflowConfig の再実行（ページ再訪問を想定）で選択値が失われないかを検証する。
        /// SizeLabelList.Init() 内部の ItemList.Clear() が ComboBox の SelectedValue を一時的に
        /// 未選択にし、TwoWay バインディング経由で ViewModel 側のプロパティに null を書き戻してしまう
        /// 問題は、単体で ViewModel のプロパティを直接操作するテストでは検出できないため、実際の
        /// WPF コントロールを使って検証する。
        /// </summary>
        [Fact]
        public void RealComboBoxBinding_ApplyWorkflowConfigAgain_DoesNotClearSelection()
        {
            Exception? caught = null;
            var thread = new Thread(() =>
            {
                try
                {
                    var job = new QueueJobViewModel { WorkflowName = "sdxl" };
                    job.ApplyWorkflowConfig(CreateMultiWorkflowConfig());
                    job.SelectedSizeOption = "horizontal";

                    var comboBox = new ComboBox
                    {
                        DisplayMemberPath = "Label",
                        SelectedValuePath = "Key",
                    };
                    BindingOperations.SetBinding(
                        comboBox,
                        ItemsControl.ItemsSourceProperty,
                        new Binding("SizeLabelList.ItemList") { Source = job });
                    BindingOperations.SetBinding(
                        comboBox,
                        Selector.SelectedValueProperty,
                        new Binding("SelectedSizeOption") { Source = job, Mode = BindingMode.TwoWay });

                    Assert.Equal("horizontal", job.SelectedSizeOption);

                    // ページ再訪問時に QueueViewModel.TryLoadConfig() が全ジョブへ呼び出す処理を再現する
                    job.ApplyWorkflowConfig(CreateMultiWorkflowConfig());

                    Assert.Equal("horizontal", job.SelectedSizeOption);
                    Assert.Equal("horizontal", comboBox.SelectedValue);
                }
                catch (Exception ex)
                {
                    caught = ex;
                }
            });
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join();
            if (caught is not null)
                ExceptionDispatchInfo.Capture(caught).Throw();
        }

        /// <summary>
        /// カスタムサイズ選択時（Key="custom"）でも、上記と同じ ComboBox 巻き添え上書きが
        /// 発生しないことを検証する。
        /// </summary>
        [Fact]
        public void RealComboBoxBinding_ApplyWorkflowConfigAgain_PreservesCustomSizeSelection()
        {
            Exception? caught = null;
            var thread = new Thread(() =>
            {
                try
                {
                    var job = new QueueJobViewModel { WorkflowName = "sdxl" };
                    job.ApplyWorkflowConfig(CreateMultiWorkflowConfig());
                    job.SelectedSizeOption = "custom";
                    job.CustomWidth = 999;
                    job.CustomHeight = 777;

                    var comboBox = new ComboBox
                    {
                        DisplayMemberPath = "Label",
                        SelectedValuePath = "Key",
                    };
                    BindingOperations.SetBinding(
                        comboBox,
                        ItemsControl.ItemsSourceProperty,
                        new Binding("SizeLabelList.ItemList") { Source = job });
                    BindingOperations.SetBinding(
                        comboBox,
                        Selector.SelectedValueProperty,
                        new Binding("SelectedSizeOption") { Source = job, Mode = BindingMode.TwoWay });

                    Assert.Equal("custom", job.SelectedSizeOption);

                    job.ApplyWorkflowConfig(CreateMultiWorkflowConfig());

                    Assert.True(job.IsCustomSize);
                    Assert.Equal("custom", job.SelectedSizeOption);
                    Assert.Equal("custom", comboBox.SelectedValue);
                    Assert.Equal(999, job.CustomWidth);
                    Assert.Equal(777, job.CustomHeight);
                }
                catch (Exception ex)
                {
                    caught = ex;
                }
            });
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join();
            if (caught is not null)
                ExceptionDispatchInfo.Capture(caught).Throw();
        }

        [Fact]
        public void FromData_ThenApplyWorkflowConfig_PreservesRestoredCustomSize()
        {
            var data = new QueueJobData
            {
                WorkflowName = "sdxl",
                IsCustomSize = true,
                CustomWidth = 999,
                CustomHeight = 777,
            };

            var job = QueueJobViewModel.FromData(data);
            job.ApplyWorkflowConfig(CreateMultiWorkflowConfig());

            Assert.True(job.IsCustomSize);
            Assert.Equal(999, job.CustomWidth);
            Assert.Equal(777, job.CustomHeight);
        }

        // ── LoRA 操作 ─────────────────────────────────────────────────────────

        [Fact]
        public void AddLoraCommand_Execute_AddsSlot()
        {
            var job = new QueueJobViewModel();
            job.AddLoraCommand.Execute(null);
            Assert.Single(job.LoraSlots);
        }

        [Fact]
        public void AddLoraCommand_AtMaxFour_DoesNotAddMore()
        {
            var job = new QueueJobViewModel();
            for (int i = 0; i < 5; i++)
                job.AddLoraCommand.Execute(null);
            Assert.Equal(4, job.LoraSlots.Count);
        }

        [Fact]
        public void RemoveLoraCommand_Execute_RemovesSlot()
        {
            var job = new QueueJobViewModel();
            job.AddLoraCommand.Execute(null);
            var slot = job.LoraSlots[0];

            job.RemoveLoraCommand.Execute(slot);

            Assert.Empty(job.LoraSlots);
        }

        // ── ResolveImageSize ─────────────────────────────────────────────────

        [Fact]
        public void ResolveImageSize_CustomSize_ReturnsCustomDimensions()
        {
            var job = new QueueJobViewModel { IsCustomSize = true, CustomWidth = 640, CustomHeight = 960 };
            var size = job.ResolveImageSize();

            Assert.NotNull(size);
            Assert.Equal(640, size!.Width);
            Assert.Equal(960, size.Height);
        }

        [Fact]
        public void ResolveImageSize_PresetOrientation_ReturnsPresetDimensions()
        {
            var job = new QueueJobViewModel { WorkflowName = "sdxl" };
            job.ApplyWorkflowConfig(CreateMultiWorkflowConfig());
            job.ImageSizeOrientation = "horizontal";

            var size = job.ResolveImageSize();

            Assert.NotNull(size);
            Assert.Equal(1216, size!.Width);
            Assert.Equal(832, size.Height);
        }

        [Fact]
        public void ResolveImageSize_UnknownOrientation_ReturnsNull()
        {
            var job = new QueueJobViewModel();
            Assert.Null(job.ResolveImageSize());
        }

        // ── ToData / FromData ────────────────────────────────────────────────

        [Fact]
        public void ToData_ReflectsCurrentState()
        {
            var job = new QueueJobViewModel
            {
                WorkflowName = "sdxl",
                PositivePrompt = "1girl",
                NegativePrompt = "bad quality",
                FilenamePrefix = "my_batch",
                BatchCount = 3,
                IsCustomSize = true,
                CustomWidth = 640,
                CustomHeight = 960,
            };
            job.LoraSlots.Add(new LoraSlot { SelectedLora = "my_lora" });
            job.LoraSlots.Add(new LoraSlot { SelectedLora = "" });

            var data = job.ToData();

            Assert.Equal("sdxl", data.WorkflowName);
            Assert.Equal("1girl", data.PositivePrompt);
            Assert.Equal("bad quality", data.NegativePrompt);
            Assert.Equal("my_batch", data.FilenamePrefix);
            Assert.Equal(3, data.BatchCount);
            Assert.True(data.IsCustomSize);
            Assert.Equal(640, data.CustomWidth);
            Assert.Equal(960, data.CustomHeight);
            Assert.Single(data.LoraFiles);
            Assert.Equal("my_lora", data.LoraFiles[0]);
        }

        [Fact]
        public void FromData_RestoresFieldsAndLoraSlots()
        {
            var data = new QueueJobData
            {
                WorkflowName = "sdxl",
                PositivePrompt = "1girl",
                NegativePrompt = "bad quality",
                FilenamePrefix = "my_batch",
                LoraFiles = new List<string> { "my_lora" },
                ImageSizeOrientation = "horizontal",
                BatchCount = 5,
            };

            var job = QueueJobViewModel.FromData(data);

            Assert.Equal("sdxl", job.WorkflowName);
            Assert.Equal("1girl", job.PositivePrompt);
            Assert.Equal("bad quality", job.NegativePrompt);
            Assert.Equal("my_batch", job.FilenamePrefix);
            Assert.Equal("horizontal", job.ImageSizeOrientation);
            Assert.Equal(5, job.BatchCount);
            Assert.Single(job.LoraSlots);
            Assert.Equal("my_lora", job.LoraSlots[0].SelectedLora);
        }

        [Fact]
        public void FromData_StatusIsPending()
        {
            var job = QueueJobViewModel.FromData(new QueueJobData());
            Assert.Equal(QueueJobStatus.Pending, job.Status);
        }
    }
}
