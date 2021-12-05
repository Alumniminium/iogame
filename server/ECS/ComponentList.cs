using System;
using System.Collections.Generic;
using System.Linq;

namespace server.ECS
{
    public static class ComponentList<T> where T : struct
    {
        private static T[] _array = new T[1];
        private static readonly Stack<int> AvailableIndicies = new(Enumerable.Range(0, _array.Length));
        private static readonly Dictionary<int, int> EntityIdToArrayOffset = new();

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

            _array[offset] = default;
            PixelWorld.InformChangesFor(owner);
            return ref _array[offset];
        }

        private static void Resize()
        {
            T[] newArray = new T[_array.Length  + 5];
            Array.Copy(_array,newArray,_array.Length);

            for(int i = newArray.Length-1; i > _array.Length; i--)
                AvailableIndicies.Push(i);
            
            _array = newArray;
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

            _array[offset] = component;
            PixelWorld.InformChangesFor(owner);
            return ref _array[offset];
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
            _array[offset] = component;
            PixelWorld.InformChangesFor(owner);
            return ref _array[offset];
        }
        public static bool HasFor(int owner) => EntityIdToArrayOffset.ContainsKey(owner);

        public static ref T Get(int owner) => ref _array[EntityIdToArrayOffset[owner]];
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