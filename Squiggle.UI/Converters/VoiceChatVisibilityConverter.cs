using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace Squiggle.UI.Converters;

public class VoiceChatVisibilityConverter : IValueConverter
{
    public static readonly VoiceChatVisibilityConverter Instance = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        string param = parameter?.ToString() ?? "";
        bool hasVoiceChat = value != null;

        return param switch
        {
            "voicechaton" => hasVoiceChat,
            "voicechatoff" => !hasVoiceChat,
            _ => false
        };
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
