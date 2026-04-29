using System.Collections;
using System.Runtime.Loader;
using AssemblyLoader.Model;

namespace Worker;

public class UnloadableProcessCollection : IEnumerable<IProcess>, IDisposable
{
    private List<(AssemblyLoadContext ctx, IProcess instance)> _entries;

    public UnloadableProcessCollection(List<(AssemblyLoadContext ctx, IProcess instance)> entries)
    {
        _entries = entries;
    }

    public IEnumerator<IProcess> GetEnumerator()
        => _entries.Select(e => e.instance).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public void Dispose()
    {
        List<WeakReference> weakRefs = [];

        {
            // 1. Extract contexts before clearing
            var contexts = _entries.Select(e => e.ctx).ToList();

            // 2. Drop all strong references first
            _entries.Clear();
            _entries = null!; // ← explicitly null the hidden primary constructor field

            // 3. Unload and track with weak refs
            weakRefs = contexts.Select(ctx =>
            {
                var weakRef = new WeakReference(ctx); // ← track the context, not the list
                ctx.Unload();
                return weakRef;
            }).ToList();

            // 4. Force GC
            ForceCollect();

        }
        // 5. Verify
        var alive = weakRefs.Count(r => r.IsAlive);
        Console.WriteLine(alive == 0
            ? "✔ All contexts fully unloaded"
            : $"⚠ {alive} context(s) still alive — check for lingering references");
    }
    
    private static void ForceCollect()
    {
        for (int i = 0; i < 3; i++)
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }
    }
}