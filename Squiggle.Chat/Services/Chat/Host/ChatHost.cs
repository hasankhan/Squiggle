using System;
using System.Diagnostics;
using System.Drawing;
using System.Net;
using System.ServiceModel;

namespace Squiggle.Chat.Services.Chat.Host
{
    public class MessageReceivedEventArgs : SessionEventArgs
    {
        public string FontName { get; set; }
        public int FontSize { get; set; }
        public Color Color { get; set; }
        public FontStyle FontStyle { get; set; }
        public string Message { get; set; }
    }

    public class SessionEventArgs: EventArgs
    {
        public Guid SessionID { get; set; }
        public IPEndPoint User {get; set; }
    }

    public class FileTransferEventArgs : SessionEventArgs
    {
        public Guid ID { get; set; }
    }

    public class TransferInvitationReceivedEventArgs : SessionEventArgs
    {
        public Guid ID {get ; set; }
        public string Name {get; set; }
        public int Size {get; set; }
    }

    public class FileTransferDataReceivedEventArgs: EventArgs
    {
        public Guid ID { get; set; }
        public byte[] Chunk { get; set; }
    }

    public enum ActivityType
    {
        Message,
        Typing,
        Buzz,
        TransferInvite
    }

    public class UserActivityEventArgs : SessionEventArgs
    {
        public ActivityType Type { get; set; }
    }

    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)] 
    public class ChatHost: IChatHost
    {
        public event EventHandler<SessionEventArgs> BuzzReceived = delegate { };
        public event EventHandler<MessageReceivedEventArgs> MessageReceived = delegate { };
        public event EventHandler<SessionEventArgs> UserTyping = delegate { };
        public event EventHandler<FileTransferEventArgs> InvitationAccepted = delegate { };
        public event EventHandler<FileTransferEventArgs> TransferCancelled = delegate { };
        public event EventHandler<TransferInvitationReceivedEventArgs> TransferInvitationReceived = delegate { };
        public event EventHandler<FileTransferDataReceivedEventArgs> TransferDataReceived = delegate { };
        public event EventHandler<UserActivityEventArgs> UserActivity = delegate { };

        #region IChatHost Members

        public void Buzz(Guid sessionId, IPEndPoint user)
        {
            OnUserActivity(sessionId, user, ActivityType.Buzz);
            BuzzReceived(this, new SessionEventArgs() { SessionID = sessionId, User = user });
            Trace.WriteLine(user.ToString() + " is buzzing.");
        }

        public void UserIsTyping(Guid sessionId, IPEndPoint user)
        {
            OnUserActivity(sessionId, user, ActivityType.Typing);
            UserTyping(this, new SessionEventArgs() { SessionID = sessionId, User = user });
            Trace.WriteLine(user.ToString() + " is typing.");
        }                

        public void ReceiveMessage(Guid sessionId, IPEndPoint user, string fontName, int fontSize, Color color, FontStyle fontStyle, string message)
        {
            OnUserActivity(sessionId, user, ActivityType.Message);
            MessageReceived(this, new MessageReceivedEventArgs() { SessionID = sessionId, 
                                                                   User = user,
                                                                   FontName = fontName,
                                                                   FontSize = fontSize,
                                                                   Color = color,
                                                                   FontStyle = fontStyle,
                                                                   Message = message });
            Trace.WriteLine("Message received from: " + user.ToString() + ", message = " + message);
        }

        public void ReceiveFileInvite(Guid sessionId, IPEndPoint user, Guid id, string name, int size)
        {
            OnUserActivity(sessionId, user, ActivityType.TransferInvite);
            Trace.WriteLine(user.ToString() + " wants to send a file " + name);
            TransferInvitationReceived(this, new TransferInvitationReceivedEventArgs()
            {
                SessionID = sessionId,
                User = user,
                Name = name,
                ID = id,
                Size = size
            });
        }

        public void ReceiveFileContent(Guid id, byte[] chunk)
        {
            TransferDataReceived(this, new FileTransferDataReceivedEventArgs() { ID = id, Chunk = chunk });
        }

        public void AcceptFileInvite(Guid id)
        {
            InvitationAccepted(this, new FileTransferEventArgs() { ID = id });
        }

        public void CancelFileTransfer(Guid id)
        {
            TransferCancelled(this, new FileTransferEventArgs() { ID = id });
        }       

        #endregion

        void OnUserActivity(Guid sessionId, IPEndPoint user, ActivityType type)
        {
            UserActivity(this, new UserActivityEventArgs() { User = user, 
                                                             SessionID = sessionId,
                                                             Type = type });
        }
    }
}
