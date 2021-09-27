using System;
using System.Collections.Concurrent;
using System.Numerics;
using iogame.Net.Packets;
using iogame.Simulation;
using iogame.Simulation.Entities;

namespace server.Simulation.Systems
{
    public class Screen
    {
        public Player Owner;
        public ConcurrentDictionary<uint, Entity> Entities = new();
        public ConcurrentDictionary<uint, Player> Players = new();

        public Screen(Player owner)
        {
            Owner = owner;
        }

        public void Check(IEnumerable<List<Entity>> entities)
        {
            foreach (var kvp in Entities)
            {
                var entity = kvp.Value;
                if (!CanSee(entity))
                    Remove(entity);
            }

            foreach (var list in entities)
                foreach (var entity in list)
                    if (CanSee(entity))
                        Add(entity);
        }

        public void Add(Entity entity)
        {
            if (entity.UniqueId == Owner.UniqueId)
                return;

            if (entity is Player p)
            {
                Players.TryAdd(entity.UniqueId, (Player)entity);
                p.Send(SpawnPacket.Create(Owner));
            }


            if (Entities.TryAdd(entity.UniqueId, entity))
                Owner.Send(SpawnPacket.Create(entity));
        }

        public void Remove(Entity entity)
        {
            Entities.TryRemove(entity.UniqueId, out var _);
            Players.TryRemove(entity.UniqueId, out var _);
        }

        public void Send(byte[] buffer, bool sendToSelf = false)
        {
            foreach (var kvp in Players)
                kvp.Value.Send(buffer);

            if (sendToSelf)
                Owner.Send(buffer);
        }

        public bool CanSee(Entity entity) => (Vector2.Distance(Owner.Position, entity.Position) < 6000);
    }
}