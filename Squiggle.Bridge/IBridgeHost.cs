using System.ServiceModel;
using Squiggle.Core.Presence.Transport.Host;
using Squiggle.Core.Chat.Host;
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
