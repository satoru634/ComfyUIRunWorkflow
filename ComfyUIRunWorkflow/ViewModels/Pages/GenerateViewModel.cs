using ComfyUILibs.Common;
using ComfyUIRunWorkflow.Helpers;
using ComfyUIRunWorkflow.Models;
using ComfyUIRunWorkflow.Services;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.IO;
using Wpf.Ui;
using Wpf.Ui.Abstractions.Controls;
using Wpf.Ui.Controls;

namespace ComfyUIRunWorkflow.ViewModels.Pages
{
    /// <summary>
    /// バッチジョブ生成ページ（GeneratePage）の ViewModel。
    /// ベースプロンプトディレクトリ・置換リスト・ジョブテンプレートから QueuePage 用ジョブ一覧を一括生成し、
    /// JSON ファイルとして出力する。生成自体は ComfyUI と通信しないローカル処理のため同期的に実行する。
    /// </summary>
    public partial class GenerateViewModel : ObservableObject, INavigationAware
    {
        /// <summary>アプリケーション設定。入力パス（ベースプロンプトディレクトリ等）はここに直接保持・永続化する。</summary>
        public Setting<AppConfig> Config { get; }

        private readonly ISnackbarService _snackbarService;
        private readonly BatchJobGenerator _generator = new();

        /// <summary>直近の生成処理のログ（1行ずつ改行区切りで蓄積）。GeneratePage 下部のログ欄に表示する。</summary>
        [ObservableProperty]
        private string _log = "";

        /// <summary>DI コンテナから設定を受け取って初期化する。</summary>
        public GenerateViewModel(Setting<AppConfig> config, ISnackbarService snackbarService)
        {
            Config = config;
            _snackbarService = snackbarService;
        }

        // ── INavigationAware ─────────────────────────────────────────────────

        /// <summary>ページへ遷移するときは何もしない（入力値は Config.Data に直接保持されている）。</summary>
        public Task OnNavigatedToAsync() => Task.CompletedTask;

        /// <summary>ページから離れるときに入力パスを設定ファイルへ保存する。</summary>
        public Task OnNavigatedFromAsync()
        {
            Config.Save();
            return Task.CompletedTask;
        }

        // ── ファイル参照コマンド ──────────────────────────────────────────────

        /// <summary>ベースプロンプトを格納したディレクトリの選択ダイアログを開く。</summary>
        [RelayCommand]
        private void BrowseBasePromptDirectory()
        {
            var dialog = new OpenFolderDialog
            {
                Title = LocalizationManager.Instance["Generate_BasePromptDirectoryDialogTitle"],
            };

            if (!string.IsNullOrWhiteSpace(Config.Data.GenerateBasePromptDirectory))
                dialog.InitialDirectory = Config.Data.GenerateBasePromptDirectory;

            if (dialog.ShowDialog() == true)
            {
                Config.Data.GenerateBasePromptDirectory = dialog.FolderName;
                GenerateCommand.NotifyCanExecuteChanged();
            }
        }

        /// <summary>置換リストファイルの選択ダイアログを開く。</summary>
        [RelayCommand]
        private void BrowseReplacementListPath()
        {
            var dialog = new OpenFileDialog
            {
                Title = LocalizationManager.Instance["Generate_ReplacementListDialogTitle"],
                Filter = LocalizationManager.Instance["Common_JsonFileDialogFilter"],
            };

            if (!string.IsNullOrWhiteSpace(Config.Data.GenerateReplacementListPath))
            {
                var dir = Path.GetDirectoryName(Config.Data.GenerateReplacementListPath);
                if (!string.IsNullOrEmpty(dir))
                    dialog.InitialDirectory = dir;
            }

            if (dialog.ShowDialog() == true)
            {
                Config.Data.GenerateReplacementListPath = dialog.FileName;
                GenerateCommand.NotifyCanExecuteChanged();
            }
        }

