using System;
using System.Diagnostics;
using System.Net.Sockets;
using System.ServiceModel;
using System.ServiceModel.Channels;
using Squiggle.Chat;
using Squiggle.Chat.Services.Presence;
using System.Net;
using System.Drawing;
using Squiggle.Utilities;
using Squiggle.Chat.Services.Chat.Host;
using Squiggle.Chat.Services.Chat;
using Squiggle.Chat.Services;
using System.Collections.Generic;

namespace Squiggle.Bridge
{
    class BridgeHostProxy: ProxyBase<IBridgeHost>, IBridgeHost
    {
        Binding binding;
        EndpointAddress address;

        public BridgeHostProxy(Binding binding, EndpointAddress remoteAddress)
        {
            this.binding = binding;
            this.address = remoteAddress;
        }

        protected override ClientBase<IBridgeHost> CreateProxy()
        {
            return new InnerProxy(binding, address);
        }

        #region IBridgeHost

        public void ForwardPresenceMessage(byte[] message, IPEndPoint bridgeEndPoint)
        {
            EnsureProxy(p => p.ForwardPresenceMessage(message, bridgeEndPoint));
        }
        
        #endregion        

        #region IPresenceHost
        
        public UserInfo GetUserInfo(SquiggleEndPoint user)
        {
            return EnsureProxy<UserInfo>(p => p.GetUserInfo(user));
        }

        public void ReceivePresenceMessage(SquiggleEndPoint sender, SquiggleEndPoint recepient, byte[] message)
        {
            EnsureProxy(p => p.ReceivePresenceMessage(sender, recepient, message));
        }

        public SessionInfo GetSessionInfo(Guid sessionId, SquiggleEndPoint sender, SquiggleEndPoint recepient)
        {
            return EnsureProxy<SessionInfo>(p => p.GetSessionInfo(sessionId, sender, recepient));
        }
        
        #endregion        

        #region IChatHost

        public void Buzz(Guid sessionId, SquiggleEndPoint sender, SquiggleEndPoint recepient)
        {
            EnsureProxy(p => p.Buzz(sessionId, sender, recepient));
        }

        public void UserIsTyping(Guid sessionId, SquiggleEndPoint sender, SquiggleEndPoint recepient)
        {
            EnsureProxy(p => p.UserIsTyping(sessionId, sender, recepient));
        }

        public void ReceiveMessage(Guid sessionId, SquiggleEndPoint sender, SquiggleEndPoint recepient, string fontName, int fontSize, Color color, FontStyle fontStyle, string message)
        {
            EnsureProxy(p => p.ReceiveMessage(sessionId, sender, recepient, fontName, fontSize, color, fontStyle, message));
        }

        public void ReceiveChatInvite(Guid sessionId, SquiggleEndPoint sender, SquiggleEndPoint recepient, SquiggleEndPoint[] participants)
        {
            EnsureProxy(p => p.ReceiveChatInvite(sessionId, sender, recepient, participants));
        }

        public void JoinChat(Guid sessionId, SquiggleEndPoint sender, SquiggleEndPoint recepient)
        {
            EnsureProxy(p => p.JoinChat(sessionId, sender, recepient));
        }

        public void LeaveChat(Guid sessionId, SquiggleEndPoint sender, SquiggleEndPoint recepient)
        {
            EnsureProxy(p => p.LeaveChat(sessionId, sender, recepient));
        }

        public void ReceiveAppInvite(Guid sessionId, SquiggleEndPoint sender, SquiggleEndPoint recepient, Guid appId, Guid appSessionId, IEnumerable<KeyValuePair<string, string>> metadata)
        {
            EnsureProxy(p => p.ReceiveAppInvite(sessionId, sender, recepient, appId, appSessionId, metadata));
        }

        public void ReceiveAppData(Guid id, SquiggleEndPoint sender, SquiggleEndPoint recepient, byte[] chunk)
        {
            EnsureProxy(p => p.ReceiveAppData(id, sender, recepient, chunk));
        }

