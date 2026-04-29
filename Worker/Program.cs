using AssemblyLoader.InternalProcess;
using AssemblyLoader.Model;
using Worker;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<Worker.Worker>();
// builder.Services.AddSingleton<IProcess, InternalProcess>();

//builder.Services.AddPlugins("./Assemblies");

// PluginLoader loader = new("./Assemblies"); //new(Path.GetDirectoryName(Path.Combine(Assembly.GetExecutingAssembly().Location, "Assemblies")));
// var processes = loader.LoadAll();
// Console.WriteLine("Search processes");
// foreach (var externalProcess in processes)
// {
//     Console.WriteLine("Process added");
//     builder.Services.AddSingleton<IProcess>(externalProcess);
// }
// Console.WriteLine("Search processes finished");

// builder.Services.AddSingleton<FirstPluginFactory>();

builder.Services.AddSingleton<PluginFactory>(sp => new PluginFactory("./Assemblies", sp));

var host = builder.Build();
host.Run();