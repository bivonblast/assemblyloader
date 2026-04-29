using AssemblyLoader.Model;
using Microsoft.Extensions.Logging;

namespace AssemblyLoader.InternalProcess;

public class InternalProcess(ILogger<InternalProcess> logger) : IProcess
{
    public bool RunProcess(Data processData)
    {
        logger.LogInformation("InternalProcess success!");
        return true;
    }
}