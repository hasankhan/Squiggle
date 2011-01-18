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
        SessionInfo GetSessionInfo(Guid sessionId, ChatEndPoint sender, ChatEndPoint recepient);

        [OperationContract]
        void Buzz(Guid sessionId, ChatEndPoint sender, ChatEndPoint recepient);

        [OperationContract]
        void UserIsTyping(Guid sessionId, ChatEndPoint sender, ChatEndPoint recepient);

        [OperationContract]
        void ReceiveMessage(Guid sessionId, ChatEndPoint sender, ChatEndPoint recepient, string fontName, int fontSize, Color color, FontStyle fontStyle, string message);

        [OperationContract]
        void ReceiveChatInvite(Guid sessionId, ChatEndPoint sender, ChatEndPoint recepient, ChatEndPoint[] participants);

        [OperationContract]
        void JoinChat(Guid sessionId, ChatEndPoint sender, ChatEndPoint recepient);

        [OperationContract]
        void LeaveChat(Guid sessionId, ChatEndPoint sender, ChatEndPoint recepient);

        [OperationContract]
        void ReceiveFileInvite(Guid sessionId, ChatEndPoint sender, ChatEndPoint recepient, Guid id, string name, long size);

        [OperationContract]
        void ReceiveFileContent(Guid id, ChatEndPoint sender, ChatEndPoint recepient, byte[] chunk);

        [OperationContract]
        void AcceptFileInvite(Guid id, ChatEndPoint sender, ChatEndPoint recepient);

        [OperationContract]
        void CancelFileTransfer(Guid id, ChatEndPoint sender, ChatEndPoint recepient);
    }
}
