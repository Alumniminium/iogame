namespace server.ECS
{
    public static class ComponentList<T> where T : struct
    {
        private static readonly T[] Array = new T[PixelWorld.MaxEntities];
        private static readonly Stack<int> AvailableIndicies = new(Enumerable.Range(0, Array.Length));
        private static readonly Dictionary<int, int> EntityIdToArrayOffset = new();

        public static ref T AddFor(int owner)
        {
            if (!EntityIdToArrayOffset.TryGetValue(owner, out var offset))
                if (AvailableIndicies.TryPop(out offset))
                    EntityIdToArrayOffset.TryAdd(owner, offset);

            Array[offset] = default;
            PixelWorld.InformChangesFor(owner);
            return ref Array[offset];
        }

        public static ref T ReplaceFor(int owner, T component)
        {
            if (!EntityIdToArrayOffset.TryGetValue(owner, out var offset))
                if (AvailableIndicies.TryPop(out offset))
                    EntityIdToArrayOffset.TryAdd(owner, offset);

            Array[offset] = component;
            PixelWorld.InformChangesFor(owner);
            return ref Array[offset];
        }
        public static ref T AddFor(int owner, ref T component)
        {
            if (!EntityIdToArrayOffset.TryGetValue(owner, out var offset))
                if (AvailableIndicies.TryPop(out offset))
                    EntityIdToArrayOffset.TryAdd(owner, offset);
            
            Array[offset] = component;
            PixelWorld.InformChangesFor(owner);
            return ref Array[offset];
        }
        public static bool HasFor(int owner) => EntityIdToArrayOffset.ContainsKey(owner);

        public static ref T Get(int owner) => ref Array[EntityIdToArrayOffset[owner]];
        // called via reflection @ ReflectionHelper.Remove<T>()
        public static void Remove(int owner)
        {
            if (!EntityIdToArrayOffset.Remove(owner, out var offset)) 
                return;
            AvailableIndicies.Push(offset);
            PixelWorld.InformChangesFor(owner);
        }
    }
}