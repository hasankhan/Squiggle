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

namespace Squiggle.UI
{
    /// <summary>
    /// Interaction logic for EmoticonSelector.xaml
    /// </summary>
    public partial class EmoticonSelector : Window
    {
        public string Code { get; set; }

        public event EventHandler EmoticonSelected = delegate { };

        bool closing = false;

        public EmoticonSelector()
        {
            InitializeComponent();

            foreach (Emoticon emoticon in Emoticons.All)
            {
                var border = new Border();
                border.BorderBrush = Brushes.LightGray;
                border.BorderThickness = new Thickness(0.5);
                border.Margin = new Thickness(3);
                border.MouseDown += (sender, e) =>
                {
                    Code = emoticon.Codes.FirstOrDefault();
                    EmoticonSelected(this, EventArgs.Empty);
                    Close();
                };
                var img = new Image();
                img.BeginInit();
                img.Source = new BitmapImage(emoticon.ImageUri);
                img.Stretch = Stretch.None;
                img.Margin = new Thickness(3);
                img.ToolTip = emoticon.Title + " " + emoticon.Codes.FirstOrDefault();
                img.EndInit();

                border.Child = img;
                root.Children.Add(border);
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
