using System.Globalization;
using System.Windows.Data;
using Wpf.Ui.Appearance;

namespace ComfyUIRunWorkflow.Helpers
{
    /// <summary>
    /// <see cref="ApplicationTheme"/> 列挙型の値と <see cref="bool"/> を相互変換する <see cref="IValueConverter"/> 実装。
    /// RadioButton の IsChecked バインディングで列挙型を直接扱うために使用する。
    /// </summary>
    internal class EnumToBooleanConverter : IValueConverter
    {
        /// <summary>
        /// 列挙型の値をパラメーターと比較し、一致すれば <c>true</c> を返す。
        /// </summary>
        /// <param name="value">バインディングソースの列挙型値。</param>
        /// <param name="parameter">比較対象の列挙型メンバー名（文字列）。</param>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (parameter is not String enumString)
            {
                throw new ArgumentException("ExceptionEnumToBooleanConverterParameterMustBeAnEnumName");
            }

            if (!Enum.IsDefined(typeof(ApplicationTheme), value))
            {
                throw new ArgumentException("ExceptionEnumToBooleanConverterValueMustBeAnEnum");
            }

            var enumValue = Enum.Parse(typeof(ApplicationTheme), enumString);

            return enumValue.Equals(value);
        }

        /// <summary>
        /// パラメーターで指定された名前の列挙型値に変換して返す。
        /// </summary>
        /// <param name="parameter">変換先の列挙型メンバー名（文字列）。</param>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (parameter is not String enumString)
            {
                throw new ArgumentException("ExceptionEnumToBooleanConverterParameterMustBeAnEnumName");
            }

            return Enum.Parse(typeof(ApplicationTheme), enumString);
        }
    }
}
