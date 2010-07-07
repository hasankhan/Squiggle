using System;
using System.Diagnostics;
using System.Drawing;
using System.Net;
using System.Net.Sockets;
using System.ServiceModel;
using System.ServiceModel.Channels;

namespace Squiggle.Chat.Services.Chat.Host
{
    public class ChatHostProxy: IChatHost
    {
        InnerProxy proxy;
        Binding binding;
        EndpointAddress address;

        public ChatHostProxy(IPEndPoint remoteEndPoint)
        {
            Uri uri = CreateServiceUri(remoteEndPoint.ToString());
            var binding = new NetTcpBinding(SecurityMode.None);
            this.binding = binding;
            this.binding.SendTimeout = TimeSpan.FromSeconds(5);
            this.address = new EndpointAddress(uri);
            EnsureProxy();
        }

        static Uri CreateServiceUri(string address)
        {
            var uri = new Uri("net.tcp://" + address + "/squigglechat");
            return uri;
        }

        void EnsureProxy(Action<IChatHost> action)
        {
            EnsureProxy();
            try
            {
                action(proxy);
            }
            catch (CommunicationException ex)
            {
                if (ex.InnerException is SocketException)
                {
                    EnsureProxy();
                    action(proxy);
                }
                else
                    throw;
            }
        }

        void EnsureProxy()
        {
            if (proxy == null ||
                proxy.State == CommunicationState.Faulted ||
                proxy.State == CommunicationState.Closed ||
                proxy.State == CommunicationState.Closing)
            {
                if (proxy == null)
                    proxy = new InnerProxy(binding, address);
                else
                {
                    try
                    {
                        proxy.Close();
                    }
                    catch (Exception ex)
                    {
                        Trace.WriteLine(ex.Message);
                        proxy.Abort();
                    }
                    finally
                    {
                        proxy = new InnerProxy(binding, address);
                    }
                }
            }
        }

        #region IChatHost Members

        public void UserIsTyping(Guid sessionId, IPEndPoint user)
        {
            EnsureProxy(p => p.UserIsTyping(sessionId, user));
        }

        public void Buzz(Guid sessionId, IPEndPoint user)
        {
            EnsureProxy(p => p.Buzz(sessionId, user));
        }

        public void ReceiveMessage(Guid sessionId, IPEndPoint user, string fontName, int fontSize, Color color, FontStyle fontStyle, string message)
        {
            EnsureProxy(p => p.ReceiveMessage(sessionId, user, fontName, fontSize, color, fontStyle, message));
        }

        public void ReceiveChatInvite(Guid sessionId, IPEndPoint user, IPEndPoint[] participants)
        {
            EnsureProxy(p => p.ReceiveChatInvite(sessionId, user, participants));
        }

        public void JoinChat(Guid sessionId, IPEndPoint user)
        {
            EnsureProxy(p => p.JoinChat(sessionId, user));
        }

        public void LeaveChat(Guid sessionId, IPEndPoint user)
        {
            EnsureProxy(p => p.LeaveChat(sessionId, user));
        }

        public void ReceiveFileInvite(Guid sessionId, IPEndPoint user, Guid id, string name, int size)
        {
            EnsureProxy(p => p.ReceiveFileInvite(sessionId, user, id, name, size));
        }

        public void ReceiveFileContent(Guid id, byte[] chunk)
        {
            EnsureProxy(p => p.ReceiveFileContent(id, chunk));
        }        

        public void AcceptFileInvite(Guid id)
        {
            EnsureProxy(p => p.AcceptFileInvite(id));
        }

        public void CancelFileTransfer(Guid id)
        {
            EnsureProxy(p => p.CancelFileTransfer(id));
        }

        #endregion

        class InnerProxy : ClientBase<IChatHost>, IChatHost
        {
            public InnerProxy()
            {
            }

            public InnerProxy(string endpointConfigurationName)
                :
                    base(endpointConfigurationName)
            {
            }

            public InnerProxy(string endpointConfigurationName, string remoteAddress)
                :
                    base(endpointConfigurationName, remoteAddress)
            {
            }

            public InnerProxy(string endpointConfigurationName, System.ServiceModel.EndpointAddress remoteAddress)
                :
                    base(endpointConfigurationName, remoteAddress)
            {
            }

            public InnerProxy(System.ServiceModel.Channels.Binding binding, System.ServiceModel.EndpointAddress remoteAddress)
                :
                    base(binding, remoteAddress)
            {
            }

            #region IChatHost Members

            public void UserIsTyping(Guid sessionId, IPEndPoint user)
            {
                Trace.WriteLine("Sending typing notification to: " + user.ToString());
                base.Channel.UserIsTyping(sessionId, user);
            }

            public void Buzz(Guid sessionId, IPEndPoint user)
            {
                Trace.WriteLine("Sending buzz to: " + user.ToString());
                base.Channel.Buzz(sessionId, user);
            }

            public void ReceiveMessage(Guid sessionId, IPEndPoint user, string fontName, int fontSize, Color color, FontStyle fontStyle, string message)
            {
                Trace.WriteLine("Sending message to: " + user.ToString() + ", message = " + message);
                base.Channel.ReceiveMessage(sessionId, user, fontName, fontSize, color, fontStyle, message);
            }

            public void ReceiveChatInvite(Guid sessionId, IPEndPoint user, IPEndPoint[] participants)
            {
                Trace.WriteLine("Sending chat invite to: " + user.ToString());
                base.Channel.ReceiveChatInvite(sessionId, user, participants);
            }

            public void JoinChat(Guid sessionId, IPEndPoint user)
            {
                Trace.WriteLine(user.ToString() + " has joined chat: " + sessionId);
                base.Channel.JoinChat(sessionId, user);
            }

            public void LeaveChat(Guid sessionId, IPEndPoint user)
            {
                Trace.WriteLine(user.ToString() + " has left chat: " + sessionId);
                base.Channel.LeaveChat(sessionId, user);
            }

            public void ReceiveFileInvite(Guid sessionId, IPEndPoint user, Guid id, string name, int size)
            {
                Trace.WriteLine("Sending file invite to: " + user.ToString() + ", name = " + name);
                base.Channel.ReceiveFileInvite(sessionId, user, id, name, size);
            }

            public void ReceiveFileContent(Guid id, byte[] chunk)
            {
                Trace.WriteLine("Sending file content: " + id.ToString());
                base.Channel.ReceiveFileContent(id, chunk);
            }

            public void AcceptFileInvite(Guid id)
            {
                Trace.WriteLine("Accepting file invite: " + id.ToString());
                base.Channel.AcceptFileInvite(id);
            }

            public void CancelFileTransfer(Guid id)
            {
                Trace.WriteLine("Cancel file transfer: " + id.ToString());
                base.Channel.CancelFileTransfer(id);
            }

            #endregion
        }
    }
}
