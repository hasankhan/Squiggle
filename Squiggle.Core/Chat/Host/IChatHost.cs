using System;
using System.Drawing;
using System.Net;
using System.ServiceModel;
using System.Collections.Generic;

namespace Squiggle.Core.Chat.Host
{
    [ServiceContract]
    public interface IChatHost
    {
        [OperationContract]
        void GetSessionInfo(Guid sessionId, SquiggleEndPoint sender, SquiggleEndPoint recipient);

        [OperationContract]
        void ReceiveSessionInfo(Guid sessionId, SquiggleEndPoint sender, SquiggleEndPoint recipient, SessionInfo sessionInfo);

        [OperationContract]
        void Buzz(Guid sessionId, SquiggleEndPoint sender, SquiggleEndPoint recipient);

        [OperationContract]
        void UserIsTyping(Guid sessionId, SquiggleEndPoint sender, SquiggleEndPoint recipient);

        [OperationContract]
        void ReceiveMessage(Guid sessionId, SquiggleEndPoint sender, SquiggleEndPoint recipient, string fontName, int fontSize, Color color, FontStyle fontStyle, string message);

        [OperationContract]
        void ReceiveChatInvite(Guid sessionId, SquiggleEndPoint sender, SquiggleEndPoint recipient, SquiggleEndPoint[] participants);

        [OperationContract]
        void JoinChat(Guid sessionId, SquiggleEndPoint sender, SquiggleEndPoint recipient);

        [OperationContract]
        void LeaveChat(Guid sessionId, SquiggleEndPoint sender, SquiggleEndPoint recipient);

        [OperationContract]
        void ReceiveAppInvite(Guid sessionId, SquiggleEndPoint sender, SquiggleEndPoint recipient, Guid appId, Guid appSessionId, IEnumerable<KeyValuePair<string, string>> metadata);

        [OperationContract]
        void ReceiveAppData(Guid appSessionId, SquiggleEndPoint sender, SquiggleEndPoint recipient, byte[] chunk);

        [OperationContract]
        void AcceptAppInvite(Guid appSessionId, SquiggleEndPoint sender, SquiggleEndPoint recipient);

        [OperationContract]
        void CancelAppSession(Guid appSessionId, SquiggleEndPoint sender, SquiggleEndPoint recipient);
    }
}
