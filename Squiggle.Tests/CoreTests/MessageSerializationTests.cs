using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using FluentAssertions;
using ProtoBuf;
using Squiggle.Core;
using Squiggle.Core.Chat.Transport;
using Squiggle.Core.Chat.Transport.Messages;
using Xunit;

namespace Squiggle.Tests.CoreTests
{
    public class MessageSerializationTests
    {
        private static SquiggleEndPoint MakeSender()
            => new SquiggleEndPoint("sender1", new IPEndPoint(IPAddress.Parse("10.0.0.1"), 9000));

        private static SquiggleEndPoint MakeRecipient()
            => new SquiggleEndPoint("recipient1", new IPEndPoint(IPAddress.Parse("10.0.0.2"), 9001));

        private static T RoundTrip<T>(T message) where T : class
        {
            using var ms = new MemoryStream();
            Serializer.Serialize(ms, message);
            ms.Position = 0;
            return Serializer.Deserialize<T>(ms);
        }

        [Fact]
        public void BuzzMessage_RoundTrip_PreservesBaseFields()
        {
            var sessionId = Guid.NewGuid();
            var msg = new BuzzMessage
            {
                SessionId = sessionId,
                Sender = MakeSender(),
                Recipient = MakeRecipient()
            };

            // Serialize as base Message to test ProtoInclude polymorphism
            var deserialized = RoundTrip<Message>(msg);

            deserialized.Should().BeOfType<BuzzMessage>();
            deserialized.SessionId.Should().Be(sessionId);
            deserialized.Sender.ClientID.Should().Be("sender1");
            deserialized.Recipient.ClientID.Should().Be("recipient1");
        }

        [Fact]
        public void TextMessage_RoundTrip_PreservesAllFields()
        {
            var msgId = Guid.NewGuid();
            var sessionId = Guid.NewGuid();
            var msg = new TextMessage
            {
                SessionId = sessionId,
                Sender = MakeSender(),
                Recipient = MakeRecipient(),
                Id = msgId,
                FontName = "Arial",
                FontSize = 12,
                Message = "Assalamu Alaikum"
            };

            var deserialized = RoundTrip<Message>(msg);

            deserialized.Should().BeOfType<TextMessage>();
            var text = (TextMessage)deserialized;
            text.Id.Should().Be(msgId);
            text.FontName.Should().Be("Arial");
            text.FontSize.Should().Be(12);
            text.Message.Should().Be("Assalamu Alaikum");
            text.SessionId.Should().Be(sessionId);
        }

        [Fact]
        public void ChatInviteMessage_RoundTrip_PreservesParticipants()
        {
            var msg = new ChatInviteMessage
            {
                SessionId = Guid.NewGuid(),
                Sender = MakeSender(),
                Recipient = MakeRecipient(),
                Participants = new List<SquiggleEndPoint>
                {
                    new SquiggleEndPoint("p1", new IPEndPoint(IPAddress.Loopback, 3000)),
                    new SquiggleEndPoint("p2", new IPEndPoint(IPAddress.Loopback, 3001))
                }
            };

            var deserialized = RoundTrip<Message>(msg);

            deserialized.Should().BeOfType<ChatInviteMessage>();
            var invite = (ChatInviteMessage)deserialized;
            invite.Participants.Should().HaveCount(2);
            invite.Participants[0].ClientID.Should().Be("p1");
            invite.Participants[1].ClientID.Should().Be("p2");
        }

        [Fact]
        public void ChatInviteMessage_EmptyParticipants_RoundTrips()
        {
            var msg = new ChatInviteMessage
            {
                SessionId = Guid.NewGuid(),
                Sender = MakeSender(),
                Recipient = MakeRecipient()
            };

            var deserialized = RoundTrip<Message>(msg);

            deserialized.Should().BeOfType<ChatInviteMessage>();
            var invite = (ChatInviteMessage)deserialized;
            // Empty list may deserialize as empty or null depending on protobuf behavior
            if (invite.Participants != null)
                invite.Participants.Should().BeEmpty();
        }

        [Fact]
        public void Message_Polymorphism_DeserializesCorrectDerivedType()
        {
            // Serialize different message types as base Message
            var buzz = new BuzzMessage { SessionId = Guid.NewGuid(), Sender = MakeSender(), Recipient = MakeRecipient() };
            var text = new TextMessage { SessionId = Guid.NewGuid(), Sender = MakeSender(), Recipient = MakeRecipient(), Message = "test", FontName = "Consolas", FontSize = 10 };

            var buzzResult = RoundTrip<Message>(buzz);
            var textResult = RoundTrip<Message>(text);

            buzzResult.Should().BeOfType<BuzzMessage>();
            textResult.Should().BeOfType<TextMessage>();
        }
    }
}
