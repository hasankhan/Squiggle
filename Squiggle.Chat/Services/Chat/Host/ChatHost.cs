using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Net;
using System.Diagnostics;
using System.IO;
using System.Drawing;

namespace Squiggle.Chat.Services.Chat.Host
{
    public class MessageReceivedEventArgs : EventArgs
    {
        public IPEndPoint User { get; set; }
        public string FontName { get; set; }
        public int FontSize { get; set; }
        public Color Color { get; set; }
        public FontStyle FontStyle { get; set; }
        public string Message { get; set; }
    }

    public class UserEventArgs: EventArgs
    {
        public IPEndPoint User {get; set; }
    }

    public class FileTransferEventArgs : EventArgs
    {
        public Guid ID { get; set; }
    }

    public class TransferInvitationReceivedEventArgs: EventArgs
    {
        public IPEndPoint User {get; set; }
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

    public class UserActivityEventArgs : EventArgs
    {
        public IPEndPoint User { get; set; }
        public ActivityType Type { get; set; }
    }

    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)] 
    public class ChatHost: IChatHost
    {
        public event EventHandler<UserEventArgs> BuzzReceived = delegate { };
        public event EventHandler<MessageReceivedEventArgs> MessageReceived = delegate { };
        public event EventHandler<UserEventArgs> UserTyping = delegate { };
        public event EventHandler<FileTransferEventArgs> InvitationAccepted = delegate { };
        public event EventHandler<FileTransferEventArgs> TransferCancelled = delegate { };
        public event EventHandler<TransferInvitationReceivedEventArgs> TransferInvitationReceived = delegate { };
        public event EventHandler<FileTransferDataReceivedEventArgs> TransferDataReceived = delegate { };
        public event EventHandler<UserActivityEventArgs> UserActivity = delegate { };

        #region IChatHost Members

        public void Buzz(IPEndPoint user)
        {
            OnUserActivity(user, ActivityType.Buzz);
            BuzzReceived(this, new UserEventArgs() { User = user });
            Trace.WriteLine(user.ToString() + " is buzzing.");
        }

        public void UserIsTyping(IPEndPoint user)
        {
            OnUserActivity(user, ActivityType.Typing);
            UserTyping(this, new UserEventArgs() { User = user });
            Trace.WriteLine(user.ToString() + " is typing.");
        }        

        public void ReceiveFileInvite(IPEndPoint user, Guid id, string name, int size)
        {
            OnUserActivity(user, ActivityType.TransferInvite);
            Trace.WriteLine(user.ToString() + " wants to send a file " + name);
            TransferInvitationReceived(this, new TransferInvitationReceivedEventArgs()
            {
                User = user,
                Name = name,
                ID = id,
                Size = size
            });
        }

        public void ReceiveMessage(IPEndPoint user, string fontName, int fontSize, Color color, FontStyle fontStyle, string message)
        {
            OnUserActivity(user, ActivityType.Message);
            MessageReceived(this, new MessageReceivedEventArgs() { User = user, 
                                                                   FontName = fontName,
                                                                   FontSize = fontSize,
                                                                   Color = color,
                                                                   FontStyle = fontStyle,
                                                                   Message = message });
            Trace.WriteLine("Message received from: " + user.ToString() + ", message = " + message);
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

        void OnUserActivity(IPEndPoint user, ActivityType type)
        {
            UserActivity(this, new UserActivityEventArgs() { User = user, Type = type });
        }
    }
}
