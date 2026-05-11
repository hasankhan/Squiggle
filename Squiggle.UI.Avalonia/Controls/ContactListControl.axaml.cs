using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Squiggle.Client;

namespace Squiggle.UI.Avalonia.Controls;

public partial class ContactListControl : UserControl
{
    public ContactListControl()
    {
        InitializeComponent();
    }

    private void ContactsList_DoubleTapped(object? sender, TappedEventArgs e)
    {
        if (contactsList.SelectedItem is IBuddy buddy)
        {
            // Chat window will be implemented in #51
        }
    }

    private void StartChat_Click(object? sender, RoutedEventArgs e)
    {
        if (contactsList.SelectedItem is IBuddy buddy)
        {
            // Chat window will be implemented in #51
        }
    }

    private void SendFile_Click(object? sender, RoutedEventArgs e)
    {
        // File send will be implemented later
    }

    private void SendEmail_Click(object? sender, RoutedEventArgs e)
    {
        // Email will be implemented later
    }
}
