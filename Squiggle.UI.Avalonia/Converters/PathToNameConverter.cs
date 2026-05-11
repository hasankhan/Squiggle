using System;
using System.Globalization;
using System.IO;
using Avalonia.Data.Converters;

namespace Squiggle.UI.Avalonia.Converters;

public class PathToNameConverter : IValueConverter
{
    public static readonly PathToNameConverter Instance = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is string path ? Path.GetFileName(path) : value;

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
