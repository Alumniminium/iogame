using System.Numerics;
using System.Security.Cryptography.X509Certificates;
using iogame.Net.Packets;
using iogame.Util;

namespace iogame.Simulation.Entities
{
    public class Screen
    {
        public Entity Owner;
        public Dictionary<uint, Entity> Entities = new();
        public Dictionary<uint, Player> Players = new();

        public Screen(Entity owner)
        {
            Owner = owner;
        }

        public unsafe void Update()
        {
            var list = CollisionDetection.Grid.GetEntitiesInViewport(Owner);
            foreach(var entity in Entities)
            {
                if(list.Contains(entity.Value) && entity.Value.HealthComponent.Health >= 0)
                    continue;
                Remove(entity.Value);
            }
            foreach (var entity in list)
            {
                if (Entities.ContainsKey(entity.UniqueId) || entity.UniqueId == Owner.UniqueId)
                {
                    if (entity.PositionComponent.LastPosition != entity.PositionComponent.Position)
                        entity.MoveFor(Owner);
                }
                else
                    Add(entity, true);
            }
        }
        public void Add(Entity entity, bool spawnPacket)
        {
            if (entity.UniqueId == Owner.UniqueId)
                return;

            if (entity is Player p)
                Players.TryAdd(entity.UniqueId, p);

            if (Entities.TryAdd(entity.UniqueId, entity))
                if (spawnPacket)
                    entity.SpawnTo(Owner);

            if(!entity.Viewport.Contains(Owner))
                entity.Viewport.Add(Owner, true);
        }

        public void Remove(Entity entity)
        {
            Entities.Remove(entity.UniqueId, out var _);

            if(entity.Viewport.Contains(Owner))
                entity.Viewport.Remove(Owner);

            entity.DespawnFor(Owner);
            Players.Remove(entity.UniqueId, out var _);
        }

        public void Clear()
        {
            foreach(var kvp in Entities)
                Remove(kvp.Value);
        }

        public bool Contains(Entity entity) => Entities.ContainsKey(entity.UniqueId);
        public void Send(byte[] buffer, bool sendToSelf = false)
        {
            foreach (var kvp in Players)
                kvp.Value.Send(buffer);

            if (sendToSelf)
                (Owner as Player)?.Send(buffer);
        }
    }
}
