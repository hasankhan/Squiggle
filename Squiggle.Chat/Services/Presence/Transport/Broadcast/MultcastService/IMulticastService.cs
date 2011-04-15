using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Squiggle.Chat.Services.Presence.Transport.Messages;
using System.ServiceModel;

namespace Squiggle.Chat.Services.Presence.Transport.Broadcast.MultcastService
{
    [ServiceContract(CallbackContract=typeof(IMulticastServiceCallback))]
    public interface IMulticastService
    {
        [OperationContract]
        void RegisterClient();

        [OperationContract]
        void UnRegisterClient();

        [OperationContract]
        [ServiceKnownType(typeof(UserUpdateMessage))]
        [ServiceKnownType(typeof(LogoutMessage))]
        [ServiceKnownType(typeof(LoginMessage))]
        [ServiceKnownType(typeof(KeepAliveMessage))]
        void ForwardMessage(Message message);
    }
}
