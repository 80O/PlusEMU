using Plus.Plugins;

namespace PluginExample;

public class PluginExample : IPlugin
{
    public void Start()
    {
        Console.WriteLine("This plugin has been started :)");
    }
}