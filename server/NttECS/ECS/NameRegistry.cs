using System;
using System.Collections.Concurrent;

namespace server.ECS;

/// <summary>
/// Registry for mapping entity names to entities for quick lookup.
/// Enables finding entities by human-readable names.
/// </summary>
public static class NameRegistry
{
    private static readonly ConcurrentDictionary<string, NTT> _nameToEntity = new();
    private static readonly ConcurrentDictionary<Guid, string> _entityToName = new();

    /// <summary>
    /// Add an entity with a name to the registry.
    /// </summary>
    /// <param name="entity">Entity to register</param>
    /// <param name="name">Name to associate with the entity</param>
    public static void Add(in NTT entity, string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return;

        _nameToEntity[name] = entity;
        _entityToName[entity.Id] = name;
    }

    /// <summary>
    /// Remove an entity from the registry.
    /// </summary>
    /// <param name="entity">Entity to remove</param>
    public static void Remove(NTT entity)
    {
        if (_entityToName.TryRemove(entity.Id, out var name))
        {
            _nameToEntity.TryRemove(name, out _);
        }
    }

    /// <summary>
    /// Get an entity by name.
    /// </summary>
    /// <param name="name">Name to look up</param>
    /// <returns>Tuple indicating if found and the entity</returns>
    public static (bool found, NTT entity) GetEntity(string name)
    {
        if (_nameToEntity.TryGetValue(name, out var entity))
        {
            return (true, entity);
        }
        return (false, default);
    }

    /// <summary>
    /// Get the name of an entity.
    /// </summary>
    /// <param name="entity">Entity to get name for</param>
    /// <returns>Name of the entity, or null if not found</returns>
    public static string GetName(NTT entity)
    {
        return _entityToName.TryGetValue(entity.Id, out var name) ? name : null;
    }
}