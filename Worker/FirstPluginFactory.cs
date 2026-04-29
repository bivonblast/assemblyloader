using System.Runtime.Loader;
using AssemblyLoader.Model;

namespace Worker;

public class FirstPluginFactory
{
    private readonly string _pluginFolder;
    private readonly IServiceProvider _provider; // ← inject the container

    public FirstPluginFactory(IConfiguration config, IServiceProvider provider)
    {
        _pluginFolder = config["PluginFolder"] ?? "./Assemblies";
        _provider = provider;
    }

    public UnloadableProcessCollection LoadCurrent()
    {
        var entries = new List<(AssemblyLoadContext ctx, IProcess instance)>();

        foreach (var dllPath in Directory.GetFiles(_pluginFolder, "*.dll"))
        {
            var context = new AssemblyLoadContext(Path.GetFileName(dllPath), isCollectible: true);
            var assembly = context.LoadFromAssemblyPath(Path.GetFullPath(dllPath));

            var types = assembly.GetTypes()
                .Where(t => typeof(IProcess).IsAssignableFrom(t)
                            && t.IsClass
                            && !t.IsAbstract);

            foreach (var type in types)
            {
                // Resolves constructor dependencies from the DI container
                var instance = (IProcess)ActivatorUtilities.CreateInstance(_provider, type);
                entries.Add((context, instance));
            }
        }

        return new UnloadableProcessCollection(entries);
    }
}