using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Squiggle.Chat.Services.Chat;
using System.Windows.Threading;

namespace Squiggle.Chat
{
    public interface IVoiceChat: IAppHandler
    {
        Dispatcher Dispatcher { get; set; }
        bool IsMuted { get; set; }
        float Volume { get; set; }
        void Accept();
    }
}
