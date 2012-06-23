using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using Squiggle.Core.Presence.Transport.Messages;

namespace Squiggle.Core.Presence.Transport.Broadcast.MultcastService
{
    public interface IMulticastServiceCallback
    {
        [OperationContract(IsOneWay = true)]
        [ServiceKnownType(typeof(UserUpdateMessage))]
        [ServiceKnownType(typeof(LogoutMessage))]
        [ServiceKnownType(typeof(LoginMessage))]
        [ServiceKnownType(typeof(KeepAliveMessage))]
        void MessageForwarded(Message message);
    }
}
