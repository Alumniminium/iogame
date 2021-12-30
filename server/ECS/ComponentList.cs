using System.Collections.Concurrent;

namespace server.ECS
{
    public static class ComponentList<T> where T : struct
    {
        private static readonly T[] Array = new T[PixelWorld.MaxEntities];
        private static readonly Dictionary<int, int> EntityIdToArrayOffset = new();

        public static ref T AddFor(in PixelEntity owner)
        {
            EntityIdToArrayOffset.TryAdd(owner.Id, owner.Id);
            Array[owner.Id] = default;
            return ref Array[owner.Id];
        }

        public static ref T ReplaceFor(in PixelEntity owner, ref T component) => ref AddFor(in owner, ref component);
        public static ref T AddFor(in PixelEntity owner, ref T component)
        {
            EntityIdToArrayOffset.TryAdd(owner.Id, owner.Id);
            Array[owner.Id] = component;
            return ref Array[owner.Id];
        }
        public static bool HasFor(in PixelEntity owner) => EntityIdToArrayOffset.ContainsKey(owner.Id);

        public static ref T Get(PixelEntity owner) => ref Array[owner.Id];
        // called via reflection @ ReflectionHelper.Remove<T>()
        public static void Remove(PixelEntity owner)
        {
            EntityIdToArrayOffset.Remove(owner.Id);
            PixelWorld.InformChangesFor(in owner);
        }
    }
}