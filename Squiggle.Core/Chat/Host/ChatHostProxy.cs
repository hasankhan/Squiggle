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

        public void UserIsTyping(Guid sessionId, SquiggleEndPoint sender, SquiggleEndPoint recepient)
        {
            EnsureProxy(p => p.UserIsTyping(sessionId, sender, recepient));
        }

        public SessionInfo GetSessionInfo(Guid sessionId, SquiggleEndPoint sender, SquiggleEndPoint recepient)
        {
            SessionInfo info = null;
            EnsureProxy(p => info = p.GetSessionInfo(sessionId, sender, recepient));
            return info;
        }

        public void Buzz(Guid sessionId, SquiggleEndPoint sender, SquiggleEndPoint recepient)
        {
            EnsureProxy(p => p.Buzz(sessionId, sender, recepient));
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

        public void ReceiveAppData(Guid appSessionId, SquiggleEndPoint sender, SquiggleEndPoint recepient, byte[] chunk)
        {
            EnsureProxy(p => p.ReceiveAppData(appSessionId, sender, recepient, chunk));
        }

        public void AcceptAppInvite(Guid appSessionId, SquiggleEndPoint sender, SquiggleEndPoint recepient)
        {
            EnsureProxy(p => p.AcceptAppInvite(appSessionId, sender, recepient));
        }

        public void CancelAppSession(Guid appSessionId, SquiggleEndPoint sender, SquiggleEndPoint recepient)
        {
            EnsureProxy(p => p.CancelAppSession(appSessionId, sender, recepient));
        }

        #endregion

        class InnerProxy : ClientBase<IChatHost>, IChatHost
        {
            public InnerProxy(System.ServiceModel.Channels.Binding binding, System.ServiceModel.EndpointAddress remoteAddress)
                : base(binding, remoteAddress)
            {
            }

            #region IChatHost Members

            public void UserIsTyping(Guid sessionId, SquiggleEndPoint sender, SquiggleEndPoint recepient)
            {
                Trace.WriteLine("Sending typing notification from: " + sender);
                base.Channel.UserIsTyping(sessionId, sender, recepient);
            }

            public SessionInfo GetSessionInfo(Guid sessionId, SquiggleEndPoint sender, SquiggleEndPoint recepient)
            {
                Trace.WriteLine("Getting session information from: " + sender);
                return base.Channel.GetSessionInfo(sessionId, sender, recepient);
            }

            public void Buzz(Guid sessionId, SquiggleEndPoint sender, SquiggleEndPoint recepient)
            {
                Trace.WriteLine("Sending buzz from: " + sender);
                base.Channel.Buzz(sessionId, sender, recepient);
            }

            public void ReceiveMessage(Guid sessionId, SquiggleEndPoint sender, SquiggleEndPoint recepient, string fontName, int fontSize, Color color, FontStyle fontStyle, string message)
            {
                Trace.WriteLine("Sending message from: " + sender);
                base.Channel.ReceiveMessage(sessionId, sender, recepient, fontName, fontSize, color, fontStyle, message);
            }

            public void ReceiveChatInvite(Guid sessionId, SquiggleEndPoint sender, SquiggleEndPoint recepient, SquiggleEndPoint[] participants)
            {
                Trace.WriteLine("Sending chat invite from: " + sender);
                base.Channel.ReceiveChatInvite(sessionId, sender, recepient, participants);
            }

            public void JoinChat(Guid sessionId, SquiggleEndPoint sender, SquiggleEndPoint recepient)
            {
                Trace.WriteLine(sender + " has joined chat: " + sessionId);
                base.Channel.JoinChat(sessionId, sender, recepient);
            }

            public void LeaveChat(Guid sessionId, SquiggleEndPoint sender, SquiggleEndPoint recepient)
            {
                Trace.WriteLine(sender + " has left chat: " + sessionId);
                base.Channel.LeaveChat(sessionId, sender, recepient);
            }

            public void ReceiveAppInvite(Guid sessionId, SquiggleEndPoint sender, SquiggleEndPoint recepient, Guid appId, Guid appSessionId, IEnumerable<KeyValuePair<string, string>> metadata)
            {
                Trace.WriteLine("Sending file invite from: " + sender + ", " + metadata.ToTraceString());
                base.Channel.ReceiveAppInvite(sessionId, sender, recepient, appId, appSessionId, metadata);
            }

            public void ReceiveAppData(Guid appSessionId, SquiggleEndPoint sender, SquiggleEndPoint recepient, byte[] chunk)
            {
                Trace.WriteLine("Sending file content: " + appSessionId.ToString());
                base.Channel.ReceiveAppData(appSessionId, sender, recepient, chunk);
            }

            public void AcceptAppInvite(Guid appSessionId, SquiggleEndPoint sender, SquiggleEndPoint recepient)
            {
                Trace.WriteLine("Accepting file invite: " + appSessionId.ToString());
                base.Channel.AcceptAppInvite(appSessionId, sender, recepient);
            }

            public void CancelAppSession(Guid appSessionId, SquiggleEndPoint sender, SquiggleEndPoint recepient)
            {
                Trace.WriteLine("Cancel file transfer: " + appSessionId.ToString());
                base.Channel.CancelAppSession(appSessionId, sender, recepient);
            }

            #endregion
        }
    }
}
