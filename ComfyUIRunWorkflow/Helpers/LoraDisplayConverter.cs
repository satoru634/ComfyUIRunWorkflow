using System.Globalization;
using System.Windows.Data;

namespace ComfyUIRunWorkflow.Helpers
{
    /// <summary>
    /// LoRA の Name・File・Strength を「name  (file,  strength: 0.80）」形式の表示文字列に変換する
    /// マルチバインディングコンバーター。文言は Strings.resx の ResultDetail_LoraFormat を参照するため、
    /// XAML の StringFormat では対応できない多言語対応をここで行う。
    /// </summary>
    public class LoraDisplayConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length < 3 || values[0] is not string name || values[1] is not string file || values[2] is not double strength)
                return "";

            return string.Format(
                LocalizationManager.Instance["ResultDetail_LoraFormat"],
                name, file, strength.ToString("F2", culture));
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
