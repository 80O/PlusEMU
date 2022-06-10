using Plus.Plugins;

namespace PluginExample
{
    public class PluginExampleDefinition : IPluginDefinition
    {
        public string Name => "Example Plugin";
        public string Author => "The General";
        public Version Version => new(1, 0, 0);
        public Type PluginClass => typeof(PluginExample);
    }
}