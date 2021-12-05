using iogame.Net.Packets;
using iogame.Simulation.Entities;
using iogame.Simulation.Managers;
using iogame.Util;

namespace iogame.ECS
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
        public bool Has<T>() where T : struct => ComponentList<T>.HasFor(EntityId);
        public ref T Get<T>() where T : struct => ref ComponentList<T>.Get(EntityId);
        public void Remove<T>() => ReflectionHelper.Remove<T>(EntityId);
        public bool Has<T, T2>() where T : struct where T2 : struct => Has<T>() && Has<T2>();
        public bool Has<T, T2, T3>() where T : struct where T2 : struct where T3 : struct => Has<T, T2>() && Has<T3>();
        public bool Has<T, T2, T3, T4>() where T : struct where T2 : struct where T3 : struct where T4 : struct => Has<T, T2, T3>() && Has<T4>();
        public bool Has<T, T2, T3, T4,T5>() where T : struct where T2 : struct where T3 : struct where T4 : struct where T5:struct => Has<T, T2, T3,T4>() && Has<T5>();
        public void AddChild(ref PixelEntity nt) => PixelWorld.AddChildFor(ref this, ref nt);
        internal void Recycle() => ReflectionHelper.RecycleComponents(EntityId);

        public bool IsPlayer() => EntityId >= IdGenerator.PLAYER_START && EntityId <= IdGenerator.PLAYER_END;
        public bool IsFood() => EntityId >= IdGenerator.FOOD_START && EntityId <= IdGenerator.FOOD_END;
        public bool IsNpc() => EntityId >= IdGenerator.NPC_START && EntityId <= IdGenerator.NPC_END;
        public bool IsBullet() => EntityId >= IdGenerator.BULLET_START;

        internal void NetSync(byte[] packet) => OutgoingPacketQueue.Add(this, packet);
    }
}