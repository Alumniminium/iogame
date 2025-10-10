using System;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;
using System.Threading;
using server.Simulation.Components;

namespace server.ECS;

/// <summary>
/// Core entity identifier and interface for the Entity Component System.
/// Provides a lightweight handle for accessing, modifying, and synchronizing entity components across the game world.
/// NTT stands for "Named Typed Thing" - the fundamental unit of the ECS architecture.
/// </summary>
/// <param name="id">Unique identifier for this entity</param>
[method: JsonConstructor]
public readonly struct NTT(Guid id)
{
    /// <summary>Unique identifier for this entity across the entire game world</summary>
    public readonly Guid Id = id;

    /// <summary>
    /// Sets a component on this entity by reference for optimal performance.
    /// </summary>
    /// <typeparam name="T">Component type to set</typeparam>
    /// <param name="t">Component data to set</param>
    public readonly void Set<T>(ref T t) where T : struct => PackedComponentStorage<T>.AddFor(in this, ref t);

    /// <summary>
    /// Sets a component on this entity by value.
    /// </summary>
    /// <typeparam name="T">Component type to set</typeparam>
    /// <param name="component">Component data to set</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly void Set<T>(T component) where T : struct => PackedComponentStorage<T>.AddFor(in this, ref component);

    /// <summary>
    /// Sets a default-initialized component on this entity (marker component).
    /// </summary>
    /// <typeparam name="T">Component type to set with default values</typeparam>
    public readonly void Set<T>() where T : struct => PackedComponentStorage<T>.AddFor(in this);
    /// <summary>
    /// Gets a mutable reference to a component on this entity for direct modification.
    /// </summary>
    /// <typeparam name="T">Component type to retrieve</typeparam>
    /// <returns>Mutable reference to the component data</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly ref T Get<T>() where T : struct => ref PackedComponentStorage<T>.Get(this);
    /// <summary>
    /// Checks if this entity has a specific component type.
    /// </summary>
    /// <typeparam name="T">Component type to check for</typeparam>
    /// <returns>True if entity has the component</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly bool Has<T>() where T : struct => PackedComponentStorage<T>.HasFor(in this);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly bool Has<T, T2>() where T : struct where T2 : struct => Has<T>() && Has<T2>();
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly bool Has<T, T2, T3>() where T : struct where T2 : struct where T3 : struct => Has<T, T2>() && Has<T3>();
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly bool Has<T, T2, T3, T4>() where T : struct where T2 : struct where T3 : struct where T4 : struct => Has<T, T2, T3>() && Has<T4>();
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly bool Has<T, T2, T3, T4, T5>() where T : struct where T2 : struct where T3 : struct where T4 : struct where T5 : struct => Has<T, T2, T3, T4>() && Has<T5>();
    public readonly bool Has<T, T2, T3, T4, T5, T6>() where T : struct where T2 : struct where T3 : struct where T4 : struct where T5 : struct where T6 : struct => Has<T, T2, T3, T4>() && Has<T5, T6>();

    /// <summary>
    /// Removes a component from this entity.
    /// </summary>
    /// <typeparam name="T">Component type to remove</typeparam>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly void Remove<T>() where T : struct => ReflectionHelper.Remove<T>(this);
    /// <summary>
    /// Recycles all components from this entity back to object pools for memory efficiency.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly void Recycle() => ReflectionHelper.RecycleComponents(this);


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override int GetHashCode() => Id.GetHashCode();
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override bool Equals(object obj) => obj is NTT nttId && nttId.Id == Id;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator ==(in NTT a, in NTT b) => a.Id == b.Id;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator !=(in NTT a, in NTT b) => a.Id != b.Id;
    public static implicit operator Guid(in NTT a) => a.Id;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override string ToString() => $"NTT {Id}";

    internal void NetSync(Memory<byte> buffer) => Get<NetworkComponent>().Socket.SendAsync(buffer, System.Net.WebSockets.WebSocketMessageType.Binary, true, CancellationToken.None);
}