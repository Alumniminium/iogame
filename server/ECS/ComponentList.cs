using iogame.Simulation.Components;

namespace iogame.Simulation.Managers
{
    public static partial class ComponentList<T> where T : struct
    {
        public const int AMOUNT = 100_000;
        private readonly static T[] array = new T[AMOUNT];
        private readonly static Stack<int> AvailableIndicies = new(Enumerable.Range(0, AMOUNT));
        private readonly static Dictionary<int, int> EntityIdToArrayOffset = new();

        public static ref T AddFor(int owner) => ref AddFor(owner, default);
        public static ref T AddFor(int owner, T component)
        {
            if (!EntityIdToArrayOffset.TryGetValue(owner, out var offset))
                if (AvailableIndicies.TryPop(out offset))
                    EntityIdToArrayOffset.TryAdd(owner, offset);
                else
                    throw new System.Exception("ran out of available indicies");

            array[offset] = component;
            World.InformChangesFor(owner);
            return ref array[offset];
        }
        public static bool HasFor(int owner) => EntityIdToArrayOffset.ContainsKey(owner);

        public static ref T Get(int owner)
        {
            if (EntityIdToArrayOffset.TryGetValue(owner, out var index))
                return ref array[index];
            throw new KeyNotFoundException($"Fucking index not found. ({nameof(array)} Len: {array.Length}, index for entity {owner} not found.)");
        }
        // called via refelction @ ReflectionHelper.Remove<T>()
        public static void Remove(int owner)
        {
            if (EntityIdToArrayOffset.Remove(owner, out int offset))
                AvailableIndicies.Push(offset);
            World.InformChangesFor(owner);
        }
    }
}