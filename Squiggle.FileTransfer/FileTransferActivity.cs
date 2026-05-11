using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Squiggle.Client.Activities;
using Squiggle.Core.Chat.Activity;
using Squiggle.Plugins;

namespace Squiggle.FileTransfer
{
    /// <summary>
    /// Registers file transfer as an activity in the Squiggle plugin system.
    /// File data flows through the gRPC activity stream pipeline:
    /// FileTransferHandler → ActivityExecutor → ActivitySession → ChatHost → gRPC (ActivityDataPayload).
    /// </summary>
    public class FileTransferActivity : IActivity
    {
        /// <inheritdoc />
        public virtual Guid Id => SquiggleActivities.FileTransfer;

        /// <inheritdoc />
        public virtual string Title => "File Transfer";

        /// <summary>
        /// Creates a file transfer handler from an incoming invite.
        /// </summary>
        public IActivityHandler FromInvite(IActivityExecutor executor, IDictionary<string, string> metadata)
        {
            var inviteData = new FileInviteData(metadata);
            return new FileTransferHandler(executor, inviteData.Name, inviteData.Size);
        }

        /// <summary>
        /// Creates a file transfer handler for an outgoing invite.
        /// </summary>
        /// <param name="executor">The activity executor managing this transfer session.</param>
        /// <param name="args">Must contain "content" (Stream), "name" (string), and "size" (long).</param>
        public IActivityHandler CreateInvite(IActivityExecutor executor, IDictionary<string, object> args)
        {
            if (!args.TryGetValue("content", out var contentObj) || contentObj is not Stream stream)
                throw new ArgumentException("args must include a 'content' Stream.", nameof(args));

            var inviteData = new FileInviteData(args.ToDictionary(x => x.Key, x => x.Value.ToString()!));
            return new FileTransferHandler(executor, inviteData.Name, inviteData.Size, stream);
        }

        /// <inheritdoc />
        public virtual Task<IDictionary<string, object>> LaunchInviteUI(ISquiggleContext context, IChatWindow window)
        {
            throw new NotImplementedException();
        }
    }
}
