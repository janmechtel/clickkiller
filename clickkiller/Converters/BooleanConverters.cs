using Avalonia.Data.Converters;
using Avalonia.Media;
using System;
using System.Globalization;

namespace clickkiller.Converters
{
    public class BooleanToTextDecorationConverter : IValueConverter
    {
        public static readonly BooleanToTextDecorationConverter Instance = new BooleanToTextDecorationConverter();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (bool)value ? TextDecorations.Strikethrough : null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class BooleanToOpacityConverter : IValueConverter
    {
        public static readonly BooleanToOpacityConverter Instance = new BooleanToOpacityConverter();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (bool)value ? 0.5 : 1.0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
