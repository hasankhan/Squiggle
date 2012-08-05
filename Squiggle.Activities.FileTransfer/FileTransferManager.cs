using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.Composition;
using Squiggle.Core.Chat;
using System.IO;
using Squiggle.UI.Plugins.Activity;

namespace Squiggle.Activities.FileTransfer
{
    [Export(typeof(IActivityManager))]
    public class FileTransferManager: IActivityManager
    {
        public Guid ActivityId
        {
            get { return SquiggleActivities.FileTransfer; }
        }

        public string Title
        {
            get { return "File Transfer"; }
        }

        public IActivityHandler FromInvite(Core.Chat.ActivitySession session, IDictionary<string, string> metadata)
        {
            var inviteData = new FileInviteData(metadata);
            IFileTransfer handler = new FileTransfer(session, inviteData.Name, inviteData.Size);
            return handler;
        }

        public IActivityHandler CreateInvite(ActivitySession session, IDictionary<string, object> args)
        {
            if (!args.ContainsKey("content") || !(args["content"] is Stream))
                throw new ArgumentException("metadata must include content stream.", "metadata");

            var stream = (Stream)args["content"];

            var inviteData = new FileInviteData(args.ToDictionary(x=>x.Key, x=>x.Value.ToString()));
            IFileTransfer handler = new FileTransfer(session, inviteData.Name, inviteData.Size, stream);
            return handler;
        }

        public IDictionary<string, object> LaunchInviteUI()
        {
            throw new NotImplementedException();
        }
    }
}
