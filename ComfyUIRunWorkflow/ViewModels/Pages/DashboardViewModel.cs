using ComfyUILibs.Common;
using ComfyUILibs.Exceptions;
using ComfyUILibs.Models;
using ComfyUILibs.Services;
using ComfyUILibs.Ui;
using ComfyUIRunWorkflow.Models;
using ComfyUIRunWorkflow.Services;
using System.Collections.ObjectModel;
using System.IO;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;
using Wpf.Ui;
using Wpf.Ui.Abstractions.Controls;
using Wpf.Ui.Controls;

namespace ComfyUIRunWorkflow.ViewModels.Pages
{
    /// <summary>
    /// ダッシュボードページの ViewModel。ワークフロー実行 UI を担当する。
    /// </summary>
    public partial class DashboardViewModel : ObservableObject, INavigationAware
    {
        /// <summary>アプリケーション設定。</summary>
        public Setting<AppConfig> Config { get; }

        /// <summary>スナックバー通知サービス。ワークフロー実行中のエラー通知などに使用する。</summary>
        private readonly ISnackbarService _snackbarService;

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

        /// <summary>config が正常に読み込まれているか。</summary>
        [ObservableProperty]
        private bool _isConfigLoaded = false;

        // ── プリセット画像サイズラベル ────────────────────────────────────────

        /// <summary>画像サイズ選択コンボボックスのキー一覧（vertical / horizontal / square / custom）。</summary>
        private static readonly List<string> _sizeOptionKeys = new() { "vertical", "horizontal", "square", "custom" };

        /// <summary>画像サイズ選択コンボボックスに表示する項目（キー＋表示ラベル）のリスト。</summary>
        [ObservableProperty]
        private UIItemBaseModel<SizeOption> _sizeLabelList = new();

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

        // ── LoRA ──────────────────────────────────────────────────────────────

        /// <summary>ユーザーが追加した LoRA 選択スロット（最大 4 個）。</summary>
        public ObservableCollection<LoraSlot> LoraSlots { get; } = new();

        // ── 実行状態 ─────────────────────────────────────────────────────────

