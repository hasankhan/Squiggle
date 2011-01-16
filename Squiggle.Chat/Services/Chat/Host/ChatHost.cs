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
        public event EventHandler<SessionInfoRequestedEventArgs> SessionInfoRequested = delegate { };

        ActionQueue eventQueue = new ActionQueue();
        Thread eventProcessor;
        volatile bool disposed = false;

        public ChatHost()
        {
            eventProcessor = new Thread(() =>
            {
                while (!disposed)
                {
                    eventQueue.Dequeue();
                    Thread.Sleep(1);
                }
            });
            eventProcessor.IsBackground = true;
            eventProcessor.Start();
        }

        #region IChatHost Members

        public SessionInfo GetSessionInfo(Guid sessionId, ChatEndPoint user)
        {
            var args = new SessionInfoRequestedEventArgs() 
            { 
                SessionID = sessionId, 
                User = user, 
                Info = new SessionInfo() 
            };
            SessionInfoRequested(this, args);
            return args.Info;
        }

        public void Buzz(Guid sessionId, ChatEndPoint user)
        {
            OnUserActivity(sessionId, user, ActivityType.Buzz);
            eventQueue.Enqueue(()=>BuzzReceived(this, new SessionEventArgs(sessionId, user )));
            Trace.WriteLine(user + " is buzzing.");
        }

        public void UserIsTyping(Guid sessionId, ChatEndPoint user)
        {
            OnUserActivity(sessionId, user, ActivityType.Typing);
            eventQueue.Enqueue(()=>UserTyping(this, new SessionEventArgs(sessionId, user )));
            Trace.WriteLine(user + " is typing.");
        }

        public void ReceiveMessage(Guid sessionId, ChatEndPoint user, string fontName, int fontSize, Color color, FontStyle fontStyle, string message)
        {
            OnUserActivity(sessionId, user, ActivityType.Message);
            eventQueue.Enqueue(() => MessageReceived(this, new MessageReceivedEventArgs(){SessionID = sessionId, 
                                                                                          User = user,
                                                                                          FontName = fontName,
                                                                                          FontSize = fontSize,
                                                                                          Color = color,
                                                                                          FontStyle = fontStyle,
                                                                                          Message = message }));
            Trace.WriteLine("Message received from: " + user + ", sessionId= " + sessionId + ", message = " + message);
        }

        public void ReceiveChatInvite(Guid sessionId, ChatEndPoint user, ChatEndPoint[] participants)
        {
            OnUserActivity(sessionId, user, ActivityType.ChatInvite);
            Trace.WriteLine(user + " invited you to group chat.");
            eventQueue.Enqueue(() => ChatInviteReceived(this, new ChatInviteReceivedEventArgs() 
            { 
                SessionID = sessionId, 
                User = user, 
                Participants = participants 
            }));
        }

        public void JoinChat(Guid sessionId, ChatEndPoint user)
        {
            Trace.WriteLine(user + " has joined the chat.");
            eventQueue.Enqueue(() => UserJoined(this, new UserActivityEventArgs() { SessionID = sessionId, User = user}));
        }

        public void LeaveChat(Guid sessionId, ChatEndPoint user)
        {
            Trace.WriteLine(user + " has left the chat.");
            eventQueue.Enqueue(() => UserLeft(this, new UserActivityEventArgs() { SessionID = sessionId, User = user}));
        }

        public void ReceiveFileInvite(Guid sessionId, ChatEndPoint user, Guid id, string name, long size)
        {
            OnUserActivity(sessionId, user, ActivityType.TransferInvite);
            Trace.WriteLine(user + " wants to send a file " + name);
            eventQueue.Enqueue(() => TransferInvitationReceived(this, new TransferInvitationReceivedEventArgs()
            {
                SessionID = sessionId,
                User = user,
                Name = name,
                ID = id,
                Size = size
            }));
        }

        public void ReceiveFileContent(Guid id, byte[] chunk)
        {
            eventQueue.Enqueue(() => TransferDataReceived(this, new FileTransferDataReceivedEventArgs() { ID = id, Chunk = chunk }));
        }

        public void AcceptFileInvite(Guid id)
        {
            eventQueue.Enqueue(() => InvitationAccepted(this, new FileTransferEventArgs() { ID = id }));
        }

        public void CancelFileTransfer(Guid id)
        {
            eventQueue.Enqueue(() => TransferCancelled(this, new FileTransferEventArgs() { ID = id }));
        }       

        #endregion

        void OnUserActivity(Guid sessionId, ChatEndPoint user, ActivityType type)
        {
            eventQueue.Enqueue(() => UserActivity(this, new UserActivityEventArgs(){User = user, 
                                                                                    SessionID = sessionId,
                                                                                    Type = type}));
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
        public ChatEndPoint User { get; set; }

        public SessionEventArgs(){}

        public SessionEventArgs(Guid sessionId, ChatEndPoint user)
        {
            this.SessionID = sessionId;
            this.User = user;
        }        
    }

    public class ChatInviteReceivedEventArgs : SessionEventArgs
    {
        public ChatEndPoint[] Participants { get; set; }
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
