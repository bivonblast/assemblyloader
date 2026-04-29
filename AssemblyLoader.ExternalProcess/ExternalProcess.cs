using AssemblyLoader.Model;
using Microsoft.Extensions.Logging;

namespace AssemblyLoader.ExternalProcess;

public class ExternalProcess(ILogger<ExternalProcess> logger) : IProcess
{
    public bool RunProcess(Data processData)
    {
        logger.LogWarning("ExternalProcess seems to be ready!");
        return false;
    }
}