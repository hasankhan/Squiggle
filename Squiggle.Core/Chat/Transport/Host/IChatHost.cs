using System;
using System.Collections.Generic;
using System.Drawing;
using System.Net;
using System.ServiceModel;

namespace Squiggle.Core.Chat.Transport.Host
{
    [ServiceContract]
    public interface IChatHost
    {
        [OperationContract]
        void ReceiveChatMessage(byte[] message);
    }
}
