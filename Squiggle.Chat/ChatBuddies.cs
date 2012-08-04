using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Squiggle.Core;
using Squiggle.Core.Chat;
using BuddyResolver = System.Func<string, Squiggle.Chat.Buddy>;

namespace Squiggle.Chat
{
    class ChatBuddies: IEnumerable<Buddy>
    {
        Dictionary<string, Buddy> buddies;
        IChatSession session;
        BuddyResolver buddyResolver;

        public event EventHandler GroupChatStarted = delegate { };
        public event EventHandler<BuddyEventArgs> BuddyJoined = delegate { };
        public event EventHandler<BuddyEventArgs> BuddyLeft = delegate { };

        public ChatBuddies(IChatSession session, BuddyResolver buddyResolver, IEnumerable<Buddy> buddies)
        {
            this.buddies = new Dictionary<string, Buddy>();
            this.buddyResolver = buddyResolver;
            this.session = session;

            foreach (Buddy buddy in buddies)
                AddBuddy(buddy);

            this.session.GroupChatStarted += new EventHandler(session_GroupChatStarted);
            this.session.UserJoined += new EventHandler<Core.Chat.Transport.Host.SessionEventArgs>(session_UserJoined);
            this.session.UserLeft += new EventHandler<Core.Chat.Transport.Host.SessionEventArgs>(session_UserLeft);
        }

        public bool TryGet(string clientId, out Buddy buddy)
        {
            return buddies.TryGetValue(clientId, out buddy);
        }

        void session_UserLeft(object sender, Core.Chat.Transport.Host.SessionEventArgs e)
        {
            Buddy buddy = RemoveBuddy(e.Sender.ClientID);
            if (buddy != null)
                BuddyLeft(this, new BuddyEventArgs(buddy));
        }

        void session_UserJoined(object sender, Core.Chat.Transport.Host.SessionEventArgs e)
        {
            Buddy buddy = AddBuddy(e.Sender);
            if (buddy != null)
                BuddyJoined(this, new BuddyEventArgs(buddy));
        }

        void session_GroupChatStarted(object sender, EventArgs e)
        {
            foreach (SquiggleEndPoint user in session.RemoteUsers)
                AddBuddy(user);
        }

        Buddy AddBuddy(SquiggleEndPoint user)
        {
            Buddy buddy = buddyResolver(user.ClientID);
            AddBuddy(buddy);
            return buddy;
        }

        void AddBuddy(Buddy buddy)
        {
            if (buddy == null || buddies.ContainsKey(buddy.Id))
                return;

            buddy.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(buddy_PropertyChanged);
            buddies[buddy.Id] = buddy;
        }

        Buddy RemoveBuddy(string clientId)
        {
            Buddy buddy;
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

        public IEnumerator<Buddy> GetEnumerator()
        {
            return buddies.Values.ToList().GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
}
