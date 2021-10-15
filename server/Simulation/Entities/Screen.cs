using System.Numerics;
using iogame.Net.Packets;

namespace iogame.Simulation.Entities
{
    public class Screen
    {
        public Player Owner;
        public Dictionary<uint, Entity> Entities = new();
        public Dictionary<uint, Player> Players = new();

        public Screen(Player owner)
        {
            Owner = owner;
        }

        public void Update()
        {
            var list = Collections.Grid.GetEntitiesInViewport(Owner);

            foreach (var entity in list)
            {
                if (Entities.ContainsKey(entity.UniqueId) || entity.UniqueId == Owner.UniqueId)
                {
                    if (entity.LastPosition != entity.Position)
                        Owner.Send(MovementPacket.Create(entity.UniqueId, entity.Position, entity.Velocity));
                }
                else
                    Owner.Send(MovementPacket.Create(entity.UniqueId, entity.Position, entity.Velocity));
            }
            Entities.Clear();
            Players.Clear();

            foreach (var entity in list)
                Add(entity, false);
        }
        public void Add(Entity entity, bool spawnPacket)
        {
            if (entity.UniqueId == Owner.UniqueId)
                return;

            if (entity is Player p)
            {
                Players.TryAdd(entity.UniqueId, (Player)entity);
                if (spawnPacket)
                    p.Send(SpawnPacket.Create(Owner));
            }


            if (Entities.TryAdd(entity.UniqueId, entity))
                if (spawnPacket)
                    Owner.Send(SpawnPacket.Create(entity));
        }

        public void Remove(Entity entity)
        {
            Entities.Remove(entity.UniqueId, out var _);
            Players.Remove(entity.UniqueId, out var _);
        }

        public void Send(byte[] buffer, bool sendToSelf = false)
        {
            foreach (var kvp in Players)
                kvp.Value.Send(buffer);

            if (sendToSelf)
                Owner.Send(buffer);
        }
    }
}
