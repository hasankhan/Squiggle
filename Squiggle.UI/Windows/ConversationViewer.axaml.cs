using System;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Microsoft.Extensions.DependencyInjection;
using Squiggle.History;
using Squiggle.History.DAL.Entities;

namespace Squiggle.UI.Windows;

public partial class ConversationViewer : Window
{
    public string SessionId { get; }

    public ConversationViewer()
    {
        InitializeComponent();
        SessionId = "";
    }

    public ConversationViewer(string sessionId) : this()
    {
        SessionId = sessionId;
        Loaded += async (_, _) => await LoadMessages();
    }

    private async Task LoadMessages()
    {
        var historyManager = App.Services.GetService<HistoryManager>();
        if (historyManager == null)
            return;

        var messages = await Task.Run(() =>
        {
            var session = historyManager.GetSession(SessionId);
            if (session == null)
                return [];

            return session.Events
                .Where(e => e.Type == EventType.Message)
                .OrderBy(e => e.Stamp)
                .Select(e => new MessageItem
                {
                    SenderName = e.SenderName,
                    Text = e.Data,
                    Timestamp = e.Stamp.ToLocalTime().ToString("g")
                })
                .ToList();
        });

        messagesList.ItemsSource = messages;
        if (messages.Count > 0)
            Title = $"Conversation - {messages.First().SenderName}";
    }

    private async void Export_Click(object? sender, RoutedEventArgs e)
    {
        var file = await StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = "Export Conversation",
            DefaultExtension = "txt",
            FileTypeChoices = new[]
            {
                new FilePickerFileType("Text Files") { Patterns = new[] { "*.txt" } }
            }
        });

        if (file != null)
        {
            var items = messagesList.ItemsSource?.Cast<MessageItem>() ?? Enumerable.Empty<MessageItem>();
            var lines = items.Select(m => $"[{m.Timestamp}] {m.SenderName}: {m.Text}");
            await System.IO.File.WriteAllLinesAsync(file.Path.LocalPath, lines);
        }
    }

    private void Close_Click(object? sender, RoutedEventArgs e)
    {
        Close();
    }
}

public class MessageItem
{
    public string SenderName { get; set; } = "";
    public string Text { get; set; } = "";
    public string Timestamp { get; set; } = "";
}
