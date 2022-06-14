using Plus.Core;

namespace Plus.Plugins
{
    public class PluginsCache : IPluginsCache, IStartable
    {
        public IReadOnlyDictionary<IPluginDefinition, IPlugin> Plugins => _plugins;
        private readonly Dictionary<IPluginDefinition, IPlugin> _plugins;
        public PluginsCache(IEnumerable<IPluginDefinition> pluginDefinitions, IEnumerable<IPlugin> plugins)
        {
            _plugins = pluginDefinitions.ToDictionary(kvp => kvp, kvp => plugins.First(p => p.GetType() == kvp.PluginClass));
        }

        public Task Start()
        {
            foreach (var (_, plugin) in _plugins)
                Task.Run(plugin.Start);
            return Task.CompletedTask;
        }
    }
}