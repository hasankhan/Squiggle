using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf;
using Grpc.Net.Client;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Squiggle.Core.Chat.Encryption;
using Squiggle.Core.Chat.Transport.Grpc;
using Squiggle.Core.Chat.Transport.Messages;
using Squiggle.Utilities;

namespace Squiggle.Core.Chat.Transport.Host
{
    
    public class ChatHost: IDisposable
    {
        IPEndPoint endpoint;
        bool useQuic;
        WebApplication? grpcApp;
        readonly ConcurrentDictionary<string, GrpcChannel> channels = new();
        readonly ILogger<ChatHost> logger;
        readonly EncryptionManager encryptionManager;

        public event EventHandler<SessionEventArgs> BuzzReceived = delegate { };
        public event EventHandler<TextMessageReceivedEventArgs> TextMessageReceived = delegate { };
        public event EventHandler<TextMessageUpdatedEventArgs> TextMessageUpdated = delegate { };
        public event EventHandler<SessionEventArgs> UserTyping = delegate { };
        public event EventHandler<ChatInviteReceivedEventArgs> ChatInviteReceived = delegate { };
        public event EventHandler<SessionEventArgs> UserJoined = delegate { };
        public event EventHandler<SessionEventArgs> UserLeft = delegate { };
        public event EventHandler<ActivitySessionEventArgs> ActivityInvitationAccepted = delegate { };
        public event EventHandler<ActivitySessionEventArgs> ActivitySessionCancelled = delegate { };
        public event EventHandler<ActivityInvitationReceivedEventArgs> ActivityInvitationReceived = delegate { };
        public event EventHandler<ActivityDataReceivedEventArgs> ActivityDataReceived = delegate { };
        public event EventHandler<MessageReceivedEventArgs> MessageReceived = delegate { };
        public event EventHandler<SessionEventArgs> SessionInfoRequested = delegate { };
        public event EventHandler<SessionInfoEventArgs> SessionInfoReceived = delegate { };
        public event EventHandler<KeyExchangeReceivedEventArgs> KeyExchangeReceived = delegate { };

        public ChatHost(IPEndPoint endpoint, ILogger<ChatHost>? logger = null, bool useQuic = false)
        {
            this.endpoint = endpoint;
            this.useQuic = useQuic;
            this.logger = logger ?? NullLogger<ChatHost>.Instance;
            this.encryptionManager = new EncryptionManager(this.logger);
        }

        public void Start()
        {
            bool quicEnabled = useQuic && QuicSupported();

            var builder = WebApplication.CreateBuilder();
            builder.Logging.ClearProviders();
            builder.WebHost.ConfigureKestrel(options =>
            {
                options.Listen(endpoint, listenOptions =>
                {
                    if (quicEnabled)
                    {
                        listenOptions.Protocols = HttpProtocols.Http1AndHttp2AndHttp3;
                        listenOptions.UseHttps();
                    }
                    else
                    {
                        listenOptions.Protocols = HttpProtocols.Http2;
                    }
                });
            });
            builder.Services.AddGrpc();
            builder.Services.AddSingleton(this);
            builder.Services.AddSingleton<ILogger>(logger);

            grpcApp = builder.Build();
            grpcApp.MapGrpcService<SquiggleChatGrpcService>();

            grpcApp.StartAsync().GetAwaiter().GetResult();
            logger.LogInformation("gRPC ChatHost started on {Endpoint} (QUIC={QuicEnabled})", endpoint, quicEnabled);
        }

        static bool QuicSupported()
        {
            try
            {
                return System.Net.Quic.QuicListener.IsSupported;
            }
            catch
            {
                return false;
            }
        }

        public void Send(Message message)
        {
            var envelope = MessageToEnvelope(message);
            var target = message.Recipient.Address;
            var peerId = message.Recipient.ClientID;

            // Encrypt the envelope payload if we have a shared key with this peer
            if (peerId != null && encryptionManager.IsEncrypted(peerId))
            {
                EncryptEnvelope(envelope, peerId);
            }

            SendGrpcMessage(target, envelope);
        }

