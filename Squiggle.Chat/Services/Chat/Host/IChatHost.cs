using System;
using System.Drawing;
using System.Net;
using System.ServiceModel;
using System.Collections.Generic;

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
        void ReceiveAppInvite(Guid sessionId, SquiggleEndPoint sender, SquiggleEndPoint recepient, Guid appId, Guid appSessionId, IEnumerable<KeyValuePair<string, string>> metadata);

        [OperationContract]
        void ReceiveAppData(Guid appSessionId, SquiggleEndPoint sender, SquiggleEndPoint recepient, byte[] chunk);

        [OperationContract]
        void AcceptAppInvite(Guid appSessionId, SquiggleEndPoint sender, SquiggleEndPoint recepient);

        [OperationContract]
        void CancelAppSession(Guid appSessionId, SquiggleEndPoint sender, SquiggleEndPoint recepient);
    }
}
