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

namespace Squiggle.UI
{
    /// <summary>
    /// Interaction logic for HistoryViewer.xaml
    /// </summary>
    public partial class HistoryViewer : Window
    {
        public HistoryViewer()
        {
            InitializeComponent();
        }

        void Search_Click(object sender, RoutedEventArgs e)
        {
            var historyManager = new HistoryManager();
            DateTime? from;
            if (!GetDate(txtFrom.Text, out from))
                return;
            DateTime? to;
            if (!GetDate(txtTo.Text, out to))
                return;
            string message = txtMessage.Text;

            var conversations = historyManager.GetConversations(new ConversationCriteria()
            {
                From = from,
                To = to,
                Text = message.Length == 0 ? null : message,
            });

            results.ItemsSource = conversations.Select(c => new Result()
            {
                Id = c.Id,
                Start = c.Start,
                End = c.End,
                Participants = String.Join(", ", c.Participants.Select(p=>p.Name).ToArray())
            }).ToList();
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
                    MessageBox.Show("Please enter a valid date.");
                    return false;
                }
            }
            return true;
        }

        class Result
        {
            public Guid Id { get; set; }
            public DateTime Start { get; set; }
            public DateTime End { get; set; }
            public string Participants { get; set; }
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
    }
}
