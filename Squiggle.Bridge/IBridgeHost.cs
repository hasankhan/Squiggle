using System.Net;
using System.ServiceModel;
using Squiggle.Core;
using Squiggle.Core.Chat.Transport.Host;
using Squiggle.Core.Presence.Transport.Host;

namespace Squiggle.Bridge
{
    [ServiceContract]
    public interface IBridgeHost: IChatHost
    {
        [OperationContract]
        void ForwardPresenceMessage(SquiggleEndPoint recipient, byte[] message, IPEndPoint bridgeEndPoint);
    }
}
