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
using Squiggle.Chat.History;
using Squiggle.Chat.History.DAL;
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
        public HistoryViewer()
        {
            InitializeComponent();
        }

        private void StickyWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
                Close();
        }
    }
}
