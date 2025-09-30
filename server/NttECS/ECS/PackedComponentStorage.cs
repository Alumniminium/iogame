using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;

namespace server.ECS;

/// <summary>
/// High-performance packed storage system for ECS components providing excellent cache locality.
/// Uses dense arrays for component storage with entity-to-index mapping for O(1) access.
/// Components are stored contiguously in memory enabling efficient system iteration and vectorization.
/// </summary>
/// <typeparam name="T">Component type to store (must be value type)</typeparam>
public static class PackedComponentStorage<T> where T : struct
{
    /// <summary>Dense array storing components contiguously for optimal cache performance</summary>
    private static T[] _components = new T[1024];

    /// <summary>Maps entity GUIDs to component array indices for O(1) lookups</summary>
    private static readonly Dictionary<Guid, int> _entityToIndex = [];

    /// <summary>Maps component array indices back to entity GUIDs for iteration</summary>
    private static Guid[] _indexToEntity = new Guid[1024];

    /// <summary>Current number of components stored</summary>
    private static int _count = 0;

    /// <summary>Reader-writer lock for thread-safe concurrent access</summary>
    private static readonly ReaderWriterLockSlim _lock = new();

    /// <summary>Default component instance returned when component doesn't exist</summary>
    private static readonly T[] _default = new T[1];

    /// <summary>
    /// Adds or updates a component for the specified entity with optimal cache placement.
    /// Components are packed together for excellent system iteration performance.
    /// </summary>
    /// <param name="ntt">Entity to add component to</param>
    /// <param name="component">Component data to store</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void AddFor(in NTT ntt, ref T component)
    {
        if (ntt.Id == Guid.Empty)
            return;

        _lock.EnterWriteLock();
        try
        {
            if (_entityToIndex.TryGetValue(ntt, out var existingIndex))
            {
                // Update existing component
                _components[existingIndex] = component;
            }
            else
            {
                // Add new component
                EnsureCapacity(_count + 1);

                var index = _count;
                _components[index] = component;
                _entityToIndex[ntt.Id] = index;
                _indexToEntity[index] = ntt.Id;
                _count++;

                NttWorld.InformChangesFor(ntt);
            }
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }

    /// <summary>
    /// Adds a default-initialized component for the specified entity.
    /// </summary>
    /// <param name="ntt">Entity to add default component to</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void AddFor(in NTT ntt)
    {
        var defaultComponent = default(T);
        AddFor(ntt, ref defaultComponent);
    }

    /// <summary>
    /// Checks if the specified entity has this component type.
    /// </summary>
    /// <param name="ntt">Entity to check for component</param>
    /// <returns>True if entity has the component</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool HasFor(in NTT ntt)
    {
        if (ntt.Id == Guid.Empty)
            return false;

        _lock.EnterReadLock();
        try
        {
            return _entityToIndex.ContainsKey(ntt.Id);
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    /// <summary>
    /// Gets a mutable reference to the component for the specified entity.
    /// Returns a reference to default component if entity doesn't have this component type.
    /// </summary>
    /// <param name="ntt">Entity to get component for</param>
    /// <returns>Mutable reference to component data</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ref T Get(NTT ntt)
    {
        if (ntt.Id == Guid.Empty)
            return ref _default[0];

        _lock.EnterReadLock();
        try
        {
            if (!_entityToIndex.TryGetValue(ntt, out var index))
                return ref _default[0];

            return ref _components[index];
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    /// <summary>
    /// Removes the component from the specified entity with optimal array compaction.
    /// Maintains component array density by moving the last component to fill the gap.
    /// </summary>
    /// <param name="ntt">Entity to remove component from</param>
    /// <param name="notify">Whether to notify world of entity changes</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Remove(NTT ntt, bool notify)
    {
        if (ntt.Id == Guid.Empty)
            return;

        _lock.EnterWriteLock();
        try
        {
            if (!_entityToIndex.TryGetValue(ntt, out var indexToRemove))
                return;

            var lastIndex = _count - 1;

            if (indexToRemove != lastIndex)
            {
                // Move last component to fill the gap (maintain density)
                var lastEntityId = _indexToEntity[lastIndex];
                _components[indexToRemove] = _components[lastIndex];
                _entityToIndex[lastEntityId] = indexToRemove;
                _indexToEntity[indexToRemove] = lastEntityId;
            }

            // Clear the last slot
            _components[lastIndex] = default;
            _indexToEntity[lastIndex] = Guid.Empty;
            _entityToIndex.Remove(ntt.Id);
            _count--;

            if (notify)
                NttWorld.InformChangesFor(ntt);
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }

    /// <summary>
    /// Gets a read-only span of all components for efficient system iteration.
    /// Provides excellent cache locality for processing all components of this type.
    /// </summary>
    /// <returns>Read-only span covering all active components</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ReadOnlySpan<T> GetComponentSpan()
    {
        _lock.EnterReadLock();
        try
        {
            return _components.AsSpan(0, _count);
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    /// <summary>
    /// Gets a span of all entity GUIDs that have this component type.
    /// Parallel to GetComponentSpan() for entity-component iteration.
    /// </summary>
    /// <returns>Read-only span of entity GUIDs with this component</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ReadOnlySpan<Guid> GetEntitySpan()
    {
        _lock.EnterReadLock();
        try
        {
            return _indexToEntity.AsSpan(0, _count);
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    /// <summary>
    /// Ensures the component arrays have sufficient capacity for the specified count.
    /// Grows arrays exponentially to minimize allocations.
    /// </summary>
    /// <param name="requiredCapacity">Minimum required capacity</param>
    private static void EnsureCapacity(int requiredCapacity)
    {
        if (requiredCapacity <= _components.Length)
            return;

        var newCapacity = Math.Max(requiredCapacity, _components.Length * 2);
        Array.Resize(ref _components, newCapacity);
        Array.Resize(ref _indexToEntity, newCapacity);
    }
}