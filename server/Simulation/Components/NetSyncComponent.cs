using server.ECS;
using server.Helpers;

namespace server.Simulation.Components;

[Component]
public readonly struct NetSyncComponent(NTT EntityId, SyncThings fields)
{
    public readonly NTT EntityId = EntityId;
    public readonly SyncThings Fields = fields;


}