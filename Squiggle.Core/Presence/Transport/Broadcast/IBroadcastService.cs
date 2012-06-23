using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Squiggle.Core.Presence.Transport.Broadcast
{
    interface IBroadcastService
    {
        event EventHandler<MessageReceivedEventArgs> MessageReceived;
        void SendMessage(Message message);
        void Start();
        void Stop();
    }
}
