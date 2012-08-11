using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Threading;

namespace Squiggle.Activities
{
    public interface IVoiceChatHandler: IActivityHandler
    {
        Dispatcher Dispatcher { get; set; }
        bool IsMuted { get; set; }
        float Volume { get; set; }
        void Accept();
    }
}
