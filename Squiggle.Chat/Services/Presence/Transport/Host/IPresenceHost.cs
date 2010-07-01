using System.Net;
using System.ServiceModel;

namespace Squiggle.Chat.Services.Presence.Transport.Host
{
    [ServiceContract]
    public interface IPresenceHost
    {
        [OperationContract]
        UserInfo GetUserInfo();

        [OperationContract]
        void ReceiveMessage(IPEndPoint sender, byte[] message);
    }
}
