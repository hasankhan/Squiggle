using System.ServiceModel;
using Squiggle.Chat.Services.Presence.Transport.Host;
using Squiggle.Chat.Services.Chat.Host;

namespace Squiggle.Bridge
{
    [ServiceContract]
    public interface IBridgeHost: IPresenceHost, IChatHost
    {
        [OperationContract]
        void ReceiveMessage(byte[] message);
    }
}
