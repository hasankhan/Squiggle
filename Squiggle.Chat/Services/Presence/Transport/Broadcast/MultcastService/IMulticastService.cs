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
        [OperationContract(IsOneWay = true)]
        void RegisterClient();

        [OperationContract(IsOneWay = true)]
        void UnRegisterClient();

        [OperationContract(IsOneWay = true)]
        [ServiceKnownType(typeof(UserUpdateMessage))]
        [ServiceKnownType(typeof(LogoutMessage))]
        [ServiceKnownType(typeof(LoginMessage))]
        [ServiceKnownType(typeof(KeepAliveMessage))]
        void ForwardMessage(Message message);
    }
}
