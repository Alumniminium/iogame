using System;
using System.Numerics;
using server.ECS;
using server.Enums;
using server.Simulation.Components;

namespace server.Simulation.Systems;

public unsafe sealed class AABBSystem : PixelSystem<AABBComponent, PhysicsComponent>
{
    public AABBSystem() : base("AABB System", threads: 1) { }
    protected override bool MatchesFilter(in PixelEntity ntt) => ntt.Type != EntityType.Pickable && ntt.Type != EntityType.Static && base.MatchesFilter(in ntt);

    public override void Update(in PixelEntity ntt, ref AABBComponent aabb, ref PhysicsComponent phy)
    {
        if (phy.LastPosition == phy.Position && phy.LastRotation == phy.RotationRadians)
            return;

        if (phy.AABBUpdateRequired)
        {
            if (phy.ShapeType != ShapeType.Circle)
            {
                Memory<Vector2> vertices = phy.GetTransformedVertices();

                var min = new Vector2(float.MaxValue);
                var max = new Vector2(float.MinValue);

                for (int i = 0; i < vertices.Length; i++)
                {
                    var v = vertices.Span[i];

                    if (v.X < min.X)
                        min.X = v.X;
                    else if (v.X > max.X)
                        max.X = v.X;
                    if (v.Y < min.Y)
                        min.Y = v.Y;
                    else if (v.Y > max.Y)
                        max.Y = v.Y;
                }

                aabb.AABB.X = min.X;
                aabb.AABB.Y = min.Y;
                aabb.AABB.Width = max.X - min.X;
                aabb.AABB.Height = max.Y - min.Y;
            }
            else
            {
                aabb.AABB.X = phy.Position.X - phy.Radius;
                aabb.AABB.Y = phy.Position.Y - phy.Radius;
                aabb.AABB.Width = phy.Radius * 2;
                aabb.AABB.Height = phy.Radius * 2;
            }
            phy.AABBUpdateRequired = false;
        }
        aabb.PotentialCollisions.Clear();
        Game.Grid.GetPotentialCollisions(ref aabb);
    }
}