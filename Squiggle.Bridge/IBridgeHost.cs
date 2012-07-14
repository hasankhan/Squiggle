using System.ServiceModel;
using Squiggle.Core.Presence.Transport.Host;
using Squiggle.Core.Chat.Host;
using System.Net;
using Squiggle.Core;

namespace Squiggle.Bridge
{
    [ServiceContract]
    public interface IBridgeHost: IChatHost
    {
        [OperationContract]
        void ForwardPresenceMessage(SquiggleEndPoint recepient, byte[] message, IPEndPoint bridgeEndPoint);
    }
}
