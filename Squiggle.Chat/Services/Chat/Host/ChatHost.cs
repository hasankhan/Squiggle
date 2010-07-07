using System;
using System.Diagnostics;
using System.Drawing;
using System.Net;
using System.ServiceModel;
using System.Threading;

namespace Squiggle.Chat.Services.Chat.Host
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)] 
    public class ChatHost: IChatHost, IDisposable
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

        EventQueue eventQueue = new EventQueue();
        Thread eventProcessor;
        bool disposed = false;

        public ChatHost()
        {
            eventProcessor = new Thread(() =>
            {
                while (!disposed)
                {
                    eventQueue.DequeueAll();
                    Thread.Sleep(1);
                }
            });
            eventProcessor.IsBackground = true;
            eventProcessor.Start();
        }

        #region IChatHost Members

        public void Buzz(Guid sessionId, IPEndPoint user)
        {
            OnUserActivity(sessionId, user, ActivityType.Buzz);
            eventQueue.Enqueue(this, new SessionEventArgs() { SessionID = sessionId, User = user }, BuzzReceived);
            Trace.WriteLine(user.ToString() + " is buzzing.");
        }

        public void UserIsTyping(Guid sessionId, IPEndPoint user)
        {
            OnUserActivity(sessionId, user, ActivityType.Typing);
            eventQueue.Enqueue(this, new SessionEventArgs() { SessionID = sessionId, User = user }, UserTyping);
            Trace.WriteLine(user.ToString() + " is typing.");
        }                

        public void ReceiveMessage(Guid sessionId, IPEndPoint user, string fontName, int fontSize, Color color, FontStyle fontStyle, string message)
        {
            OnUserActivity(sessionId, user, ActivityType.Message);
            eventQueue.Enqueue(this, new MessageReceivedEventArgs() { SessionID = sessionId, 
                                                                   User = user,
                                                                   FontName = fontName,
                                                                   FontSize = fontSize,
                                                                   Color = color,
                                                                   FontStyle = fontStyle,
                                                                   Message = message }, MessageReceived);
            Trace.WriteLine("Message received from: " + user.ToString() + ", message = " + message);
        }

        public void ReceiveChatInvite(Guid sessionId, IPEndPoint user, IPEndPoint[] participants)
        {
            OnUserActivity(sessionId, user, ActivityType.ChatInvite);
            Trace.WriteLine(user.ToString() + " invited you to group chat.");
            eventQueue.Enqueue(this, new ChatInviteReceivedEventArgs() { SessionID = sessionId, User = user, Participants = participants }, ChatInviteReceived);
        }

        public void JoinChat(Guid sessionId, IPEndPoint user)
        {
            Trace.WriteLine(user.ToString() + " has joined the chat.");
            eventQueue.Enqueue(this, new UserActivityEventArgs() { SessionID = sessionId, User = user }, UserJoined);
        }

        public void LeaveChat(Guid sessionId, IPEndPoint user)
        {
            Trace.WriteLine(user.ToString() + " has left the chat.");
            eventQueue.Enqueue(this, new UserActivityEventArgs() { SessionID = sessionId, User = user }, UserLeft);
        }

        public void ReceiveFileInvite(Guid sessionId, IPEndPoint user, Guid id, string name, int size)
        {
            OnUserActivity(sessionId, user, ActivityType.TransferInvite);
            Trace.WriteLine(user.ToString() + " wants to send a file " + name);
            eventQueue.Enqueue(this, new TransferInvitationReceivedEventArgs()
            {
                SessionID = sessionId,
                User = user,
                Name = name,
                ID = id,
                Size = size
            }, TransferInvitationReceived);
        }

        public void ReceiveFileContent(Guid id, byte[] chunk)
        {
            eventQueue.Enqueue(this, new FileTransferDataReceivedEventArgs() { ID = id, Chunk = chunk }, TransferDataReceived);
        }

        public void AcceptFileInvite(Guid id)
        {
            eventQueue.Enqueue(this, new FileTransferEventArgs() { ID = id }, InvitationAccepted);
        }

        public void CancelFileTransfer(Guid id)
        {
            eventQueue.Enqueue(this, new FileTransferEventArgs() { ID = id }, TransferCancelled);
        }       

        #endregion

        void OnUserActivity(Guid sessionId, IPEndPoint user, ActivityType type)
        {
            UserActivity(this, new UserActivityEventArgs() { User = user, 
                                                             SessionID = sessionId,
                                                             Type = type });
        }

        public void Dispose()
        {
            disposed = true;
            eventProcessor.Join();
        }
    }


    public class SessionEventArgs : EventArgs
    {
        public Guid SessionID { get; set; }
        public IPEndPoint User { get; set; }
    }

    public class ChatInviteReceivedEventArgs : SessionEventArgs
    {
        public IPEndPoint[] Participants { get; set; }
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
        public int Size { get; set; }
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
}
