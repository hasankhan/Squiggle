using System;
using Avalonia.Controls;
using Avalonia.Interactivity;

namespace Squiggle.UI.Controls;

public partial class FilterTextBox : UserControl
{
    public event EventHandler<string>? FilterChanged;

    public FilterTextBox()
    {
        InitializeComponent();
    }

    private void TxtFilter_TextChanged(object? sender, TextChangedEventArgs e)
    {
        FilterChanged?.Invoke(this, txtFilter.Text ?? "");
    }

    private void ClearButton_Click(object? sender, RoutedEventArgs e)
    {
        txtFilter.Text = "";
    }
}
