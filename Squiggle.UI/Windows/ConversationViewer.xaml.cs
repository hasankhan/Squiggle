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
using Squiggle.History.DAL.Entities;
using Squiggle.UI.StickyWindow;

namespace Squiggle.UI.Windows
{
    /// <summary>
    /// Interaction logic for Conversation.xaml
    /// </summary>
    public partial class ConversationViewer : StickyWindowBase
    {
        public Guid SessionId { get; private set; }

        public ConversationViewer()
        {
            InitializeComponent();
        }

        public ConversationViewer(Guid sessionId): this()
        {
            this.SessionId = sessionId;
            var historyManager = new HistoryManager();
            Session session = historyManager.GetSession(sessionId);
            messages.ItemsSource = session.Events.OrderBy(e=>e.Stamp).ToList();
        }

        private void StickyWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
                Close();
        }
    }
}
