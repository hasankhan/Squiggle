using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using Squiggle.Core.Chat.Transport.Messages;

namespace Squiggle.Core.Chat.Transport.Host
{
    public static class ChatHostExtensions
    {
        public static void GetSessionInfo(this IChatHost host, Guid sessionId, SquiggleEndPoint sender, SquiggleEndPoint recipient)
        {
            host.ReceiveChatMessage(new GiveSessionInfoMessage() { SessionId = sessionId, Sender = sender, Recipient = recipient }.Serialize());
        }

        public static void ReceiveSessionInfo(this IChatHost host, Guid sessionId, SquiggleEndPoint sender, SquiggleEndPoint recipient, SquiggleEndPoint[] participants)
        {
            host.ReceiveChatMessage(new SessionInfoMessage() { SessionId = sessionId, Sender = sender, Recipient = recipient, Participants = participants.ToList() }.Serialize());
        }

        public static void Buzz(this IChatHost host, Guid sessionId, SquiggleEndPoint sender, SquiggleEndPoint recipient)
        {
            host.ReceiveChatMessage(new BuzzMessage() { SessionId = sessionId, Sender = sender, Recipient = recipient }.Serialize());
        }

        public static void UserIsTyping(this IChatHost host, Guid sessionId, SquiggleEndPoint sender, SquiggleEndPoint recipient)
        {
            host.ReceiveChatMessage(new UserTypingMessage() { SessionId = sessionId, Sender = sender, Recipient = recipient }.Serialize());
        }

        public static void ReceiveMessage(this IChatHost host, Guid sessionId, SquiggleEndPoint sender, SquiggleEndPoint recipient, string fontName, int fontSize, Color color, FontStyle fontStyle, string message)
        {
            host.ReceiveChatMessage(new TextMessage() { SessionId = sessionId, Sender = sender, Recipient = recipient, FontName = fontName, FontSize = fontSize, Color = color, FontStyle = fontStyle, Message = message }.Serialize());
        }

        public static void ReceiveChatInvite(this IChatHost host, Guid sessionId, SquiggleEndPoint sender, SquiggleEndPoint recipient, IEnumerable<SquiggleEndPoint> participants)
        {
            host.ReceiveChatMessage(new ChatInviteMessage() { SessionId = sessionId, Sender = sender, Recipient = recipient, Participants = participants.ToList() }.Serialize());
        }

        public static void JoinChat(this IChatHost host, Guid sessionId, SquiggleEndPoint sender, SquiggleEndPoint recipient)
        {
            host.ReceiveChatMessage(new ChatJoinMessage() { SessionId = sessionId, Sender = sender, Recipient = recipient }.Serialize());
        }

        public static void LeaveChat(this IChatHost host, Guid sessionId, SquiggleEndPoint sender, SquiggleEndPoint recipient)
        {
            host.ReceiveChatMessage(new ChatLeaveMessage() { SessionId = sessionId, Sender = sender, Recipient = recipient }.Serialize());
        }

        public static void ReceiveAppInvite(this IChatHost host, Guid sessionId, SquiggleEndPoint sender, SquiggleEndPoint recipient, Guid appId, Guid appSessionId, IEnumerable<KeyValuePair<string, string>> metadata)
        {
            host.ReceiveChatMessage(new AppInviteMessage() { SessionId = sessionId, Sender = sender, Recipient = recipient, AppId = appId, AppSessionId = appSessionId, Metadata = metadata.ToDictionary(kv => kv.Key, kv => kv.Value) }.Serialize());
        }

        public static void ReceiveAppData(this IChatHost host, Guid appSessionId, SquiggleEndPoint sender, SquiggleEndPoint recipient, byte[] chunk)
        {
            host.ReceiveChatMessage(new AppDataMessage() { SessionId = appSessionId, Sender = sender, Recipient = recipient, Data = chunk }.Serialize());
        }

        public static void AcceptAppInvite(this IChatHost host, Guid appSessionId, SquiggleEndPoint sender, SquiggleEndPoint recipient)
        {
            host.ReceiveChatMessage(new AppInviteAcceptMessage() { SessionId = appSessionId, Sender = sender, Recipient = recipient }.Serialize());
        }

        public static void CancelAppSession(this IChatHost host, Guid appSessionId, SquiggleEndPoint sender, SquiggleEndPoint recipient)
        {
            host.ReceiveChatMessage(new AppCancelMessage() { SessionId = appSessionId, Sender = sender, Recipient = recipient }.Serialize());
        }
    }
}
