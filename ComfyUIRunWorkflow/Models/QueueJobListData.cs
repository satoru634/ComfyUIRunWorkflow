using System.Collections.ObjectModel;

namespace ComfyUIRunWorkflow.Models
{
    /// <summary>
    /// QueuePage に登録されたジョブ定義一覧の永続化ルートクラス。
    /// <c>Setting&lt;QueueJobListData&gt;</c> 経由でアプリのカレントディレクトリ直下の
    /// <c>queue_jobs.json</c> に永続化される。ファイルが存在しない場合は空リスト（初期状態）として扱われる。
    /// 実行ステータス・実行結果はセッション限りのため含まれない。
    /// </summary>
    public partial class QueueJobListData : ObservableObject
    {
        /// <summary>登録されたジョブの定義一覧。</summary>
        [ObservableProperty]
        private ObservableCollection<QueueJobData> _jobs = new();
    }
}
