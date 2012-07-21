using System;
using System.Drawing;
using System.IO;
using System.Net;
using Squiggle.Core.Chat.Transport.Host;
using System.Collections.Generic;
using System.Windows.Threading;
using Squiggle.Core.Chat.FileTransfer;
using Squiggle.Core.Chat.Voice;

namespace Squiggle.Core.Chat
{
    public class FileTransferInviteEventArgs: SessionEventArgs
    {
        public IFileTransfer Invitation {get; set; }
    }

    public class VoiceChatInvitationReceivedEventArgs : SessionEventArgs
    {
        public IVoiceChat Invitation { get; set; }
    }

    public interface IChatSession
    {
        event EventHandler<MessageReceivedEventArgs> MessageReceived;
        event EventHandler<SessionEventArgs> BuzzReceived;
        event EventHandler<SessionEventArgs> UserTyping;
        event EventHandler<SessionEventArgs> UserJoined;
        event EventHandler<SessionEventArgs> UserLeft;
        event EventHandler<FileTransferInviteEventArgs> TransferInvitationReceived;
        event EventHandler<VoiceChatInvitationReceivedEventArgs> VoiceChatInvitationReceived;
        event EventHandler GroupChatStarted;

        IEnumerable<SquiggleEndPoint> RemoteUsers { get; }
        Guid ID { get; }
        bool IsGroupSession { get; }
        IEnumerable<IAppHandler> AppSessions { get; }

        void SendBuzz();
        void SendMessage(string fontName, int fontSize, Color color, FontStyle fontStyle, string message);
        void NotifyTyping();
        IFileTransfer SendFile(string name, Stream content);
        IVoiceChat StartVoiceChat(Dispatcher dispatcher);
        void End();
        void Invite(SquiggleEndPoint user);
        void UpdateUser(SquiggleEndPoint user);
    }
}
