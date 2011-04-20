using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;

namespace Squiggle.Utilities
{
    public abstract class WcfHost
    {
        ServiceHost serviceHost;

        public void Start()
        {
            if (serviceHost != null)
                throw new InvalidOperationException();

            OnStart();
        }        

        public void Stop()
        {
            if (serviceHost == null)
                return;

            OnStop();

            serviceHost = null;
        }        

        protected abstract ServiceHost CreateHost();

        protected virtual void OnStart() 
        {
            InitServiceHost();
        }

        protected virtual void OnStop()
        {
            DestroyServiceHost();
        }

        void InitServiceHost()
        {
            serviceHost = CreateHost();
            WcfConfig.ConfigureHost(serviceHost);
            serviceHost.Faulted += new EventHandler(serviceHost_Faulted);
            serviceHost.Open();
        }

        void DestroyServiceHost()
        {
            serviceHost.Faulted -= new EventHandler(serviceHost_Faulted);

            try
            {
                serviceHost.Close();
            }
            catch (CommunicationObjectFaultedException)
            {
                serviceHost.Abort();
            }
            catch (TimeoutException)
            {
                serviceHost.Abort();
            }
        }

        void serviceHost_Faulted(object sender, EventArgs e)
        {
            DestroyServiceHost();
            InitServiceHost();
        }
    }
}