        /// <summary>
        /// Initiates E2EE key exchange with a peer by sending our public key.
        /// </summary>
        public void InitiateKeyExchange(Guid sessionId, ISquiggleEndPoint sender, ISquiggleEndPoint recipient)
        {
            var peerId = recipient.ClientID;
            var publicKey = encryptionManager.GetOrCreateLocalPublicKey(peerId);
            encryptionManager.MarkKeySent(peerId);

            var envelope = new ChatMessageEnvelope
            {
                SessionId = sessionId.ToString(),
                Sender = ToEndPointInfo(sender),
                Recipient = ToEndPointInfo(recipient),
                KeyExchange = new KeyExchangePayload
                {
                    PublicKey = ByteString.CopyFrom(publicKey)
                }
            };

            logger.LogInformation("Initiating E2EE key exchange with peer {PeerId}", peerId);
            SendGrpcMessage(recipient.Address, envelope);
        }

        /// <summary>
        /// Returns true if encryption is established with the given peer.
        /// </summary>
        public bool IsEncryptedWith(string peerId) => encryptionManager.IsEncrypted(peerId);

        void EncryptEnvelope(ChatMessageEnvelope envelope, string peerId)
        {
            // Serialize the payload portion to bytes, encrypt, and replace
            var payloadBytes = envelope.ToByteArray();
            var result = encryptionManager.Encrypt(peerId, payloadBytes);
            if (result == null)
                return;

            // Build an encrypted envelope — clear the original payload and set encrypted fields
            var encryptedEnvelope = new ChatMessageEnvelope
            {
                SessionId = envelope.SessionId,
                Sender = envelope.Sender,
                Recipient = envelope.Recipient,
                Encrypted = true,
                Nonce = ByteString.CopyFrom(result.Value.Nonce),
                SenderPublicKey = ByteString.CopyFrom(encryptionManager.GetOrCreateLocalPublicKey(peerId)),
                // Store the encrypted full envelope bytes in a text message as a carrier
                TextMessage = new TextMessagePayload
                {
                    Id = Guid.Empty.ToString(),
                    Message = Convert.ToBase64String(result.Value.Ciphertext)
                }
            };

            // Copy encrypted envelope fields back
            envelope.Encrypted = encryptedEnvelope.Encrypted;
            envelope.Nonce = encryptedEnvelope.Nonce;
            envelope.SenderPublicKey = encryptedEnvelope.SenderPublicKey;
            envelope.TextMessage = encryptedEnvelope.TextMessage;
            // Clear the original payload — the encrypted data is in TextMessage.Message
            // We repurpose the envelope in-place
        }

        void SendGrpcMessage(IPEndPoint target, ChatMessageEnvelope envelope)
        {
            bool quicEnabled = useQuic && QuicSupported();
            var scheme = quicEnabled ? "https" : "http";
            var key = $"{scheme}://{target.Address}:{target.Port}";
            var channel = channels.GetOrAdd(key, _ =>
            {
                var handler = new System.Net.Http.SocketsHttpHandler
                {
                    EnableMultipleHttp2Connections = true
                };

                if (quicEnabled)
                {
                    // Skip certificate validation for LAN peers using self-signed certs
                    handler.SslOptions = new System.Net.Security.SslClientAuthenticationOptions
                    {
                        RemoteCertificateValidationCallback = (_, _, _, _) => true
                    };
                }

                return GrpcChannel.ForAddress($"{scheme}://{target.Address}:{target.Port}", new GrpcChannelOptions
                {
                    HttpHandler = handler
                });
            });

            var client = new SquiggleChat.SquiggleChatClient(channel);
            client.SendChatMessage(envelope);
        }

        #region Message-to-Envelope conversion

