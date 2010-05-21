using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.Net;

namespace Squiggle.Chat.Services
{
    [ServiceContract]
    public interface IChatHost
    {
        [OperationContract]
        void UserIsTyping(IPEndPoint user);

        [OperationContract]
        void ReceiveMessage(IPEndPoint user, string message);
    }
}
