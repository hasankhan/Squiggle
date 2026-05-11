using Avalonia.Controls;
using Avalonia.Interactivity;
using Squiggle.UI.Services;

namespace Squiggle.UI.Windows;

public partial class MessageBoxWindow : Window
{
    private readonly MessageBoxButton _buttons;

    public MessageBoxWindow()
    {
        InitializeComponent();
        _buttons = MessageBoxButton.Ok;
    }

    public MessageBoxWindow(string title, string message, MessageBoxButton buttons)
    {
        InitializeComponent();
        _buttons = buttons;

        Title = title;
        txtMessage.Text = message;

        switch (buttons)
        {
            case MessageBoxButton.Ok:
                btnPrimary.Content = "OK";
                break;
            case MessageBoxButton.OkCancel:
                btnPrimary.Content = "OK";
                btnSecondary.Content = "Cancel";
                btnSecondary.IsVisible = true;
                break;
            case MessageBoxButton.YesNo:
                btnPrimary.Content = "Yes";
                btnSecondary.Content = "No";
                btnSecondary.IsVisible = true;
                break;
        }
    }

    private void PrimaryButton_Click(object? sender, RoutedEventArgs e)
    {
        var result = _buttons switch
        {
            MessageBoxButton.YesNo => MessageBoxResult.Yes,
            _ => MessageBoxResult.Ok,
        };
        Close(result);
    }

    private void SecondaryButton_Click(object? sender, RoutedEventArgs e)
    {
        var result = _buttons switch
        {
            MessageBoxButton.YesNo => MessageBoxResult.No,
            _ => MessageBoxResult.Cancel,
        };
        Close(result);
    }
}
