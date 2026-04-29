using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using AssemblyLoader.Model;

namespace Worker;

public class PluginLoader
{
    private readonly string _pluginFolder;

    public PluginLoader(string pluginFolder)
    {
        _pluginFolder = pluginFolder;
    }

    public IEnumerable<IProcess> LoadAll()
    {
        if (!Directory.Exists(_pluginFolder))
            throw new DirectoryNotFoundException($"Plugin folder not found: {_pluginFolder}");

        var processes = new List<IProcess>();

        foreach (var dllPath in Directory.GetFiles(_pluginFolder, "*.dll"))
        {
            try
            {
                var instances = LoadFromAssembly(dllPath);
                processes.AddRange(instances);
                Console.WriteLine($"✔ Loaded {instances.Count} plugin(s) from {Path.GetFileName(dllPath)}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✘ Failed to load {Path.GetFileName(dllPath)}: {ex.Message}");
            }
        }

        return processes;
    }

    private List<IProcess> LoadFromAssembly(string dllPath)
    {
        // Use a separate context per DLL to avoid conflicts
        var context = new AssemblyLoadContext(Path.GetFileName(dllPath), isCollectible: true);
        var assembly = context.LoadFromAssemblyPath(Path.GetFullPath(dllPath));

        return assembly.GetTypes()
            .Where(t => typeof(IProcess).IsAssignableFrom(t)
                        && t.IsClass
                        && !t.IsAbstract)
            .Select(t => (IProcess)Activator.CreateInstance(t)!)
            .ToList();
    }
}