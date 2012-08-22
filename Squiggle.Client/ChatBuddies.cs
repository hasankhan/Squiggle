using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Squiggle.Core;
using Squiggle.Core.Chat;
using BuddyResolver = System.Func<string, Squiggle.Chat.Buddy>;

namespace Squiggle.Chat
{
    class ChatBuddies: IEnumerable<IBuddy>
    {
        Dictionary<string, IBuddy> buddies;
        IChatSession session;
        BuddyResolver buddyResolver;

        public event EventHandler GroupChatStarted = delegate { };
        public event EventHandler<BuddyEventArgs> BuddyJoined = delegate { };
        public event EventHandler<BuddyEventArgs> BuddyLeft = delegate { };

        public ChatBuddies(IChatSession session, BuddyResolver buddyResolver, IEnumerable<IBuddy> buddies)
        {
            this.buddies = new Dictionary<string, IBuddy>();
            this.buddyResolver = buddyResolver;
            this.session = session;

            foreach (Buddy buddy in buddies)
                AddBuddy(buddy);

            this.session.GroupChatStarted += new EventHandler(session_GroupChatStarted);
            this.session.UserJoined += new EventHandler<Core.Chat.SessionEventArgs>(session_UserJoined);
            this.session.UserLeft += new EventHandler<Core.Chat.SessionEventArgs>(session_UserLeft);
        }

        public bool TryGet(string clientId, out IBuddy buddy)
        {
            return buddies.TryGetValue(clientId, out buddy);
        }

        void session_UserLeft(object sender, Core.Chat.SessionEventArgs e)
        {
            IBuddy buddy = RemoveBuddy(e.Sender.ClientID);
            if (buddy != null)
                BuddyLeft(this, new BuddyEventArgs(buddy));
        }

        void session_UserJoined(object sender, Core.Chat.SessionEventArgs e)
        {
            Buddy buddy = AddBuddy(e.Sender);
            if (buddy != null)
                BuddyJoined(this, new BuddyEventArgs(buddy));
        }

        void session_GroupChatStarted(object sender, EventArgs e)
        {
            foreach (SquiggleEndPoint user in session.RemoteUsers)
                AddBuddy(user);
            GroupChatStarted(this, EventArgs.Empty);
        }

        Buddy AddBuddy(ISquiggleEndPoint user)
        {
            Buddy buddy = buddyResolver(user.ClientID);
            AddBuddy(buddy);
            return buddy;
        }

        void AddBuddy(IBuddy buddy)
        {
            if (buddy == null || buddies.ContainsKey(buddy.Id))
                return;

            buddy.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(buddy_PropertyChanged);
            buddies[buddy.Id] = buddy;
        }

        IBuddy RemoveBuddy(string clientId)
        {
            IBuddy buddy;
            if (TryGet(clientId, out buddy))
                buddy.PropertyChanged -= new System.ComponentModel.PropertyChangedEventHandler(buddy_PropertyChanged);
            buddies.Remove(clientId);
            return buddy;
        }

        void buddy_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "ChatEndPoint")
            {
                var buddy = (Buddy)sender;
                session.UpdateUser(new SquiggleEndPoint(buddy.Id, buddy.ChatEndPoint));
            }
        }

        public IEnumerator<IBuddy> GetEnumerator()
        {
            return buddies.Values.ToList().GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
}
