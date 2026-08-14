using ComfyUILibs.Common;
using ComfyUILibs.Exceptions;
using ComfyUILibs.Models;
using ComfyUILibs.Services;
using ComfyUIRunWorkflow.Helpers;
using ComfyUIRunWorkflow.Models;
using ComfyUIRunWorkflow.Services;
using ComfyUIRunWorkflow.Views.Controls;
using System.Collections.ObjectModel;
using Wpf.Ui;
using Wpf.Ui.Abstractions.Controls;
using Wpf.Ui.Controls;

namespace ComfyUIRunWorkflow.ViewModels.Pages
{
    /// <summary>
    /// 複数ワークフロー連続実行ページ（QueuePage）の ViewModel。
    /// ジョブの追加・削除・編集・順次実行（協調的キャンセル対応）・実行結果詳細ダイアログ表示を担当する。
    /// </summary>
    public partial class QueueViewModel : ObservableObject, INavigationAware
    {
        /// <summary>アプリケーション設定。</summary>
        public Setting<AppConfig> Config { get; }

        /// <summary>Queue ジョブ定義一覧の永続化先（queue_jobs.json）。</summary>
        private readonly Setting<QueueJobListData> _queueJobsSetting;

        private readonly ISnackbarService _snackbarService;
        private readonly IContentDialogService _contentDialogService;
        private readonly WorkflowExecutionService _executionService = new();

        /// <summary>workflow_config.json から読み込んだワークフロー名の一覧。</summary>
        [ObservableProperty]
        private List<string> _workflowNames = new();

        /// <summary>config が正常に読み込まれているか。</summary>
        [ObservableProperty]
        private bool _isConfigLoaded = false;

        /// <summary>登録済みジョブの一覧（上から順に実行される）。</summary>
        [ObservableProperty]
        private ObservableCollection<QueueJobViewModel> _jobs = new();

        /// <summary>編集パネルに表示中のジョブ。未選択の場合は null。</summary>
        [ObservableProperty]
        private QueueJobViewModel? _selectedJob;

