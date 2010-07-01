using System;
using System.Drawing;
using System.IO;
using System.Net;
using Squiggle.Chat.Services.Chat.Host;

namespace Squiggle.Chat.Services
{
    public class FileTransferInviteEventArgs: EventArgs
    {
        public IFileTransfer Invitation {get; set; }
    }

    public interface IChatSession
    {
        event EventHandler<MessageReceivedEventArgs> MessageReceived;
        event EventHandler<UserEventArgs> BuzzReceived;
        event EventHandler<UserEventArgs> UserTyping;
        event EventHandler<FileTransferInviteEventArgs> TransferInvitationReceived;

        IPEndPoint RemoteUser { get; set; }

        void SendBuzz();
        void SendMessage(string fontName, int fontSize, Color color, FontStyle fontStyle, string message);
        void NotifyTyping();
        IFileTransfer SendFile(string name, Stream content);
        void End();
    }
}
