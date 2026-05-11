using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace Squiggle.UI.Converters;

public class TitleCaseConverter : IValueConverter
{
    public static readonly TitleCaseConverter Instance = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value?.ToString() is string s ? culture.TextInfo.ToTitleCase(s) : value;

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
