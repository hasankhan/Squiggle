using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Squiggle.Core.Chat.Activity;

namespace Squiggle.Client.Activities
{
    public interface IVoiceChatHandler: IActivityHandler
    {
        bool IsMuted { get; set; }
        float Volume { get; set; }
    }
}
