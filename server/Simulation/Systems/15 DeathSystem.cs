using server.ECS;
using server.Helpers;
using server.Simulation.Components;

namespace server.Simulation.Systems
{
    public sealed class DeathSystem : PixelSystem<DeathTagComponent>
    {
        public DeathSystem() : base("Death System", threads: 1) { }

        public override void Update(in PixelEntity ntt, ref DeathTagComponent c1)
        {
            if (ntt.Type == EntityType.Player)
            {
                try
                {
                    var net = ntt.Get<NetworkComponent>();
                    net.Socket.CloseAsync(System.Net.WebSockets.WebSocketCloseStatus.NormalClosure, "You died", System.Threading.CancellationToken.None).GetAwaiter().GetResult();
                    net.Socket.Dispose();
                }
                catch { }
            }
            Game.Grid.Remove(in ntt);
            PixelWorld.Destroy(in ntt);
        }
    }
}