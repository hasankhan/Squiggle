using System;
using System.Drawing;
using System.Net;
using System.ServiceModel;
using System.Collections.Generic;

namespace Squiggle.Core.Chat.Transport.Host
{
    [ServiceContract]
    public interface IChatHost
    {
        [OperationContract]
        void ReceiveChatMessage(SquiggleEndPoint recipient, byte[] message);
    }
}
