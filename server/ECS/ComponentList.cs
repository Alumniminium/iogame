using System.Collections.Concurrent;

namespace server.ECS
{
    public static class ComponentList<T> where T : struct
    {
        private static readonly T[] Array = new T[PixelWorld.MaxEntities];
        private static readonly ConcurrentQueue<int> AvailableIndicies = new(Enumerable.Range(0, Array.Length));
        private static readonly ConcurrentDictionary<int, int> EntityIdToArrayOffset = new();

        public static ref T AddFor(in PixelEntity owner)
        {
            if (!EntityIdToArrayOffset.TryGetValue(owner.Id, out var offset))
                if (AvailableIndicies.TryDequeue(out offset))
                    EntityIdToArrayOffset.TryAdd(owner.Id, offset);

            Array[offset] = default;
            PixelWorld.InformChangesFor(in owner);
            return ref Array[offset];
        }

        public static ref T ReplaceFor(in PixelEntity owner, ref T component) => ref AddFor(in owner, ref component);
        public static ref T AddFor(in PixelEntity owner, ref T component)
        {
            if (!EntityIdToArrayOffset.TryGetValue(owner.Id, out var offset))
                if (AvailableIndicies.TryDequeue(out offset))
                    EntityIdToArrayOffset.TryAdd(owner.Id, offset);
            
            Array[offset] = component;
            PixelWorld.InformChangesFor(in owner);
            return ref Array[offset];
        }
        public static bool HasFor(in PixelEntity owner) => EntityIdToArrayOffset.ContainsKey(owner.Id);

        public static ref T Get(PixelEntity owner) => ref Array[EntityIdToArrayOffset[owner.Id]];
        // called via reflection @ ReflectionHelper.Remove<T>()
        public static void Remove(PixelEntity owner)
        {
            if (!EntityIdToArrayOffset.Remove(owner.Id, out var offset)) 
                return;
            AvailableIndicies.Enqueue(offset);
            PixelWorld.InformChangesFor(in owner);
        }
    }
}