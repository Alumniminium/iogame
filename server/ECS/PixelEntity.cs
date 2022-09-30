using System;
using Packets.Enums;
using server.Helpers;

namespace server.ECS
{
    public readonly struct PixelEntity
    {
        public readonly int Id;
        public readonly EntityType Type;
        public PixelEntity(int id, EntityType type)
        {
            Id = id;
            Type = type;
        }

        public readonly void Add<T>(ref T component) where T : struct => ComponentList<T>.AddFor(in this, ref component);
        public readonly ref T Get<T>() where T : struct => ref ComponentList<T>.Get(this);
        public readonly bool Has<T>() where T : struct => ComponentList<T>.HasFor(in this);
        public readonly bool Has<T, T2>() where T : struct where T2 : struct => Has<T>() && Has<T2>();
        public readonly bool Has<T, T2, T3>() where T : struct where T2 : struct where T3 : struct => Has<T, T2>() && Has<T3>();
        public readonly bool Has<T, T2, T3, T4>() where T : struct where T2 : struct where T3 : struct where T4 : struct => Has<T, T2, T3>() && Has<T4>();
        public readonly bool Has<T, T2, T3, T4, T5>() where T : struct where T2 : struct where T3 : struct where T4 : struct where T5 : struct => Has<T, T2, T3, T4>() && Has<T5>();
        public readonly void Remove<T>() => ReflectionHelper.Remove<T>(in this);
        public readonly void Recycle() => ReflectionHelper.RecycleComponents(in this);
        public readonly void NetSync(in Memory<byte> packet) => OutgoingPacketQueue.Add(in this, in packet);

        public override int GetHashCode() => Id;
    }
}