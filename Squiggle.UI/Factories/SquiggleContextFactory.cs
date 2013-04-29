using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Squiggle.Client;
using Squiggle.UI.Components;
using Squiggle.UI.Windows;

namespace Squiggle.UI.Factories
{
    class SquiggleContextFactory: IInstanceFactory<SquiggleContext>
    {
        IInstanceFactory<PluginLoader> pluginLoaderFactory;
        MainWindow window;
        string clientId;

        public SquiggleContextFactory(IInstanceFactory<PluginLoader> pluginLoaderFactory, MainWindow window, string clientId)
        {
            this.pluginLoaderFactory = pluginLoaderFactory;
            this.window = window;
            this.clientId = clientId;
        }

        public SquiggleContext CreateInstance()
        {
            if (SquiggleContext.Current == null)
            {
                var pluginLoader = pluginLoaderFactory.CreateInstance();
                SquiggleContext context = new SquiggleContext();
                context.MainWindow = window;
                context.PluginLoader = pluginLoader;
                context.ChatClient = new ChatClient(clientId);
                SquiggleContext.Current = context;
            }
            return SquiggleContext.Current;
        }
    }
}