        static ChatMessageEnvelope MessageToEnvelope(Message msg)
        {
            var envelope = new ChatMessageEnvelope
            {
                SessionId = msg.SessionId.ToString(),
                Sender = ToEndPointInfo(msg.Sender),
                Recipient = ToEndPointInfo(msg.Recipient),
            };

            switch (msg)
            {
                case TextMessage tm:
                    envelope.TextMessage = new TextMessagePayload
                    {
                        Id = tm.Id.ToString(),
                        FontName = tm.FontName ?? "",
                        FontSize = tm.FontSize,
                        ColorR = tm.Color.R,
                        ColorG = tm.Color.G,
                        ColorB = tm.Color.B,
                        FontStyle = (int)tm.FontStyle,
                        Message = tm.Message ?? ""
                    };
                    break;
                case BuzzMessage:
                    envelope.Buzz = new BuzzPayload();
                    break;
                case ChatInviteMessage cim:
                    var invite = new ChatInvitePayload();
                    invite.Participants.AddRange(cim.Participants.Select(ToEndPointInfo));
                    envelope.ChatInvite = invite;
                    break;
                case ChatJoinMessage:
                    envelope.ChatJoin = new ChatJoinPayload();
                    break;
                case ChatLeaveMessage:
                    envelope.ChatLeave = new ChatLeavePayload();
                    break;
                case UserTypingMessage:
                    envelope.Typing = new TypingPayload();
                    break;
                case UpdateTextMessage utm:
                    envelope.UpdateText = new UpdateTextPayload
                    {
                        Id = utm.Id.ToString(),
                        Message = utm.Message ?? ""
                    };
                    break;
                case ActivityInviteMessage aim:
                    var actInvite = new ActivityInvitePayload
                    {
                        ActivityId = aim.ActivityId.ToString(),
                        ActivitySessionId = aim.ActivitySessionId.ToString()
                    };
                    foreach (var kv in aim.Metadata)
                        actInvite.Metadata[kv.Key] = kv.Value;
                    envelope.ActivityInvite = actInvite;
                    break;
                case ActivityInviteAcceptMessage:
                    envelope.ActivityAccept = new ActivityAcceptPayload();
                    break;
                case ActivityCancelMessage:
                    envelope.ActivityCancel = new ActivityCancelPayload();
                    break;
                case ActivityDataMessage adm:
                    envelope.ActivityData = new ActivityDataPayload
                    {
                        Data = Google.Protobuf.ByteString.CopyFrom(adm.Data)
                    };
                    break;
                case GiveSessionInfoMessage:
                    envelope.GiveSessionInfo = new GiveSessionInfoPayload();
                    break;
                case SessionInfoMessage sim:
                    var info = new SessionInfoPayload();
                    info.Participants.AddRange(sim.Participants.Select(ToEndPointInfo));
                    envelope.SessionInfo = info;
                    break;
            }

            return envelope;
        }

        static EndPointInfo ToEndPointInfo(SquiggleEndPoint ep)
        {
            return new EndPointInfo
            {
                ClientId = ep.ClientID ?? "",
                Ip = ep.Address?.Address?.ToString() ?? "",
                Port = ep.Address?.Port ?? 0
            };
        }

        static EndPointInfo ToEndPointInfo(ISquiggleEndPoint ep)
        {
            return new EndPointInfo
            {
                ClientId = ep.ClientID ?? "",
                Ip = ep.Address?.Address?.ToString() ?? "",
                Port = ep.Address?.Port ?? 0
            };
        }

        #endregion

        #region gRPC inbound handlers (called by SquiggleChatGrpcService)

        internal void OnGrpcTextMessageReceived(Guid sessionId, SquiggleEndPoint sender, TextMessagePayload payload)
        {
            OnMessageReceived(sessionId, sender, typeof(TextMessage));
            TextMessageReceived(this, new TextMessageReceivedEventArgs()
            {
                Id = Guid.Parse(payload.Id),
                SessionID = sessionId,
                Sender = sender,
                FontName = payload.FontName,
                FontSize = payload.FontSize,
                Color = Color.FromArgb(payload.ColorR, payload.ColorG, payload.ColorB),
                FontStyle = (FontStyle)payload.FontStyle,
                Message = payload.Message
            });
            logger.LogDebug("Message received from {Sender}, SessionId={SessionId}", sender, sessionId);
        }

        internal void OnGrpcBuzzReceived(Guid sessionId, SquiggleEndPoint sender)
        {
            OnMessageReceived(sessionId, sender, typeof(BuzzMessage));
            BuzzReceived(this, new SessionEventArgs(sessionId, sender));
            logger.LogDebug("{Sender} is buzzing", sender);
        }

        internal void OnGrpcChatInviteReceived(Guid sessionId, SquiggleEndPoint sender, ChatInvitePayload payload)
        {
            OnMessageReceived(sessionId, sender, typeof(ChatInviteMessage));
            var participants = payload.Participants.Select(p =>
                new SquiggleEndPoint(p.ClientId, new IPEndPoint(IPAddress.Parse(p.Ip), p.Port))).ToArray();
            logger.LogDebug("{Sender} invited you to group chat", sender);
            ChatInviteReceived(this, new ChatInviteReceivedEventArgs()
            {
                SessionID = sessionId,
                Sender = sender,
                Participants = participants
            });
        }

        internal void OnGrpcUserJoined(Guid sessionId, SquiggleEndPoint sender)
        {
            OnMessageReceived(sessionId, sender, typeof(ChatJoinMessage));
            logger.LogDebug("{Sender} has joined the chat", sender);
            UserJoined(this, new MessageReceivedEventArgs() { SessionID = sessionId, Sender = sender });
        }

