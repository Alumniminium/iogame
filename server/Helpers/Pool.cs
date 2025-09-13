using System;
using System.Collections.Concurrent;

namespace server.Helpers;

public sealed class Pool<T> where T : new()
{
    public ulong Rentals, Returns;
    public static Pool<T> Shared { get; } = new(() => new T(), (t) => t = new T(), 50);
    public int Count => _queue.Count;
    private readonly Queue<T> _queue;

    private readonly Func<T> _onCreate;
    private readonly Action<T> _onReturn;
    public Pool(Func<T> createInstruction, Action<T> returnAction, int amount)
    {
        _onCreate = createInstruction;
        _onReturn = returnAction;
        _queue = new Queue<T>();

        for (var i = 0; i < amount; i++)
            _queue.Enqueue(createInstruction());
    }

    public T Get()
    {
        Rentals++;
        if (!_queue.TryDequeue(out T found))
            found = _onCreate();

        return found;
    }

    public void Return(T obj)
    {
        Returns++;
        _onReturn?.Invoke(obj);
        _queue.Enqueue(obj);
    }
}