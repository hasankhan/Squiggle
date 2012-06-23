using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Squiggle.Core.Chat;
using System.Windows.Threading;

namespace Squiggle.Core.Chat.Voice
{
    public interface IVoiceChat: IAppHandler
    {
        Dispatcher Dispatcher { get; set; }
        bool IsMuted { get; set; }
        float Volume { get; set; }
        void Accept();
    }
}
