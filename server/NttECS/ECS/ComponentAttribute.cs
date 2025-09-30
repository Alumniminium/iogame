using System;
using server.Enums;

namespace server.ECS;

/// <summary>
/// Attribute to mark structs as ECS components with optional persistence and networking configuration.
/// Components marked with this attribute are automatically discovered by the reflection system.
/// </summary>
[AttributeUsage(AttributeTargets.Struct)]
public class ComponentAttribute : Attribute
{
    /// <summary>
    /// Gets whether this component should be saved/loaded to/from disk for server persistence.
    /// </summary>
    public bool SaveEnabled { get; set; } = false;

    /// <summary>
    /// Network component type for serialization. If not None, this component will be synced to clients.
    /// </summary>
    public ComponentType ComponentType { get; set; } = ComponentType.None;

    /// <summary>
    /// Whether this component should be synchronized to clients over the network.
    /// </summary>
    public bool NetworkSync { get; set; } = false;
}