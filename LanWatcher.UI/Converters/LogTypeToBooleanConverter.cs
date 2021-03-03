using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace LanWatcher.UI.Converters
{
    public enum LogTypeEnum
    {
        Core,
        DB,
        Scanner
    }

    public class LogTypeToBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            LogTypeEnum param = (LogTypeEnum)parameter;
            LogTypeEnum val = (LogTypeEnum)value;
            return val == param;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return null;
            }
            return (bool)value ? parameter : null;
        }
    }
}
