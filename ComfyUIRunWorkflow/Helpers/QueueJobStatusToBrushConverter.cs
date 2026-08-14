using ComfyUIRunWorkflow.Models;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace ComfyUIRunWorkflow.Helpers
{
    /// <summary>
    /// <see cref="QueueJobStatus"/> をステータス表示用の前景色ブラシに変換する。
    /// QueuePage のジョブ一覧で、実行状態を色で区別するために使用する。
    /// </summary>
    public class QueueJobStatusToBrushConverter : IValueConverter
    {
        private static readonly Brush PendingBrush = new SolidColorBrush(Color.FromRgb(0x8A, 0x8A, 0x8A));
        private static readonly Brush RunningBrush = new SolidColorBrush(Color.FromRgb(0x0A, 0x66, 0xC2));
        private static readonly Brush SuccessBrush = new SolidColorBrush(Color.FromRgb(0x1E, 0x8E, 0x3E));
        private static readonly Brush ErrorBrush = new SolidColorBrush(Color.FromRgb(0xC4, 0x2B, 0x1C));

        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) => value switch
        {
            QueueJobStatus.Running => RunningBrush,
            QueueJobStatus.Success => SuccessBrush,
            QueueJobStatus.Error => ErrorBrush,
            _ => PendingBrush,
        };

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }
}
