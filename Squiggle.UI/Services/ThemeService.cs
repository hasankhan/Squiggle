using Avalonia;
using Avalonia.Styling;

namespace Squiggle.UI.Services;

public class ThemeService
{
    public void ApplyTheme(string mode)
    {
        var app = Avalonia.Application.Current;
        if (app == null) return;

        app.RequestedThemeVariant = mode switch
        {
            "Dark" => ThemeVariant.Dark,
            "Light" => ThemeVariant.Light,
            _ => ThemeVariant.Default
        };
    }

    public string GetCurrentTheme()
    {
        var app = Avalonia.Application.Current;
        if (app?.RequestedThemeVariant == ThemeVariant.Dark) return "Dark";
        if (app?.RequestedThemeVariant == ThemeVariant.Light) return "Light";
        return "System";
    }
}
