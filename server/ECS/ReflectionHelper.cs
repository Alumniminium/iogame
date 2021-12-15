using System.Reflection;

namespace server.ECS
{
    public static class ReflectionHelper
    {
        private static readonly List<Action<PixelEntity>> RemoveMethodCache;
        private static readonly Dictionary<Type, Action<PixelEntity>> Cache = new ();
        static ReflectionHelper()
        {
            var types = from t in Assembly.GetExecutingAssembly().GetTypes()
                let aList = t.GetCustomAttributes(typeof(ComponentAttribute), true)
                where aList.Length > 0
                select t;

            var enumerable = types as Type[] ?? types.ToArray();
            var methods = enumerable.Select(ct => (Action<PixelEntity>)typeof(ComponentList<>).MakeGenericType(ct).GetMethod("Remove")!.CreateDelegate(typeof(Action<PixelEntity>)));

            RemoveMethodCache = new List<Action<PixelEntity>>(methods);
            var componentTypes = new List<Type>(enumerable);

            for (var i = 0; i < componentTypes.Count; i++)
            {
                var type = componentTypes[i];
                var method = RemoveMethodCache[i];
                Cache.Add(type, method);
            }
        }
        public static void Remove<T>(in PixelEntity entity)
        {
            if (!Cache.TryGetValue(typeof(T), out var method)) 
                return;
            method.Invoke(entity);
            PixelWorld.InformChangesFor(in entity);
        }
        public static void RecycleComponents(in PixelEntity entity)
        {
            for (var i = 0; i < RemoveMethodCache.Count; i++)
                RemoveMethodCache[i].Invoke(entity);
        }
    }
}