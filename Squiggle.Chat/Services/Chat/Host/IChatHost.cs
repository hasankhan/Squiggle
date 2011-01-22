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
        SessionInfo GetSessionInfo(Guid sessionId, SquiggleEndPoint sender, SquiggleEndPoint recepient);

        [OperationContract]
        void Buzz(Guid sessionId, SquiggleEndPoint sender, SquiggleEndPoint recepient);

        [OperationContract]
        void UserIsTyping(Guid sessionId, SquiggleEndPoint sender, SquiggleEndPoint recepient);

        [OperationContract]
        void ReceiveMessage(Guid sessionId, SquiggleEndPoint sender, SquiggleEndPoint recepient, string fontName, int fontSize, Color color, FontStyle fontStyle, string message);

        [OperationContract]
        void ReceiveChatInvite(Guid sessionId, SquiggleEndPoint sender, SquiggleEndPoint recepient, SquiggleEndPoint[] participants);

        [OperationContract]
        void JoinChat(Guid sessionId, SquiggleEndPoint sender, SquiggleEndPoint recepient);

        [OperationContract]
        void LeaveChat(Guid sessionId, SquiggleEndPoint sender, SquiggleEndPoint recepient);

        [OperationContract]
        void ReceiveFileInvite(Guid sessionId, SquiggleEndPoint sender, SquiggleEndPoint recepient, Guid id, string name, long size);

        [OperationContract]
        void ReceiveFileContent(Guid id, SquiggleEndPoint sender, SquiggleEndPoint recepient, byte[] chunk);

        [OperationContract]
        void AcceptFileInvite(Guid id, SquiggleEndPoint sender, SquiggleEndPoint recepient);

        [OperationContract]
        void CancelFileTransfer(Guid id, SquiggleEndPoint sender, SquiggleEndPoint recepient);
    }
}
