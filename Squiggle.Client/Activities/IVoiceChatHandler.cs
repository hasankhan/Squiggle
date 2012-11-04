using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Threading;
using Squiggle.Core.Chat.Activity;

namespace Squiggle.Client.Activities
{
    public interface IVoiceChatHandler: IActivityHandler
    {
        Dispatcher Dispatcher { get; set; }
        bool IsMuted { get; set; }
        float Volume { get; set; }
    }
}
