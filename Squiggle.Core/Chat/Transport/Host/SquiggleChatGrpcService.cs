using System;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Squiggle.Core.Chat.Transport.Grpc;

namespace Squiggle.Core.Chat.Transport.Host
{
    /// <summary>
    /// gRPC service implementation that receives incoming chat messages from peers
    /// and raises typed events on the owning ChatHost.
    /// </summary>
    class SquiggleChatGrpcService : SquiggleChat.SquiggleChatBase
    {
        readonly ChatHost chatHost;
        readonly ILogger logger;

        public SquiggleChatGrpcService(ChatHost chatHost, ILogger logger)
        {
            this.chatHost = chatHost;
            this.logger = logger;
        }

        public override Task<ChatResponse> SendChatMessage(ChatMessageEnvelope request, ServerCallContext context)
        {
            try
            {
                var sessionId = Guid.Parse(request.SessionId);
                var sender = ToSquiggleEndPoint(request.Sender);

                DispatchMessage(sessionId, sender, request);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error processing incoming gRPC chat message");
            }

            return Task.FromResult(new ChatResponse { Success = true });
        }

        void DispatchMessage(Guid sessionId, SquiggleEndPoint sender, ChatMessageEnvelope envelope)
        {
            switch (envelope.PayloadCase)
            {
                case ChatMessageEnvelope.PayloadOneofCase.TextMessage:
                    chatHost.OnGrpcTextMessageReceived(sessionId, sender, envelope.TextMessage);
                    break;
                case ChatMessageEnvelope.PayloadOneofCase.Buzz:
                    chatHost.OnGrpcBuzzReceived(sessionId, sender);
                    break;
                case ChatMessageEnvelope.PayloadOneofCase.ChatInvite:
                    chatHost.OnGrpcChatInviteReceived(sessionId, sender, envelope.ChatInvite);
                    break;
                case ChatMessageEnvelope.PayloadOneofCase.ChatJoin:
                    chatHost.OnGrpcUserJoined(sessionId, sender);
                    break;
                case ChatMessageEnvelope.PayloadOneofCase.ChatLeave:
                    chatHost.OnGrpcUserLeft(sessionId, sender);
                    break;
                case ChatMessageEnvelope.PayloadOneofCase.Typing:
                    chatHost.OnGrpcUserTyping(sessionId, sender);
                    break;
                case ChatMessageEnvelope.PayloadOneofCase.UpdateText:
                    chatHost.OnGrpcTextMessageUpdated(sessionId, sender, envelope.UpdateText);
                    break;
                case ChatMessageEnvelope.PayloadOneofCase.ActivityInvite:
                    chatHost.OnGrpcActivityInvitationReceived(sessionId, sender, envelope.ActivityInvite);
                    break;
                case ChatMessageEnvelope.PayloadOneofCase.ActivityAccept:
                    chatHost.OnGrpcActivityInvitationAccepted(sessionId, sender);
                    break;
                case ChatMessageEnvelope.PayloadOneofCase.ActivityCancel:
                    chatHost.OnGrpcActivitySessionCancelled(sessionId, sender);
                    break;
                case ChatMessageEnvelope.PayloadOneofCase.ActivityData:
                    chatHost.OnGrpcActivityDataReceived(sessionId, sender, envelope.ActivityData);
                    break;
                case ChatMessageEnvelope.PayloadOneofCase.GiveSessionInfo:
                    chatHost.OnGrpcSessionInfoRequested(sessionId, sender);
                    break;
                case ChatMessageEnvelope.PayloadOneofCase.SessionInfo:
                    chatHost.OnGrpcSessionInfoReceived(sessionId, sender, envelope.SessionInfo);
                    break;
                default:
                    logger.LogWarning("Unknown gRPC chat message payload type: {PayloadCase}", envelope.PayloadCase);
                    break;
            }
        }

        static SquiggleEndPoint ToSquiggleEndPoint(EndPointInfo info)
        {
            return new SquiggleEndPoint(info.ClientId, new IPEndPoint(IPAddress.Parse(info.Ip), info.Port));
        }
    }
}
