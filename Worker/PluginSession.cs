using System.Collections;
using System.Runtime.Loader;
using AssemblyLoader.Model;

namespace Worker;

public class PluginSession : IEnumerable<IProcess>, IDisposable
{
    private List<IProcess>? _entries;

    public PluginSession(List<IProcess> entries)
    {
        _entries = entries;
    }

    public IEnumerator<IProcess> GetEnumerator()
    {
        if (_entries is null) throw new ObjectDisposedException(nameof(PluginSession));
        return _entries.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
    
    public void Dispose()
    {
        if (_entries is null) return;

        // 2. Null out entries to drop all strong references
        _entries.Clear();
        _entries = null;

        // 4. Force GC to release file handles
        for (int i = 0; i < 3; i++)
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }
    }
}