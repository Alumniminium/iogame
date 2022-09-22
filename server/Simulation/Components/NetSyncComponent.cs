using server.ECS;
using server.Helpers;

namespace server.Simulation.Components
{
    [Component]
    public readonly struct NetSyncComponent
    {
        public readonly SyncThings Fields = SyncThings.None;

        public NetSyncComponent(SyncThings fields)
        {
            Fields = fields;
        }
    }
}