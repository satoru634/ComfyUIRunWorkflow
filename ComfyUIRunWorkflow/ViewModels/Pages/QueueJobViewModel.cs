using ComfyUILibs.Models;
using ComfyUILibs.Ui;
using ComfyUIRunWorkflow.Helpers;
using ComfyUIRunWorkflow.Models;
using ComfyUIRunWorkflow.Services;
using System.Collections.ObjectModel;

namespace ComfyUIRunWorkflow.ViewModels.Pages
{
    /// <summary>
    /// QueuePage の1ジョブ分の編集状態・実行状態を保持する ViewModel。
    /// DashboardViewModel の「ワークフロー選択 → LoRA/画像サイズ一覧更新」ロジックを踏襲する。
    /// </summary>
    public partial class QueueJobViewModel : ObservableObject
    {
        /// <summary>使用するワークフロー名。</summary>
        [ObservableProperty]
        private string _workflowName = "";

        /// <summary>選択中ワークフローで使用可能な LoRA 論理名の一覧。</summary>
        [ObservableProperty]
        private List<string> _availableLoras = new();

        /// <summary>画像サイズ選択コンボボックスに表示する項目（キー＋表示ラベル）のリスト。</summary>
        [ObservableProperty]
        private UIItemBaseModel<SizeOption> _sizeLabelList = new();

        /// <summary>ポジティブプロンプト。</summary>
        [ObservableProperty]
        private string _positivePrompt = "";

        /// <summary>ネガティブプロンプト。</summary>
        [ObservableProperty]
        private string _negativePrompt = "";

        /// <summary>
        /// 出力ファイル名のプレフィックス。空文字の場合はワークフローに記述された値をそのまま使用する。
        /// </summary>
        [ObservableProperty]
        private string _filenamePrefix = "";

        /// <summary>選択中の画像サイズ向き（"vertical" / "horizontal" / "square"）。カスタムサイズ選択時は参照されない。</summary>
        [ObservableProperty]
        private string _imageSizeOrientation = "vertical";

        /// <summary>カスタムサイズ入力モードかどうか。</summary>
        [ObservableProperty]
        private bool _isCustomSize = false;

        /// <summary>カスタムサイズの幅（ピクセル）。</summary>
        [ObservableProperty]
        private int _customWidth = 832;

        /// <summary>カスタムサイズの高さ（ピクセル）。</summary>
        [ObservableProperty]
        private int _customHeight = 1216;

        /// <summary>バッチ数（1〜10）。</summary>
        [ObservableProperty]
        private int _batchCount = 1;

        /// <summary>このジョブの実行状態。</summary>
        [ObservableProperty]
        private QueueJobStatus _status = QueueJobStatus.Pending;

        /// <summary>状態メッセージ（実行中はバッチ進捗、失敗時はエラーメッセージ）。</summary>
        [ObservableProperty]
        private string _statusMessage = "";

        /// <summary>直近の実行結果。詳細ダイアログ表示に使用する。未実行の場合は null。</summary>
        [ObservableProperty]
        private WorkflowResult? _lastResult;

        /// <summary>Status の現在の言語での表示ラベル。</summary>
        public string StatusLabel => Status switch
        {
            QueueJobStatus.Pending => LocalizationManager.Instance["Queue_StatusPending"],
            QueueJobStatus.Running => LocalizationManager.Instance["Queue_StatusRunning"],
            QueueJobStatus.Success => LocalizationManager.Instance["Queue_StatusSuccess"],
            QueueJobStatus.Error => LocalizationManager.Instance["Queue_StatusError"],
            QueueJobStatus.Cancelled => LocalizationManager.Instance["Queue_StatusCancelled"],
            _ => Status.ToString(),
        };

        /// <summary>Status が変わったとき、派生プロパティ StatusLabel を通知する。</summary>
        partial void OnStatusChanged(QueueJobStatus value) => OnPropertyChanged(nameof(StatusLabel));

        /// <summary>実行結果があるか（「詳細を見る」ボタンの活性状態に使用）。</summary>
        public bool HasLastResult => LastResult != null;

        /// <summary>LastResult が変わったとき、派生プロパティ HasLastResult を通知する。</summary>
        partial void OnLastResultChanged(WorkflowResult? value) => OnPropertyChanged(nameof(HasLastResult));

        /// <summary>ユーザーが追加した LoRA 選択スロット（最大 4 個）。</summary>
        public ObservableCollection<LoraSlot> LoraSlots { get; } = new();

        /// <summary>
        /// 画像サイズ選択コンボボックスの選択値（"vertical" / "horizontal" / "square" / "custom"）。
        /// ImageSizeOrientation と IsCustomSize を合成した値。
        /// </summary>
        public string SelectedSizeOption
        {
            get => IsCustomSize ? "custom" : ImageSizeOrientation;
            set
            {
                if (value == "custom")
                {
                    IsCustomSize = true;
                }
                else
                {
                    IsCustomSize = false;
                    ImageSizeOrientation = value;
                }
            }
        }

        private WorkflowConfig? _loadedConfig;
        private Dictionary<string, ImageSize> _presetSizes = new();

        /// <summary>
        /// ワークフロー選択が変わったとき、LoRA 一覧と画像サイズ選択肢を更新する。
        /// ワークフロー種別が実際に変わった場合のみ、画像サイズ選択をプリセット既定値にリセットする
        /// （異なるワークフローでは有効なプリセットサイズが異なりうるため）。
        /// </summary>
        partial void OnWorkflowNameChanged(string value) => RefreshForWorkflow(resetSizeSelection: true);

