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
using Squiggle.Chat.Services.Chat;

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

        public void ForwardPresenceMessage(byte[] message, IPEndPoint bridgeEndPoint)
        {
            EnsureProxy(p => p.ForwardPresenceMessage(message, bridgeEndPoint));
        }
        
        #endregion        

        #region IPresenceHost
        
        public UserInfo GetUserInfo(ChatEndPoint user)
        {
            return EnsureProxy<UserInfo>(p => p.GetUserInfo(user));
        }

        public void ReceivePresenceMessage(ChatEndPoint sender, ChatEndPoint recepient, byte[] message)
        {
            EnsureProxy(p => p.ReceivePresenceMessage(sender, recepient, message));
        }

        public SessionInfo GetSessionInfo(Guid sessionId, ChatEndPoint sender, ChatEndPoint recepient)
        {
            return EnsureProxy<SessionInfo>(p => p.GetSessionInfo(sessionId, sender, recepient));
        }
        
        #endregion        

        #region IChatHost

        public void Buzz(Guid sessionId, ChatEndPoint sender, ChatEndPoint recepient)
        {
            EnsureProxy(p => p.Buzz(sessionId, sender, recepient));
        }

        public void UserIsTyping(Guid sessionId, ChatEndPoint sender, ChatEndPoint recepient)
        {
            EnsureProxy(p => p.UserIsTyping(sessionId, sender, recepient));
        }

        public void ReceiveMessage(Guid sessionId, ChatEndPoint sender, ChatEndPoint recepient, string fontName, int fontSize, Color color, FontStyle fontStyle, string message)
        {
            EnsureProxy(p => p.ReceiveMessage(sessionId, sender, recepient, fontName, fontSize, color, fontStyle, message));
        }

        public void ReceiveChatInvite(Guid sessionId, ChatEndPoint sender, ChatEndPoint recepient, ChatEndPoint[] participants)
        {
            EnsureProxy(p => p.ReceiveChatInvite(sessionId, sender, recepient, participants));
        }

        public void JoinChat(Guid sessionId, ChatEndPoint sender, ChatEndPoint recepient)
        {
            EnsureProxy(p => p.JoinChat(sessionId, sender, recepient));
        }

        public void LeaveChat(Guid sessionId, ChatEndPoint sender, ChatEndPoint recepient)
        {
            EnsureProxy(p => p.LeaveChat(sessionId, sender, recepient));
        }

        public void ReceiveFileInvite(Guid sessionId, ChatEndPoint sender, ChatEndPoint recepient, Guid id, string name, long size)
        {
            EnsureProxy(p => p.ReceiveFileInvite(sessionId, sender, recepient, id, name, size));
        }

        public void ReceiveFileContent(Guid id, ChatEndPoint sender, ChatEndPoint recepient, byte[] chunk)
        {
            EnsureProxy(p => p.ReceiveFileContent(id, sender, recepient, chunk));
        }

        public void AcceptFileInvite(Guid id, ChatEndPoint sender, ChatEndPoint recepient)
        {
            EnsureProxy(p => p.AcceptFileInvite(id, sender, recepient));
        }

        public void CancelFileTransfer(Guid id, ChatEndPoint sender, ChatEndPoint recepient)
        {
            EnsureProxy(p => p.CancelFileTransfer(id, sender, recepient));
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
            
            public void ForwardPresenceMessage(byte[] message, IPEndPoint sourceBridge)
            {
                this.Channel.ForwardPresenceMessage(message, sourceBridge);
            } 

            #endregion

            #region IPresenceHost
            
            public Chat.Services.Presence.UserInfo GetUserInfo(ChatEndPoint user)
            {
                return this.Channel.GetUserInfo(user);
            }

            public void ReceivePresenceMessage(ChatEndPoint sender, ChatEndPoint recepient, byte[] message)
            {
                this.Channel.ReceivePresenceMessage(sender, recepient, message);
            }

            public Chat.Services.Chat.Host.SessionInfo GetSessionInfo(Guid sessionId, ChatEndPoint sender, ChatEndPoint recepient)
            {
                return this.Channel.GetSessionInfo(sessionId, sender, recepient);
            } 

            #endregion

            #region IChatHost

            public void Buzz(Guid sessionId, ChatEndPoint sender, ChatEndPoint recepient)
            {
                this.Channel.Buzz(sessionId, sender, recepient);
            }

            public void UserIsTyping(Guid sessionId, ChatEndPoint sender, ChatEndPoint recepient)
            {
                this.Channel.UserIsTyping(sessionId, sender, recepient);
            }

            public void ReceiveMessage(Guid sessionId, ChatEndPoint sender, ChatEndPoint recepient, string fontName, int fontSize, Color color, System.Drawing.FontStyle fontStyle, string message)
            {
                this.Channel.ReceiveMessage(sessionId, sender, recepient, fontName, fontSize, color, fontStyle, message);
            }

            public void ReceiveChatInvite(Guid sessionId, ChatEndPoint sender, ChatEndPoint recepient, ChatEndPoint[] participants)
            {
                this.Channel.ReceiveChatInvite(sessionId, sender, recepient, participants);
            }

            public void JoinChat(Guid sessionId, ChatEndPoint sender, ChatEndPoint recepient)
            {
                this.Channel.JoinChat(sessionId, sender, recepient);
            }

            public void LeaveChat(Guid sessionId, ChatEndPoint sender, ChatEndPoint recepient)
            {
                this.Channel.LeaveChat(sessionId, sender, recepient);
            }

            public void ReceiveFileInvite(Guid sessionId, ChatEndPoint sender, ChatEndPoint recepient, Guid id, string name, long size)
            {
                this.Channel.ReceiveFileInvite(sessionId, sender, recepient, id, name, size);
            }

            public void ReceiveFileContent(Guid id, ChatEndPoint sender, ChatEndPoint recepient, byte[] chunk)
            {
                this.Channel.ReceiveFileContent(id, sender, recepient, chunk);
            }

            public void AcceptFileInvite(Guid id, ChatEndPoint sender, ChatEndPoint recepient)
            {
                this.Channel.AcceptFileInvite(id, sender, recepient);
            }

            public void CancelFileTransfer(Guid id, ChatEndPoint sender, ChatEndPoint recepient)
            {
                this.Channel.CancelFileTransfer(id, sender, recepient);
            } 

            #endregion
        }

    }
}
