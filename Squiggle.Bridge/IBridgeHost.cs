using System.ServiceModel;

namespace Squiggle.Bridge
{
    [ServiceContract]
    public interface IBridgeHost
    {
        [OperationContract]
        void ReceiveMessage(byte[] message);
    }
}
