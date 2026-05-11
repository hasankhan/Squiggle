using System;
using System.Runtime.Versioning;
using Microsoft.Win32;

namespace Squiggle.UI.Avalonia.Services;

[SupportedOSPlatform("windows")]
public class WindowsAutoStartService : IAutoStartService
{
    private const string RegistryKeyPath = @"Software\Microsoft\Windows\CurrentVersion\Run";
    private const string AppName = "Squiggle";

    private static string AppPath => Environment.ProcessPath ?? string.Empty;

    public bool IsEnabled
    {
        get
        {
            using var key = Registry.CurrentUser.OpenSubKey(RegistryKeyPath, false);
            return key?.GetValue(AppName) is string value
                   && string.Equals(value, AppPath, StringComparison.OrdinalIgnoreCase);
        }
    }

    public void Enable()
    {
        using var key = Registry.CurrentUser.OpenSubKey(RegistryKeyPath, true);
        key?.SetValue(AppName, AppPath);
    }

    public void Disable()
    {
        using var key = Registry.CurrentUser.OpenSubKey(RegistryKeyPath, true);
        key?.DeleteValue(AppName, throwOnMissingValue: false);
    }
}
