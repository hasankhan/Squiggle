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
using System.Windows.Navigation;
using System.Windows.Shapes;
using Squiggle.Chat;
using StackOverflowClient;
using System.Windows.Controls.Primitives;
using System.Net;
using System.Windows.Threading;

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
