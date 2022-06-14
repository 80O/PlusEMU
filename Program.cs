using Microsoft.Extensions.DependencyInjection;
using NLog;
using Plus.Core;
using Plus.Plugins;
using Plus.Utilities.DependencyInjection;
using Scrutor;
using System.Reflection;

namespace Plus;

public static class Program
{
    private static IEnumerable<Type> _transientTypes;

    public static async Task Main(string[] args)
    {
        Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;

        var services = new ServiceCollection();
        _transientTypes = typeof(Program).Assembly.GetTypes().Where(t => t.IsInterface && t.GetCustomAttributes<TransientAttribute>().Any());

        // Plugins
        Directory.CreateDirectory("plugins");
        var pluginAssemblies = new DirectoryInfo("plugins").GetDirectories().Select(d => PluginLoadContext.LoadPlugin(Path.Join("plugins", d.Name), d.Name)).ToList();
        pluginAssemblies.AddRange(new DirectoryInfo("plugins").GetFiles().Where(f => Path.GetExtension(f.Name).Equals(".dll")).Select(f => PluginLoadContext.LoadPlugin(Path.Join("plugins"), Path.GetFileNameWithoutExtension(f.Name))));
        var pluginDefinitions = pluginAssemblies.SelectMany(pluginAssembly => AddPlugin(services, pluginAssembly)).ToList();

        // Dependency Injection
        services.AddDefaultRules(typeof(Program).Assembly);

        foreach (var plugin in pluginDefinitions)
            plugin.OnServicesConfigured();


        // Configuration
        var projectSolutionPath = Directory.GetCurrentDirectory();
        var configuration = new ConfigurationData(projectSolutionPath + "//Config//config.ini");
        services.AddSingleton(configuration);
        LogManager.LoadConfiguration("Config/nlog.config");

        var serviceProvider = services.BuildServiceProvider();
        foreach (var plugin in pluginDefinitions)
            plugin.OnServiceProviderBuild(serviceProvider);

        Console.ForegroundColor = ConsoleColor.White;
        Console.CursorVisible = false;
        AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;

        // Start
        var environment = serviceProvider.GetRequiredService<IPlusEnvironment>();
        var started = await environment.Start();
        if (!started)
        {
            Environment.Exit(1);
            return;
        }
        while (true)
        {
            if (Console.ReadKey(true).Key == ConsoleKey.Enter)
            {
                Console.Write("plus> ");
                var input = Console.ReadLine();
                if (input.Length > 0)
                {
                    var s = input.Split(' ')[0];
                    ConsoleCommands.InvokeCommand(s);
                }
            }
        }
    }

    public static IServiceCollection AddAssignableTo<T>(this IServiceCollection services, Assembly assembly) => services.AddAssignableTo(new[] { assembly }, typeof(T));

    public static IServiceCollection AddAssignableTo<T>(this IServiceCollection services, IEnumerable<Assembly> assemblies) => services.AddAssignableTo(assemblies, typeof(T));

    public static IServiceCollection AddAssignableTo(this IServiceCollection services, Assembly assembly, Type type) => services.AddAssignableTo(new[] { assembly }, type);

    public static IServiceCollection AddAssignableTo(this IServiceCollection services, IEnumerable<Assembly> assemblies, Type type) =>
        services.Scan(scan => scan.FromAssemblies(assemblies)
            .AddClasses(classes => classes.Where(t => t.IsAssignableTo(type) && !t.IsAbstract && !t.IsInterface))
            .UsingRegistrationStrategy(RegistrationStrategy.Append)
            .AsSelfWithInterfaces()
            .WithSingletonLifetime());

    private static IServiceCollection AddDefaultRules(this IServiceCollection services, Assembly assembly)
    {
        foreach (var type in _transientTypes)
            services.AddAssignableTo(assembly, type);

        services.Scan(scan => scan.FromAssemblies(assembly)
            .AddClasses(classes => classes.Where(c => c.GetInterface($"I{c.Name}") != null))
            .UsingRegistrationStrategy(RegistrationStrategy.Skip)
            .AsSelfWithInterfaces()
            .WithSingletonLifetime());
        return services;
    }

    private static IEnumerable<IPluginDefinition> AddPlugin(IServiceCollection services, Assembly pluginAssembly)
    {
        var pluginDefinitions = new List<IPluginDefinition>();
        try
        {
            services.AddDefaultRules(pluginAssembly);

            foreach (var pluginDefinition in pluginAssembly.DefinedTypes.Where(t =>
                         t.ImplementedInterfaces.Contains(typeof(IPluginDefinition))))
            {
                var plugin = (IPluginDefinition?)Activator.CreateInstance(pluginDefinition);
                if (plugin != null)
                {
                    plugin.ConfigureServices(services);
                    services.AddSingleton(plugin);
                    pluginDefinitions.Add(plugin);
                }
            }
        }
        catch (Exception e)
        {
            Console.WriteLine($"Failed to load plugin assembly { pluginAssembly.FullName}. Possibly outdated. {e.Message}");
        }
        return pluginDefinitions;
    }

    private static void OnUnhandledException(object sender, UnhandledExceptionEventArgs args)
    {
        var e = (Exception)args.ExceptionObject;
        //Logger.LogCriticalException("SYSTEM CRITICAL EXCEPTION: " + e);
        PlusEnvironment.PerformShutDown();
    }
}