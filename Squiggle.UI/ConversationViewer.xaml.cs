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

namespace Squiggle.UI
{
    /// <summary>
    /// Interaction logic for Conversation.xaml
    /// </summary>
    public partial class ConversationViewer : Window
    {
        public ConversationViewer()
        {
            InitializeComponent();
        }

        public ConversationViewer(Guid sessionId): this()
        {
            var historyManager = new HistoryManager();
            var events = historyManager.GetEvents(sessionId);
            messages.ItemsSource = events;

        }
    }
}