        internal void OnGrpcUserLeft(Guid sessionId, SquiggleEndPoint sender)
        {
            OnMessageReceived(sessionId, sender, typeof(ChatLeaveMessage));
            logger.LogDebug("{Sender} has left the chat", sender);
            UserLeft(this, new MessageReceivedEventArgs() { SessionID = sessionId, Sender = sender });
        }

        internal void OnGrpcUserTyping(Guid sessionId, SquiggleEndPoint sender)
        {
            OnMessageReceived(sessionId, sender, typeof(UserTypingMessage));
            UserTyping(this, new SessionEventArgs(sessionId, sender));
            logger.LogDebug("{Sender} is typing", sender);
        }

        internal void OnGrpcTextMessageUpdated(Guid sessionId, SquiggleEndPoint sender, UpdateTextPayload payload)
        {
            OnMessageReceived(sessionId, sender, typeof(UpdateTextMessage));
            TextMessageUpdated(this, new TextMessageUpdatedEventArgs()
            {
                Id = Guid.Parse(payload.Id),
                SessionID = sessionId,
                Sender = sender,
                Message = payload.Message
            });
            logger.LogDebug("Message updated by {Sender}, SessionId={SessionId}", sender, sessionId);
        }

        internal void OnGrpcActivityInvitationReceived(Guid sessionId, SquiggleEndPoint sender, ActivityInvitePayload payload)
        {
            OnMessageReceived(sessionId, sender, typeof(ActivityInviteMessage));
            logger.LogDebug("{Sender} wants to send a file {Metadata}", sender, payload.Metadata.ToTraceString());
            ActivityInvitationReceived(this, new ActivityInvitationReceivedEventArgs()
            {
                SessionID = sessionId,
                Sender = sender,
                ActivityId = Guid.Parse(payload.ActivityId),
                ActivitySessionId = Guid.Parse(payload.ActivitySessionId),
                Metadata = payload.Metadata.ToDictionary(kv => kv.Key, kv => kv.Value)
            });
        }

        internal void OnGrpcActivityInvitationAccepted(Guid sessionId, SquiggleEndPoint sender)
        {
            OnMessageReceived(sessionId, sender, typeof(ActivityInviteAcceptMessage));
            ActivityInvitationAccepted(this, new ActivitySessionEventArgs() { ActivitySessionId = sessionId });
        }

        internal void OnGrpcActivitySessionCancelled(Guid sessionId, SquiggleEndPoint sender)
        {
            OnMessageReceived(sessionId, sender, typeof(ActivityCancelMessage));
            ActivitySessionCancelled(this, new ActivitySessionEventArgs() { ActivitySessionId = sessionId });
        }

        internal void OnGrpcActivityDataReceived(Guid sessionId, SquiggleEndPoint sender, ActivityDataPayload payload)
        {
            OnMessageReceived(sessionId, sender, typeof(ActivityDataMessage));
            ActivityDataReceived(this, new ActivityDataReceivedEventArgs()
            {
                ActivitySessionId = sessionId,
                Chunk = payload.Data.ToByteArray()
            });
        }

        internal void OnGrpcSessionInfoRequested(Guid sessionId, SquiggleEndPoint sender)
        {
            OnMessageReceived(sessionId, sender, typeof(GiveSessionInfoMessage));
            SessionInfoRequested(this, new SessionEventArgs(sessionId, sender));
            logger.LogDebug("{Sender} is requesting session info", sender);
        }

        internal void OnGrpcSessionInfoReceived(Guid sessionId, SquiggleEndPoint sender, SessionInfoPayload payload)
        {
            OnMessageReceived(sessionId, sender, typeof(SessionInfoMessage));
            var participants = payload.Participants.Select(p =>
                (ISquiggleEndPoint)new SquiggleEndPoint(p.ClientId, new IPEndPoint(IPAddress.Parse(p.Ip), p.Port))).ToArray();
            SessionInfoReceived(this, new SessionInfoEventArgs()
            {
                Participants = participants,
                Sender = sender,
                SessionID = sessionId
            });
            logger.LogDebug("{Sender} sent session info", sender);
        }

