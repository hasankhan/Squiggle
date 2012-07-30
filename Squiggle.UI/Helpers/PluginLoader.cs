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
        [ImportMany(typeof(IActivityHandlerFactory))]
        public IEnumerable<IActivityHandlerFactory> AppHandlerFactories { get; set; }

        public bool VoiceChat { get; private set; }
        public bool FileTransfer { get; private set; }

        public PluginLoader(ComposablePartCatalog catalog)
        {
            var container = new CompositionContainer(catalog);
            container.SatisfyImportsOnce(this);

            VoiceChat = GetHandlerFactory(SquiggleActivities.VoiceChat) != null;
            FileTransfer = GetHandlerFactory(SquiggleActivities.FileTransfer) != null;
        }

        public IAppHandler GetHandler(Guid appId, Func<IActivityHandlerFactory, IAppHandler> getAction)
        {
            IActivityHandlerFactory factory = GetHandlerFactory(appId);
            if (factory == null)
                return null;
            IAppHandler handler = getAction(factory);
            return handler;
        }

        IActivityHandlerFactory GetHandlerFactory(Guid appId)
        {
            IActivityHandlerFactory factory = AppHandlerFactories.FirstOrDefault(f => f.AppId.Equals(appId));
            return factory;
        }
    }
}
