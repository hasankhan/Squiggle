using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using Squiggle.Chat.Services.Presence.Transport;

namespace Squiggle.Bridge
{
    [ServiceContract]
    public interface IBridgeHost
    {
        [OperationContract]
        void ReceiveMessage(byte[] message);
    }
}
