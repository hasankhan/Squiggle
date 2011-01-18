using System.ServiceModel;
using Squiggle.Chat.Services.Presence.Transport.Host;
using Squiggle.Chat.Services.Chat.Host;
using System.Net;

namespace Squiggle.Bridge
{
    [ServiceContract]
    public interface IBridgeHost: IPresenceHost, IChatHost
    {
        [OperationContract]
        void ForwardPresenceMessage(byte[] message, IPEndPoint bridgeEndPoint);
    }
}
