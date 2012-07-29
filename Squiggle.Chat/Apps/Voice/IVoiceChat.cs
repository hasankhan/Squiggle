using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Threading;
using Squiggle.Core.Chat;

namespace Squiggle.Chat.Apps.Voice
{
    public interface IVoiceChat: IAppHandler
    {
        Dispatcher Dispatcher { get; set; }
        bool IsMuted { get; set; }
        float Volume { get; set; }
        void Accept();
    }
}