        internal void OnGrpcKeyExchangeReceived(Guid sessionId, SquiggleEndPoint sender, KeyExchangePayload payload)
        {
            var peerId = sender.ClientID;
            var peerPublicKey = payload.PublicKey.ToByteArray();

            logger.LogInformation("Received E2EE public key from peer {PeerId}, fingerprint={Fingerprint}",
                peerId, Encryption.E2EEncryptionService.ComputeFingerprint(peerPublicKey));

            // Derive shared key and get our public key if we need to respond
            var ourPublicKey = encryptionManager.OnPeerPublicKeyReceived(peerId, peerPublicKey);

            KeyExchangeReceived(this, new KeyExchangeReceivedEventArgs
            {
                SessionID = sessionId,
                Sender = sender,
                PublicKey = peerPublicKey,
                NeedsResponse = ourPublicKey != null
            });

            // If this is the first time we see this peer's key, send ours back
            if (ourPublicKey != null)
            {
                encryptionManager.MarkKeySent(peerId);
                var responseEnvelope = new ChatMessageEnvelope
                {
                    SessionId = sessionId.ToString(),
                    Sender = new EndPointInfo
                    {
                        ClientId = "",  // Will be set properly via the endpoint info
                        Ip = endpoint.Address.ToString(),
                        Port = endpoint.Port
                    },
                    Recipient = ToEndPointInfo(sender),
                    KeyExchange = new KeyExchangePayload
                    {
                        PublicKey = ByteString.CopyFrom(ourPublicKey)
                    }
                };
                // We can't easily get our own EndPointInfo here, so fire event for ChatSession to respond
                logger.LogDebug("Peer {PeerId} needs our public key — will be sent via key exchange response", peerId);
            }
        }

        /// <summary>
        /// Attempts to decrypt an incoming encrypted envelope.
        /// Returns the decrypted envelope, or null if decryption fails.
        /// </summary>
        internal ChatMessageEnvelope? TryDecryptEnvelope(ChatMessageEnvelope envelope)
        {
            if (!envelope.Encrypted)
                return envelope;

            var senderId = envelope.Sender.ClientId;
            var ciphertext = Convert.FromBase64String(envelope.TextMessage.Message);
            var nonce = envelope.Nonce.ToByteArray();

            var plaintext = encryptionManager.Decrypt(senderId, ciphertext, nonce);
            if (plaintext == null)
            {
                logger.LogWarning("Failed to decrypt message from {Sender} — no shared key", senderId);
                return null;
            }

            // Parse the decrypted bytes back into a ChatMessageEnvelope
            var decrypted = ChatMessageEnvelope.Parser.ParseFrom(plaintext);
            return decrypted;
        }

        /// <summary>
        /// Removes encryption state for a peer.
        /// </summary>
        public void RemoveEncryptionState(string peerId)
        {
            encryptionManager.RemovePeer(peerId);
        }

        #endregion

        void OnMessageReceived(Guid sessionId, ISquiggleEndPoint sender, Type messageType)
        {
            MessageReceived(this, new MessageReceivedEventArgs() { Sender = sender, SessionID = sessionId, Type = messageType });
        }

        public void Dispose()
        {
            if (grpcApp != null)
            {
                grpcApp.StopAsync().GetAwaiter().GetResult();
                grpcApp.DisposeAsync().AsTask().GetAwaiter().GetResult();
                grpcApp = null;
            }

            foreach (var channel in channels.Values)
                channel.Dispose();
            channels.Clear();

            encryptionManager.Dispose();
        }
    }

    public class ChatInviteReceivedEventArgs : SessionEventArgs
    {
        public ISquiggleEndPoint[] Participants { get; set; } = null!;
    }

    public class ActivitySessionEventArgs : EventArgs
    {
        public Guid ActivitySessionId { get; set; }
    }

    public class ActivityInvitationReceivedEventArgs : SessionEventArgs
    {
        public Guid ActivitySessionId { get; set; }
        public Guid ActivityId { get; set; }
        public IDictionary<string, string> Metadata { get; set; } = null!;
    }

    public class ActivityDataReceivedEventArgs : ActivitySessionEventArgs
    {
        public byte[] Chunk { get; set; } = null!;
    }

    public class MessageReceivedEventArgs : SessionEventArgs
    {
        public Type Type { get; set; } = null!;
    }

    public class SessionInfoEventArgs : SessionEventArgs
    {
        public ISquiggleEndPoint[] Participants { get; set; } = null!;
    }

    public class KeyExchangeReceivedEventArgs : SessionEventArgs
    {
        public byte[] PublicKey { get; set; } = null!;
        public bool NeedsResponse { get; set; }
    }
}
