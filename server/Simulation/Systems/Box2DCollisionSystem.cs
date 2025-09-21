using System;
using System.Numerics;
using Box2D.NET;
using server.ECS;
using server.Simulation.Components;
using static Box2D.NET.B2Worlds;
using static Box2D.NET.B2Shapes;
using static Box2D.NET.B2Bodies;

namespace server.Simulation.Systems;

public sealed class Box2DCollisionSystem : NttSystem
{
    public Box2DCollisionSystem() : base("Box2D Collision System", threads: 1) { }

    protected override void Update(int start, int amount)
    {
        // Process Box2D collision events once per frame (not per entity)
        // Only process on the first thread call
        if (start == 0)
        {
            ProcessCollisionEvents();
        }
    }

    private void ProcessCollisionEvents()
    {
        // Use contact events from Box2D
        var contactEvents = b2World_GetContactEvents(Box2DPhysicsWorld.WorldId);

        // Always log to see if we're getting here
        Console.WriteLine($"🔍 Box2DCollisionSystem checking events: {contactEvents.beginCount} begin, {contactEvents.endCount} end");

        for (int i = 0; i < contactEvents.beginCount; i++)
        {
            var beginEvent = contactEvents.beginEvents[i];
            ProcessCollisionBegin(beginEvent);
        }
    }

    private void ProcessCollisionBegin(B2ContactBeginTouchEvent touchEvent)
    {
        var bodyA = b2Shape_GetBody(touchEvent.shapeIdA);
        var bodyB = b2Shape_GetBody(touchEvent.shapeIdB);

        // Find entities corresponding to these Box2D bodies
        var entityA = FindEntityByBodyId(bodyA);
        var entityB = FindEntityByBodyId(bodyB);

        if (entityA.HasValue && entityB.HasValue)
        {
            var nttA = entityA.Value;
            var nttB = entityB.Value;

            // Debug what types are colliding
            var typeA = nttA.Has<BulletComponent>() ? "Bullet" : nttA.Has<NetworkComponent>() ? "Player" : "Other";
            var typeB = nttB.Has<BulletComponent>() ? "Bullet" : nttB.Has<NetworkComponent>() ? "Player" : "Other";
            Console.WriteLine($"💥 Collision: {typeA} vs {typeB}");

            // Add collision component to both entities
            // Box2D collision filtering should already prevent bullet-owner collisions
            AddCollisionToEntity(nttA, nttB, touchEvent.manifold);
            AddCollisionToEntity(nttB, nttA, touchEvent.manifold);
        }
        else
        {
            Console.WriteLine($"⚠️ Could not find entities for collision");
        }
    }


    private void AddCollisionToEntity(NTT entity, NTT otherEntity, B2Manifold manifold)
    {
        CollisionComponent collision;

        if (entity.Has<CollisionComponent>())
        {
            collision = entity.Get<CollisionComponent>();
        }
        else
        {
            collision = new CollisionComponent(entity);
            entity.Set(ref collision);
        }

        // Convert Box2D manifold to our collision data
        var contactPoint = new Vector2(manifold.normal.X, manifold.normal.Y);
        var penetration = manifold.pointCount > 0 ? manifold.points[0].separation : 0f;

        collision.Collisions.Add((otherEntity, contactPoint, penetration));
    }

    private NTT? FindEntityByBodyId(B2BodyId bodyId)
    {
        // Search through all entities with Box2DBodyComponent to find matching bodyId
        foreach (var entity in NttWorld.NTTs.Values)
        {
            if (entity.Has<Box2DBodyComponent>())
            {
                var body = entity.Get<Box2DBodyComponent>();
                if (body.BodyId.index1 == bodyId.index1)
                {
                    return entity;
                }
            }
        }
        return null;
    }
}