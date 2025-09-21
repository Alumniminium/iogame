using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace server.ECS;

/// <summary>
/// High-performance entity query system providing type-safe component filtering with foreach enumeration support.
/// Offers compile-time optimized queries for entities with specific component combinations up to 6 component types.
/// </summary>
public static class NttQuery
{
    /// <summary>
    /// Enumerator for querying entities with exactly one component type.
    /// Provides efficient foreach iteration over matching entities.
    /// </summary>
    /// <typeparam name="T">Required component type for entity matching</typeparam>
    /// <param name="dict">Entity dictionary to enumerate</param>
    [method: MethodImpl(MethodImplOptions.AggressiveInlining)]
    public struct QueryEnumerator<T>(Dictionary<Guid, NTT> dict) where T : struct
    {
        private Dictionary<Guid, NTT>.Enumerator _enumerator = dict.GetEnumerator();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool MoveNext()
        {
            while (_enumerator.MoveNext())
            {
                if (_enumerator.Current.Value.Has<T>())
                    return true;
            }
            return false;
        }

        public readonly NTT Current => _enumerator.Current.Value;
        public readonly QueryEnumerator<T> GetEnumerator() => this;
    }

    [method: MethodImpl(MethodImplOptions.AggressiveInlining)]
    public struct QueryEnumerator<T1, T2>(Dictionary<Guid, NTT> dict) where T1 : struct where T2 : struct
    {
        private Dictionary<Guid, NTT>.Enumerator _enumerator = dict.GetEnumerator();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool MoveNext()
        {
            while (_enumerator.MoveNext())
            {
                if (_enumerator.Current.Value.Has<T1, T2>())
                    return true;
            }
            return false;
        }

        public readonly NTT Current => _enumerator.Current.Value;
        public readonly QueryEnumerator<T1, T2> GetEnumerator() => this;
    }

    [method: MethodImpl(MethodImplOptions.AggressiveInlining)]
    public struct QueryEnumerator<T1, T2, T3>(Dictionary<Guid, NTT> dict) where T1 : struct where T2 : struct where T3 : struct
    {
        private Dictionary<Guid, NTT>.Enumerator _enumerator = dict.GetEnumerator();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool MoveNext()
        {
            while (_enumerator.MoveNext())
            {
                if (_enumerator.Current.Value.Has<T1, T2, T3>())
                    return true;
            }
            return false;
        }

        public readonly NTT Current => _enumerator.Current.Value;
        public readonly QueryEnumerator<T1, T2, T3> GetEnumerator() => this;
    }

    [method: MethodImpl(MethodImplOptions.AggressiveInlining)]
    public struct QueryEnumerator<T1, T2, T3, T4>(Dictionary<Guid, NTT> dict) where T1 : struct where T2 : struct where T3 : struct where T4 : struct
    {
        private Dictionary<Guid, NTT>.Enumerator _enumerator = dict.GetEnumerator();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool MoveNext()
        {
            while (_enumerator.MoveNext())
            {
                if (_enumerator.Current.Value.Has<T1, T2, T3, T4>())
                    return true;
            }
            return false;
        }

        public readonly NTT Current => _enumerator.Current.Value;
        public readonly QueryEnumerator<T1, T2, T3, T4> GetEnumerator() => this;
    }

    [method: MethodImpl(MethodImplOptions.AggressiveInlining)]
    public struct QueryEnumerator<T1, T2, T3, T4, T5>(Dictionary<Guid, NTT> dict) where T1 : struct where T2 : struct where T3 : struct where T4 : struct where T5 : struct
    {
        private Dictionary<Guid, NTT>.Enumerator _enumerator = dict.GetEnumerator();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool MoveNext()
        {
            while (_enumerator.MoveNext())
            {
                if (_enumerator.Current.Value.Has<T1, T2, T3, T4, T5>())
                    return true;
            }
            return false;
        }

        public readonly NTT Current => _enumerator.Current.Value;
        public readonly QueryEnumerator<T1, T2, T3, T4, T5> GetEnumerator() => this;
    }

    [method: MethodImpl(MethodImplOptions.AggressiveInlining)]
    public struct QueryEnumerator<T1, T2, T3, T4, T5, T6>(Dictionary<Guid, NTT> dict) where T1 : struct where T2 : struct where T3 : struct where T4 : struct where T5 : struct where T6 : struct
    {
        private Dictionary<Guid, NTT>.Enumerator _enumerator = dict.GetEnumerator();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool MoveNext()
        {
            while (_enumerator.MoveNext())
            {
                if (_enumerator.Current.Value.Has<T1, T2, T3, T4, T5, T6>())
                    return true;
            }
            return false;
        }

        public readonly NTT Current => _enumerator.Current.Value;
        public readonly QueryEnumerator<T1, T2, T3, T4, T5, T6> GetEnumerator() => this;
    }

    /// <summary>
    /// Creates a query for entities with exactly one component type.
    /// </summary>
    /// <typeparam name="T">Required component type</typeparam>
    /// <returns>Enumerator for foreach iteration over matching entities</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static QueryEnumerator<T> Query<T>() where T : struct => new(NttWorld.NTTs);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static QueryEnumerator<T1, T2> Query<T1, T2>() where T1 : struct where T2 : struct => new(NttWorld.NTTs);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static QueryEnumerator<T1, T2, T3> Query<T1, T2, T3>() where T1 : struct where T2 : struct where T3 : struct => new(NttWorld.NTTs);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static QueryEnumerator<T1, T2, T3, T4> Query<T1, T2, T3, T4>() where T1 : struct where T2 : struct where T3 : struct where T4 : struct => new(NttWorld.NTTs);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static QueryEnumerator<T1, T2, T3, T4, T5> Query<T1, T2, T3, T4, T5>() where T1 : struct where T2 : struct where T3 : struct where T4 : struct where T5 : struct => new(NttWorld.NTTs);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static QueryEnumerator<T1, T2, T3, T4, T5, T6> Query<T1, T2, T3, T4, T5, T6>() where T1 : struct where T2 : struct where T3 : struct where T4 : struct where T5 : struct where T6 : struct => new(NttWorld.NTTs);
}