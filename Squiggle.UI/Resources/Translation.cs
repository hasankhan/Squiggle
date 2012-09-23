using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows;

namespace Squiggle.UI.Resources
{
    class Translation
    {
        public string Confirmation_FileTransferWindowClose_Title { get; set; }
        public string Confirmation_FileTransferWindowClose_Message { get; set; }
        
        public string Global_ContactSays { get; set; }
        public string Global_You { get; set; }
        public string Global_ContactSaid { get; set; }
        public string Global_ImageFilter { get; set; }        
        
        public string Popup_NewMessage { get; set; }
        
        public string ContactList_SearchContacts { get; set; }
        
        public string MainWindow_ShareAMessage { get; set; }
        
        public string ContactSelectWindow_MultiSelect { get; set; }
        public string ContactSelectWindow_Heading_InstantMessage { get; set; }
        public string ContactSelectWindow_Heading_File { get; set; }
        
        public string ChatWindow_IsTyping { get; set; }
        public string ChatWindow_MessageCouldNotBeDelivered { get; set; }
        public string ChatWindow_HasJoinedConversation { get; set; }
        public string ChatWindow_HasLeftConversation { get; set; }
        public string ChatWindow_HasSentYouBuzz { get; set; }
        public string ChatWindow_YouSentBuzz { get; set; }
        public string ChatWindow_BuzzTooEarly { get; set; }
        public string ChatWindow_CouldNotReadFile { get; set; }
        public string ChatWindow_MakeSureFileNotInUse { get; set; }
        public string ChatWindow_BroadCastChatTitle { get; set; }
        public string ChatWindow_LastMessageAt { get; set; }
        public string ChatWindow_InviteContact { get; set; }
        public string ChatWindow_FileTransferInviteNotSupported { get; set; }
        public string ChatWindow_UnknownActivityInvite { get; set; }
        public string ChatWindow_VoiceChatInviteNotSupported { get; set; }
        public string ChatWindow_FileTransferNotAllowedInGroup { get; set; }
        public string ChatWindow_AlreadyInVoiceChat { get; set; }
        public string ChatWindow_VoiceChatNotAllowedInGroup { get; set; }
        public string ChatWindow_NoBuddyWithName { get; set; }

        public string FileTransfer_Waiting { get; set; }
        public string FileTransfer_Cancel { get; set; }
        public string FileTransfer_Reject { get; set; }
        public string FileTransfer_FileSent { get; set; }
        public string FileTransfer_FileReceived { get; set; }
        public string FileTransfer_SendingCancelled { get; set; }
        public string FileTransfer_Cancelled { get; set; }
        public string FileTransfer_Sending { get; set; }
        public string FileTransfer_Receiving { get; set; }
        
        public string BuddyStatus_Online { get; set; }
        public string BuddyStatus_Busy { get; set; }
        public string BuddyStatus_BeRightBack { get; set; }
        public string BuddyStatus_Away { get; set; }
        public string BuddyStatus_Idle { get; set; }
        public string BuddyStatus_Offline { get; set; }
        
        public string Emoticon_Angry { get; set; }
        public string Emoticon_Crying { get; set; }
        public string Emoticon_Disappointed { get; set; }
        public string Emoticon_Hot { get; set; }
        public string Emoticon_Laughing { get; set; }
        public string Emoticon_Sad { get; set; }
        public string Emoticon_Sarcastic { get; set; }
        public string Emoticon_Silence { get; set; }
        public string Emoticon_Smile { get; set; }
        public string Emoticon_Surprised { get; set; }
        public string Emoticon_ToungeOut { get; set; }
        public string Emoticon_Wink { get; set; }
        public string Emoticon_Blushing { get; set; }
        public string Emoticon_Love { get; set; }
        public string Emoticon_Burn { get; set; }
        public string Emoticon_Shout { get; set; }
        public string Emoticon_Slobber { get; set; }
        public string Emoticon_Question { get; set; }
        
        public string VoiceChat_ReceivedWaiting { get; set; }
        public string VoiceChat_SentWaiting { get; set; }
        
        public string SettingsWindow_Error_InvalidPresenceIP { get; set; }
        
        public string Error { get; set; }
        public string Error_InvalidImage { get; set; }
        public string Error_NoNetwork { get; set; }
        
        public string HistoryViewer_ConfirmClear { get; set; }
        public string HistoryViewer_ConfirmDelete { get; set; }        
        
        public static Translation Instance { get; set; }

        static Translation()
        {
            Instance = new Translation();
        }

        public static void Initialize()
        {
            Instance = Instance ?? new Translation();
            foreach (PropertyInfo property in typeof(Translation).GetProperties(BindingFlags.Instance | BindingFlags.Public))
            {
                var translation = GetTranslation(property.Name);
                property.SetValue(Instance, translation, null);
            }
        }

        public static string GetTranslation(string key)
        {
            var translation = Application.Current.TryFindResource(key) as String;
            return translation;
        }
    }
}
