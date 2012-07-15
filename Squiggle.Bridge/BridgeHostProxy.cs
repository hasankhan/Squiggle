using System;
using System.Diagnostics;
using System.Net.Sockets;
using System.ServiceModel;
using System.ServiceModel.Channels;
using Squiggle.Core.Presence;
using System.Net;
using System.Drawing;
using Squiggle.Utilities;
using Squiggle.Core.Chat.Host;
using Squiggle.Core.Chat;
using Squiggle.Core;
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

        public void ForwardPresenceMessage(SquiggleEndPoint recipient, byte[] message, IPEndPoint bridgeEndPoint)
        {
            EnsureProxy(p => p.ForwardPresenceMessage(recipient, message, bridgeEndPoint));
        }
        
        #endregion        

        #region IChatHost

        public void GetSessionInfo(Guid sessionId, SquiggleEndPoint sender, SquiggleEndPoint recipient)
        {
            EnsureProxy(p => p.GetSessionInfo(sessionId, sender, recipient));
        }

        public void ReceiveSessionInfo(Guid sessionId, SquiggleEndPoint sender, SquiggleEndPoint recipient, SessionInfo sessionInfo)
        {
            EnsureProxy(p=>p.ReceiveSessionInfo(sessionId, sender, recipient, sessionInfo));
        }

        public void Buzz(Guid sessionId, SquiggleEndPoint sender, SquiggleEndPoint recipient)
        {
            EnsureProxy(p => p.Buzz(sessionId, sender, recipient));
        }

        public void UserIsTyping(Guid sessionId, SquiggleEndPoint sender, SquiggleEndPoint recipient)
        {
            EnsureProxy(p => p.UserIsTyping(sessionId, sender, recipient));
        }

        public void ReceiveMessage(Guid sessionId, SquiggleEndPoint sender, SquiggleEndPoint recipient, string fontName, int fontSize, Color color, FontStyle fontStyle, string message)
        {
            EnsureProxy(p => p.ReceiveMessage(sessionId, sender, recipient, fontName, fontSize, color, fontStyle, message));
        }

        public void ReceiveChatInvite(Guid sessionId, SquiggleEndPoint sender, SquiggleEndPoint recipient, SquiggleEndPoint[] participants)
        {
            EnsureProxy(p => p.ReceiveChatInvite(sessionId, sender, recipient, participants));
        }

        public void JoinChat(Guid sessionId, SquiggleEndPoint sender, SquiggleEndPoint recipient)
        {
            EnsureProxy(p => p.JoinChat(sessionId, sender, recipient));
        }

        public void LeaveChat(Guid sessionId, SquiggleEndPoint sender, SquiggleEndPoint recipient)
        {
            EnsureProxy(p => p.LeaveChat(sessionId, sender, recipient));
        }

        public void ReceiveAppInvite(Guid sessionId, SquiggleEndPoint sender, SquiggleEndPoint recipient, Guid appId, Guid appSessionId, IEnumerable<KeyValuePair<string, string>> metadata)
        {
            EnsureProxy(p => p.ReceiveAppInvite(sessionId, sender, recipient, appId, appSessionId, metadata));
        }

        public void ReceiveAppData(Guid id, SquiggleEndPoint sender, SquiggleEndPoint recipient, byte[] chunk)
        {
            EnsureProxy(p => p.ReceiveAppData(id, sender, recipient, chunk));
        }

        public void AcceptAppInvite(Guid id, SquiggleEndPoint sender, SquiggleEndPoint recipient)
        {
            EnsureProxy(p => p.AcceptAppInvite(id, sender, recipient));
        }

        public void CancelAppSession(Guid id, SquiggleEndPoint sender, SquiggleEndPoint recipient)
        {
            EnsureProxy(p => p.CancelAppSession(id, sender, recipient));
        }

        #endregion        

        class InnerProxy : ClientBase<IBridgeHost>, IBridgeHost
        {          
            public InnerProxy(System.ServiceModel.Channels.Binding binding, System.ServiceModel.EndpointAddress remoteAddress)
                : base(binding, remoteAddress)
            {
            }

            #region IBridgeHost

            public void ForwardPresenceMessage(SquiggleEndPoint recipient, byte[] message, IPEndPoint bridgeEndPoint)
            {
                this.Channel.ForwardPresenceMessage(recipient, message, bridgeEndPoint);
            }

            public void GetSessionInfo(Guid sessionId, SquiggleEndPoint sender, SquiggleEndPoint recipient)
            {
                this.Channel.GetSessionInfo(sessionId, sender, recipient);
            }

            public void ReceiveSessionInfo(Guid sessionId, SquiggleEndPoint sender, SquiggleEndPoint recipient, SessionInfo sessionInfo)
            {
                this.Channel.ReceiveSessionInfo(sessionId, sender, recipient, sessionInfo);
            }

            #endregion

            #region IChatHost

            public void Buzz(Guid sessionId, SquiggleEndPoint sender, SquiggleEndPoint recipient)
            {
                this.Channel.Buzz(sessionId, sender, recipient);
            }

            public void UserIsTyping(Guid sessionId, SquiggleEndPoint sender, SquiggleEndPoint recipient)
            {
                this.Channel.UserIsTyping(sessionId, sender, recipient);
            }

            public void ReceiveMessage(Guid sessionId, SquiggleEndPoint sender, SquiggleEndPoint recipient, string fontName, int fontSize, Color color, System.Drawing.FontStyle fontStyle, string message)
            {
                this.Channel.ReceiveMessage(sessionId, sender, recipient, fontName, fontSize, color, fontStyle, message);
            }

            public void ReceiveChatInvite(Guid sessionId, SquiggleEndPoint sender, SquiggleEndPoint recipient, SquiggleEndPoint[] participants)
            {
                this.Channel.ReceiveChatInvite(sessionId, sender, recipient, participants);
            }

            public void JoinChat(Guid sessionId, SquiggleEndPoint sender, SquiggleEndPoint recipient)
            {
                this.Channel.JoinChat(sessionId, sender, recipient);
            }

            public void LeaveChat(Guid sessionId, SquiggleEndPoint sender, SquiggleEndPoint recipient)
            {
                this.Channel.LeaveChat(sessionId, sender, recipient);
            }

            public void ReceiveAppInvite(Guid sessionId, SquiggleEndPoint sender, SquiggleEndPoint recipient, Guid appId, Guid appSessionId, IEnumerable<KeyValuePair<string, string>> metadata)
            {
                this.Channel.ReceiveAppInvite(sessionId, sender, recipient, appId, appSessionId, metadata);
            }

            public void ReceiveAppData(Guid id, SquiggleEndPoint sender, SquiggleEndPoint recipient, byte[] chunk)
            {
                this.Channel.ReceiveAppData(id, sender, recipient, chunk);
            }

            public void AcceptAppInvite(Guid id, SquiggleEndPoint sender, SquiggleEndPoint recipient)
            {
                this.Channel.AcceptAppInvite(id, sender, recipient);
            }

            public void CancelAppSession(Guid id, SquiggleEndPoint sender, SquiggleEndPoint recipient)
            {
                this.Channel.CancelAppSession(id, sender, recipient);
            } 

            #endregion
        }

    }
}
