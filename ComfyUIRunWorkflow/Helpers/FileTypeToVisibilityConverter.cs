using System.Globalization;
using System.Windows.Data;

namespace ComfyUIRunWorkflow.Helpers
{
    /// <summary>
    /// ファイルタイプが "output" かどうかを判定し、Visibility に変換するコンバーター。
    /// </summary>
    public class FileTypeToVisibilityConverter : IValueConverter
    {
        /// <summary>
        /// ファイルタイプが "output" の場合は Visible を返し、それ以外の場合は Collapsed を返す。
        /// </summary>
        /// <param name="value">変換対象の値</param>
        /// <param name="targetType">変換後の型</param>
        /// <param name="parameter">変換パラメータ</param>
        /// <param name="culture">カルチャ情報</param>
        /// <returns>変換結果</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string fileType)
            {
                // ファイルタイプが "output" の場合は Visible を返す
                if (fileType == "output")
                {
                    return Visibility.Visible;
                }
            }

            // それ以外の場合は Collapsed を返す
            return Visibility.Collapsed;
        }

        /// <summary>
        /// 逆変換は未実装で例外を投げる。
        /// </summary>
        /// <param name="value">変換対象の値</param>
        /// <param name="targetType">変換後の型</param>
        /// <param name="parameter">変換パラメータ</param>
        /// <param name="culture">カルチャ情報</param>
        /// <returns>変換結果</returns>
        /// <exception cref="NotImplementedException"></exception>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
