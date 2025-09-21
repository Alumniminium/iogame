using System;

namespace server.ECS;

/// <summary>
/// Attribute to mark structs as ECS components with optional persistence configuration.
/// Components marked with this attribute are automatically discovered by the reflection system.
/// </summary>
/// <param name="SaveEnabled">Whether this component should be persisted to disk</param>
[AttributeUsage(AttributeTargets.Struct)]
public class ComponentAttribute(bool SaveEnabled = false) : Attribute
{
    /// <summary>
    /// Gets whether this component should be saved/loaded to/from disk for server persistence.
    /// </summary>
    public bool SaveEnabled { get; } = SaveEnabled;
}