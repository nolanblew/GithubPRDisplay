using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace GithubDisplay.Converters
{
    class InvBoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is bool b)
            {
                return !b ? Visibility.Visible : Visibility.Collapsed;
            }

            if (value is string s)
            {
                if (bool.TryParse(s, out var result))
                {
                    return !result ? Visibility.Visible : Visibility.Collapsed;

                }
            }

            return DependencyProperty.UnsetValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language) { throw new NotImplementedException(); }
    }
}
