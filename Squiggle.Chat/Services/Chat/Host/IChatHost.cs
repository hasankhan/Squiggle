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
        SessionInfo GetSessionInfo(Guid sessionId, ChatEndPoint user);

        [OperationContract]
        void Buzz(Guid sessionId, ChatEndPoint user);

        [OperationContract]
        void UserIsTyping(Guid sessionId, ChatEndPoint user);

        [OperationContract]
        void ReceiveMessage(Guid sessionId, ChatEndPoint user, string fontName, int fontSize, Color color, FontStyle fontStyle, string message);

        [OperationContract]
        void ReceiveChatInvite(Guid sessionId, ChatEndPoint user, ChatEndPoint[] participants);

        [OperationContract]
        void JoinChat(Guid sessionId, ChatEndPoint user);

        [OperationContract]
        void LeaveChat(Guid sessionId, ChatEndPoint user);

        [OperationContract]
        void ReceiveFileInvite(Guid sessionId, ChatEndPoint user, Guid id, string name, long size);

        [OperationContract]
        void ReceiveFileContent(Guid id, byte[] chunk);

        [OperationContract]
        void AcceptFileInvite(Guid id);

        [OperationContract]
        void CancelFileTransfer(Guid id);
    }
}
