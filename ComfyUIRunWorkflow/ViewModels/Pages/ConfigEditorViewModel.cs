using ComfyUILibs.Common;
using ComfyUILibs.Exceptions;
using ComfyUILibs.Models;
using ComfyUILibs.Services;
using ComfyUIRunWorkflow.Helpers;
using ComfyUIRunWorkflow.Models;
using System.Collections.ObjectModel;
using System.IO;
using Wpf.Ui;
using Wpf.Ui.Abstractions.Controls;
using Wpf.Ui.Controls;
using Wpf.Ui.Extensions;

namespace ComfyUIRunWorkflow.ViewModels.Pages
{
    /// <summary>
    /// ワークフロー設定編集ページ（ConfigEditorPage）の ViewModel。
    /// 設定ページで指定された ConfigPath の workflow_config.json をそのまま読み込み・編集・保存する。
    /// </summary>
    public partial class ConfigEditorViewModel : ObservableObject, INavigationAware
    {
        /// <summary>アプリケーション設定（ConfigPath の参照元）。</summary>
        public Setting<AppConfig> Config { get; }

        private readonly ISnackbarService _snackbarService;
        private readonly IContentDialogService _contentDialogService;

        /// <summary>workflow_config.json が正常に読み込まれているか（保存ボタンの活性状態に使用）。</summary>
        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
        private bool _isConfigLoaded = false;

        /// <summary>comfyui_url。</summary>
        [ObservableProperty]
        private string _comfyUIUrl = "";

        /// <summary>default_workflow。DefaultWorkflowNameList のいずれかと一致している必要がある（保存時に検証）。</summary>
        [ObservableProperty]
        private string _defaultWorkflow = "";

        /// <summary>default_workflow 選択コンボボックス用の、現在のワークフロー名一覧。</summary>
        [ObservableProperty]
        private List<string> _workflowNameList = new();

        /// <summary>編集中のワークフロー設定一覧。</summary>
        [ObservableProperty]
        private ObservableCollection<ConfigWorkflowItemViewModel> _workflows = new();

        /// <summary>編集パネルに表示中のワークフロー。未選択の場合は null。</summary>
        [ObservableProperty]
        private ConfigWorkflowItemViewModel? _selectedWorkflow;

        /// <summary>wd14_tagger.model_name。空文字の場合、保存時に wd14_tagger セクション自体を出力しない。</summary>
        [ObservableProperty]
        private string _wd14ModelName = "";

        /// <summary>wd14_tagger.general_threshold。</summary>
        [ObservableProperty]
        private double _wd14GeneralThreshold = 0.35;

        /// <summary>wd14_tagger.character_threshold。</summary>
        [ObservableProperty]
        private double _wd14CharacterThreshold = 0.85;

        /// <summary>prepend_tags をカンマ区切りで表示・編集するテキスト。空文字の場合、保存時にキー自体を出力しない。</summary>
        [ObservableProperty]
        private string _prependTagsText = "";

        /// <summary>exclude_tags をカンマ区切りで表示・編集するテキスト。空文字の場合、保存時にキー自体を出力しない。</summary>
        [ObservableProperty]
        private string _excludeTagsText = "";

        /// <summary>DI コンテナから設定を受け取って初期化する。</summary>
        public ConfigEditorViewModel(
            Setting<AppConfig> config,
            ISnackbarService snackbarService,
            IContentDialogService contentDialogService)
        {
            Config = config;
            _snackbarService = snackbarService;
            _contentDialogService = contentDialogService;
        }

        // ── INavigationAware ─────────────────────────────────────────────────

        /// <summary>
        /// ページへ遷移するたびに workflow_config.json を再読み込みする。
        /// 保存していない編集内容はここで破棄される（このページは Save ボタンによる明示的な保存のみ行う）。
        /// </summary>
        public Task OnNavigatedToAsync()
        {
            LoadConfig();
            return Task.CompletedTask;
        }

        /// <summary>ページから離れる際は何もしない（永続化は Save ボタン経由のみ）。</summary>
        public Task OnNavigatedFromAsync() => Task.CompletedTask;

        // ── 読み込み ──────────────────────────────────────────────────────────

