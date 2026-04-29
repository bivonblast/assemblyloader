using System.Runtime.Loader;
using AssemblyLoader.Model;

namespace Worker;

public class PluginFactory
{
    private readonly string _pluginFolder;
    private readonly IServiceProvider _provider;

    public PluginFactory(string pluginFolder, IServiceProvider provider)
    {
        _pluginFolder = pluginFolder;
        _provider = provider;
    }

    public PluginSession LoadCurrent()
    {
        var entries = new List<IProcess>();
        var tempDir = Path.Combine(Path.GetTempPath(), "plugins_shadow");
        Directory.CreateDirectory(tempDir);

        foreach (var dllPath in Directory.GetFiles(_pluginFolder, "*.dll"))
        {
            try
            {
                WeakReference? weakRef = null;
                {
                    // Shadow copy — load from temp, original file stays unlocked
                    var shadowPath = Path.Combine(tempDir, Path.GetFileName(dllPath));
                    File.Copy(dllPath, shadowPath, overwrite: true);

                    var context = new AssemblyLoadContext(Path.GetFileName(dllPath), isCollectible: true);
                    weakRef = new WeakReference(context);
                    var assembly = context.LoadFromAssemblyPath(Path.GetFullPath(shadowPath));

                    var types = assembly.GetTypes()
                        .Where(t => typeof(IProcess).IsAssignableFrom(t)
                                    && t.IsClass
                                    && !t.IsAbstract);

                    foreach (var type in types)
                    {
                        var instance = (IProcess) ActivatorUtilities.CreateInstance(_provider, type);
                        entries.Add(instance);
                    }
                }

                ((AssemblyLoadContext)weakRef.Target).Unload();
                for (int i = 0; weakRef.IsAlive && (i < 10); i++)
                {
                    Console.WriteLine("Error error:  " + i);
                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✘ Skipped {Path.GetFileName(dllPath)}: {ex.Message}");
            }
        }

        return new PluginSession(entries);
    }
}