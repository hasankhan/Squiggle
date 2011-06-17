using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Squiggle.Chat.Services.Chat;

namespace Squiggle.Chat
{
    public interface IVoiceChat: IAppHandler
    {
        void Accept();
    }
}
