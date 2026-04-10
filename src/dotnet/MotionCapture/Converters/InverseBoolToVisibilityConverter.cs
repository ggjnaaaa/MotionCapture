using System.Globalization;
using System.Windows.Data;

namespace MotionCapture.Converters;

public class InverseBoolToVisibilityConverter : IValueConverter
{
    private readonly BoolToVisibilityConverter _visibilityConverter = new();
    private readonly InverseBoolConverter _inverseConverter = new();

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var inverted = _inverseConverter.Convert(value, typeof(bool), parameter, culture);
        return _visibilityConverter.Convert(inverted, targetType, parameter, culture);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}