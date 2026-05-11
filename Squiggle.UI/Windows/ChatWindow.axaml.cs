using System;
using System.Collections.ObjectModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Squiggle.Client;

namespace Squiggle.UI.Windows;

public class ChatMessage
{
    public string SenderName { get; init; } = "";
    public string Text { get; init; } = "";
    public string Timestamp { get; init; } = "";
    public IBrush Background { get; init; } = Brushes.Transparent;
}

public partial class ChatWindow : Window
{
    private readonly IBuddy _buddy;
    private IChat? _chatSession;
    private readonly ObservableCollection<ChatMessage> _messages = new();

    public ChatWindow()
    {
        InitializeComponent();
        // Design-time only
        _buddy = null!;
    }

    public ChatWindow(IBuddy buddy, IChat? chatSession = null) : this()
    {
        _buddy = buddy;
        _chatSession = chatSession;
        Title = $"Chat - {buddy.DisplayName}";

        messagesControl.ItemsSource = _messages;

        if (_chatSession != null)
            SetupChatSession();
    }

    private void SetupChatSession()
    {
        if (_chatSession == null)
            return;

        _chatSession.MessageReceived += ChatSession_MessageReceived;
        _chatSession.BuddyTyping += ChatSession_BuddyTyping;
        _chatSession.BuddyJoined += ChatSession_BuddyJoined;
        _chatSession.BuddyLeft += ChatSession_BuddyLeft;
        _chatSession.MessageFailed += ChatSession_MessageFailed;
    }

    private void DetachChatSession()
    {
        if (_chatSession == null)
            return;

        _chatSession.MessageReceived -= ChatSession_MessageReceived;
        _chatSession.BuddyTyping -= ChatSession_BuddyTyping;
        _chatSession.BuddyJoined -= ChatSession_BuddyJoined;
        _chatSession.BuddyLeft -= ChatSession_BuddyLeft;
        _chatSession.MessageFailed -= ChatSession_MessageFailed;
    }

    public void SetChatSession(IChat chat)
    {
        DetachChatSession();
        _chatSession = chat;
        SetupChatSession();
    }

    private void ChatSession_MessageReceived(object? sender, ChatMessageReceivedEventArgs e)
    {
        Avalonia.Threading.Dispatcher.UIThread.Post(() =>
        {
            _messages.Add(new ChatMessage
            {
                SenderName = e.Sender.DisplayName,
                Text = e.Message,
                Timestamp = DateTime.Now.ToString("HH:mm"),
                Background = new SolidColorBrush(Color.Parse("#E3F2FD"))
            });
            ScrollToBottom();
            typingIndicator.IsVisible = false;
        });
    }

    private void ChatSession_BuddyTyping(object? sender, BuddyEventArgs e)
    {
        Avalonia.Threading.Dispatcher.UIThread.Post(() =>
        {
            typingIndicator.Text = $"{e.Buddy.DisplayName} {FindTranslation("ChatWindow_IsTyping", "is typing...")}";
            typingIndicator.IsVisible = true;
        });
    }

    private void ChatSession_BuddyJoined(object? sender, BuddyEventArgs e)
    {
        AddSystemMessage($"{e.Buddy.DisplayName} {FindTranslation("ChatWindow_HasJoinedConversation", "has joined the conversation.")}");
    }

    private void ChatSession_BuddyLeft(object? sender, BuddyEventArgs e)
    {
        AddSystemMessage($"{e.Buddy.DisplayName} {FindTranslation("ChatWindow_HasLeftConversation", "has left the conversation.")}");
    }

    private void ChatSession_MessageFailed(object? sender, MessageFailedEventArgs e)
    {
        AddSystemMessage($"{FindTranslation("ChatWindow_MessageCouldNotBeDelivered", "Message could not be delivered:")} {e.Message}");
    }

    private void SendButton_Click(object? sender, RoutedEventArgs e)
    {
        SendMessage();
    }

    private void TxtMessage_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter && !e.KeyModifiers.HasFlag(KeyModifiers.Shift))
        {
            e.Handled = true;
            SendMessage();
        }
        else
        {
            _chatSession?.NotifyTyping();
        }
    }

    private void SendMessage()
    {
        string message = txtMessage.Text?.Trim() ?? "";
        if (string.IsNullOrEmpty(message))
            return;

        _messages.Add(new ChatMessage
        {
            SenderName = FindTranslation("Global_You", "You"),
            Text = message,
            Timestamp = DateTime.Now.ToString("HH:mm"),
            Background = new SolidColorBrush(Color.Parse("#DCF8C6"))
        });

        _chatSession?.SendMessage(
            Guid.NewGuid(),
            "Segoe UI",
            12,
            System.Drawing.Color.Black,
            System.Drawing.FontStyle.Regular,
            message);

        txtMessage.Text = "";
        ScrollToBottom();
    }

    private void AddSystemMessage(string text)
    {
        Avalonia.Threading.Dispatcher.UIThread.Post(() =>
        {
            _messages.Add(new ChatMessage
            {
                SenderName = "",
                Text = text,
                Timestamp = DateTime.Now.ToString("HH:mm"),
                Background = new SolidColorBrush(Color.Parse("#F5F5F5"))
            });
            ScrollToBottom();
        });
    }

    private void ScrollToBottom()
    {
        messageScroller.Offset = new Vector(0, messageScroller.Extent.Height);
    }

    private void SendFile_Click(object? sender, RoutedEventArgs e)
    {
        // File transfer will be connected later
    }

    private void Emoticon_Click(object? sender, RoutedEventArgs e)
    {
        // Emoticon picker will be added later
    }

    private void Save_Click(object? sender, RoutedEventArgs e)
    {
        // Save conversation will be implemented later
    }

    private void Copy_Click(object? sender, RoutedEventArgs e)
    {
        // Copy will be implemented later
    }

    private void SelectAll_Click(object? sender, RoutedEventArgs e)
    {
        // Select all will be implemented later
    }

    private void CloseMenu_Click(object? sender, RoutedEventArgs e)
    {
        Close();
    }

    protected override void OnClosed(EventArgs e)
    {
        DetachChatSession();
        _chatSession?.Leave();
        base.OnClosed(e);
    }

    private string FindTranslation(string key, string fallback)
    {
        if (this.TryFindResource(key, out var value) && value is string s)
            return s;
        return fallback;
    }
}
