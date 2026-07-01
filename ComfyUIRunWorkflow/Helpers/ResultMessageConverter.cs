using ComfyUILibs.Models;
using System.Globalization;
using System.Windows.Data;

namespace ComfyUIRunWorkflow.Helpers
{
    /// <summary>
    /// 結果を表示用文字列に変換するコンバーター。
    /// </summary>
    public class ResultMessageConverter : IValueConverter
    {
        /// <summary>
        /// 変換処理。WorkflowResult オブジェクトを受け取り、成功時は出力ファイル名、失敗時はエラーメッセージを返す。
        /// </summary>
        /// <param name="value">変換対象の値。WorkflowResult オブジェクトを想定。</param>
        /// <param name="targetType">変換後の型。</param>
        /// <param name="parameter">変換パラメーター。</param>
        /// <param name="culture">カルチャ情報。</param>
        /// <returns>変換後の文字列。成功時は出力ファイル名、失敗時はエラーメッセージ、その他の場合は空文字。</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is WorkflowResult result)
            {
                // 成功時の処理
                if (result.Status == "success")
                {
                    var output = result.Outputs.Find(output => output.Type == "output");
                    if (output != null)
                    {
                        return output.Filename;
                    }
                }
                // 失敗時の処理
                else if ((result.Status == "error") && (result.Error != null))
                {
                    return result.Error;
                }
            }

            // それ以外の場合は空文字を返す
            return "";
        }

        /// <summary>
        /// 逆変換処理。今回は未実装で例外を投げる。
        /// </summary>
        /// <param name="value">変換対象の値。</param>
        /// <param name="targetType">変換後の型。</param>
        /// <param name="parameter">変換パラメーター。</param>
        /// <param name="culture">カルチャ情報。</param>
        /// <returns>変換後の値。今回は未実装で例外を投げる。</returns>
        /// <exception cref="NotImplementedException"></exception>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
