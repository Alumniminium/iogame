using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace server.ECS;

/// <summary>
/// Reflection-based utility for performing type-safe operations on all component types.
/// Pre-compiles method delegates for efficient execution of component operations across all registered types.
/// </summary>
public static class ReflectionHelper
{
    /// <summary>Cached delegates for component removal operations</summary>
    private static readonly Dictionary<Type, Action<NTT, bool>> RemoveCache = [];
    /// <summary>Cached delegates for component save operations</summary>
    private static readonly Dictionary<Type, Action<string>> SaveCache = [];
    /// <summary>Cached delegates for component load operations</summary>
    private static readonly Dictionary<Type, Action<string>> LoadCache = [];
    /// <summary>Cached delegates for component ownership transfer operations</summary>
    private static readonly Dictionary<Type, Action<NTT, NTT>> ChangeOwnerCache = [];

    /// <summary>
    /// Initializes reflection helper by discovering and caching component operation delegates.
    /// </summary>
    static ReflectionHelper() => LoadMethods();


    /// <summary>
    /// Discovers all component types and pre-compiles operation delegates for efficient execution.
    /// </summary>
    public static void LoadMethods()
    {
        var assemblies = AppDomain.CurrentDomain.GetAssemblies();
        var types = assemblies.SelectMany(x => x.GetTypes());

        var componentTypes = types
            .Where(t => t.GetCustomAttributes(typeof(ComponentAttribute), true).Length > 0)
            .ToList();

        foreach (var ct in componentTypes)
        {
            var removeMethod = typeof(PackedComponentStorage<>).MakeGenericType(ct).GetMethod("Remove", [typeof(NTT), typeof(bool)])!.CreateDelegate<Action<NTT, bool>>();
            RemoveCache.TryAdd(ct, removeMethod);

            var changeOwnerMethod = (Action<NTT, NTT>)typeof(PackedComponentStorage<>).MakeGenericType(ct).GetMethod("ChangeOwner", [typeof(NTT), typeof(NTT)])!.CreateDelegate(typeof(Action<NTT, NTT>));
            ChangeOwnerCache.TryAdd(ct, changeOwnerMethod);

            var saveAttribute = ct.GetCustomAttribute<ComponentAttribute>();
            if (saveAttribute?.SaveEnabled ?? false)
            {
                var saveMethod = typeof(PackedComponentStorage<>).MakeGenericType(ct).GetMethod("Save", [typeof(string)])!.CreateDelegate<Action<string>>();
                SaveCache.TryAdd(ct, saveMethod);

                var loadMethod = typeof(PackedComponentStorage<>).MakeGenericType(ct).GetMethod("Load", [typeof(string)])!.CreateDelegate<Action<string>>();
                LoadCache.TryAdd(ct, loadMethod);
            }
        }
    }

    /// <summary>
    /// Removes a component of the specified type from an entity using cached reflection delegates.
    /// </summary>
    /// <typeparam name="T">Component type to remove</typeparam>
    /// <param name="ntt">Entity to remove component from</param>
    public static void Remove<T>(NTT ntt)
    {
        if (!RemoveCache.TryGetValue(typeof(T), out var method))
            return;
        method.Invoke(ntt, true);
    }
    /// <summary>
    /// Transfers ownership of all components from one entity to another in parallel.
    /// </summary>
    /// <param name="from">Source entity to transfer components from</param>
    /// <param name="to">Target entity to transfer components to</param>
    public static void ChangeOwner(NTT from, NTT to) => Parallel.ForEach(ChangeOwnerCache.Values, method => method.Invoke(from, to));

    /// <summary>
    /// Removes all components from an entity without notifying systems (for recycling).
    /// </summary>
    /// <param name="ntt">Entity to recycle components from</param>
    public static void RecycleComponents(NTT ntt) => Parallel.ForEach(RemoveCache.Values, method => method.Invoke(ntt, false));

    /// <summary>
    /// Loads all component types from disk in parallel for server startup.
    /// </summary>
    /// <param name="path">Directory path to load component data from</param>
    public static void LoadComponents(string path) => Parallel.ForEach(LoadCache.Values, method => method.Invoke(path));
}