        /// <summary>ジョブテンプレートファイルの選択ダイアログを開く。</summary>
        [RelayCommand]
        private void BrowseJobTemplatePath()
        {
            var dialog = new OpenFileDialog
            {
                Title = LocalizationManager.Instance["Generate_JobTemplateDialogTitle"],
                Filter = LocalizationManager.Instance["Common_JsonFileDialogFilter"],
            };

            if (!string.IsNullOrWhiteSpace(Config.Data.GenerateJobTemplatePath))
            {
                var dir = Path.GetDirectoryName(Config.Data.GenerateJobTemplatePath);
                if (!string.IsNullOrEmpty(dir))
                    dialog.InitialDirectory = dir;
            }

            if (dialog.ShowDialog() == true)
            {
                Config.Data.GenerateJobTemplatePath = dialog.FileName;
                GenerateCommand.NotifyCanExecuteChanged();
            }
        }

        /// <summary>出力先ファイルの選択ダイアログを開く。同名ファイルが存在する場合はダイアログ自体が上書き確認する。</summary>
        [RelayCommand]
        private void BrowseOutputPath()
        {
            var dialog = new SaveFileDialog
            {
                Title = LocalizationManager.Instance["Generate_OutputDialogTitle"],
                Filter = LocalizationManager.Instance["Common_JsonFileDialogFilter"],
                FileName = "queue_jobs.json",
            };

            if (!string.IsNullOrWhiteSpace(Config.Data.GenerateOutputPath))
            {
                var dir = Path.GetDirectoryName(Config.Data.GenerateOutputPath);
                if (!string.IsNullOrEmpty(dir))
                    dialog.InitialDirectory = dir;
            }

            if (dialog.ShowDialog() == true)
            {
                Config.Data.GenerateOutputPath = dialog.FileName;
                GenerateCommand.NotifyCanExecuteChanged();
            }
        }

        // ── 生成 ──────────────────────────────────────────────────────────────

        /// <summary>
        /// 4つの入力パスが揃っているかどうか（未入力の場合はボタンを無効化する）。
        /// </summary>
        private bool CanGenerate() =>
            !string.IsNullOrWhiteSpace(Config.Data.GenerateBasePromptDirectory) &&
            !string.IsNullOrWhiteSpace(Config.Data.GenerateReplacementListPath) &&
            !string.IsNullOrWhiteSpace(Config.Data.GenerateJobTemplatePath) &&
            !string.IsNullOrWhiteSpace(Config.Data.GenerateOutputPath);

        /// <summary>
        /// ベースプロンプト・置換リスト・ジョブテンプレートからジョブ一覧を生成し、queue_jobs.json と同形式
        /// （<see cref="QueueJobListData"/>）で出力先ファイルへ書き出す。失敗時はスナックバーでエラー内容を表示する。
        /// </summary>
        [RelayCommand(CanExecute = nameof(CanGenerate))]
        private void Generate()
        {
            Log = "";

            List<QueueJobData> jobs;
            try
            {
                jobs = _generator.Generate(
                    Config.Data.GenerateBasePromptDirectory,
                    Config.Data.GenerateReplacementListPath,
                    Config.Data.GenerateJobTemplatePath,
                    onLog: AppendLog);

                var listData = new QueueJobListData
                {
                    Jobs = new ObservableCollection<QueueJobData>(jobs),
                };
                JsonLoader.WriteJson(Config.Data.GenerateOutputPath, listData);
                AppendLog(string.Format(
                    LocalizationManager.Instance["Generate_Log_WroteOutput_Format"], jobs.Count, Config.Data.GenerateOutputPath));
            }
            catch (Exception ex)
            {
                AppendLog(string.Format(LocalizationManager.Instance["Generate_Error_Format"], ex.Message));
                _snackbarService.Show(
                    LocalizationManager.Instance["Common_Error"],
                    string.Format(LocalizationManager.Instance["Generate_Error_Format"], ex.Message),
                    ControlAppearance.Danger,
                    new SymbolIcon(SymbolRegular.ErrorCircle24),
                    TimeSpan.FromSeconds(5.0)
                );
                return;
            }

            _snackbarService.Show(
                LocalizationManager.Instance["Common_Completed"],
                string.Format(LocalizationManager.Instance["Generate_Success_Format"], jobs.Count),
                ControlAppearance.Success,
                new SymbolIcon(SymbolRegular.CheckmarkCircle24),
                TimeSpan.FromSeconds(4.0)
            );
        }

        /// <summary>ログ欄へ1行追記する。</summary>
        private void AppendLog(string line) => Log = string.IsNullOrEmpty(Log) ? line : Log + Environment.NewLine + line;
    }
}
