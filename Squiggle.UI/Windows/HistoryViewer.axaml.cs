using System;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Microsoft.Extensions.DependencyInjection;
using Squiggle.History;
using Squiggle.History.DAL;

namespace Squiggle.UI.Windows;

public partial class HistoryViewer : Window
{
    public HistoryViewer()
    {
        InitializeComponent();
    }

    private async void Search_Click(object? sender, RoutedEventArgs e)
    {
        var from = txtFrom.SelectedDate;
        var to = txtTo.SelectedDate;
        var message = txtMessage.Text;

        await SearchAsync(from?.DateTime, to?.DateTime, message);
    }

    private async Task SearchAsync(DateTime? from, DateTime? to, string? message)
    {
        var historyManager = App.Services.GetService<HistoryManager>();
        if (historyManager == null)
            return;

        var sessions = await Task.Run(() =>
        {
            return historyManager.GetSessions(new SessionCriteria
            {
                From = from?.ToUniversalTime(),
                To = to?.ToUniversalTime(),
                Text = string.IsNullOrEmpty(message) ? null : message,
            })
            .Select(s => new HistoryResult
            {
                Id = s.Id,
                Start = s.Start.ToLocalTime(),
                End = s.End?.ToLocalTime(),
                Participants = string.Join(", ", s.Participants.Select(p => p.ContactName))
            })
            .ToList();
        });

        resultsGrid.ItemsSource = sessions;
    }

    private void Results_DoubleTapped(object? sender, TappedEventArgs e)
    {
        if (resultsGrid.SelectedItem is HistoryResult result)
        {
            var viewer = new ConversationViewer(result.Id);
            viewer.ShowDialog(this);
        }
    }

    private void Clear_Click(object? sender, RoutedEventArgs e)
    {
        txtFrom.SelectedDate = null;
        txtTo.SelectedDate = null;
        txtMessage.Text = "";
        resultsGrid.ItemsSource = null;
    }
}

public class HistoryResult
{
    public string Id { get; set; } = "";
    public DateTime Start { get; set; }
    public DateTime? End { get; set; }
    public string Participants { get; set; } = "";
}
