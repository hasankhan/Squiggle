using System;
using System.Diagnostics;
using System.Drawing;
using System.Net;
using System.ServiceModel;
using System.Threading;
using System.Collections.Generic;
using Squiggle.Utilities;

namespace Squiggle.Chat.Services.Chat.Host
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode=ConcurrencyMode.Multiple, UseSynchronizationContext=false)] 
    public class ChatHost: IChatHost
    {
        public event EventHandler<SessionEventArgs> BuzzReceived = delegate { };
        public event EventHandler<MessageReceivedEventArgs> MessageReceived = delegate { };
        public event EventHandler<SessionEventArgs> UserTyping = delegate { };
        public event EventHandler<ChatInviteReceivedEventArgs> ChatInviteReceived = delegate { };
        public event EventHandler<SessionEventArgs> UserJoined = delegate { };
        public event EventHandler<SessionEventArgs> UserLeft = delegate { };
        public event EventHandler<FileTransferEventArgs> InvitationAccepted = delegate { };
        public event EventHandler<FileTransferEventArgs> TransferCancelled = delegate { };        
        public event EventHandler<TransferInvitationReceivedEventArgs> TransferInvitationReceived = delegate { };
        public event EventHandler<FileTransferDataReceivedEventArgs> TransferDataReceived = delegate { };
        public event EventHandler<UserActivityEventArgs> UserActivity = delegate { };
        public event EventHandler<SessionInfoRequestedEventArgs> SessionInfoRequested = delegate { };

        #region IChatHost Members

        public SessionInfo GetSessionInfo(Guid sessionId, SquiggleEndPoint sender, SquiggleEndPoint recepient)
        {
            var args = new SessionInfoRequestedEventArgs() 
            { 
                SessionID = sessionId, 
                Sender = sender, 
                Info = new SessionInfo() 
            };
            SessionInfoRequested(this, args);
            return args.Info;
        }

        public void Buzz(Guid sessionId, SquiggleEndPoint sender, SquiggleEndPoint recepient)
        {
            OnUserActivity(sessionId, sender, recepient, ActivityType.Buzz);
            BuzzReceived(this, new SessionEventArgs(sessionId, sender ));
            Trace.WriteLine(sender + " is buzzing.");
        }

        public void UserIsTyping(Guid sessionId, SquiggleEndPoint sender, SquiggleEndPoint recepient)
        {
            OnUserActivity(sessionId, sender, recepient, ActivityType.Typing);
            UserTyping(this, new SessionEventArgs(sessionId, sender ));
            Trace.WriteLine(sender + " is typing.");
        }

        public void ReceiveMessage(Guid sessionId, SquiggleEndPoint sender, SquiggleEndPoint recepient, string fontName, int fontSize, Color color, FontStyle fontStyle, string message)
        {
            OnUserActivity(sessionId, sender, recepient, ActivityType.Message);
            MessageReceived(this, new MessageReceivedEventArgs(){SessionID = sessionId, 
                                                                                          Sender = sender,
                                                                                          FontName = fontName,
                                                                                          FontSize = fontSize,
                                                                                          Color = color,
                                                                                          FontStyle = fontStyle,
                                                                                          Message = message });
            Trace.WriteLine("Message received from: " + sender + ", sessionId= " + sessionId + ", message = " + message);
        }

        public void ReceiveChatInvite(Guid sessionId, SquiggleEndPoint sender, SquiggleEndPoint recepient, SquiggleEndPoint[] participants)
        {
            OnUserActivity(sessionId, sender, recepient, ActivityType.ChatInvite);
            Trace.WriteLine(sender + " invited you to group chat.");
            ChatInviteReceived(this, new ChatInviteReceivedEventArgs() 
            { 
                SessionID = sessionId, 
                Sender = sender, 
                Participants = participants 
            });
        }

        public void JoinChat(Guid sessionId, SquiggleEndPoint sender, SquiggleEndPoint recepient)
        {
            Trace.WriteLine(sender + " has joined the chat.");
            UserJoined(this, new UserActivityEventArgs() { SessionID = sessionId, Sender = sender});
        }

        public void LeaveChat(Guid sessionId, SquiggleEndPoint sender, SquiggleEndPoint recepient)
        {
            Trace.WriteLine(sender + " has left the chat.");
            UserLeft(this, new UserActivityEventArgs() { SessionID = sessionId, Sender = sender});
        }

        public void ReceiveAppInvite(Guid sessionId, SquiggleEndPoint sender, SquiggleEndPoint recepient, Guid appId, Guid appSessionId, IEnumerable<KeyValuePair<string, string>> metadata)
        {
            OnUserActivity(sessionId, sender, recepient, ActivityType.TransferInvite);
            Trace.WriteLine(sender + " wants to send a file " + metadata.ToTraceString());
            var data = new FileInviteData(metadata);
            TransferInvitationReceived(this, new TransferInvitationReceivedEventArgs()
            {
                SessionID = sessionId,
                Sender = sender,
                Name = data.Name,
                ID = appSessionId,
                Size = data.Size
            });
        }

        public void ReceiveAppData(Guid appSessionId, SquiggleEndPoint sender, SquiggleEndPoint recepient, byte[] chunk)
        {
            TransferDataReceived(this, new FileTransferDataReceivedEventArgs() { ID = appSessionId, Chunk = chunk });
        }

        public void AcceptAppInvite(Guid appSessionId, SquiggleEndPoint sender, SquiggleEndPoint recepient)
        {
            InvitationAccepted(this, new FileTransferEventArgs() { ID = appSessionId });
        }

        public void CancelAppSession(Guid appSessionId, SquiggleEndPoint sender, SquiggleEndPoint recepient)
        {
            TransferCancelled(this, new FileTransferEventArgs() { ID = appSessionId });
        }       

        #endregion

        void OnUserActivity(Guid sessionId, SquiggleEndPoint sender, SquiggleEndPoint recepient, ActivityType type)
        {
            UserActivity(this, new UserActivityEventArgs(){Sender = sender, SessionID = sessionId, Type = type});
        }
    }


    public class SessionEventArgs : EventArgs
    {
        public Guid SessionID { get; set; }
        public SquiggleEndPoint Sender { get; set; }

        public SessionEventArgs(){}

        public SessionEventArgs(Guid sessionId, SquiggleEndPoint user)
        {
            this.SessionID = sessionId;
            this.Sender = user;
        }        
    }

    public class ChatInviteReceivedEventArgs : SessionEventArgs
    {
        public SquiggleEndPoint[] Participants { get; set; }
    }

    public class MessageReceivedEventArgs : SessionEventArgs
    {
        public string FontName { get; set; }
        public int FontSize { get; set; }
        public Color Color { get; set; }
        public FontStyle FontStyle { get; set; }
        public string Message { get; set; }
    }    

    public class FileTransferEventArgs : SessionEventArgs
    {
        public Guid ID { get; set; }
    }

    public class TransferInvitationReceivedEventArgs : SessionEventArgs
    {
        public Guid ID { get; set; }
        public string Name { get; set; }
        public long Size { get; set; }
    }

    public class FileTransferDataReceivedEventArgs : EventArgs
    {
        public Guid ID { get; set; }
        public byte[] Chunk { get; set; }
    }

    public enum ActivityType
    {
        Message,
        Typing,
        Buzz,
        TransferInvite,
        ChatInvite
    }

    public class UserActivityEventArgs : SessionEventArgs
    {
        public ActivityType Type { get; set; }
    }

    public class SessionInfoRequestedEventArgs : SessionEventArgs
    {
        public SessionInfo Info { get; set; }
    }
}
