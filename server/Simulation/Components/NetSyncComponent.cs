using server.ECS;
using server.Helpers;

namespace server.Simulation.Components;

[Component]
public readonly struct NetSyncComponent(int entityId, SyncThings fields)
{
    public readonly int EntityId = entityId;
    public readonly SyncThings Fields = fields;

    public override int GetHashCode() => EntityId;
}