using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace GithubDisplay.Converters
{
    public class InvBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is bool b)
            {
                return !b;
            }

            return DependencyProperty.UnsetValue;
        }
        public object ConvertBack(object value, Type targetType, object parameter, string language) { throw new NotImplementedException(); }
    }
}
