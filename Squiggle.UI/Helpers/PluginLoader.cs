using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Squiggle.Activities;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Primitives;
using System.ComponentModel.Composition.Hosting;
using Squiggle.Core.Chat;

namespace Squiggle.UI.Helpers
{
    class PluginLoader
    {
        [ImportMany(typeof(IAppHandlerFactory))]
        public IEnumerable<IAppHandlerFactory> AppHandlerFactories { get; set; }

        public bool VoiceChat { get; private set; }
        public bool FileTransfer { get; private set; }

        public PluginLoader(ComposablePartCatalog catalog)
        {
            var container = new CompositionContainer(catalog);
            container.SatisfyImportsOnce(this);

            VoiceChat = GetHandlerFactory(ChatApps.VoiceChat) != null;
            FileTransfer = GetHandlerFactory(ChatApps.FileTransfer) != null;
        }

        public IAppHandler GetHandler(Guid appId, Func<IAppHandlerFactory, IAppHandler> getAction)
        {
            IAppHandlerFactory factory = GetHandlerFactory(appId);
            if (factory == null)
                return null;
            IAppHandler handler = getAction(factory);
            return handler;
        }

        IAppHandlerFactory GetHandlerFactory(Guid appId)
        {
            IAppHandlerFactory factory = AppHandlerFactories.FirstOrDefault(f => f.AppId.Equals(appId));
            return factory;
        }
    }
}
