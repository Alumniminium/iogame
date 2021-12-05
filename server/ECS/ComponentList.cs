using iogame.Util;
using Microsoft.AspNetCore.Mvc;

namespace iogame.Simulation.Managers
{
    public static class ComponentList<T> where T : struct
    {
        private static T[] array = new T[1];
        private readonly static Stack<int> AvailableIndicies = new(Enumerable.Range(0, array.Length));
        private readonly static Dictionary<int, int> EntityIdToArrayOffset = new();

        public static ref T AddFor(int owner)
        {
            if (!EntityIdToArrayOffset.TryGetValue(owner, out var offset))
                if (AvailableIndicies.TryPop(out offset))
                    EntityIdToArrayOffset.TryAdd(owner, offset);
                else
                {
                    Resize();
                    return ref AddFor(owner);
                }

            array[offset] = default;
            PixelWorld.InformChangesFor(owner);
            return ref array[offset];
        }

        private static void Resize()
        {
            T[] newArray = new T[array.Length  + 5];
            Array.Copy(array,newArray,array.Length);

            for(int i = newArray.Length-1; i > array.Length; i--)
                AvailableIndicies.Push(i);
            
            array = newArray;
        }

        public static ref T ReplaceFor(int owner, T component)
        {
            if (!EntityIdToArrayOffset.TryGetValue(owner, out var offset))
                if (AvailableIndicies.TryPop(out offset))
                    EntityIdToArrayOffset.TryAdd(owner, offset);
                else
                {
                    Resize();
                    return ref ReplaceFor(owner,component);
                }

            array[offset] = component;
            PixelWorld.InformChangesFor(owner);
            return ref array[offset];
        }
        public static ref T AddFor(int owner, ref T component)
        {
            if (!EntityIdToArrayOffset.TryGetValue(owner, out var offset))
                if (AvailableIndicies.TryPop(out offset))
                    EntityIdToArrayOffset.TryAdd(owner, offset);
                else
                {
                    Resize();
                    return ref AddFor(owner, ref component);
                }
            array[offset] = component;
            PixelWorld.InformChangesFor(owner);
            return ref array[offset];
        }
        public static bool HasFor(int owner) => EntityIdToArrayOffset.ContainsKey(owner);

        public static ref T Get(int owner) => ref array[EntityIdToArrayOffset[owner]];
        // called via refelction @ ReflectionHelper.Remove<T>()
        public static void Remove(int owner)
        {
            if (EntityIdToArrayOffset.Remove(owner, out int offset))
            {
                AvailableIndicies.Push(offset);
                PixelWorld.InformChangesFor(owner);
            }
        }
    }
}