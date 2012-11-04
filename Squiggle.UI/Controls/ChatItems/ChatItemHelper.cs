using Squiggle.Client.Activities;
using Squiggle.Core.Chat.Activity;
using Squiggle.UI.Components;
using Squiggle.UI.Controls;
using Squiggle.UI.Controls.ChatItems;
using Squiggle.UI.Controls.ChatItems.Activity;
using Squiggle.UI.MessageParsers;
using System;
using System.Collections.Generic;

namespace Squiggle.UI
{
    static class ChatItemHelper
    {
        public static void RemoveAll(this IEnumerable<ChatItem> items)
        {
            foreach (var item in items)
                item.Remove();
        }

        public static void AddItems(this ChatTextBox textbox, IEnumerable<ChatItem> items)
        {
            foreach (var item in items)
                textbox.AddItem(item);
        }

        public static void AddInfo(this ChatTextBox textbox, string info)
        {
            var item = new InfoItem(info);
            textbox.AddItem(item);
        }

        public static void AddMessage(this ChatTextBox textbox, Guid id, string user, string message, string fontName, int fontSize, System.Drawing.FontStyle fontStyle, System.Drawing.Color color, MultiParser parsers, bool allowEdit)
        {
            var item = new MessageItem(user, id, message, fontName, fontSize, fontStyle, color, parsers);
            textbox.AddItem(item, allowEdit);
        }

        public static void UpdateMessage(this ChatTextBox textbox, Guid id, string message)
        {
            textbox.UpdateItem<MessageItem>(item => item.Id == id, item => item.Update(message));
        }

        public static void AddError(this ChatTextBox textbox, string error, string detail)
        {
            var item = new ErrorItem(error, detail);
            textbox.AddItem(item);
        }

        public static void AddVoiceChatSentRequest(this ChatTextBox textbox, SquiggleContext context, IVoiceChatHandler session, string buddyName)
        {
            var item = new VoiceChatItem(context, session, buddyName, true, false);
            textbox.AddItem(item);
        }

        public static void AddVoiceChatReceivedRequest(this ChatTextBox textbox, SquiggleContext context, IVoiceChatHandler session, string buddyName, bool alreadyInChat)
        {
            var item = new VoiceChatItem(context, session, buddyName, false, alreadyInChat);
            textbox.AddItem(item);
        }

        public static void AddFileSentRequest(this ChatTextBox textbox, IFileTransferHandler session)
        {
            var item = new FileTransferItem(session);
            textbox.AddItem(item);
        }

        public static void AddFileReceiveRequest(this ChatTextBox textbox, IFileTransferHandler session, string downloadsFolder)
        {
            var item = new FileTransferItem(session, downloadsFolder);
            textbox.AddItem(item);
        }

        public static void AddActivitySentRequest(this ChatTextBox textbox, string buddyName, string activity, IActivityHandler session)
        {
            var item = new GenericActivityChatItem(session, buddyName, activity, sending: true);
            textbox.AddItem(item);
        }

        public static void AddActivityReceiveRequest(this ChatTextBox textbox, string buddyName, string activity, IActivityHandler session)
        {
            var item = new GenericActivityChatItem(session, buddyName, activity, sending: false);
            textbox.AddItem(item);
        }
    }
}
