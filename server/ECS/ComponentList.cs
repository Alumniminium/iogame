using System.Collections.Generic;

namespace server.ECS
{
    public static class ComponentList<T> where T : struct
    {
        private static readonly T[] Array = new T[PixelWorld.MaxEntities];
        private static readonly HashSet<PixelEntity> Entities = new();

        public static void AddFor(in PixelEntity owner, ref T component)
        {
            if (!Entities.Contains(owner))
                Entities.Add(owner);

            Array[owner.Id] = component;
            PixelWorld.InformChangesFor(in owner);
        }
        public static void AddFor(in PixelEntity owner, T component) => AddFor(in owner, ref component);
        public static bool HasFor(in PixelEntity owner) => Entities.Contains(owner);
        public static ref T Get(PixelEntity owner) => ref Array[owner.Id];

        // called via reflection @ ReflectionHelper.Remove<T>()
        public static void Remove(PixelEntity owner, bool notify)
        {
            if (Entities.Remove(owner))
            {
                Array[owner.Id] = default;
                if (notify)
                    PixelWorld.InformChangesFor(in owner);
            }
        }
    }
}