        /// <summary>ワークフローを実行中かどうか。</summary>
        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(RunWorkflowCommand))]
        private bool _isRunning = false;

        /// <summary>バッチ数（1〜10）。指定回数だけワークフローを繰り返し実行する。</summary>
        [ObservableProperty]
        private int _batchCount = 1;

        /// <summary>バッチ実行中の進捗テキスト（例: "2/5件目を実行中"）。バッチ数が1の場合は空文字。</summary>
        [ObservableProperty]
        private string _batchProgressText = "";

        /// <summary>直近の実行で生成された出力ファイルのプレビュー一覧。</summary>
        [ObservableProperty]
        private ObservableCollection<OutputFilePreview> _previewThumbnails = new();

        /// <summary>プレビューセクションを表示すべきかどうか（PreviewThumbnails が 1 件以上あるか）。</summary>
        public bool HasPreviewThumbnails => PreviewThumbnails.Count > 0;

        /// <summary>PreviewThumbnails が差し替えられたとき、派生プロパティ HasPreviewThumbnails を通知する。</summary>
        partial void OnPreviewThumbnailsChanged(ObservableCollection<OutputFilePreview> value)
            => OnPropertyChanged(nameof(HasPreviewThumbnails));

        // ── 内部状態 ─────────────────────────────────────────────────────────

        /// <summary>プレビュー画像のキャッシュ先サブフォルダ名。</summary>
        private const string PreviewCacheDirectoryName = "preview_cache";

        private readonly PreviewImageLoader _previewLoader = new();

        private WorkflowConfig? _loadedConfig;
        private Dictionary<string, ImageSize> _presetSizes = new();

        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            WriteIndented = true,
            Encoder = JavaScriptEncoder.Create(UnicodeRanges.All),
        };

        // ─────────────────────────────────────────────────────────────────────

        /// <summary>DI コンテナから設定を受け取って初期化する。</summary>
        public DashboardViewModel(Setting<AppConfig> config, ISnackbarService snackbarService)
        {
            Config = config;
            _snackbarService = snackbarService;
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
        /// 失敗した場合はスナックバーでエラーメッセージを表示する。
        /// </summary>
        private void TryLoadConfig()
        {
            var path = Config.Data.ConfigPath;
            if (string.IsNullOrWhiteSpace(path))
            {
                _loadedConfig = null;
                IsConfigLoaded = false;
                WorkflowNames = new List<string>();
                SelectedWorkflow = "";

                _snackbarService.Show(
                    "Error",
                    "設定ページで config.json のパスを指定してください。",
                    ControlAppearance.Danger,
                    new SymbolIcon(SymbolRegular.ErrorCircle24),
                    TimeSpan.FromSeconds(3.0)
                );

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
            }
            catch (ComfyUIException ex)
            {
                _loadedConfig = null;
                IsConfigLoaded = false;
                WorkflowNames = new List<string>();
                SelectedWorkflow = "";

                _snackbarService.Show(
                    "Error",
                    $"config.json 読み込みエラー: {ex.Message}",
                    ControlAppearance.Danger,
                    new SymbolIcon(SymbolRegular.ErrorCircle24),
                    TimeSpan.FromSeconds(3.0)
                );
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
                var fallbackOptions = _sizeOptionKeys.Select(key => new SizeOption(key, key)).ToList();
                SizeLabelList.Init(fallbackOptions, fallbackOptions[0]);
                return;
            }

            AvailableLoras = ws.Loras?.Keys.ToList() ?? new List<string>();

            _presetSizes = ws.ImageSize ?? new Dictionary<string, ImageSize>();
            var options = _sizeOptionKeys.Select(key => new SizeOption(key, FormatSizeLabel(key))).ToList();
            SizeLabelList.Init(options, options[0]);

            // デフォルトは最初の項目（vertical）を選択
            SelectedSizeOption = options[0].Key;

            // LoRA スロットの選択が新しいワークフローの LoRA に含まれない場合はリセット
            foreach (var slot in LoraSlots)
            {
                if (!string.IsNullOrEmpty(slot.SelectedLora) && !AvailableLoras.Contains(slot.SelectedLora))
                    slot.SelectedLora = "";
            }
        }

        /// <summary>
        /// ラベル文字列を生成する。プリセットサイズが存在する場合は "(width×height)" を付加する。
        /// </summary>
        /// <param name="orientation"></param>
        /// <returns></returns>
        private string FormatSizeLabel(string orientation)
        {
            if (_presetSizes.TryGetValue(orientation, out var size))
                return $"{orientation} ({size.Width}×{size.Height})";
            return orientation;
        }

        /// <summary>
        /// ImageSizeOrientation が変わったとき、RadioButton・ComboBox 向けの derived プロパティを通知する。
        /// </summary>
        partial void OnImageSizeOrientationChanged(string value)
        {
            OnPropertyChanged(nameof(SelectedSizeOption));
        }

        /// <summary>
        /// IsCustomSize が変わったとき、ComboBox 向けの derived プロパティを通知する。
        /// </summary>
        partial void OnIsCustomSizeChanged(bool value)
        {
            OnPropertyChanged(nameof(SelectedSizeOption));
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

        private bool CanRun() => _loadedConfig != null && !string.IsNullOrWhiteSpace(PositivePrompt) && !IsRunning;

        /// <summary>バッチ実行中の進捗テキストを組み立てる（例: "2/5件目を実行中"）。</summary>
        internal static string FormatBatchProgress(int current, int total) => $"{current}/{total}件目を実行中";

        /// <summary>
        /// ワークフローを ComfyUI に送信して実行する。BatchCount が2以上の場合は同じ内容で
        /// 指定回数分を順番に実行し、結果を1件にまとめる（各回シードは自動で変わる）。
        /// 完了または失敗後、ResultsFolder に result_*.json を保存する。
        /// </summary>
        [RelayCommand(CanExecute = nameof(CanRun))]
        private async Task RunWorkflowAsync()
        {
            IsRunning = true;
            PreviewThumbnails = new ObservableCollection<OutputFilePreview>();
            BatchProgressText = "";

            var totalBatches = Math.Max(1, BatchCount);
            var allOutputs = new List<OutputFile>();
            string? lastSuccessPromptId = null;
            string? lastSuccessTemplatePath = null;
            WorkflowParameters? lastSuccessParameters = null;

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

                for (int i = 1; i <= totalBatches; i++)
                {
                    BatchProgressText = totalBatches > 1 ? FormatBatchProgress(i, totalBatches) : "";

                    var outputs = await runner.ExecuteAsync(loras, prompts, imageSize);

                    allOutputs.AddRange(outputs);
                    lastSuccessPromptId = runner.PromptId;
                    lastSuccessTemplatePath = runner.TemplatePath;
                    lastSuccessParameters = runner.Parameters;

                    var newThumbnails = outputs
                        .Where(o => o.Type == "output")
                        .Select(o => new OutputFilePreview(o))
                        .ToList();
                    foreach (var thumbnail in newThumbnails)
                        PreviewThumbnails.Add(thumbnail);
                    // PreviewThumbnails への Add は再代入ではないため OnPreviewThumbnailsChanged が発火しない。
                    // 派生プロパティ HasPreviewThumbnails を手動で再通知し、右パネルの表示切り替えを反映させる。
                    if (newThumbnails.Count > 0)
                        OnPropertyChanged(nameof(HasPreviewThumbnails));
                    _ = LoadPreviewThumbnailsAsync(
                        new ObservableCollection<OutputFilePreview>(newThumbnails), runner.PromptId);
                }

                result = new WorkflowResult
                {
                    Status = "success",
                    PromptId = lastSuccessPromptId,
                    Timestamp = DateTime.Now.ToString("s"),
                    Template = lastSuccessTemplatePath,
                    Parameters = lastSuccessParameters ?? new WorkflowParameters(),
                    Outputs = allOutputs,
                };

                int count = allOutputs.FindAll(o => o.Type == "output").Count;
                _snackbarService.Show(
                    "完了",
                    $"{count} 件のファイルが生成されました",
                    ControlAppearance.Success,
                    new SymbolIcon(SymbolRegular.CheckmarkCircle24),
                    TimeSpan.FromSeconds(4.0)
                );
            }
            catch (ComfyUIException ex)
            {
                result = new WorkflowResult
                {
                    Status = "error",
                    PromptId = lastSuccessPromptId,
                    Timestamp = DateTime.Now.ToString("s"),
                    Template = lastSuccessTemplatePath ?? runner?.TemplatePath,
                    Parameters = lastSuccessParameters ?? runner?.Parameters ?? new WorkflowParameters(),
                    Outputs = allOutputs,
                    Error = ex.Message,
                };

                _snackbarService.Show(
                    "エラー",
                    ex.Message,
                    ControlAppearance.Danger,
                    new SymbolIcon(SymbolRegular.ErrorCircle24),
                    TimeSpan.FromSeconds(5.0)
                );
            }
            catch (Exception ex)
            {
                result = new WorkflowResult
                {
                    Status = "error",
                    PromptId = lastSuccessPromptId,
                    Timestamp = DateTime.Now.ToString("s"),
                    Template = lastSuccessTemplatePath,
                    Parameters = lastSuccessParameters ?? new WorkflowParameters(),
                    Outputs = allOutputs,
                    Error = ex.Message,
                };

                _snackbarService.Show(
                    "予期しないエラー",
                    ex.Message,
                    ControlAppearance.Caution,
                    new SymbolIcon(SymbolRegular.Warning24),
                    TimeSpan.FromSeconds(5.0)
                );
            }
            finally
            {
                IsRunning = false;
                BatchProgressText = "";
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

        /// <summary>
        /// 実行直後のプレビューサムネイルを非同期に取得する。
        /// ComfyUIUrl・ResultsFolder が未設定の場合は何もしない（テキスト表示のみになる）。
        /// </summary>
        private async Task LoadPreviewThumbnailsAsync(ObservableCollection<OutputFilePreview> thumbnails, string? promptId)
        {
            var url = Config.Data.ComfyUIUrl;
            var resultsFolder = Config.Data.ResultsFolder;
            if (string.IsNullOrWhiteSpace(url) || string.IsNullOrWhiteSpace(resultsFolder))
                return;

            var client = new ComfyUIClient(url);
            var cacheDirectory = Path.Combine(resultsFolder, PreviewCacheDirectoryName);

            try
            {
                await Task.WhenAll(thumbnails.Select(t => _previewLoader.LoadAsync(t, client, promptId, cacheDirectory)));
            }
            catch
            {
                // サムネイル取得失敗は実行結果表示に影響させない
            }
        }
    }
}
