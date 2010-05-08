using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;

namespace Squiggle.Chat.Service
{
    [ServiceContract]
    public interface IChatHost
    {
        [OperationContract]
        void ReceiveMessage(string user, string message);
    }
}
