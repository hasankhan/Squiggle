using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Squiggle.History;
using Squiggle.History.DAL;
using System.Diagnostics;
using Squiggle.UI.StickyWindows;
using Squiggle.Utilities;
using Squiggle.UI.Resources;

namespace Squiggle.UI
{
    /// <summary>
    /// Interaction logic for HistoryViewer.xaml
    /// </summary>
    public partial class HistoryViewer : StickyWindow
    {
        Action lastSearch;

        public HistoryViewer()
        {
            InitializeComponent();
        }

        void Search_Click(object sender, RoutedEventArgs e)
        {
            DateTime? from;
            if (!GetDate(txtFrom.Text, out from))
                return;
            DateTime? to;
            if (!GetDate(txtTo.Text, out to))
                return;
            string message = txtMessage.Text;

            lastSearch = ()=>AsyncInvoke(()=>SearchSessions(from, to, message));
            lastSearch();
        }               

        private void results_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var source = (FrameworkElement)e.OriginalSource;
            var result = source.DataContext as Result;
            if (result != null)
            {
                var viewer = this.OwnedWindows.OfType<ConversationViewer>().FirstOrDefault(cv => cv.SessionId == result.Id);
                if (viewer == null)
                {
                    viewer = new ConversationViewer(result.Id);
                    viewer.Owner = this;
                    viewer.Show();
                }
                else
                    viewer.Activate();
            }
        }

        private void StickyWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
                Close();
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
                Participants = String.Join(", ", session.Participants.Select(p => p.ParticpantName).ToArray())
            }).ToList();

            Dispatcher.Invoke(() =>
            {
                results.ItemsSource = sessions;
            });
        }

        bool GetDate(string input, out DateTime? result)
        {
            result = null;
            DateTime date;
            if (input.Length > 0)
            {
                if (DateTime.TryParse(input, out date))
                    result = date;
                else
                {
                    MessageBox.Show(Translation.Instance.Error_InvalidDate);
                    return false;
                }
            }
            return true;
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
                AsyncInvoke(()=>
                {
                    var historyManager = new HistoryManager();
                    historyManager.Clear();
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
                try
                {
                    action();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            },
            () =>
            {
                onComplete();
                busyIndicator.IsBusy = false;
            }
            , Dispatcher);
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
    }
}