        public void AcceptAppInvite(Guid id, SquiggleEndPoint sender, SquiggleEndPoint recepient)
        {
            EnsureProxy(p => p.AcceptAppInvite(id, sender, recepient));
        }

        public void CancelAppSession(Guid id, SquiggleEndPoint sender, SquiggleEndPoint recepient)
        {
            EnsureProxy(p => p.CancelAppSession(id, sender, recepient));
        }

        #endregion        

        class InnerProxy : ClientBase<IBridgeHost>, IBridgeHost
        {          
            public InnerProxy(System.ServiceModel.Channels.Binding binding, System.ServiceModel.EndpointAddress remoteAddress)
                : base(binding, remoteAddress)
            {
            }

            #region IBridgeHost
            
            public void ForwardPresenceMessage(byte[] message, IPEndPoint sourceBridge)
            {
                this.Channel.ForwardPresenceMessage(message, sourceBridge);
            } 

            #endregion

            #region IPresenceHost
            
            public Chat.Services.Presence.UserInfo GetUserInfo(SquiggleEndPoint user)
            {
                return this.Channel.GetUserInfo(user);
            }

            public void ReceivePresenceMessage(SquiggleEndPoint sender, SquiggleEndPoint recepient, byte[] message)
            {
                this.Channel.ReceivePresenceMessage(sender, recepient, message);
            }

            public Chat.Services.Chat.Host.SessionInfo GetSessionInfo(Guid sessionId, SquiggleEndPoint sender, SquiggleEndPoint recepient)
            {
                return this.Channel.GetSessionInfo(sessionId, sender, recepient);
            } 

            #endregion

            #region IChatHost

            public void Buzz(Guid sessionId, SquiggleEndPoint sender, SquiggleEndPoint recepient)
            {
                this.Channel.Buzz(sessionId, sender, recepient);
            }

            public void UserIsTyping(Guid sessionId, SquiggleEndPoint sender, SquiggleEndPoint recepient)
            {
                this.Channel.UserIsTyping(sessionId, sender, recepient);
            }

            public void ReceiveMessage(Guid sessionId, SquiggleEndPoint sender, SquiggleEndPoint recepient, string fontName, int fontSize, Color color, System.Drawing.FontStyle fontStyle, string message)
            {
                this.Channel.ReceiveMessage(sessionId, sender, recepient, fontName, fontSize, color, fontStyle, message);
            }

            public void ReceiveChatInvite(Guid sessionId, SquiggleEndPoint sender, SquiggleEndPoint recepient, SquiggleEndPoint[] participants)
            {
                this.Channel.ReceiveChatInvite(sessionId, sender, recepient, participants);
            }

            public void JoinChat(Guid sessionId, SquiggleEndPoint sender, SquiggleEndPoint recepient)
            {
                this.Channel.JoinChat(sessionId, sender, recepient);
            }

            public void LeaveChat(Guid sessionId, SquiggleEndPoint sender, SquiggleEndPoint recepient)
            {
                this.Channel.LeaveChat(sessionId, sender, recepient);
            }

            public void ReceiveAppInvite(Guid sessionId, SquiggleEndPoint sender, SquiggleEndPoint recepient, Guid appId, Guid appSessionId, IEnumerable<KeyValuePair<string, string>> metadata)
            {
                this.Channel.ReceiveAppInvite(sessionId, sender, recepient, appId, appSessionId, metadata);
            }

            public void ReceiveAppData(Guid id, SquiggleEndPoint sender, SquiggleEndPoint recepient, byte[] chunk)
            {
                this.Channel.ReceiveAppData(id, sender, recepient, chunk);
            }

            public void AcceptAppInvite(Guid id, SquiggleEndPoint sender, SquiggleEndPoint recepient)
            {
                this.Channel.AcceptAppInvite(id, sender, recepient);
            }

            public void CancelAppSession(Guid id, SquiggleEndPoint sender, SquiggleEndPoint recepient)
            {
                this.Channel.CancelAppSession(id, sender, recepient);
            } 

            #endregion
        }

    }
}
