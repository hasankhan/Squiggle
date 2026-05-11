using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using Serilog;
using Squiggle.Plugins;
using Squiggle.Plugins.MessageFilter;
using Squiggle.Plugins.MessageParser;

namespace Squiggle.UI.Avalonia.Helpers;

/// <summary>
/// Loads plugins from a folder using AssemblyLoadContext, replacing the WPF MEF-based approach.
/// </summary>
public class PluginLoader
{
    public IReadOnlyList<IExtension> Extensions { get; }
    public IReadOnlyList<IMessageFilter> MessageFilters { get; }
    public IReadOnlyList<IMessageParser> MessageParsers { get; }

    [RequiresUnreferencedCode("Plugin loading uses reflection to scan and instantiate types.")]
    public PluginLoader(string pluginsPath)
    {
        var extensions = new List<IExtension>();
        var messageFilters = new List<IMessageFilter>();
        var messageParsers = new List<IMessageParser>();

        if (Directory.Exists(pluginsPath))
        {
            foreach (var dll in Directory.GetFiles(pluginsPath, "*.dll"))
            {
                try
                {
                    var assembly = AssemblyLoadContext.Default.LoadFromAssemblyPath(
                        Path.GetFullPath(dll));

                    foreach (var type in assembly.GetTypes())
                    {
                        if (type.IsAbstract || type.IsInterface)
                            continue;

                        if (type.GetConstructor(Type.EmptyTypes) is null)
                            continue;

                        var instance = Activator.CreateInstance(type);

                        if (instance is IExtension ext)
                            extensions.Add(ext);
                        if (instance is IMessageFilter filter)
                            messageFilters.Add(filter);
                        if (instance is IMessageParser parser)
                            messageParsers.Add(parser);
                    }
                }
                catch (Exception ex)
                {
                    Log.Warning(ex, "Failed to load plugin assembly: {Path}", dll);
                }
            }
        }

        Extensions = extensions;
        MessageFilters = messageFilters;
        MessageParsers = messageParsers;

        Log.Information(
            "Loaded {ExtCount} extensions, {FilterCount} message filters, {ParserCount} message parsers from {Path}",
            Extensions.Count, MessageFilters.Count, MessageParsers.Count, pluginsPath);
    }

    public void StartExtensions(ISquiggleContext context)
    {
        foreach (var ext in Extensions)
        {
            try
            {
                ext.Start(context);
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "Failed to start extension: {Type}", ext.GetType().FullName);
            }
        }
    }
}
