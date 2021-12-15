using server.Helpers;
using server.Simulation.Entities;

namespace server.ECS
{
    public readonly struct PixelEntity
    {
        public readonly int EntityId;
        public PixelEntity(int id) => EntityId = id;
        // public int Parent;
        public readonly void AttachTo(ShapeEntity entity) => PixelWorld.AttachEntityToShapeEntity(this, entity);
        // public List<PixelEntity> Children => PixelWorld.GetChildren(this);
        public readonly void Add<T>(in T component) where T : struct => ComponentList<T>.AddFor(in this, in component);
        public readonly ref T Get<T>() where T : struct => ref ComponentList<T>.Get(this);
        public readonly bool Has<T>() where T : struct => ComponentList<T>.HasFor(in this);
        public readonly bool Has<T, T2>() where T : struct where T2 : struct => Has<T>() && Has<T2>();
        public readonly bool Has<T, T2, T3>() where T : struct where T2 : struct where T3 : struct => Has<T, T2>() && Has<T3>();
        public readonly bool Has<T, T2, T3, T4>() where T : struct where T2 : struct where T3 : struct where T4 : struct => Has<T, T2, T3>() && Has<T4>();
        public readonly bool Has<T, T2, T3, T4,T5>() where T : struct where T2 : struct where T3 : struct where T4 : struct where T5:struct => Has<T, T2, T3,T4>() && Has<T5>();
        // public void AddChild(ref PixelEntity nt) => PixelWorld.AddChildFor(ref this, ref nt);
        public readonly void Replace<T>(in T component) where T : struct => ComponentList<T>.ReplaceFor(this, in component);
        public readonly void Remove<T>() => ReflectionHelper.Remove<T>(in this);
        public readonly void Recycle() => ReflectionHelper.RecycleComponents(in this);

        public readonly bool IsPlayer() => EntityId is >= IdGenerator.PlayerStart and <= IdGenerator.PlayerEnd;
        public readonly bool IsFood() => EntityId is >= IdGenerator.FoodStart and <= IdGenerator.FoodEnd;
        public readonly bool IsNpc() => EntityId is >= IdGenerator.NpcStart and <= IdGenerator.NpcEnd;
        public readonly bool IsBullet() => EntityId >= IdGenerator.BulletStart;
        public readonly void NetSync(byte[] packet) => OutgoingPacketQueue.Add(in this, packet);
    }
}