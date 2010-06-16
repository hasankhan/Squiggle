using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.CodeDom.Compiler;
using System.Net;
using System.Diagnostics;
using System.ServiceModel.Channels;
using System.IO;
using System.Drawing;

namespace Squiggle.Chat.Services.Chat.Host
{
    public class ChatHostProxy: IChatHost
    {
        InnerProxy proxy;
        Binding binding;
        EndpointAddress address;

        public ChatHostProxy(System.ServiceModel.Channels.Binding binding, System.ServiceModel.EndpointAddress remoteAddress)
        {
            this.binding = binding;
            this.address = remoteAddress;
            EnsureProxy();
        }

         void EnsureProxy()
         {
             if (proxy == null || proxy.State == CommunicationState.Faulted)
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

        public void UserIsTyping(IPEndPoint user)
        {
            EnsureProxy();
            proxy.UserIsTyping(user);
        }

        public void ReceiveFileInvite(IPEndPoint user, Guid id, string name, int size)
        {
            EnsureProxy();
            proxy.ReceiveFileInvite(user, id, name, size);
        }

        public void ReceiveFileContent(Guid id, byte[] chunk)
        {
            EnsureProxy();
            proxy.ReceiveFileContent(id, chunk);
        }

        public void ReceiveMessage(IPEndPoint user, string fontName, int fontSize, Color color, string message)
        {
            EnsureProxy();
            proxy.ReceiveMessage(user, fontName, fontSize, color, message);
        }

        public void AcceptFileInvite(Guid id)
        {
            EnsureProxy();
            proxy.AcceptFileInvite(id);
        }

        public void CancelFileTransfer(Guid id)
        {
            EnsureProxy();
            proxy.CancelFileTransfer(id);
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

            public void UserIsTyping(IPEndPoint user)
            {
                Trace.WriteLine("Sending typing notification to: " + user.ToString());
                base.Channel.UserIsTyping(user);
            }

            public void ReceiveFileInvite(IPEndPoint user, Guid id, string name, int size)
            {
                Trace.WriteLine("Sending file invite to: " + user.ToString() + ", name = " + name);
                base.Channel.ReceiveFileInvite(user, id, name, size);
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

            public void ReceiveMessage(IPEndPoint user, string fontName, int fontSize, Color color, string message)
            {
                Trace.WriteLine("Sending message to: " + user.ToString() + ", message = " + message);
                base.Channel.ReceiveMessage(user, fontName, fontSize, color, message);
            }

            #endregion
        }
    }
}
