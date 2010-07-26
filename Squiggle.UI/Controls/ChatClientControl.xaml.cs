using System.Windows.Controls;
using Squiggle.UI.ViewModel;

namespace Squiggle.UI.Controls
{
    /// <summary>
    /// Interaction logic for ChatClientControl.xaml
    /// </summary>
    public partial class ChatClientControl : UserControl
    {
        public ClientViewModel ChatContext
        {
            get { return ContactList.ChatContext; }
            set { ContactList.ChatContext = value; }
        }

        public ChatClientControl()
        {
           InitializeComponent();
        }
    }
}
