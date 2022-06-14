using System.Reflection;
using System.Runtime.Loader;

namespace Plus.Plugins
{
    public class PluginLoadContext : AssemblyLoadContext
    {
        private static readonly List<PluginLoadContext> LoadContexts = new();
        public static Assembly LoadPlugin(string pluginsPath, string pluginName)
        {
            var loadContext = new PluginLoadContext(pluginsPath);
            LoadContexts.Add(loadContext);
            return loadContext.LoadFromAssemblyPath(Path.GetFullPath(Path.Join(pluginsPath, $"{pluginName}.dll")));
        }

        private readonly string _pluginPath;
        private readonly AssemblyDependencyResolver _resolver;
        private readonly List<Assembly> _assemblies;

        public PluginLoadContext(string pluginPath)
        {
            _pluginPath = pluginPath;
            _resolver = new AssemblyDependencyResolver(pluginPath);
            _assemblies = Directory.GetFiles(pluginPath).Where(f => f.EndsWith(".dll")).Select(f =>
            {
                try
                {
                    return Assembly.LoadFrom(f);
                }
                catch (Exception)
                {
                    // Ignored
                    return null;
                }
            }).Where(f => f != null).Select(s => s!).ToList();
            Resolving += OnResolving;
        }

        private Assembly? OnResolving(AssemblyLoadContext arg1, AssemblyName arg2)
        {
            //return Assembly.Load(arg2);
            return Assembly.LoadFrom($"{_pluginPath}/{arg2.Name}.dll");
        }

        protected override Assembly? Load(AssemblyName assemblyName)
        {
            var existingAssembly = Default.Assemblies.FirstOrDefault(a => a.GetName().Equals(assemblyName)) ?? Default.Assemblies.FirstOrDefault(a => a.GetName(true).Name!.Equals(assemblyName.Name));
            if (existingAssembly != null) return existingAssembly;
            var assemblyPath = _resolver.ResolveAssemblyToPath(assemblyName);
            return assemblyPath != null ? LoadFromAssemblyPath(assemblyPath) : null;
        }

        protected override IntPtr LoadUnmanagedDll(string unmanagedDllName)
        {
            var libraryPath = _resolver.ResolveUnmanagedDllToPath(unmanagedDllName);
            return libraryPath != null ? LoadUnmanagedDllFromPath(libraryPath) : IntPtr.Zero;
        }
    }
}
