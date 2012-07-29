using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Squiggle.Chat;
using Squiggle.UI.Controls;
using Squiggle.UI.Controls.ChatItems;
using Squiggle.Apps;

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

        public static void AddMessage(this ChatTextBox textbox, string user, string message, string fontName, int fontSize, System.Drawing.FontStyle fontStyle, System.Drawing.Color color)
        {
            var item = new MessageItem(user, message, fontName, fontSize, fontStyle, color, textbox.MessageParsers);
            textbox.AddItem(item);
        }

        public static void AddError(this ChatTextBox textbox, string error, string detail)
        {
            var item = new ErrorItem(error, detail);
            textbox.AddItem(item);
        }

        public static void AddVoiceChatSentRequest(this ChatTextBox textbox, IVoiceChat session, string buddyName)
        {
            var item = new VoiceChatItem(session, buddyName, true, false);
            textbox.AddItem(item);
        }

        public static void AddVoiceChatReceivedRequest(this ChatTextBox textbox, IVoiceChat session, string buddyName, bool alreadyInChat)
        {
            var item = new VoiceChatItem(session, buddyName, false, alreadyInChat);
            textbox.AddItem(item);
        }

        public static void AddFileReceiveRequest(this ChatTextBox textbox, IFileTransfer session, string downloadsFolder)
        {
            var item = new FileTransferItem(session, downloadsFolder);
            textbox.AddItem(item);
        }

        public static void AddFileSentRequest(this ChatTextBox textbox, IFileTransfer session)
        {
            var item = new FileTransferItem(session);
            textbox.AddItem(item);
        }
    }
}
