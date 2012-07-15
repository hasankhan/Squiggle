using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Squiggle.Core.Chat.Transport.Messages;
using System.Drawing;

namespace Squiggle.Core.Chat.Transport.Host
{
    public static class ChatHostExtensions
    {
        public static void GetSessionInfo(this IChatHost host, Guid sessionId, SquiggleEndPoint sender, SquiggleEndPoint recipient)
        {
            host.ReceiveChatMessage(recipient, new GiveSessionInfoMessage() { SessionId = sessionId, Sender = sender }.Serialize());
        }

        public static void ReceiveSessionInfo(this IChatHost host, Guid sessionId, SquiggleEndPoint sender, SquiggleEndPoint recipient, SquiggleEndPoint[] participants)
        {
            host.ReceiveChatMessage(recipient, new SessionInfoMessage() { SessionId = sessionId, Sender = sender, Participants = participants }.Serialize());
        }

        public static void Buzz(this IChatHost host, Guid sessionId, SquiggleEndPoint sender, SquiggleEndPoint recipient)
        {
            host.ReceiveChatMessage(recipient, new BuzzMessage() { SessionId = sessionId, Sender = sender }.Serialize());
        }

        public static void UserIsTyping(this IChatHost host, Guid sessionId, SquiggleEndPoint sender, SquiggleEndPoint recipient)
        {
            host.ReceiveChatMessage(recipient, new UserTypingMessage() { SessionId = sessionId, Sender = sender }.Serialize());
        }

        public static void ReceiveMessage(this IChatHost host, Guid sessionId, SquiggleEndPoint sender, SquiggleEndPoint recipient, string fontName, int fontSize, Color color, FontStyle fontStyle, string message)
        {
            host.ReceiveChatMessage(recipient, new TextMessage() { SessionId = sessionId, Sender = sender, FontName = fontName, FontSize = fontSize, Color = color, FontStyle = fontStyle, Message = message }.Serialize());
        }

        public static void ReceiveChatInvite(this IChatHost host, Guid sessionId, SquiggleEndPoint sender, SquiggleEndPoint recipient, SquiggleEndPoint[] participants)
        {
            host.ReceiveChatMessage(recipient, new ChatInviteMessage() { SessionId = sessionId, Sender = sender, Participants = participants }.Serialize());
        }

        public static void JoinChat(this IChatHost host, Guid sessionId, SquiggleEndPoint sender, SquiggleEndPoint recipient)
        {
            host.ReceiveChatMessage(recipient, new ChatJoinMessage() { SessionId = sessionId, Sender = sender }.Serialize());
        }

        public static void LeaveChat(this IChatHost host, Guid sessionId, SquiggleEndPoint sender, SquiggleEndPoint recipient)
        {
            host.ReceiveChatMessage(recipient, new ChatLeaveMessage() { SessionId = sessionId, Sender = sender }.Serialize());
        }

        public static void ReceiveAppInvite(this IChatHost host, Guid sessionId, SquiggleEndPoint sender, SquiggleEndPoint recipient, Guid appId, Guid appSessionId, IEnumerable<KeyValuePair<string, string>> metadata)
        {
            host.ReceiveChatMessage(recipient, new AppInvitationMessage() { SessionId = sessionId, Sender = sender, AppId = appId, AppSessionId = appSessionId, Metadata = metadata.ToDictionary(kv=>kv.Key, kv=>kv.Value) }.Serialize());
        }

        public static void ReceiveAppData(this IChatHost host, Guid appSessionId, SquiggleEndPoint sender, SquiggleEndPoint recipient, byte[] chunk)
        {
            host.ReceiveChatMessage(recipient, new AppDataMessage() { SessionId = appSessionId, Sender = sender, Data = chunk }.Serialize());
        }

        public static void AcceptAppInvite(this IChatHost host, Guid appSessionId, SquiggleEndPoint sender, SquiggleEndPoint recipient)
        {
            host.ReceiveChatMessage(recipient, new AppInvitationAcceptMessage() { SessionId = appSessionId, Sender = sender } .Serialize());
        }

        public static void CancelAppSession(this IChatHost host, Guid appSessionId, SquiggleEndPoint sender, SquiggleEndPoint recipient)
        {
            host.ReceiveChatMessage(recipient, new AppCancelMessage() { SessionId = appSessionId, Sender = sender }.Serialize());
        }
    }
}
