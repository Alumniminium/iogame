using server.Helpers;
using server.Simulation.Entities;

namespace server.ECS
{
    public readonly struct PixelEntity
    {
        public readonly int Id;
        public readonly int Parent;
        public PixelEntity(int id, int parentId=0) 
        {
            Id = id;
            Parent = parentId;
        }
        public readonly void AttachTo(ShapeEntity ntt) => PixelWorld.AttachEntityToShapeEntity(in this, ntt);
        public readonly List<PixelEntity> Children => PixelWorld.GetChildren(in this);
        public readonly void Add<T>(ref T component) where T : struct => ComponentList<T>.AddFor(in this, ref component);
        public readonly void Replace<T>(ref T component) where T : struct => ComponentList<T>.ReplaceFor(in this, ref component);
        public readonly ref T Get<T>() where T : struct => ref ComponentList<T>.Get(this);
        public readonly bool Has<T>() where T : struct => ComponentList<T>.HasFor(in this);
        public readonly bool Has<T, T2>() where T : struct where T2 : struct => Has<T>() && Has<T2>();
        public readonly bool Has<T, T2, T3>() where T : struct where T2 : struct where T3 : struct => Has<T, T2>() && Has<T3>();
        public readonly bool Has<T, T2, T3, T4>() where T : struct where T2 : struct where T3 : struct where T4 : struct => Has<T, T2, T3>() && Has<T4>();
        public readonly bool Has<T, T2, T3, T4,T5>() where T : struct where T2 : struct where T3 : struct where T4 : struct where T5:struct => Has<T, T2, T3,T4>() && Has<T5>();
        public readonly void AddChild(in PixelEntity nt) => PixelWorld.AddChildFor(in this,in nt);
        public readonly void Remove<T>() => ReflectionHelper.Remove<T>(in this);
        public readonly void Recycle() => ReflectionHelper.RecycleComponents(in this);

        public readonly bool IsPlayer() => Id is >= IdGenerator.PlayerStart and <= IdGenerator.PlayerEnd;
        public readonly bool IsAsteroid() => Id is >= IdGenerator.AsteroidStart and <= IdGenerator.AsteroidEnd;
        public readonly bool IsFood() => Id is >= IdGenerator.FoodStart and <= IdGenerator.FoodEnd;
        public readonly bool IsNpc() => Id is >= IdGenerator.NpcStart and <= IdGenerator.NpcEnd;

        internal readonly bool IsDrop() => Id is >= IdGenerator.DropStart and <= IdGenerator.DropEnd;

        public readonly bool IsBullet() => Id is >= IdGenerator.BulletStart and <= IdGenerator.BulletEnd;
        public readonly void NetSync(in byte[] packet) => OutgoingPacketQueue.Add(in this, in packet);
    }
}