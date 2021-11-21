using iogame.Simulation.Managers;

namespace iogame.Simulation.Entities
{
    public class BoidScreen : Screen
    {
        public BoidScreen(ShapeEntity owner) : base(owner)
        {

        }

        public override void Update(bool _ = false)
        {
            var list = CollisionDetection.Grid.GetObjects(Owner.Rect);
            Entities.Clear();
            Players.Clear();
            foreach (var entity in list)
                Add(entity, false);
        }
        public override void Add(ShapeEntity entity, bool spawnPacket)
        {
            if (entity.EntityId == Owner.EntityId)
                return;
            if (!PixelWorld.EntityExists(entity.EntityId) || !PixelWorld.EntityExists(Owner.EntityId))
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
