// See https://aka.ms/new-console-template for more information

// 1. Setup DI Container

using System.Diagnostics;
using System.Runtime.Loader;
using AssemblyLoader.Model;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var host = Host.CreateApplicationBuilder(args);

// 2. Register Services
host.Services.AddSingleton<IMyService, MyService>();
host.Services.AddTransient<AppLauncher>(); // Your entry-point class

// 3. Build the Provider
var app = host.Build();
//var serviceProvider = services.BuildServiceProvider();

// 4. Resolve the entry-point service and run
app.Services.GetRequiredService<AppLauncher>().Run();

public interface IMyService { void DoWork(); }

public class MyService(IServiceProvider provider) : IMyService 
{
    public void DoWork()
    {
        Console.WriteLine("Service is working!");
        
        var entries = new List<IProcess>();
        var tempDir = Path.Combine(Path.GetTempPath(), "plugins_shadow");
        Directory.CreateDirectory(tempDir);

        WeakReference? weakRef = null;
        
        var dllPath = "./Assemblies/AssemblyLoader.ExternalProcess.dll";
        
        {
            var context = new AssemblyLoadContext(Path.GetFileName(dllPath), isCollectible: true);
            weakRef = new WeakReference(context);
            var assembly = context.LoadFromAssemblyPath(Path.GetFullPath(dllPath));

            var types = assembly.GetTypes()
                .Where(t => typeof(IProcess).IsAssignableFrom(t)
                            && t.IsClass
                            && !t.IsAbstract);

            foreach (var type in types)
            {
                var instance = (IProcess) ActivatorUtilities.CreateInstance(provider, type);
                entries.Add(instance);
            }
        }
        ((AssemblyLoadContext) weakRef.Target).Unload();
        for (int i = 0; weakRef.IsAlive && (i < 10); i++)
        {
            Console.WriteLine("Error error:  " + i);
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }

        
        Debugger.Break();
    }
}

public class AppLauncher 
{
    private readonly IMyService _service;
    // Constructor Injection
    public AppLauncher(IMyService service) => _service = service;

    public void Run() => _service.DoWork();
}
