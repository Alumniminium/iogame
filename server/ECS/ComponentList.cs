namespace server.ECS
{
    public static class ComponentList<T> where T : struct
    {
        private static readonly T[] Array = new T[PixelWorld.MaxEntities];
        private static readonly HashSet<int> Entities = new();

        public static ref T AddFor(in PixelEntity owner)
        {
            Entities.Add(owner.Id);
            Array[owner.Id] = default;
            return ref Array[owner.Id];
        }

        public static ref T AddFor(in PixelEntity owner, ref T component)
        {
            Entities.Add(owner.Id);
            Array[owner.Id] = component;
            return ref Array[owner.Id];
        }
        public static ref T ReplaceFor(in PixelEntity ntt, ref T component)
        {
            Array[ntt.Id] = component;
            return ref Array[ntt.Id];
        }
        public static bool HasFor(in PixelEntity owner) => Entities.Contains(owner.Id);
        public static ref T Get(PixelEntity owner) => ref Array[owner.Id];

        // called via reflection @ ReflectionHelper.Remove<T>()
        public static void Remove(PixelEntity owner)
        {
            Entities.Remove(owner.Id);
            PixelWorld.InformChangesFor(in owner);
        }
    }
}