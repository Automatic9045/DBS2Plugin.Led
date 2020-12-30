using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Markup;

namespace DbsPlugin.Standard.Led
{
    public class StringToColorConverter : MarkupExtension, IValueConverter
    {
        private StringToColorConverter thisConverter;
        private BrushConverter brushConverter = new BrushConverter();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                return brushConverter.ConvertFromString((string)value);
            }
            catch (FormatException)
            {
                try
                {
                    return brushConverter.ConvertFromString("#" + (string)value);
                }
                catch (FormatException) { return Brushes.Transparent; }
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                return brushConverter.ConvertToString((SolidColorBrush)value);
            }
            catch (FormatException)
            {
                try
                {
                    return brushConverter.ConvertToString((SolidColorBrush)value);
                }
                catch (FormatException) { return Binding.DoNothing; }
            }
        }

        public override object ProvideValue(IServiceProvider serviceProvider) => thisConverter ?? (thisConverter = new StringToColorConverter());
    }
}
