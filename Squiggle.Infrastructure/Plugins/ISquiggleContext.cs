using Squiggle.Client;
using Squiggle.Plugins;
using System;
namespace Squiggle.Plugins
{
    public interface ISquiggleContext
    {
        IChatClient ChatClient { get; set; }
        IMainWindow MainWindow { get; set; }
    }
}
