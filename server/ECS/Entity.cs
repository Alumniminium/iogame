using System.Collections.Generic;
using server.Helpers;

namespace server.ECS
{
    public readonly struct PixelEntity
    {
        public readonly int Id;
        public readonly int Parent;
        public readonly EntityType Type;
        public PixelEntity(int id, EntityType type, int parentId = 0)
        {
            Id = id;
            Parent = parentId;
            Type = type;
        }

        public readonly List<PixelEntity> Children => PixelWorld.GetChildren(in this);
        public readonly void Add<T>(ref T component) where T : struct
        {
            ComponentList<T>.AddFor(in this, ref component);
        }

        public readonly void Replace<T>(ref T component) where T : struct
        {
            ComponentList<T>.ReplaceFor(in this, ref component);
        }

        public readonly ref T Get<T>() where T : struct
        {
            return ref ComponentList<T>.Get(this);
        }

        public readonly bool Has<T>() where T : struct
        {
            return ComponentList<T>.HasFor(in this);
        }

        public readonly bool Has<T, T2>() where T : struct where T2 : struct
        {
            return Has<T>() && Has<T2>();
        }

        public readonly bool Has<T, T2, T3>() where T : struct where T2 : struct where T3 : struct
        {
            return Has<T, T2>() && Has<T3>();
        }

        public readonly bool Has<T, T2, T3, T4>() where T : struct where T2 : struct where T3 : struct where T4 : struct
        {
            return Has<T, T2, T3>() && Has<T4>();
        }

        public readonly bool Has<T, T2, T3, T4, T5>() where T : struct where T2 : struct where T3 : struct where T4 : struct where T5 : struct
        {
            return Has<T, T2, T3, T4>() && Has<T5>();
        }

        public readonly void AddChild(in PixelEntity nt)
        {
            PixelWorld.AddChildFor(in this, in nt);
        }

        public readonly void Remove<T>()
        {
            ReflectionHelper.Remove<T>(in this);
        }

        public readonly void Recycle()
        {
            ReflectionHelper.RecycleComponents(in this);
        }

        public readonly bool IsPlayer()
        {
            return Id is >= IdGenerator.PlayerStart and <= IdGenerator.PlayerEnd;
        }

        public readonly bool
        IsAsteroid()
        {
            return Id is >= IdGenerator.AsteroidStart and <= IdGenerator.AsteroidEnd;
        }

        public readonly bool IsFood()
        {
            return Id is >= IdGenerator.FoodStart and <= IdGenerator.FoodEnd;
        }

        public readonly bool IsNpc()
        {
            return Id is >= IdGenerator.NpcStart and <= IdGenerator.NpcEnd;
        }

        internal readonly bool IsDrop()
        {
            return Id is >= IdGenerator.DropStart and <= IdGenerator.DropEnd;
        }

        public readonly bool IsBullet()
        {
            return Id is >= IdGenerator.BulletStart and <= IdGenerator.BulletEnd;
        }

        public readonly void NetSync(in byte[] packet)
        {
            OutgoingPacketQueue.Add(in this, in packet);
        }
    }
}