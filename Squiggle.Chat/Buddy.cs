using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Squiggle.Chat.Services.Presence;
using System.ComponentModel;
using System.Net;

namespace Squiggle.Chat
{
    public class Buddy
    {

        IChatClient chatClient;

        public object ID { get; private set; }
        public string DisplayName { get; set; }
        public string DisplayMessage { get; set; }
        public UserStatus Status { get; set; }

        public event EventHandler<ChatStartedEventArgs> ChatStarted = delegate { };

        public Buddy(IChatClient chatClient, object id)
        {
            this.ID = id;
            this.chatClient = chatClient;
            this.chatClient.BuddyOffline += new EventHandler<BuddyEventArgs>(chatClient_BuddyOffline);
            this.chatClient.BuddyOnline += new EventHandler<BuddyEventArgs>(chatClient_BuddyOnline);
            this.chatClient.ChatStarted += new EventHandler<ChatStartedEventArgs>(chatClient_ChatStarted);
        }        

        public IChatSession StartChat()
        {
            return chatClient.StartChat(this);
        }

        public void EndChat()
        {
            chatClient.EndChat(this);
        }

        void chatClient_ChatStarted(object sender, ChatStartedEventArgs e)
        {
            ChatStarted(this, e);
        }

        void chatClient_BuddyOnline(object sender, BuddyEventArgs e)
        {
            Status = UserStatus.Online;
        }

        void chatClient_BuddyOffline(object sender, BuddyEventArgs e)
        {
            Status = UserStatus.Offline;
        }

        public override bool Equals(object obj)
        {
            if (obj != null && obj is Buddy)
            {
                var otherBuddy = (Buddy)obj;
                bool equals = this.ID.Equals(otherBuddy.ID);
                return equals;
            }
            else
                return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return this.ID.GetHashCode();
        }
    }
}
