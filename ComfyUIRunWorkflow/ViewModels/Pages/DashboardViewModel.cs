using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json;
using System.Text.Encodings.Web;
using System.Text.Unicode;
using ComfyUILibs.Common;
using ComfyUILibs.Exceptions;
using ComfyUILibs.Models;
using ComfyUILibs.Services;
using ComfyUIRunWorkflow.Models;
using Wpf.Ui.Abstractions.Controls;

namespace ComfyUIRunWorkflow.ViewModels.Pages
{
    /// <summary>
    /// ダッシュボードページの ViewModel。ワークフロー実行 UI を担当する。
    /// </summary>
    public partial class DashboardViewModel : ObservableObject, INavigationAware
    {
        /// <summary>アプリケーション設定。</summary>
        public Setting<AppConfig> Config { get; }

        // ── config 読み込み状態 ───────────────────────────────────────────────

        /// <summary>config.json から読み込んだワークフロー名の一覧。</summary>
        [ObservableProperty]
        private List<string> _workflowNames = new();

        /// <summary>現在選択中のワークフロー名。</summary>
        [ObservableProperty]
        private string _selectedWorkflow = "";

        /// <summary>選択中ワークフローで使用可能な LoRA 論理名の一覧。</summary>
        [ObservableProperty]
        private List<string> _availableLoras = new();

        /// <summary>config 読み込みの状態・エラーメッセージ。</summary>
        [ObservableProperty]
        private string _configStatus = "";

        /// <summary>config が正常に読み込まれているか。</summary>
        [ObservableProperty]
        private bool _isConfigLoaded = false;

        // ── プリセット画像サイズラベル ────────────────────────────────────────

        /// <summary>vertical プリセットのサイズ表示ラベル（例: "vertical (832×1216)"）。</summary>
        [ObservableProperty]
        private string _verticalLabel = "vertical";

        /// <summary>horizontal プリセットのサイズ表示ラベル。</summary>
        [ObservableProperty]
        private string _horizontalLabel = "horizontal";

        /// <summary>square プリセットのサイズ表示ラベル。</summary>
        [ObservableProperty]
        private string _squareLabel = "square";

        // ── プロンプト ────────────────────────────────────────────────────────

