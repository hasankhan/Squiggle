using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.Net.Sockets;
using System.Diagnostics;

namespace Squiggle.Utilities
{
    public abstract class ProxyBase<T>: IDisposable where T: class
    {
        ClientBase<T> proxy;
        protected abstract ClientBase<T> CreateProxy();

        protected void EnsureProxy(Action<T> action)
        {
            EnsureProxy(proxy =>
            {
                action(proxy);
                return (object)null;
            });
        }

        protected TReturn EnsureProxy<TReturn>(Func<T, TReturn> action)
        {
            EnsureProxy();
            try
            {
                return action(proxy as T);
            }
            catch (CommunicationException ex)
            {
                if (ex.InnerException is SocketException)
                {
                    EnsureProxy();
                    return action(proxy as T);
                }
                else
                    throw;
            }
        }        

        void EnsureProxy()
        {
            if (proxy == null || proxy.State.In(CommunicationState.Faulted, CommunicationState.Closed, CommunicationState.Closing))
            {
                if (proxy != null)
                    Close();
                proxy = CreateProxy();                
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

        public void Dispose()
        {
            Close();
        }        
    }
}
