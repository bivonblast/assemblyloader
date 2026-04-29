using System.Runtime.Loader;
using AssemblyLoader.Model;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Worker;

public static class PluginServiceCollectionExtensions
{
    public static IServiceCollection AddPlugins(
        this IServiceCollection services,
        string pluginFolder)
    {
        if (!Directory.Exists(pluginFolder))
            throw new DirectoryNotFoundException($"Plugin folder not found: {pluginFolder}");

        foreach (var dllPath in Directory.GetFiles(pluginFolder, "*.dll"))
        {
            try
            {
                var context = new AssemblyLoadContext(Path.GetFileName(dllPath), isCollectible: true);
                var assembly = context.LoadFromAssemblyPath(Path.GetFullPath(dllPath));

                var pluginTypes = assembly.GetTypes()
                    .Where(t => typeof(IProcess).IsAssignableFrom(t)
                                && t.IsClass
                                && !t.IsAbstract);

                foreach (var type in pluginTypes)
                {
                    services.RemoveAll(typeof(IProcess));
                    services.AddTransient(typeof(IProcess), type); // or Scoped / Singleton
                    Console.WriteLine($"✔ Registered plugin: {type.Name}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✘ Skipped {Path.GetFileName(dllPath)}: {ex.Message}");
            }
        }

        return services;
    }
}