using System.Collections.Generic;
using server.ECS;

namespace server.Helpers;

/// <summary>
/// Simple network synchronization helper for property-based NetworkSync system.
/// Handles ChangedTick updates and network packet sending.
/// </summary>
public static class ComponentChangeTracker
{
    /// <summary>
    /// Updates a non-network field and automatically handles ChangedTick updates.
    /// Used by non-network properties to provide automatic ChangedTick management.
    /// </summary>
    public static void UpdateField<TComponent, TValue>(
        ref TComponent component,
        ref TValue field,
        TValue newValue)
        where TComponent : struct
    {
        // Check if value actually changed
        if (EqualityComparer<TValue>.Default.Equals(field, newValue))
            return;

        // Update field value
        field = newValue;

        // Update ChangedTick using reflection (minimal one-time cost)
        UpdateChangedTick(ref component);
    }

    /// <summary>
    /// Updates the ChangedTick field on a component using reflection.
    /// Uses boxing/unboxing for struct modification - optimized for infrequent calls.
    /// </summary>
    /// <typeparam name="TComponent">The component type to update</typeparam>
    /// <param name="component">The component instance to modify</param>
    private static void UpdateChangedTick<TComponent>(ref TComponent component) where TComponent : struct
    {
        var componentType = typeof(TComponent);
        var changedTickField = componentType.GetField("ChangedTick");
        if (changedTickField is not null)
        {
            var boxedComponent = (object)component;
            changedTickField.SetValue(boxedComponent, NttWorld.Tick);
            component = (TComponent)boxedComponent;
        }
    }
}