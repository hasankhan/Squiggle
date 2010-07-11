using System.Reflection;
using System.Windows;

namespace Squiggle.UI
{
    /// <summary>
    /// Interaction logic for About.xaml
    /// </summary>
    public partial class About : Window
    {
        public About()
        {
            InitializeComponent();

            txtVersion.Text = "Version "+Assembly.GetExecutingAssembly().GetName().Version.ToString() + " (BETA)";
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
