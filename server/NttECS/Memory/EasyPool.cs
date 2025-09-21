using System;
using System.Collections.Concurrent;
using System.Threading;

namespace server.Memory;

/// <summary>
/// Thread-safe easy-to-use object pool with shared instance for common scenarios.
/// </summary>
public sealed class EasyPool<T> where T : new()
{
    private long _rentals, _returns;

    public ulong Rentals => (ulong)Interlocked.Read(ref _rentals);
    public ulong Returns => (ulong)Interlocked.Read(ref _returns);
    public static EasyPool<T> Shared { get; } = new(() => new T(), null!, 50); // No reset action needed for new()
    public int Count => _queue.Count;

    private readonly ConcurrentQueue<T> _queue;
    private readonly Func<T> _onCreate;
    private readonly Action<T> _onReturn;

    public EasyPool(Func<T> createInstruction, Action<T> returnAction, int amount)
    {
        _onCreate = createInstruction;
        _onReturn = returnAction;
        _queue = new ConcurrentQueue<T>();

        for (var i = 0; i < amount; i++)
            _queue.Enqueue(createInstruction());
    }

    public T Get()
    {
        if (!_queue.TryDequeue(out var found))
            found = _onCreate();

        Interlocked.Increment(ref _rentals); // Only increment after successful operation
        return found;
    }

    public void Return(T obj)
    {
        if (obj is null) return; // Guard against null returns

        _onReturn?.Invoke(obj);

        _queue.Enqueue(obj);
        Interlocked.Increment(ref _returns); // Only increment after successful enqueue
    }
}