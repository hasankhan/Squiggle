using System.Net;
using System.ServiceModel;
using Squiggle.Chat.Services.Chat;

namespace Squiggle.Chat.Services.Presence.Transport.Host
{
    [ServiceContract]
    public interface IPresenceHost
    {
        [OperationContract]
        UserInfo GetUserInfo(ChatEndPoint user);

        [OperationContract]
        void ReceivePresenceMessage(ChatEndPoint sender, ChatEndPoint recepient, byte[] message);
    }
}
