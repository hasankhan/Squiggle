using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using Squiggle.Utilities;

namespace Squiggle.Multicast
{
    public partial class SquiggleMulticastService : ConsoleService
    {
        public SquiggleMulticastService()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
        }

        protected override void OnStop()
        {
        }
    }
}