        private void LoadConfig()
        {
            var path = Config.Data.ConfigPath;
            if (string.IsNullOrWhiteSpace(path))
            {
                ResetState();
                ShowError(LocalizationManager.Instance["Common_ConfigPathNotSet"]);
                return;
            }

            WorkflowConfig loaded;
            try
            {
                loaded = ConfigLoader.LoadConfig(path);
            }
            catch (ComfyUIException ex)
            {
                ResetState();
                ShowError(string.Format(LocalizationManager.Instance["Dashboard_ConfigLoadError_Format"], ex.Message));
                return;
            }

            ComfyUIUrl = loaded.ComfyuiUrl ?? "";
            DefaultWorkflow = loaded.DefaultWorkflow ?? "";

            Workflows = new ObservableCollection<ConfigWorkflowItemViewModel>(
                loaded.Workflows!.Select(kv => ConfigWorkflowItemViewModel.FromSettings(kv.Key, kv.Value)));
            RefreshWorkflowNameList();
            SelectedWorkflow = Workflows.FirstOrDefault();

            Wd14ModelName = loaded.Wd14Tagger?.ModelName ?? "";
            Wd14GeneralThreshold = loaded.Wd14Tagger?.GeneralThreshold ?? 0.35;
            Wd14CharacterThreshold = loaded.Wd14Tagger?.CharacterThreshold ?? 0.85;

            PrependTagsText = loaded.PrependTags != null ? string.Join(", ", loaded.PrependTags) : "";
            ExcludeTagsText = loaded.ExcludeTags != null ? string.Join(", ", loaded.ExcludeTags) : "";

            IsConfigLoaded = true;
        }

        private void ResetState()
        {
            IsConfigLoaded = false;
            Workflows = new ObservableCollection<ConfigWorkflowItemViewModel>();
            WorkflowNameList = new List<string>();
            SelectedWorkflow = null;
            ComfyUIUrl = "";
            DefaultWorkflow = "";
            Wd14ModelName = "";
            Wd14GeneralThreshold = 0.35;
            Wd14CharacterThreshold = 0.85;
            PrependTagsText = "";
            ExcludeTagsText = "";
        }

        // ── ワークフロー追加・削除・リネーム ──────────────────────────────────

        /// <summary>新しいワークフローを一意な仮名で追加し、そのまま名前をインライン編集できる状態にする。</summary>
        [RelayCommand]
        private void AddWorkflow()
        {
            var baseName = LocalizationManager.Instance["ConfigEditor_NewWorkflowDefaultName"];
            var name = baseName;
            var suffix = 2;
            while (Workflows.Any(w => w.Name == name))
            {
                name = $"{baseName}_{suffix}";
                suffix++;
            }

            var item = ConfigWorkflowItemViewModel.CreateDefault(name);
            Workflows.Add(item);
            RefreshWorkflowNameList();
            SelectedWorkflow = item;

            item.NameBeforeEdit = name;
            item.IsEditingName = true;
        }

        /// <summary>指定したワークフローを、確認ダイアログでの承認後に削除する。default_workflow に指定中の場合は削除しない。</summary>
        [RelayCommand]
        private async Task RemoveWorkflowAsync(ConfigWorkflowItemViewModel item)
        {
            if (item.Name == DefaultWorkflow)
            {
                ShowError(string.Format(LocalizationManager.Instance["ConfigEditor_CannotRemoveDefaultWorkflow_Format"], item.Name));
                return;
            }

            var result = await _contentDialogService.ShowSimpleDialogAsync(new SimpleContentDialogCreateOptions
            {
                Title = LocalizationManager.Instance["ConfigEditor_RemoveWorkflowConfirmTitle"],
                Content = string.Format(LocalizationManager.Instance["ConfigEditor_RemoveWorkflowConfirmContent_Format"], item.Name),
                PrimaryButtonText = LocalizationManager.Instance["Queue_RemoveConfirmPrimaryButtonContent"],
                CloseButtonText = LocalizationManager.Instance["Common_CancelButtonContent"],
            });
            if (result != ContentDialogResult.Primary)
                return;

            Workflows.Remove(item);
            if (SelectedWorkflow == item)
                SelectedWorkflow = null;
            RefreshWorkflowNameList();
        }

        /// <summary>ワークフロー名のインライン編集を開始する（編集前の名前を退避する）。</summary>
        public void BeginEditingWorkflowName(ConfigWorkflowItemViewModel item)
        {
            item.NameBeforeEdit = item.Name;
            item.IsEditingName = true;
        }

        /// <summary>
        /// ワークフロー名のインライン編集を確定する。空文字・他ワークフローとの重複の場合は編集前の名前に戻し、
        /// エラーを表示する。名前の変更が確定した場合、default_workflow がこのワークフローを指していれば追従させる。
        /// </summary>
        public void FinishEditingWorkflowName(ConfigWorkflowItemViewModel item)
        {
            var trimmed = item.Name.Trim();
            var previous = item.NameBeforeEdit;
            var isDuplicate = trimmed != previous && Workflows.Any(w => w != item && w.Name == trimmed);

            if (string.IsNullOrWhiteSpace(trimmed) || isDuplicate)
            {
                item.Name = previous;
                item.IsEditingName = false;
                ShowError(string.IsNullOrWhiteSpace(trimmed)
                    ? LocalizationManager.Instance["ConfigEditor_WorkflowNameEmpty"]
                    : string.Format(LocalizationManager.Instance["ConfigEditor_DuplicateWorkflowName_Format"], trimmed));
                return;
            }

            if (DefaultWorkflow == previous)
                DefaultWorkflow = trimmed;

            item.Name = trimmed;
            item.IsEditingName = false;
            RefreshWorkflowNameList();
        }

