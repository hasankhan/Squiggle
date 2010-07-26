using System;
using System.Diagnostics;
using System.Net.Sockets;
using System.ServiceModel;
using System.ServiceModel.Channels;
using Squiggle.Chat;

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

        public void ReceiveMessage(byte[] message)
        {
            EnsureProxy(p => p.ReceiveMessage(message));
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

            public void ReceiveMessage(byte[] message)
            {
                this.Channel.ReceiveMessage(message);
            }
        }

        public void Dispose()
        {
            Close();
        }
    }
}
