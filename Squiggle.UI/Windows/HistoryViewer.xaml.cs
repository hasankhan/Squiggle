using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using Squiggle.UI.Resources;
using Squiggle.UI.StickyWindows;
using Squiggle.Utilities;

namespace Squiggle.UI.Windows
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
