using System.Numerics;
using System.Security.Cryptography.X509Certificates;
using iogame.Net.Packets;
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

        public virtual void Update()
        {
        }
        public virtual void Add(ShapeEntity entity, bool spawnPacket)
        {
        }

        public virtual void Remove(ShapeEntity entity)
        {
            Entities.Remove(entity.EntityId, out var _);

            if (!entity.CanSee(Owner) && entity.Viewport.Contains(Owner))
                entity.Viewport.Remove(Owner);

            entity.DespawnFor(Owner);
            Players.Remove(entity.EntityId, out var _); // needs to be at the bottom
        }

        public void Clear()
        {
            foreach (var kvp in Entities)
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
