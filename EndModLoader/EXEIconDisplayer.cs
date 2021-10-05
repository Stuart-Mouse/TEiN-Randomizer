using System;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace TEiNRandomizer
{
    public class SettingsDisplayer : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value.Equals(false))
            {
                return Visibility.Collapsed;
            }
            else
            {
                return Visibility.Visible;
            }
        }

        //Visibility="{Binding UseCommonTileset, Converter={StaticResource SettingsDisplayer}}"

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            // You probably don't need this, this is used to convert the other way around
            // so from color to yes no or maybe
            throw new NotImplementedException();
        }
    }
}
