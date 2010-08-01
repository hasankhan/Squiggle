using System;
using System.Diagnostics;
using System.Net.Sockets;
using System.ServiceModel;
using System.ServiceModel.Channels;
using Squiggle.Chat;
using Squiggle.Chat.Services.Presence;
using System.Net;
using System.Drawing;
using Squiggle.Chat.Services.Chat.Host;

namespace Squiggle.Bridge
{
    class BridgeHostProxy: IBridgeHost, IDisposable
    {
        InnerProxy proxy;
        Binding binding;
        EndpointAddress address;

        public BridgeHostProxy(System.ServiceModel.Channels.Binding binding, System.ServiceModel.EndpointAddress remoteAddress)
        {
            this.binding = binding;
            this.address = remoteAddress;
            EnsureProxy();
        }

        void EnsureProxy(Action<IBridgeHost> action)
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

        T EnsureProxy<T>(Func<IBridgeHost, T> action)
        {
            EnsureProxy();
            try
            {
                return action(proxy);
            }
            catch (CommunicationException ex)
            {
                if (ex.InnerException is SocketException)
                {
                    EnsureProxy();
                    return action(proxy);
                }
                else
                    throw;
            }
        }

        void EnsureProxy()
        {
            if (proxy == null || proxy.State.In(CommunicationState.Faulted, CommunicationState.Closed, CommunicationState.Closing))
            {
                if (proxy == null)
                    proxy = new InnerProxy(binding, address);
                else
                {
                    Close();
                    proxy = new InnerProxy(binding, address);
                }
            }
        }

        void Close()
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
        }

        #region IBridgeHost

        public void ReceiveMessage(byte[] message)
        {
            EnsureProxy(p => p.ReceiveMessage(message));
        }
        
        #endregion        

        #region IPresenceHost
        
        public UserInfo GetUserInfo()
        {
            return EnsureProxy<UserInfo>(p => p.GetUserInfo());
        }

        public void ReceiveMessage(IPEndPoint sender, byte[] message)
        {
            EnsureProxy(p => p.ReceiveMessage(sender, message));
        }

        public SessionInfo GetSessionInfo(Guid sessionId, IPEndPoint user)
        {
            return EnsureProxy<SessionInfo>(p => p.GetSessionInfo(sessionId, user));
        }
        
        #endregion        

        #region IChatHost

        public void Buzz(Guid sessionId, IPEndPoint user)
        {
            EnsureProxy(p => p.Buzz(sessionId, user));
        }

        public void UserIsTyping(Guid sessionId, IPEndPoint user)
        {
            EnsureProxy(p => p.UserIsTyping(sessionId, user));
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

        public void Dispose()
        {
            Close();
        }

        class InnerProxy : ClientBase<IBridgeHost>, IBridgeHost
        {
            public InnerProxy()
            {
            }
            public InnerProxy(string endpointConfigurationName)
                :
                    base(endpointConfigurationName)
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

            #region IBridgeHost
            
            public void ReceiveMessage(byte[] message)
            {
                this.Channel.ReceiveMessage(message);
            } 

            #endregion

            #region IPresenceHost
            
            public Chat.Services.Presence.UserInfo GetUserInfo()
            {
                return this.Channel.GetUserInfo();
            }

            public void ReceiveMessage(IPEndPoint sender, byte[] message)
            {
                this.Channel.ReceiveMessage(sender, message);
            }

            public Chat.Services.Chat.Host.SessionInfo GetSessionInfo(Guid sessionId, IPEndPoint user)
            {
                return this.Channel.GetSessionInfo(sessionId, user);
            } 

            #endregion

            #region IChatHost
            
            public void Buzz(Guid sessionId, IPEndPoint user)
            {
                this.Channel.Buzz(sessionId, user);
            }

            public void UserIsTyping(Guid sessionId, IPEndPoint user)
            {
                this.Channel.UserIsTyping(sessionId, user);
            }

            public void ReceiveMessage(Guid sessionId, IPEndPoint user, string fontName, int fontSize, System.Drawing.Color color, System.Drawing.FontStyle fontStyle, string message)
            {
                this.Channel.ReceiveMessage(sessionId, user, fontName, fontSize, color, fontStyle, message);
            }

            public void ReceiveChatInvite(Guid sessionId, IPEndPoint user, IPEndPoint[] participants)
            {
                this.Channel.ReceiveChatInvite(sessionId, user, participants);
            }

            public void JoinChat(Guid sessionId, IPEndPoint user)
            {
                this.Channel.JoinChat(sessionId, user);
            }

            public void LeaveChat(Guid sessionId, IPEndPoint user)
            {
                this.Channel.LeaveChat(sessionId, user);
            }

            public void ReceiveFileInvite(Guid sessionId, IPEndPoint user, Guid id, string name, int size)
            {
                this.Channel.ReceiveFileInvite(sessionId, user, id, name, size);
            }

            public void ReceiveFileContent(Guid id, byte[] chunk)
            {
                this.Channel.ReceiveFileContent(id, chunk);
            }

            public void AcceptFileInvite(Guid id)
            {
                this.Channel.AcceptFileInvite(id);
            }

            public void CancelFileTransfer(Guid id)
            {
                this.Channel.CancelFileTransfer(id);
            } 

            #endregion
        }

    }
}
