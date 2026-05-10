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
        public string Confirmation_FileTransferWindowClose_Title { get; set; } = null!;
        public string Confirmation_FileTransferWindowClose_Message { get; set; } = null!;
        
        public string Global_ContactSays { get; set; } = null!;
        public string Global_You { get; set; } = null!;
        public string Global_ContactSaid { get; set; } = null!;
        public string Global_ImageFilter { get; set; } = null!;        
        
        public string Popup_NewMessage { get; set; } = null!;
        
        public string ContactList_SearchContacts { get; set; } = null!;
        
        public string MainWindow_ShareAMessage { get; set; } = null!;
        
        public string ContactSelectWindow_MultiSelect { get; set; } = null!;
        public string ContactSelectWindow_Heading_InstantMessage { get; set; } = null!;
        public string ContactSelectWindow_Heading_File { get; set; } = null!;
        
        public string ChatWindow_IsTyping { get; set; } = null!;
        public string ChatWindow_MessageCouldNotBeDelivered { get; set; } = null!;
        public string ChatWindow_HasJoinedConversation { get; set; } = null!;
        public string ChatWindow_HasLeftConversation { get; set; } = null!;
        public string ChatWindow_HasSentYouBuzz { get; set; } = null!;
        public string ChatWindow_YouSentBuzz { get; set; } = null!;
        public string ChatWindow_BuzzTooEarly { get; set; } = null!;
        public string ChatWindow_CouldNotReadFile { get; set; } = null!;
        public string ChatWindow_MakeSureFileNotInUse { get; set; } = null!;
        public string ChatWindow_BroadCastChatTitle { get; set; } = null!;
        public string ChatWindow_LastMessageAt { get; set; } = null!;
        public string ChatWindow_InviteContact { get; set; } = null!;
        public string ChatWindow_FileTransferInviteNotSupported { get; set; } = null!;
        public string ChatWindow_UnknownActivityInvite { get; set; } = null!;
        public string ChatWindow_VoiceChatInviteNotSupported { get; set; } = null!;
        public string ChatWindow_FileTransferNotAllowedInGroup { get; set; } = null!;
        public string ChatWindow_AlreadyInVoiceChat { get; set; } = null!;
        public string ChatWindow_VoiceChatNotAllowedInGroup { get; set; } = null!;
        public string ChatWindow_NoBuddyWithName { get; set; } = null!;

        public string FileTransfer_Waiting { get; set; } = null!;       
        public string FileTransfer_FileSent { get; set; } = null!;
        public string FileTransfer_FileReceived { get; set; } = null!;
        public string FileTransfer_SendingCancelled { get; set; } = null!;
        public string FileTransfer_Cancelled { get; set; } = null!;
        public string FileTransfer_Sending { get; set; } = null!;
        public string FileTransfer_Receiving { get; set; } = null!;

        public string Activity_Invitation { get; set; } = null!;
        public string Activity_Waiting { get; set; } = null!;
        public string Activity_Cancel { get; set; } = null!;
        public string Activity_Reject { get; set; } = null!;
        public string Activity_Completed { get; set; } = null!;
        public string Activity_Cancelled { get; set; } = null!;
        public string Activity_Started { get; set; } = null!;
        
        public string BuddyStatus_Online { get; set; } = null!;
        public string BuddyStatus_Busy { get; set; } = null!;
        public string BuddyStatus_BeRightBack { get; set; } = null!;
        public string BuddyStatus_Away { get; set; } = null!;
        public string BuddyStatus_Idle { get; set; } = null!;
        public string BuddyStatus_Offline { get; set; } = null!;
        
        public string Emoticon_Angry { get; set; } = null!;
        public string Emoticon_Crying { get; set; } = null!;
        public string Emoticon_Disappointed { get; set; } = null!;
        public string Emoticon_Hot { get; set; } = null!;
        public string Emoticon_Laughing { get; set; } = null!;
        public string Emoticon_Sad { get; set; } = null!;
        public string Emoticon_Sarcastic { get; set; } = null!;
        public string Emoticon_Silence { get; set; } = null!;
        public string Emoticon_Smile { get; set; } = null!;
        public string Emoticon_Surprised { get; set; } = null!;
        public string Emoticon_ToungeOut { get; set; } = null!;
        public string Emoticon_Wink { get; set; } = null!;
        public string Emoticon_Blushing { get; set; } = null!;
        public string Emoticon_Love { get; set; } = null!;
        public string Emoticon_Burn { get; set; } = null!;
        public string Emoticon_Shout { get; set; } = null!;
        public string Emoticon_Slobber { get; set; } = null!;
        public string Emoticon_Question { get; set; } = null!;
        
        public string VoiceChat_ReceivedWaiting { get; set; } = null!;
        public string VoiceChat_SentWaiting { get; set; } = null!;
        
        public string SettingsWindow_Error_InvalidPresenceIP { get; set; } = null!;
        
        public string Error { get; set; } = null!;
        public string Error_InvalidImage { get; set; } = null!;
        public string Error_NoNetwork { get; set; } = null!;
        
        public string HistoryViewer_ConfirmClear { get; set; } = null!;
        public string HistoryViewer_ConfirmDelete { get; set; } = null!;

        public string Authentication_Failed { get; set; } = null!;
        public string Authentication_ServiceUnavailable { get; set; } = null!;

        public static Translation Instance { get; set; } = null!;

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

        public static string? GetTranslation(string key)
        {
            var translation = Application.Current.TryFindResource(key) as String;
            return translation;
        }
    }
}
