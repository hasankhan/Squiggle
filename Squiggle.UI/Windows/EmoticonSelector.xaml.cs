using System;
using System.Collections.Generic;
using System.Globalization;
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
using Squiggle.UI.StickyWindows;

namespace Squiggle.UI
{
    /// <summary>
    /// Interaction logic for EmoticonSelector.xaml
    /// </summary>
    public partial class EmoticonSelector : StickyWindow
    {
        public string Code { get; set; }

        public event EventHandler EmoticonSelected = delegate { };

        bool closing = false;

        public EmoticonSelector()
        {
            InitializeComponent();

            foreach (Emoticon emoticon in Emoticons.All)
            {
                var emoUi = new Controls.Emoticon();
                emoUi.DataContext = emoticon;
                string code = emoticon.Codes.FirstOrDefault();
                emoUi.MouseDown += (sender, e) =>
                {
                    Code = code;
                    EmoticonSelected(this, EventArgs.Empty);
                    Close();
                };
                root.Children.Add(emoUi);
            }
        }

        private void Window_Deactivated(object sender, EventArgs e)
        {            
            if (!closing)
                Close();
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
                Close();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            closing = true;
        }
    }
}