        /// <summary>ポジティブプロンプト。</summary>
        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(RunWorkflowCommand))]
        private string _positivePrompt = "";

        /// <summary>ネガティブプロンプト。</summary>
        [ObservableProperty]
        private string _negativePrompt = "";

        // ── 画像サイズ ────────────────────────────────────────────────────────

        /// <summary>
        /// 選択中の画像サイズ向き（"vertical" / "horizontal" / "square"）。
        /// カスタムサイズ選択時は参照されない。
        /// </summary>
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

        /// <summary>vertical ラジオボタンの選択状態。</summary>
        public bool IsVertical
        {
            get => ImageSizeOrientation == "vertical";
            set { if (value) ImageSizeOrientation = "vertical"; }
        }

        /// <summary>horizontal ラジオボタンの選択状態。</summary>
        public bool IsHorizontal
        {
            get => ImageSizeOrientation == "horizontal";
            set { if (value) ImageSizeOrientation = "horizontal"; }
        }

        /// <summary>square ラジオボタンの選択状態。</summary>
        public bool IsSquare
        {
            get => ImageSizeOrientation == "square";
            set { if (value) ImageSizeOrientation = "square"; }
        }

        // ── LoRA ──────────────────────────────────────────────────────────────

        /// <summary>ユーザーが追加した LoRA 選択スロット（最大 4 個）。</summary>
        public ObservableCollection<LoraSlot> LoraSlots { get; } = new();

        // ── 実行状態 ─────────────────────────────────────────────────────────

        /// <summary>ワークフローを実行中かどうか。</summary>
        [ObservableProperty]
        private bool _isRunning = false;

        /// <summary>最後の実行結果メッセージ。</summary>
        [ObservableProperty]
        private string _statusMessage = "";

        /// <summary>最後の実行が成功したか（ステータス表示の色分けに使用）。</summary>
        [ObservableProperty]
        private bool _isSuccess = false;

        /// <summary>最後の実行がエラーだったか（ステータス表示の色分けに使用）。</summary>
        [ObservableProperty]
        private bool _isError = false;

        // ── 内部状態 ─────────────────────────────────────────────────────────

        private WorkflowConfig? _loadedConfig;
        private Dictionary<string, ImageSize> _presetSizes = new();

        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            WriteIndented = true,
            Encoder = JavaScriptEncoder.Create(UnicodeRanges.All),
        };

        // ─────────────────────────────────────────────────────────────────────

        /// <summary>DI コンテナから設定を受け取って初期化する。</summary>
        public DashboardViewModel(Setting<AppConfig> config)
        {
            Config = config;
        }

        // ── INavigationAware ─────────────────────────────────────────────────

        /// <summary>ページへ遷移するたびに config.json を再読み込みする。</summary>
        public Task OnNavigatedToAsync()
        {
            TryLoadConfig();
            return Task.CompletedTask;
        }

        /// <summary>ページから離れるときは何もしない。</summary>
        public Task OnNavigatedFromAsync() => Task.CompletedTask;

        // ── config 読み込み ───────────────────────────────────────────────────

        /// <summary>
        /// 設定ページで指定された ConfigPath から config.json を読み込む。
        /// 失敗した場合は ConfigStatus にエラーメッセージを設定する。
        /// </summary>
        private void TryLoadConfig()
        {
            var path = Config.Data.ConfigPath;
            if (string.IsNullOrWhiteSpace(path))
            {
                _loadedConfig = null;
                IsConfigLoaded = false;
                ConfigStatus = "設定ページで config.json のパスを指定してください";
                WorkflowNames = new List<string>();
                SelectedWorkflow = "";
                RunWorkflowCommand.NotifyCanExecuteChanged();
                return;
            }

            try
            {
                _loadedConfig = ConfigLoader.LoadConfig(path);
                var names = _loadedConfig.Workflows!.Keys.ToList();
                WorkflowNames = names;

                // 現在の選択が有効なら維持、そうでなければデフォルトに変更
                if (!names.Contains(SelectedWorkflow))
                    SelectedWorkflow = _loadedConfig.DefaultWorkflow ?? names.FirstOrDefault() ?? "";

                IsConfigLoaded = true;
                ConfigStatus = "";
            }
            catch (ComfyUIException ex)
            {
                _loadedConfig = null;
                IsConfigLoaded = false;
                ConfigStatus = $"config.json 読み込みエラー: {ex.Message}";
                WorkflowNames = new List<string>();
                SelectedWorkflow = "";
            }

            RunWorkflowCommand.NotifyCanExecuteChanged();
        }

        /// <summary>
        /// 選択ワークフローが変わったとき、LoRA 一覧とプリセットサイズラベルを更新する。
        /// </summary>
        partial void OnSelectedWorkflowChanged(string value)
        {
            if (_loadedConfig?.Workflows == null || !_loadedConfig.Workflows.TryGetValue(value, out var ws))
            {
                AvailableLoras = new List<string>();
                _presetSizes = new Dictionary<string, ImageSize>();
                VerticalLabel = "vertical";
                HorizontalLabel = "horizontal";
                SquareLabel = "square";
                return;
            }

            AvailableLoras = ws.Loras?.Keys.ToList() ?? new List<string>();

            _presetSizes = ws.ImageSize ?? new Dictionary<string, ImageSize>();
            VerticalLabel = FormatSizeLabel("vertical");
            HorizontalLabel = FormatSizeLabel("horizontal");
            SquareLabel = FormatSizeLabel("square");

            // LoRA スロットの選択が新しいワークフローの LoRA に含まれない場合はリセット
            foreach (var slot in LoraSlots)
            {
                if (!string.IsNullOrEmpty(slot.SelectedLora) && !AvailableLoras.Contains(slot.SelectedLora))
                    slot.SelectedLora = "";
            }
        }

        private string FormatSizeLabel(string orientation)
        {
            if (_presetSizes.TryGetValue(orientation, out var size))
                return $"{orientation} ({size.Width}×{size.Height})";
            return orientation;
        }

        /// <summary>
        /// ImageSizeOrientation が変わったとき、RadioButton 向けの derived プロパティを通知する。
        /// </summary>
        partial void OnImageSizeOrientationChanged(string value)
        {
            OnPropertyChanged(nameof(IsVertical));
            OnPropertyChanged(nameof(IsHorizontal));
            OnPropertyChanged(nameof(IsSquare));
        }

        // ── LoRA 操作 ─────────────────────────────────────────────────────────

        /// <summary>LoRA スロットを 1 つ追加する（最大 4 個まで）。</summary>
        [RelayCommand]
        private void AddLora()
        {
            if (LoraSlots.Count < 4)
                LoraSlots.Add(new LoraSlot());
        }

        /// <summary>指定した LoRA スロットを削除する。</summary>
        [RelayCommand]
        private void RemoveLora(LoraSlot slot)
        {
            LoraSlots.Remove(slot);
        }

        // ── ワークフロー実行 ──────────────────────────────────────────────────

        private bool CanRun() => _loadedConfig != null && !string.IsNullOrWhiteSpace(PositivePrompt);

        /// <summary>
        /// ワークフローを ComfyUI に送信して実行する。
        /// 完了または失敗後、ResultsFolder に result_*.json を保存する。
        /// </summary>
        [RelayCommand(CanExecute = nameof(CanRun))]
        private async Task RunWorkflowAsync()
        {
            IsRunning = true;
            IsSuccess = false;
            IsError = false;
            StatusMessage = "実行中...";

            WorkflowResult result;
            WorkflowRunner? runner = null;

            try
            {
                runner = new WorkflowRunner(Config.Data.ConfigPath, SelectedWorkflow);

                var loras = LoraSlots
                    .Where(s => !string.IsNullOrWhiteSpace(s.SelectedLora))
                    .Select(s => s.SelectedLora)
                    .ToList();

                var prompts = new PromptPair
                {
                    Positive = PositivePrompt,
                    Negative = NegativePrompt
                };

                ImageSize? imageSize;
                if (IsCustomSize)
                {
                    imageSize = new ImageSize { Width = CustomWidth, Height = CustomHeight };
                }
                else if (_presetSizes.TryGetValue(ImageSizeOrientation, out var preset))
                {
                    imageSize = new ImageSize { Width = preset.Width, Height = preset.Height };
                }
                else
                {
                    imageSize = null;
                }

                var outputs = await runner.ExecuteAsync(loras, prompts, imageSize);

                result = new WorkflowResult
                {
                    Status = "success",
                    PromptId = runner.PromptId,
                    Timestamp = DateTime.Now.ToString("s"),
                    Template = runner.TemplatePath,
                    Parameters = runner.Parameters ?? new WorkflowParameters(),
                    Outputs = outputs,
                };

                StatusMessage = $"完了: {outputs.Count} 件のファイルが生成されました";
                IsSuccess = true;
            }
            catch (ComfyUIException ex)
            {
                result = new WorkflowResult
                {
                    Status = "error",
                    Timestamp = DateTime.Now.ToString("s"),
                    Template = runner?.TemplatePath,
                    Parameters = runner?.Parameters ?? new WorkflowParameters(),
                    Error = ex.Message,
                };

                StatusMessage = $"エラー: {ex.Message}";
                IsError = true;
            }
            catch (Exception ex)
            {
                result = new WorkflowResult
                {
                    Status = "error",
                    Timestamp = DateTime.Now.ToString("s"),
                    Parameters = new WorkflowParameters(),
                    Error = ex.Message,
                };

                StatusMessage = $"予期しないエラー: {ex.Message}";
                IsError = true;
            }
            finally
            {
                IsRunning = false;
            }

            await TrySaveResultAsync(result);
        }

        /// <summary>
        /// 実行結果を ResultsFolder に result_{timestamp}.json として保存する。
        /// ResultsFolder が未設定の場合は何もしない。
        /// </summary>
        private async Task TrySaveResultAsync(WorkflowResult result)
        {
            var folder = Config.Data.ResultsFolder;
            if (string.IsNullOrWhiteSpace(folder)) return;

            try
            {
                Directory.CreateDirectory(folder);
                var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                var outputPath = Path.Combine(folder, $"result_{timestamp}.json");
                var json = JsonSerializer.Serialize(result, _jsonOptions);
                await File.WriteAllTextAsync(outputPath, json);
            }
            catch
            {
                // 保存失敗は実行結果に影響させない
            }
        }
    }
}
