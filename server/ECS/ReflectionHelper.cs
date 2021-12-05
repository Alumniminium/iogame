using System;
using System.Collections.Generic;
using System.Linq;

namespace server.ECS
{
    public static class ReflectionHelper
    {
        private static readonly List<Action<int>> RemoveMethodCache;
        private static readonly Dictionary<Type, Action<int>> Cache = new ();
        static ReflectionHelper()
        {
            var types = from a in AppDomain.CurrentDomain.GetAssemblies()
                        from t in a.GetTypes()
                        let aList = t.GetCustomAttributes(typeof(ComponentAttribute), true)
                        where aList?.Length > 0
                        select t;

            var typesArray = types as Type[] ?? types.ToArray();
            var methods = Enumerable.Select(typesArray, ct => (Action<int>)typeof(ComponentList<>).MakeGenericType(ct).GetMethod("Remove")?.CreateDelegate(typeof(Action<int>))!);

            RemoveMethodCache = new List<Action<int>>(methods);
            List<Type> componentTypes = new List<Type>(typesArray);

            for (int i = 0; i < componentTypes.Count; i++)
            {
                var type = componentTypes[i];
                var method = RemoveMethodCache[i];
                Cache.Add(type, method);
            }
        }
        public static void Remove<T>(int entityId)
        {
            if (Cache.TryGetValue(typeof(T), out var method))
            {
                method.Invoke(entityId);
                PixelWorld.InformChangesFor(entityId);
            }
        }
        public static void RecycleComponents(int entityId)
        {
            for (int i = 0; i < RemoveMethodCache.Count; i++)
                RemoveMethodCache[i].Invoke(entityId);
        }
    }
}