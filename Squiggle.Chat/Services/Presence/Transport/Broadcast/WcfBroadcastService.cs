using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;

namespace Squiggle.Chat.Services.Presence.Transport.Broadcast
{
    class WcfBroadcastService: IBroadcastService
    {
        public event EventHandler<MessageReceivedEventArgs> MessageReceived = delegate { };

        public WcfBroadcastService(IPEndPoint server)
        {

        }

        public void SendMessage(Message message)
        {
            throw new NotImplementedException();
        }

        public void Start()
        {
            throw new NotImplementedException();
        }

        public void Stop()
        {
            throw new NotImplementedException();
        }
    }
}
