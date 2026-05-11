using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using Avalonia.Threading;
using Squiggle.Client;

namespace Squiggle.UI.Avalonia.ViewModel;

public class ClientViewModel : ViewModelBase
{
    public event EventHandler ContactListUpdated = delegate { };

    private readonly IChatClient _chatClient;

    public ISelfBuddy LoggedInUser { get; set; } = null!;
    public ObservableCollection<IBuddy> Buddies { get; private set; }

    public string Title
    {
        get
        {
            if (IsLoggedIn)
                return $"Squiggle Messenger - {LoggedInUser.DisplayName}";
            return "Squiggle Messenger";
        }
    }

    public bool IsLoggedIn => _chatClient.IsLoggedIn;

    public bool AnyoneOnline => Buddies.Any(b => b.IsOnline());

    private string? _updateLink;
    public string? UpdateLink
    {
        get => _updateLink;
        set { Set(ref _updateLink, value); }
    }

    private ICommand? _cancelUpdateCommand;
    public ICommand? CancelUpdateCommand
    {
        get => _cancelUpdateCommand;
        set { Set(ref _cancelUpdateCommand, value); }
    }

    public ClientViewModel(IChatClient chatClient)
    {
        _chatClient = chatClient;
        LoggedInUser = chatClient.CurrentUser;

        chatClient.BuddyOnline += ChatClient_BuddyOnline;
        chatClient.BuddyOffline += ChatClient_BuddyOffline;
        chatClient.BuddyUpdated += ChatClient_BuddyUpdated;
        chatClient.LoggedIn += ChatClient_LoggedInOut;
        chatClient.LoggedOut += ChatClient_LoggedInOut;

        Buddies = new ObservableCollection<IBuddy>(chatClient.Buddies);
        Buddies.CollectionChanged += (_, _) => OnContactListUpdated();
    }

    private void ChatClient_LoggedInOut(object? sender, EventArgs e)
    {
        OnPropertyChanged(nameof(IsLoggedIn));
        OnPropertyChanged(nameof(Title));
    }

    private void ChatClient_BuddyOffline(object? sender, BuddyEventArgs e)
    {
        OnContactListUpdated();
    }

    private void ChatClient_BuddyUpdated(object? sender, BuddyEventArgs e)
    {
        OnContactListUpdated();
    }

    private void ChatClient_BuddyOnline(object? sender, BuddyOnlineEventArgs e)
    {
        if (Buddies.Contains(e.Buddy))
            OnContactListUpdated();
        else
            Dispatcher.UIThread.Post(() => Buddies.Add(e.Buddy));
    }

    private void OnContactListUpdated()
    {
        ContactListUpdated(this, EventArgs.Empty);
        OnPropertyChanged(nameof(AnyoneOnline));
    }
}
