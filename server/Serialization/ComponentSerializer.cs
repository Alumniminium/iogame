using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using server.ECS;
using server.Enums;
using server.Helpers;
using server.Simulation.Components;
using server.Simulation.Net;

namespace server.Serialization;

public static class ComponentSerializer
{
    private static readonly Dictionary<Type, byte> _componentIds = [];
    private static readonly Dictionary<Type, bool> _netSyncTypes = [];

    public static IEnumerable<Type> NetworkSyncTypes => _netSyncTypes.Where(kvp => kvp.Value).Select(kvp => kvp.Key);

    public static bool SupportsNetworkSync<T>() where T : struct => _netSyncTypes.TryGetValue(typeof(T), out var supportsSync) && supportsSync;

    static ComponentSerializer()
    {
        // Cache component IDs at startup
        foreach (var type in Assembly.GetExecutingAssembly().GetTypes())
        {
            var attr = type.GetCustomAttribute<ComponentAttribute>();
            if (attr != null && attr.ComponentType != ComponentType.None)
            {
                _componentIds[type] = (byte)attr.ComponentType;
                _netSyncTypes[type] = attr.NetworkSync;
            }
        }
    }

    public static Memory<byte> Serialize<T>(NTT entity, ref T component) where T : struct
    {
        if (!_componentIds.TryGetValue(typeof(T), out var componentId))
        {
            throw new InvalidOperationException($"Component {typeof(T).Name} is not registered for network sync");
        }


        using var writer = new PacketWriter(PacketId.ComponentState);
        writer.WriteNtt(entity);
        writer.WriteByte(componentId);

        // Serialize struct as bytes using MemoryMarshal
        var bytes = MemoryMarshal.AsBytes(MemoryMarshal.CreateReadOnlySpan(ref component, 1));
        writer.WriteInt16((short)bytes.Length);
        writer.WriteBytes(bytes.ToArray());

        return writer.Finalize();
    }


    public static void TrySyncComponent(NTT viewer, NTT entity, Type componentType)
    {
        var method = typeof(ComponentSerializer).GetMethod(nameof(TrySyncTyped), BindingFlags.NonPublic | BindingFlags.Static);
        var genericMethod = method!.MakeGenericMethod(componentType);
        genericMethod.Invoke(null, [viewer, entity]);
    }

    private static void TrySyncTyped<T>(NTT viewer, NTT entity) where T : struct
    {
        if (!entity.Has<T>()) return;

        ref var component = ref entity.Get<T>();

        if (typeof(T) == typeof(InputComponent))
            return;

        // Read ChangedTick directly from raw bytes (first 8 bytes of the struct)
        // who needs interfaces and generics anyway?
        var bytes = MemoryMarshal.AsBytes(MemoryMarshal.CreateReadOnlySpan(ref component, 1));
        var changedTick = MemoryMarshal.Read<long>(bytes);

        if (changedTick == NttWorld.Tick)
            viewer.NetSync(Serialize(entity, ref component));
    }
}