        private void RefreshWorkflowNameList() => WorkflowNameList = Workflows.Select(w => w.Name).ToList();

        // ── 保存 ──────────────────────────────────────────────────────────────

        private bool CanSave() => IsConfigLoaded;

        /// <summary>
        /// 編集内容を <see cref="WorkflowConfig"/> に変換し、一時ファイル経由で
        /// <see cref="ConfigLoader.LoadConfig"/> と同等のバリデーションを実施してから ConfigPath へ保存する。
        /// バリデーションに失敗した場合、実ファイルは変更しない。
        /// </summary>
        [RelayCommand(CanExecute = nameof(CanSave))]
        private void Save()
        {
            var duplicateWorkflow = Workflows.GroupBy(w => w.Name).FirstOrDefault(g => g.Count() > 1);
            if (duplicateWorkflow != null)
            {
                ShowError(string.Format(LocalizationManager.Instance["ConfigEditor_DuplicateWorkflowName_Format"], duplicateWorkflow.Key));
                return;
            }

            foreach (var workflow in Workflows)
            {
                if (workflow.Loras.Any(l => string.IsNullOrWhiteSpace(l.Name)))
                {
                    ShowError(string.Format(LocalizationManager.Instance["ConfigEditor_LoraNameEmpty_Format"], workflow.Name));
                    return;
                }

                var duplicateLora = workflow.Loras.GroupBy(l => l.Name).FirstOrDefault(g => g.Count() > 1);
                if (duplicateLora != null)
                {
                    ShowError(string.Format(LocalizationManager.Instance["ConfigEditor_DuplicateLoraName_Format"], workflow.Name, duplicateLora.Key));
                    return;
                }
            }

            var config = BuildConfig();

            var tempPath = Path.Combine(Path.GetTempPath(), $"workflow_config_validate_{Guid.NewGuid():N}.json");
            try
            {
                JsonLoader.WriteJson(tempPath, config);
                ConfigLoader.LoadConfig(tempPath);
            }
            catch (ComfyUIException ex)
            {
                ShowError(string.Format(LocalizationManager.Instance["ConfigEditor_SaveValidationError_Format"], ex.Message));
                return;
            }
            finally
            {
                if (File.Exists(tempPath))
                    File.Delete(tempPath);
            }

            JsonLoader.WriteJson(Config.Data.ConfigPath, config);

            var missingTemplateWorkflows = FindMissingTemplateWorkflows();
            if (missingTemplateWorkflows.Count > 0)
            {
                _snackbarService.Show(
                    LocalizationManager.Instance["Common_Completed"],
                    string.Format(
                        LocalizationManager.Instance["ConfigEditor_SaveSuccessWithMissingTemplates_Format"],
                        string.Join(", ", missingTemplateWorkflows)),
                    ControlAppearance.Caution,
                    new SymbolIcon(SymbolRegular.Warning24),
                    TimeSpan.FromSeconds(6.0)
                );
            }
            else
            {
                _snackbarService.Show(
                    LocalizationManager.Instance["Common_Completed"],
                    LocalizationManager.Instance["ConfigEditor_SaveSuccess"],
                    ControlAppearance.Success,
                    new SymbolIcon(SymbolRegular.CheckmarkCircle24),
                    TimeSpan.FromSeconds(3.0)
                );
            }
        }

        internal WorkflowConfig BuildConfig() => new()
        {
            ComfyuiUrl = ComfyUIUrl,
            DefaultWorkflow = DefaultWorkflow,
            Workflows = Workflows.ToDictionary(w => w.Name, w => w.ToSettings()),
            Wd14Tagger = string.IsNullOrWhiteSpace(Wd14ModelName)
                ? null
                : new Wd14TaggerConfig
                {
                    ModelName = Wd14ModelName,
                    GeneralThreshold = Wd14GeneralThreshold,
                    CharacterThreshold = Wd14CharacterThreshold,
                },
            PrependTags = ParseTags(PrependTagsText),
            ExcludeTags = ParseTags(ExcludeTagsText),
        };

        private static List<string>? ParseTags(string text) =>
            string.IsNullOrWhiteSpace(text)
                ? null
                : text.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();

        /// <summary>実行ディレクトリ直下の templates/{workflow名}/template_lora_0.json が存在しないワークフロー名を返す。</summary>
        private List<string> FindMissingTemplateWorkflows()
        {
            var templatesRoot = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "templates");
            return Workflows
                .Where(w => !File.Exists(Path.Combine(templatesRoot, w.Name, "template_lora_0.json")))
                .Select(w => w.Name)
                .ToList();
        }

        private void ShowError(string message) => _snackbarService.Show(
            LocalizationManager.Instance["Common_Error"],
            message,
            ControlAppearance.Danger,
            new SymbolIcon(SymbolRegular.ErrorCircle24),
            TimeSpan.FromSeconds(5.0)
        );
    }
}
