using AssemblyLoader.Model;

namespace Worker;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;

    private readonly IEnumerable<IProcess> _internalProcesses;

    // private readonly IEnumerable<IProcess> _processes;
    private readonly PluginFactory _pluginFactory;

    public Worker(ILogger<Worker> logger, IEnumerable<IProcess> internalProcesses, PluginFactory pluginFactory) // 
    {
        _logger = logger;
        _internalProcesses = internalProcesses;
        // _processes = processes;
        _pluginFactory = pluginFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        int count = 0;
        while (!stoppingToken.IsCancellationRequested)
        {
            if (count > 0)
                await Task.Delay(10000, stoppingToken);

            count++;

            foreach (var process in _internalProcesses)
                process.RunProcess(new Data("Martin", "Florin"));

            RunPlugins(); // ← plugin references are scoped to this method's stack frame

            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            // Now safe to clean shadow copies
            var tempDir = Path.Combine(Path.GetTempPath(), "plugins_shadow");
            if (Directory.Exists(tempDir))
                Directory.Delete(tempDir, recursive: true);
            
            _logger.LogInformation("Run: {Nr}", count);
        }
    }
    
    
    private void RunPlugins()
    {
        using (var session = _pluginFactory.LoadCurrent())
        {
            foreach (var process in session)
                process.RunProcess(new Data("Martin", "Florin"));
        } // ← Dispose called here, contexts unloaded
    } // ← stack frame cleared here, all references dropped
    
    // protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    // {
    //     int count = 0;
    //     while (!stoppingToken.IsCancellationRequested)
    //     {            
    //         if(count > 0)
    //             await Task.Delay(10000, stoppingToken);
    //
    //         count++;
    //
    //         // foreach (var process in _processes)
    //         // {
    //         //     process.RunProcess(new Data("Martin", "Florin"));
    //         // }
    //         
    //         using (var externalProcesses = _pluginFactory.LoadCurrent())
    //         {
    //             foreach (var process in externalProcesses)
    //             {
    //                 process.RunProcess(new Data("Martin", "Florin"));
    //             }
    //
    //             _logger.LogInformation("Time: {Nr}", count);
    //         }
    //     }
    // }
}