using System;
using System.Runtime.CompilerServices;
using System.Threading;

namespace server.NttECS.Memory;

/// <summary>
/// Lock-free thread-safe object pool for value types with zero runtime allocations.
/// Pre-allocates all storage during construction for maximum performance.
/// </summary>
public sealed class Pool<T>
{
    private long _rentals, _returns;
    public ulong Rentals => (ulong)Interlocked.Read(ref _rentals);
    public ulong Returns => (ulong)Interlocked.Read(ref _returns);

    private readonly T[] _items;
    private int _index;
    private readonly Func<T> _factory;
    private readonly Action<T> _reset;

    public static Pool<T> Shared { get; } = new(() => default, null, 128);

    public Pool(Func<T> factory, Action<T> reset, int capacity)
    {
        if (capacity <= 0)
            throw new ArgumentException("Capacity must be greater than zero", nameof(capacity));

        _factory = factory ?? throw new ArgumentNullException(nameof(factory));
        _reset = reset;
        _items = new T[capacity];
        _index = 0;

        for (var i = 0; i < capacity; i++)
            _items[i] = factory();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T Get()
    {
        Interlocked.Increment(ref _rentals);

        var idx = Interlocked.Decrement(ref _index);
        if (idx >= 0 && idx < _items.Length)
            return _items[idx];

        Interlocked.Increment(ref _index);
        return _factory();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Return(T item)
    {
        Interlocked.Increment(ref _returns);

        _reset?.Invoke(item);

        var idx = Interlocked.Increment(ref _index) - 1;
        if (idx >= 0 && idx < _items.Length)
            _items[idx] = item;
    }
}
