using System.Globalization;
using Microsoft.Maui.Controls;

namespace FMMS.Helpers;

/// <summary>
/// Converts count to visibility - visible if count > 0
/// </summary>
public class CountToVisibilityConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is int count)
        {
            return count > 0;
        }
        return false;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// Converts count to inverse visibility - visible if count == 0
/// </summary>
public class CountToBoolInverseConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is int count)
        {
            return count == 0;
        }
        return true; // Show message if we can't determine count
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

