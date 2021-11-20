using iogame.Simulation.Managers;

namespace iogame.Simulation.Entities
{
    public class PlayerScreen : Screen
    {
        public PlayerScreen(ShapeEntity owner) : base(owner) { }

        public override void Update(bool syncNet = false)
        {
            var list = CollisionDetection.Grid.GetObjects(Owner.Rect);
            foreach (var entity in Entities)
            {
                if (list.Contains(entity.Value))
                    continue;
                Remove(entity.Value);
            }
            foreach (var entity in list)
            {
                if (Entities.ContainsKey(entity.EntityId) || entity.EntityId == Owner.EntityId)
                {
                    if (entity.PositionComponent.LastPosition != entity.PositionComponent.Position)
                        if (syncNet)
                            entity.MoveFor(Owner);
                }
                else
                    Add(entity, syncNet);
            }
        }
        public override void Add(ShapeEntity entity, bool spawnPacket)
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

            if (!entity.Viewport.Contains(Owner))
                entity.Viewport.Add(Owner, true);
        }

        public override void Remove(ShapeEntity entity)
        {
            Entities.Remove(entity.EntityId, out var _);

            if (!entity.CanSee(Owner) && entity.Viewport.Contains(Owner))
                entity.Viewport.Remove(Owner);

            entity.DespawnFor(Owner);
            Players.Remove(entity.EntityId, out var _); // needs to be at the bottom
        }
    }
}
