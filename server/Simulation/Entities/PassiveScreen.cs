using iogame.Simulation.Managers;

namespace iogame.Simulation.Entities
{
    public class PassiveScreen : Screen
    {
        public PassiveScreen(ShapeEntity owner) : base(owner)
        {

        }
        public override void Add(ShapeEntity entity, bool spawnPacket)
        {
            if (entity.EntityId == Owner.EntityId)
                return;
            if (!World.EntityExists(entity.EntityId) || !World.EntityExists(Owner.EntityId))
                return; // bandaid

            if (entity is Player p)
                Players.TryAdd(entity.EntityId, p);

            Entities.TryAdd(entity.EntityId, entity);
        }

        public override void Remove(ShapeEntity entity)
        {
            Entities.Remove(entity.EntityId, out var _);
            Players.Remove(entity.EntityId, out var _);
        }
    }
}
