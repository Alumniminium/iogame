using System;
using System.Collections.Concurrent;
using System.Numerics;
using iogame.Simulation;
using iogame.Simulation.Entities;

namespace server.Simulation.Systems
{
    public class Screen
    {
        public Entity Owner;
        public ConcurrentDictionary<uint, Entity> Entities = new();
        public ConcurrentDictionary<uint, Player> Players = new();

        public Screen(Entity owner)
        {
            Owner = owner;
        }

        public void Check()
        {
            foreach (var kvp in Collections.Entities)
            {
                if (CanSee(kvp.Value))
                {
                    Add(kvp.Value);
                    kvp.Value.Screen.Add(Owner);
                }
                else
                {
                    Remove(kvp.Value);
                    kvp.Value.Screen.Remove(Owner);
                }
            }
        }

        private bool CanSee(Entity entity)
        {
            return (Vector2.Distance(Owner.Origin, entity.Origin) < Owner.ViewDistance);
        }

        public void Add(Entity entity)
        {
            if (entity.UniqueId == Owner.UniqueId)
                return;

            if (entity is Player)
                Players.TryAdd(entity.UniqueId, (Player)entity);

            Entities.TryAdd(entity.UniqueId, entity);
        }

        public void Remove(Entity entity)
        {
            Entities.TryRemove(entity.UniqueId, out var _);
            Players.TryRemove(entity.UniqueId, out var _);
        }

        public void Send(byte[] buffer, bool sendToOwner = false)
        {
            foreach (var kvp in Players)
                kvp.Value.Send(buffer);

            if(Owner is Player p)
                p.Send(buffer);
        }
    }
}