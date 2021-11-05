using System.Numerics;
using System.Security.Cryptography.X509Certificates;
using iogame.Net.Packets;
using iogame.Simulation.Managers;
using iogame.Util;

namespace iogame.Simulation.Entities
{
    public class Screen
    {
        public ShapeEntity Owner;
        public Dictionary<int, ShapeEntity> Entities = new();
        public Dictionary<int, Player> Players = new();

        public Screen(ShapeEntity owner)
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
                if (Entities.ContainsKey(entity.EntityId) || entity.EntityId == Owner.EntityId)
                {
                    if (entity.PositionComponent.LastPosition != entity.PositionComponent.Position)
                        entity.MoveFor(Owner);
                }
                else
                    Add(entity, true);
            }
        }
        public void Add(ShapeEntity entity, bool spawnPacket)
        {
            if (entity.EntityId == Owner.EntityId)
                return;
            if (!World.EntityExists(entity.EntityId) || !World.EntityExists(Owner.EntityId))
                return; // bandaid

            if (entity is Player p)
                Players.TryAdd(entity.EntityId, p);

            if (Entities.TryAdd(entity.EntityId, entity))
                if (spawnPacket)
                    entity.SpawnTo(Owner);

            if(!entity.Viewport.Contains(Owner))
                entity.Viewport.Add(Owner, true);
        }

        public void Remove(ShapeEntity entity)
        {
            Entities.Remove(entity.EntityId, out var _);

            if(entity.Viewport.Contains(Owner))
                entity.Viewport.Remove(Owner);

            entity.DespawnFor(Owner);
            Players.Remove(entity.EntityId, out var _); // needs to be at the bottom
        }

        public void Clear()
        {
            foreach(var kvp in Entities)
                Remove(kvp.Value);
        }

        public bool Contains(ShapeEntity entity) => Entities.ContainsKey(entity.EntityId);
        public void Send(byte[] buffer, bool sendToSelf = false)
        {
            foreach (var kvp in Players)
                kvp.Value.Send(buffer);

            if (sendToSelf)
                (Owner as Player)?.Send(buffer);
        }
    }
}
