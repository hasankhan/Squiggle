using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;

namespace Squiggle.Chat.Services.Chat.Host
{
    public class ChatHostProxyFactory
    {
        static Dictionary<IPEndPoint, ChatHostProxy> proxies = new Dictionary<IPEndPoint, ChatHostProxy>();

        public static ChatHostProxy Get(IPEndPoint endpoint)
        {
            lock (proxies)
            {
                ChatHostProxy proxy;
                if (!proxies.TryGetValue(endpoint, out proxy))
                    proxies[endpoint] = proxy = new ChatHostProxy(endpoint);
                return proxy;
            }
        }
    }
}
