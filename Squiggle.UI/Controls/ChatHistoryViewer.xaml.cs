using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Squiggle.History;
using Squiggle.History.DAL;
using Squiggle.UI.Helpers;
using Squiggle.UI.Resources;
using Squiggle.UI.Windows;
using Squiggle.Utilities;
using Squiggle.Utilities.Threading;

namespace Squiggle.UI.Controls
{
    /// <summary>
    /// Interaction logic for ChatHistoryViewer.xaml
    /// </summary>
    public partial class ChatHistoryViewer : UserControl
    {
        Action lastSearch;

        public ChatHistoryViewer()
        {
            InitializeComponent();
        }

        void Search_Click(object sender, RoutedEventArgs e)
        {
            DateTime? from = txtFrom.SelectedDate;
            DateTime? to = txtTo.SelectedDate;
            string message = txtMessage.Text;

            lastSearch = () => AsyncInvoke(() => SearchSessions(from, to, message));
            lastSearch();
        }

        private void results_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var source = (FrameworkElement)e.OriginalSource;
            var result = source.DataContext as Result;
            var window = this.GetVisualParent<Window>();
            if (result != null)
            {
                var viewer = window.OwnedWindows.OfType<ConversationViewer>().FirstOrDefault(cv => cv.SessionId == result.Id);
                if (viewer == null)
                {
                    viewer = new ConversationViewer(result.Id);
                    viewer.Owner = window;
                    viewer.Show();
                }
                else
                    viewer.Activate();
            }
        }

        void SearchSessions(DateTime? from, DateTime? to, string message)
        {
            var historyManager = new HistoryManager();
            var sessions = historyManager.GetSessions(new SessionCriteria()
            {
                From = from,
                To = to,
                Text = message.Length == 0 ? null : message,
            }).Select(session => new Result()
            {
                Id = session.Id,
                Start = session.Start,
                End = session.End,
                Participants = String.Join(", ", session.Participants.Select(p => p.ContactName).ToArray())
            }).ToList();

            Dispatcher.Invoke(() =>
            {
                results.ItemsSource = sessions;
            });
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show(Translation.Instance.HistoryViewer_ConfirmDelete, "Squiggle", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                IEnumerable<Guid> sessionIds = results.SelectedItems.Cast<Result>().Select(r => r.Id).ToList();
                AsyncInvoke(() =>
                {
                    var historyManager = new HistoryManager();
                    historyManager.DeleteSessions(sessionIds);
                },
                lastSearch);
            }
        }

        private void Clear_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show(Translation.Instance.HistoryViewer_ConfirmClear, "Squiggle", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                AsyncInvoke(() =>
                {
                    var historyManager = new HistoryManager();
                    historyManager.ClearChatHistory();
                }, () => results.ItemsSource = null);
            }
        }

        void AsyncInvoke(Action action)
        {
            AsyncInvoke(action, () => { });
        }

        void AsyncInvoke(Action action, Action onComplete)
        {
            busyIndicator.IsBusy = true;
            Async.Invoke(() =>
            {
                Exception ex;
                if (!ExceptionMonster.EatTheException(action, "searching history", out ex))                
                    MessageBox.Show(ex.Message);
            },
            () =>
            {
                onComplete();
                busyIndicator.IsBusy = false;
            });
        }

        private void results_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            deleteMenuItem.IsEnabled = results.SelectedItems.Count > 0;
        }

        class Result
        {
            public Guid Id { get; set; }
            public DateTime Start { get; set; }
            public DateTime? End { get; set; }
            public string Participants { get; set; }
        }

        private void UserControl_GotFocus(object sender, RoutedEventArgs e)
        {
            if (e.OriginalSource == this)
                txtMessage.Focus();
        }
    }
}
