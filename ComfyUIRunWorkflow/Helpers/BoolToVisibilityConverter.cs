using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace ComfyUIRunWorkflow.Helpers
{
    /// <summary>
    /// null または空文字を <see cref="Visibility"/> に変換する。
    /// null/空文字 → Collapsed、それ以外 → Visible。
    /// </summary>
    [ValueConversion(typeof(object), typeof(Visibility))]
    internal class NullToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => value is null || (value is string s && string.IsNullOrEmpty(s))
                ? Visibility.Collapsed
                : Visibility.Visible;

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }


    /// <summary>
    /// null または空文字を逆転した <see cref="Visibility"/> に変換する。
    /// null/空文字 → Visible、それ以外 → Collapsed。
    /// </summary>
    [ValueConversion(typeof(object), typeof(Visibility))]
    internal class NullToVisibilityInverseConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => value is null || (value is string s && string.IsNullOrEmpty(s))
                ? Visibility.Visible
                : Visibility.Collapsed;

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }

    /// <summary>
    /// bool 値を <see cref="Visibility"/> に変換する。true → Visible、false → Collapsed。
    /// </summary>
    [ValueConversion(typeof(bool), typeof(Visibility))]
    internal class BoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => value is true ? Visibility.Visible : Visibility.Collapsed;

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => value is Visibility.Visible;
    }

    /// <summary>
    /// bool 値を逆転した <see cref="Visibility"/> に変換する。true → Collapsed、false → Visible。
    /// </summary>
    [ValueConversion(typeof(bool), typeof(Visibility))]
    internal class BoolToVisibilityInverseConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => value is true ? Visibility.Collapsed : Visibility.Visible;

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => value is not Visibility.Visible;
    }
}
