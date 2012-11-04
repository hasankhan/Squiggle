using Squiggle.Core.Chat.Activity;
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

namespace Squiggle.UI.Controls.ChatItems.Activity
{
    /// <summary>
    /// Interaction logic for GenericActivityControl.xaml
    /// </summary>
    public partial class GenericActivityControl : UserControl
    {
        IActivityHandler session;
        bool sending;

        public GenericActivityControl()
        {
            InitializeComponent();
        }

        public GenericActivityControl(IActivityHandler session, bool sending)
        {
            this.session = session;
            this.sending = sending;
        }
    }
}