        /// <summary>ImageSizeOrientation が変わったとき、ComboBox 向けの derived プロパティを通知する。</summary>
        partial void OnImageSizeOrientationChanged(string value) => OnPropertyChanged(nameof(SelectedSizeOption));

        /// <summary>IsCustomSize が変わったとき、ComboBox 向けの derived プロパティを通知する。</summary>
        partial void OnIsCustomSizeChanged(bool value) => OnPropertyChanged(nameof(SelectedSizeOption));

        /// <summary>
        /// workflow_config.json の内容を適用し、現在の WorkflowName に基づいて LoRA 一覧・画像サイズ選択肢を再構築する。
        /// QueueViewModel がページ遷移のたびに全ジョブへ呼び出す想定のため、ワークフロー名自体は変わっていない
        /// （config の再読み込みだけの）ケースがほとんどであり、その場合はユーザーが選択済みの画像サイズ
        /// （プリセット/カスタムの別・向き・カスタム幅高さ）を保持する。
        /// </summary>
        public void ApplyWorkflowConfig(WorkflowConfig? config)
        {
            _loadedConfig = config;
            RefreshForWorkflow(resetSizeSelection: false);
        }

        /// <summary>
        /// LoRA 一覧・画像サイズ選択肢を再構築する。
        /// </summary>
        /// <param name="resetSizeSelection">
        /// true の場合、画像サイズ選択をこのワークフローのプリセット既定値（先頭のオプション）にリセットする。
        /// ワークフロー種別が実際に切り替わったときのみ true にする（異なるワークフローでは有効なプリセット
        /// サイズが異なりうるため）。config の再読み込みだけの場合は false にし、ユーザーの選択を保持する。
        /// </param>
        private void RefreshForWorkflow(bool resetSizeSelection)
        {
            if (_loadedConfig?.Workflows == null || !_loadedConfig.Workflows.TryGetValue(WorkflowName, out var ws))
            {
                AvailableLoras = new List<string>();
                var (fallbackOptions, fallbackPresetSizes) = WorkflowSizeOptionBuilder.Build(null);
                _presetSizes = fallbackPresetSizes;
                SizeLabelList.Init(fallbackOptions, fallbackOptions[0]);
                return;
            }

            AvailableLoras = ws.Loras?.Keys.ToList() ?? new List<string>();

            var (options, presetSizes) = WorkflowSizeOptionBuilder.Build(ws);
            _presetSizes = presetSizes;
            SizeLabelList.Init(options, options[0]);

            if (resetSizeSelection)
                SelectedSizeOption = options[0].Key;

            foreach (var slot in LoraSlots)
            {
                if (!string.IsNullOrEmpty(slot.SelectedLora) && !AvailableLoras.Contains(slot.SelectedLora))
                    slot.SelectedLora = "";
            }
        }

        /// <summary>現在の選択状態から実行に使用する画像サイズを解決する。プリセットが見つからない場合は null（ワークフロー既定値を使用）。</summary>
        public ImageSize? ResolveImageSize()
        {
            if (IsCustomSize)
                return new ImageSize { Width = CustomWidth, Height = CustomHeight };

            if (_presetSizes.TryGetValue(ImageSizeOrientation, out var preset))
                return new ImageSize { Width = preset.Width, Height = preset.Height };

            return null;
        }

        /// <summary>LoRA スロットを 1 つ追加する（最大 4 個まで）。</summary>
        [RelayCommand]
        private void AddLora()
        {
            if (LoraSlots.Count < 4)
                LoraSlots.Add(new LoraSlot());
        }

        /// <summary>指定した LoRA スロットを削除する。</summary>
        [RelayCommand]
        private void RemoveLora(LoraSlot slot) => LoraSlots.Remove(slot);

        /// <summary>永続化用の <see cref="QueueJobData"/> に変換する。</summary>
        public QueueJobData ToData() => new()
        {
            WorkflowName = WorkflowName,
            PositivePrompt = PositivePrompt,
            NegativePrompt = NegativePrompt,
            FilenamePrefix = FilenamePrefix,
            LoraFiles = LoraSlots
                .Where(s => !string.IsNullOrWhiteSpace(s.SelectedLora))
                .Select(s => s.SelectedLora)
                .ToList(),
            ImageSizeOrientation = ImageSizeOrientation,
            IsCustomSize = IsCustomSize,
            CustomWidth = CustomWidth,
            CustomHeight = CustomHeight,
            BatchCount = BatchCount,
        };

        /// <summary>永続化データから復元する。実行状態（Status/LastResult）は含まれないため Pending のままになる。</summary>
        public static QueueJobViewModel FromData(QueueJobData data)
        {
            var vm = new QueueJobViewModel
            {
                PositivePrompt = data.PositivePrompt,
                NegativePrompt = data.NegativePrompt,
                FilenamePrefix = data.FilenamePrefix,
                ImageSizeOrientation = data.ImageSizeOrientation,
                IsCustomSize = data.IsCustomSize,
                CustomWidth = data.CustomWidth,
                CustomHeight = data.CustomHeight,
                BatchCount = data.BatchCount,
            };

            foreach (var lora in data.LoraFiles)
                vm.LoraSlots.Add(new LoraSlot { SelectedLora = lora });

            // WorkflowName の設定は LoraSlots 復元後に行う（RefreshForWorkflow 内の LoRA 妥当性チェックのため）
            vm.WorkflowName = data.WorkflowName;

            return vm;
        }
    }
}
