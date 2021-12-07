using System.Collections.Generic;
using server.Helpers;
using server.Simulation.Entities;

namespace server.ECS
{
    public struct PixelEntity
    {
        public int EntityId;
        public int Parent;
        internal void AttachTo(ShapeEntity entity) => PixelWorld.AttachEntityToShapeEntity(this, entity);
        public List<int> Children => PixelWorld.GetChildren(ref this);
        public ref T Add<T>(ref T component) where T : struct => ref ComponentList<T>.AddFor(EntityId, ref component);
        public void Replace<T>(T component) where T : struct => ComponentList<T>.ReplaceFor(EntityId, component);
        public ref T Add<T>() where T : struct => ref ComponentList<T>.AddFor(EntityId);
        public readonly bool Has<T>() where T : struct => ComponentList<T>.HasFor(EntityId);
        public readonly ref T Get<T>() where T : struct => ref ComponentList<T>.Get(EntityId);
        public readonly void Remove<T>() => ReflectionHelper.Remove<T>(EntityId);
        public readonly bool Has<T, T2>() where T : struct where T2 : struct => Has<T>() && Has<T2>();
        public readonly bool Has<T, T2, T3>() where T : struct where T2 : struct where T3 : struct => Has<T, T2>() && Has<T3>();
        public readonly bool Has<T, T2, T3, T4>() where T : struct where T2 : struct where T3 : struct where T4 : struct => Has<T, T2, T3>() && Has<T4>();
        public readonly bool Has<T, T2, T3, T4,T5>() where T : struct where T2 : struct where T3 : struct where T4 : struct where T5:struct => Has<T, T2, T3,T4>() && Has<T5>();
        public void AddChild(ref PixelEntity nt) => PixelWorld.AddChildFor(ref this, ref nt);
        internal readonly void Recycle() => ReflectionHelper.RecycleComponents(EntityId);

        public readonly bool IsPlayer() => EntityId is >= IdGenerator.PlayerStart and <= IdGenerator.PlayerEnd;
        public readonly bool IsFood() => EntityId is >= IdGenerator.FoodStart and <= IdGenerator.FoodEnd;
        public readonly bool IsNpc() => EntityId is >= IdGenerator.NpcStart and <= IdGenerator.NpcEnd;
        public readonly bool IsBullet() => EntityId >= IdGenerator.BulletStart;

        internal readonly void NetSync(byte[] packet) => OutgoingPacketQueue.Add(this, packet);
    }
}