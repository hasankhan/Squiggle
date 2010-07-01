using System;
using System.Drawing;
using System.Net;
using System.ServiceModel;

namespace Squiggle.Chat.Services.Chat.Host
{
    [ServiceContract]
    public interface IChatHost
    {
        [OperationContract]
        void Buzz(IPEndPoint user);

        [OperationContract]
        void UserIsTyping(IPEndPoint user);

        [OperationContract]
        void ReceiveMessage(IPEndPoint user, string fontName, int fontSize, Color color, FontStyle fontStyle, string message);

        [OperationContract]
        void ReceiveFileInvite(IPEndPoint user, Guid id, string name, int size);

        [OperationContract]
        void ReceiveFileContent(Guid id, byte[] chunk);

        [OperationContract]
        void AcceptFileInvite(Guid id);

        [OperationContract]
        void CancelFileTransfer(Guid id);
    }
}
