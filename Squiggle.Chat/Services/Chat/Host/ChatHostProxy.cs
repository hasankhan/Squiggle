using System;
using System.Diagnostics;
using System.Drawing;
using System.Net;
using System.Net.Sockets;
using Squiggle.Utilities;
using System.ServiceModel;
using System.ServiceModel.Channels;

namespace Squiggle.Chat.Services.Chat.Host
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

        public void ReceiveFileInvite(Guid sessionId, SquiggleEndPoint sender, SquiggleEndPoint recepient, Guid id, string name, long size)
        {
            EnsureProxy(p => p.ReceiveFileInvite(sessionId, sender, recepient, id, name, size));
        }

        public void ReceiveFileContent(Guid id, SquiggleEndPoint sender, SquiggleEndPoint recepient, byte[] chunk)
        {
            EnsureProxy(p => p.ReceiveFileContent(id, sender, recepient, chunk));
        }

        public void AcceptFileInvite(Guid id, SquiggleEndPoint sender, SquiggleEndPoint recepient)
        {
            EnsureProxy(p => p.AcceptFileInvite(id, sender, recepient));
        }

        public void CancelFileTransfer(Guid id, SquiggleEndPoint sender, SquiggleEndPoint recepient)
        {
            EnsureProxy(p => p.CancelFileTransfer(id, sender, recepient));
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
                Trace.WriteLine("Sending message from: " + sender + ", message = " + message);
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

            public void ReceiveFileInvite(Guid sessionId, SquiggleEndPoint sender, SquiggleEndPoint recepient, Guid id, string name, long size)
            {
                Trace.WriteLine("Sending file invite from: " + sender + ", name = " + name);
                base.Channel.ReceiveFileInvite(sessionId, sender, recepient, id, name, size);
            }

            public void ReceiveFileContent(Guid id, SquiggleEndPoint sender, SquiggleEndPoint recepient, byte[] chunk)
            {
                Trace.WriteLine("Sending file content: " + id.ToString());
                base.Channel.ReceiveFileContent(id, sender, recepient, chunk);
            }

            public void AcceptFileInvite(Guid id, SquiggleEndPoint sender, SquiggleEndPoint recepient)
            {
                Trace.WriteLine("Accepting file invite: " + id.ToString());
                base.Channel.AcceptFileInvite(id, sender, recepient);
            }

            public void CancelFileTransfer(Guid id, SquiggleEndPoint sender, SquiggleEndPoint recepient)
            {
                Trace.WriteLine("Cancel file transfer: " + id.ToString());
                base.Channel.CancelFileTransfer(id, sender, recepient);
            }

            #endregion
        }
    }
}
