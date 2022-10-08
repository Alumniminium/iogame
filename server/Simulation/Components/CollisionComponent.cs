using System.Collections.Generic;
using System.Numerics;
using Packets.Enums;
using server.ECS;

namespace server.Simulation.Components
{
    [Component]
    public readonly struct CollisionComponent
    {
        public readonly EntityType EntityTypes;
        public readonly PixelEntity A;
        public readonly List<(PixelEntity,Vector2,float)> Collisions;

        public CollisionComponent(PixelEntity a)
        {
            A = a;
            EntityTypes = a.Type;
            Collisions = new List<(PixelEntity, Vector2, float)>(5);
        }

        public override int GetHashCode() => A.Id;
    }
}