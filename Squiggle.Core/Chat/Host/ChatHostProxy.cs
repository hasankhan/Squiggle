using System;
using System.Diagnostics;
using System.Drawing;
using System.Net;
using System.Net.Sockets;
using Squiggle.Utilities;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Collections.Generic;

namespace Squiggle.Core.Chat.Host
{
    public class ChatHostProxy: ProxyBase<IChatHost>, IChatHost
    {
        Binding binding;
        EndpointAddress address;

        public ChatHostProxy(IPEndPoint remoteEndPoint)
        {
            Uri uri = CreateServiceUri(remoteEndPoint.ToString());
            this.binding = WcfConfig.CreateBinding(); ;
#if !DEBUG
            this.binding.SendTimeout = TimeSpan.FromSeconds(5);
#endif
            this.address = new EndpointAddress(uri);
        }

        protected override ClientBase<IChatHost> CreateProxy()
        {
            return new InnerProxy(binding, address);
        }

        static Uri CreateServiceUri(string address)
        {
            var uri = new Uri("net.tcp://" + address + "/" + ServiceNames.ChatService);
            return uri;
        }

        #region IChatHost Members

        public void UserIsTyping(Guid sessionId, SquiggleEndPoint sender, SquiggleEndPoint recipient)
        {
            EnsureProxy(p => p.UserIsTyping(sessionId, sender, recipient));
        }

        public void GetSessionInfo(Guid sessionId, SquiggleEndPoint sender, SquiggleEndPoint recipient)
        {
            EnsureProxy(p => p.GetSessionInfo(sessionId, sender, recipient));
        }

        public void ReceiveSessionInfo(Guid sessionId, SquiggleEndPoint sender, SquiggleEndPoint recipient, SessionInfo sessionInfo)
        {
            EnsureProxy(p => p.ReceiveSessionInfo(sessionId, sender, recipient, sessionInfo));
        }

        public void Buzz(Guid sessionId, SquiggleEndPoint sender, SquiggleEndPoint recipient)
        {
            EnsureProxy(p => p.Buzz(sessionId, sender, recipient));
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

        public void ReceiveAppData(Guid appSessionId, SquiggleEndPoint sender, SquiggleEndPoint recipient, byte[] chunk)
        {
            EnsureProxy(p => p.ReceiveAppData(appSessionId, sender, recipient, chunk));
        }

        public void AcceptAppInvite(Guid appSessionId, SquiggleEndPoint sender, SquiggleEndPoint recipient)
        {
            EnsureProxy(p => p.AcceptAppInvite(appSessionId, sender, recipient));
        }

        public void CancelAppSession(Guid appSessionId, SquiggleEndPoint sender, SquiggleEndPoint recipient)
        {
            EnsureProxy(p => p.CancelAppSession(appSessionId, sender, recipient));
        }

        #endregion

        class InnerProxy : ClientBase<IChatHost>, IChatHost
        {
            public InnerProxy(System.ServiceModel.Channels.Binding binding, System.ServiceModel.EndpointAddress remoteAddress)
                : base(binding, remoteAddress)
            {
            }

            #region IChatHost Members

            public void UserIsTyping(Guid sessionId, SquiggleEndPoint sender, SquiggleEndPoint recipient)
            {
                Trace.WriteLine("Sending typing notification from: " + sender);
                base.Channel.UserIsTyping(sessionId, sender, recipient);
            }

            public void GetSessionInfo(Guid sessionId, SquiggleEndPoint sender, SquiggleEndPoint recipient)
            {
                Trace.WriteLine("Getting session information from: " + sender);
                base.Channel.GetSessionInfo(sessionId, sender, recipient);
            }

            public void ReceiveSessionInfo(Guid sessionId, SquiggleEndPoint sender, SquiggleEndPoint recipient, SessionInfo sessionInfo)
            {
                Trace.WriteLine("Sending session information to: " + recipient);
                base.Channel.ReceiveSessionInfo(sessionId, sender, recipient, sessionInfo);
            }

            public void Buzz(Guid sessionId, SquiggleEndPoint sender, SquiggleEndPoint recipient)
            {
                Trace.WriteLine("Sending buzz from: " + sender);
                base.Channel.Buzz(sessionId, sender, recipient);
            }

            public void ReceiveMessage(Guid sessionId, SquiggleEndPoint sender, SquiggleEndPoint recipient, string fontName, int fontSize, Color color, FontStyle fontStyle, string message)
            {
                Trace.WriteLine("Sending message from: " + sender);
                base.Channel.ReceiveMessage(sessionId, sender, recipient, fontName, fontSize, color, fontStyle, message);
            }

            public void ReceiveChatInvite(Guid sessionId, SquiggleEndPoint sender, SquiggleEndPoint recipient, SquiggleEndPoint[] participants)
            {
                Trace.WriteLine("Sending chat invite from: " + sender);
                base.Channel.ReceiveChatInvite(sessionId, sender, recipient, participants);
            }

            public void JoinChat(Guid sessionId, SquiggleEndPoint sender, SquiggleEndPoint recipient)
            {
                Trace.WriteLine(sender + " has joined chat: " + sessionId);
                base.Channel.JoinChat(sessionId, sender, recipient);
            }

            public void LeaveChat(Guid sessionId, SquiggleEndPoint sender, SquiggleEndPoint recipient)
            {
                Trace.WriteLine(sender + " has left chat: " + sessionId);
                base.Channel.LeaveChat(sessionId, sender, recipient);
            }

            public void ReceiveAppInvite(Guid sessionId, SquiggleEndPoint sender, SquiggleEndPoint recipient, Guid appId, Guid appSessionId, IEnumerable<KeyValuePair<string, string>> metadata)
            {
                Trace.WriteLine("Sending file invite from: " + sender + ", " + metadata.ToTraceString());
                base.Channel.ReceiveAppInvite(sessionId, sender, recipient, appId, appSessionId, metadata);
            }

            public void ReceiveAppData(Guid appSessionId, SquiggleEndPoint sender, SquiggleEndPoint recipient, byte[] chunk)
            {
                Trace.WriteLine("Sending file content: " + appSessionId.ToString());
                base.Channel.ReceiveAppData(appSessionId, sender, recipient, chunk);
            }

            public void AcceptAppInvite(Guid appSessionId, SquiggleEndPoint sender, SquiggleEndPoint recipient)
            {
                Trace.WriteLine("Accepting file invite: " + appSessionId.ToString());
                base.Channel.AcceptAppInvite(appSessionId, sender, recipient);
            }

            public void CancelAppSession(Guid appSessionId, SquiggleEndPoint sender, SquiggleEndPoint recipient)
            {
                Trace.WriteLine("Cancel file transfer: " + appSessionId.ToString());
                base.Channel.CancelAppSession(appSessionId, sender, recipient);
            }

            #endregion
        }
    }
}