        /// <summary>すべて実行中かどうか。</summary>
        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(RunAllCommand))]
        [NotifyCanExecuteChangedFor(nameof(CancelQueueCommand))]
        private bool _isRunning = false;

        /// <summary>全体の実行進捗テキスト（例: "2/5件目のジョブを実行中: sdxl"）。</summary>
        [ObservableProperty]
        private string _overallProgressText = "";

        /// <summary>ジョブの追加・削除・編集が可能かどうか（実行中は不可）。</summary>
        public bool CanEditJobs => !IsRunning;

        /// <summary>IsRunning が変わったとき、派生プロパティ CanEditJobs を通知する。</summary>
        partial void OnIsRunningChanged(bool value) => OnPropertyChanged(nameof(CanEditJobs));

        /// <summary>ジョブが1件以上登録されているか（一覧の空プレースホルダー表示切り替えに使用）。</summary>
        public bool HasJobs => Jobs.Count > 0;

        private bool _cancelRequested = false;
        private WorkflowConfig? _loadedConfig;

        /// <summary>DI コンテナから設定を受け取って初期化する。</summary>
        public QueueViewModel(
            Setting<AppConfig> config,
            Setting<QueueJobListData> queueJobsSetting,
            ISnackbarService snackbarService,
            IContentDialogService contentDialogService)
        {
            Config = config;
            _queueJobsSetting = queueJobsSetting;
            _snackbarService = snackbarService;
            _contentDialogService = contentDialogService;
        }

        // ── INavigationAware ─────────────────────────────────────────────────

        /// <summary>ページへ遷移するたびに workflow_config.json を再読み込みする。ジョブ一覧は初回のみ永続化データから復元する。</summary>
        public Task OnNavigatedToAsync()
        {
            TryLoadConfig();
            LoadPersistedJobsIfNeeded();
            return Task.CompletedTask;
        }

        /// <summary>ページから離れるときにジョブ定義を永続化する。</summary>
        public Task OnNavigatedFromAsync()
        {
            PersistJobs();
            return Task.CompletedTask;
        }

        // ── config 読み込み ───────────────────────────────────────────────────

        /// <summary>
        /// 設定ページで指定された ConfigPath から workflow_config.json を読み込み、既存の各ジョブにも反映する。
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

                _snackbarService.Show(
                    "Error",
                    LocalizationManager.Instance["Common_ConfigPathNotSet"],
                    ControlAppearance.Danger,
                    new SymbolIcon(SymbolRegular.ErrorCircle24),
                    TimeSpan.FromSeconds(3.0)
                );
            }
            else
            {
                try
                {
                    _loadedConfig = ConfigLoader.LoadConfig(path);
                    WorkflowNames = _loadedConfig.Workflows!.Keys.ToList();
                    IsConfigLoaded = true;
                }
                catch (ComfyUIException ex)
                {
                    _loadedConfig = null;
                    IsConfigLoaded = false;
                    WorkflowNames = new List<string>();

                    _snackbarService.Show(
                        "Error",
                        string.Format(LocalizationManager.Instance["Dashboard_ConfigLoadError_Format"], ex.Message),
                        ControlAppearance.Danger,
                        new SymbolIcon(SymbolRegular.ErrorCircle24),
                        TimeSpan.FromSeconds(3.0)
                    );
                }
            }

            foreach (var job in Jobs)
                job.ApplyWorkflowConfig(_loadedConfig);

            RunAllCommand.NotifyCanExecuteChanged();
        }

        /// <summary>初回のみ、永続化されたジョブ定義から Jobs を復元する（実行状態は Pending から開始）。</summary>
        private void LoadPersistedJobsIfNeeded()
        {
            if (Jobs.Count > 0) return;

            foreach (var data in _queueJobsSetting.Data.Jobs)
            {
                var job = QueueJobViewModel.FromData(data);
                job.ApplyWorkflowConfig(_loadedConfig);
                Jobs.Add(job);
            }

            RunAllCommand.NotifyCanExecuteChanged();
            OnPropertyChanged(nameof(HasJobs));
        }

        // ── ジョブ操作 ────────────────────────────────────────────────────────

        /// <summary>新しいジョブをリストに追加し、選択状態にする。</summary>
        [RelayCommand]
        private void AddJob()
        {
            var job = new QueueJobViewModel();
            job.ApplyWorkflowConfig(_loadedConfig);
            if (WorkflowNames.Count > 0)
                job.WorkflowName = WorkflowNames[0];

            Jobs.Add(job);
            SelectedJob = job;
            RunAllCommand.NotifyCanExecuteChanged();
            OnPropertyChanged(nameof(HasJobs));
            PersistJobs();
        }

        /// <summary>指定したジョブをリストから削除する。</summary>
        [RelayCommand]
        private void RemoveJob(QueueJobViewModel job)
        {
            Jobs.Remove(job);
            if (SelectedJob == job)
                SelectedJob = null;

            RunAllCommand.NotifyCanExecuteChanged();
            OnPropertyChanged(nameof(HasJobs));
            PersistJobs();
        }

        // ── 実行 ──────────────────────────────────────────────────────────────

        private bool CanRunAll() => !IsRunning && Jobs.Count > 0;

        /// <summary>
        /// リストの先頭から順にジョブを実行する。既に成功済み（Success）のジョブはスキップする。
        /// あるジョブで ComfyUI エラーが発生した場合はそのジョブを失敗として記録し、次のジョブへ継続する。
        /// 中断が要求された場合は、現在実行中のジョブが完了した時点で以降のジョブへの着手を止める（協調的キャンセル）。
        /// </summary>
        [RelayCommand(CanExecute = nameof(CanRunAll))]
        private async Task RunAllAsync()
        {
            IsRunning = true;
            _cancelRequested = false;
            PersistJobs();

            try
            {
                for (int i = 0; i < Jobs.Count; i++)
                {
                    var job = Jobs[i];
                    if (job.Status == QueueJobStatus.Success)
                        continue;

                    if (_cancelRequested)
                    {
                        job.Status = QueueJobStatus.Cancelled;
                        continue;
                    }

                    job.Status = QueueJobStatus.Running;
                    job.StatusMessage = "";
                    OverallProgressText = string.Format(
                        LocalizationManager.Instance["Queue_OverallProgress_Format"],
                        i + 1, Jobs.Count, job.WorkflowName);

                    var loras = job.LoraSlots
                        .Where(s => !string.IsNullOrWhiteSpace(s.SelectedLora))
                        .Select(s => s.SelectedLora)
                        .ToList();
                    var prompts = new PromptPair { Positive = job.PositivePrompt, Negative = job.NegativePrompt };
                    var imageSize = job.ResolveImageSize();

                    var outcome = await _executionService.RunBatchAsync(
                        Config.Data.ConfigPath,
                        job.WorkflowName,
                        loras,
                        prompts,
                        imageSize,
                        job.BatchCount,
                        filenamePrefix: job.FilenamePrefix,
                        onBatchStart: (current, total) =>
                            job.StatusMessage = total > 1 ? BatchProgressFormatter.Format(current, total) : "");

                    job.LastResult = outcome.Result;
                    job.Status = outcome.Error == null ? QueueJobStatus.Success : QueueJobStatus.Error;
                    job.StatusMessage = outcome.Error?.Message ?? "";

                    await WorkflowExecutionService.SaveResultAsync(outcome.Result, Config.Data.ResultsFolder);
                }
            }
            finally
            {
                IsRunning = false;
                OverallProgressText = "";
                PersistJobs();
            }
        }

        private bool CanCancelQueue() => IsRunning;

        /// <summary>実行中のジョブが完了した時点で、以降のジョブへの着手を止めるよう要求する。</summary>
        [RelayCommand(CanExecute = nameof(CanCancelQueue))]
        private void CancelQueue() => _cancelRequested = true;

        /// <summary>指定したジョブの実行結果詳細ダイアログを開く。実行結果がない場合は何もしない。</summary>
        [RelayCommand]
        private async Task OpenJobDetailAsync(QueueJobViewModel job)
        {
            if (job.LastResult == null) return;

            var viewModel = new ViewModels.Controls.ResultDetailViewModel(job.LastResult, Config, _contentDialogService);
            var dialog = new ResultDetailWindow(viewModel, _contentDialogService.GetDialogHostEx());
            await dialog.ShowAsync();
        }

        // ── 永続化 ────────────────────────────────────────────────────────────

        /// <summary>現在のジョブ定義（実行状態を除く）を queue_jobs.json に保存する。</summary>
        private void PersistJobs()
        {
            _queueJobsSetting.Data.Jobs = new ObservableCollection<QueueJobData>(Jobs.Select(j => j.ToData()));
            _queueJobsSetting.Save();
        }
    }
}
