using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
using server.ECS;
using server.Enums;
using server.Helpers;
using server.Simulation.Components;

namespace server.Simulation.Systems;

public unsafe sealed class NarrowPhaseSystem : NttSystem<PhysicsComponent, AABBComponent>
{
    // Stability constants
    private const float ResponseCoefficient = 0.8f; // Reduced from 1.0 to prevent overcorrection
    private const float Epsilon = 0.0001f;
    private const int SubSteps = 8; // Increased iterations for better stability
    private const float Baumgarte = 0.2f; // Position correction factor (0.1-0.3 typical)
    private const float MaxPenetration = 0.05f; // Maximum penetration to correct per frame
    private const float VelocityDamping = 0.999f; // Slight damping to prevent jitter
    private const float SleepThreshold = 0.1f; // Velocity threshold for sleeping
    private const float AngularDamping = 0.98f; // Angular velocity damping

    // Batch processing buffers for vectorization
    private const int BatchSize = 8; // Process 8 collisions at once with AVX
    private readonly float[] batchPositionsX = new float[BatchSize * 2];
    private readonly float[] batchPositionsY = new float[BatchSize * 2];
    private readonly float[] batchRadii = new float[BatchSize * 2];
    private readonly bool[] batchResults = new bool[BatchSize];

    public NarrowPhaseSystem() : base("Narrow Phase Collision", threads: 1) { }
    protected override bool MatchesFilter(in NTT ntt) => base.MatchesFilter(in ntt);

    public override void Update(in NTT a, ref PhysicsComponent bodyA, ref AABBComponent aabb)
    {
        if (bodyA.LastPosition == bodyA.Position)
            return;
        if (bodyA.Static)
            return;


        for (var k = 0; k < aabb.PotentialCollisions.Count; k++)
        {
            if (aabb.PotentialCollisions[k].Id == Guid.Empty)
                continue;

            ref readonly var b = ref Unsafe.AsRef(aabb.PotentialCollisions[k]);

            if (b.Id == a.Id)
                continue;

            ref var bodyB = ref b.Get<PhysicsComponent>();

            var aShieldRadius = 0f;
            var bShieldRadius = 0f;

            if (a.Has<ShieldComponent>())
            {
                ref readonly var shi = ref a.Get<ShieldComponent>();
                if (shi.Charge > 0)
                    aShieldRadius = shi.Radius;
            }
            if (b.Has<ShieldComponent>())
            {
                ref readonly var shi = ref b.Get<ShieldComponent>();
                if (shi.Charge > 0)
                    bShieldRadius = shi.Radius;
            }

            if (a.Has<BulletComponent>() && b.Has<BulletComponent>())
            {
                ref readonly var bulletA = ref a.Get<BulletComponent>();
                ref readonly var bulletB = ref b.Get<BulletComponent>();

                if (bulletA.Owner.Id == bulletB.Owner.Id)
                    continue;
            }
            else if (a.Has<BulletComponent>())
            {
                ref readonly var bullet = ref a.Get<BulletComponent>();
                if (bullet.Owner.Id == b.Id)
                    continue;
            }
            else if (b.Has<BulletComponent>())
            {
                ref readonly var bullet = ref b.Get<BulletComponent>();
                if (bullet.Owner.Id == a.Id)
                    continue;
            }

            // Perform sub-stepped collision resolution for stability
            for (int subStep = 0; subStep < SubSteps; subStep++)
            {
                if (SolveContact(in a, in b, ref bodyA, ref bodyB, aShieldRadius, bShieldRadius))
                {
                    // Update grid positions after contact resolution
                    if (bodyA.Position != bodyA.LastPosition)
                    {
                        bodyA.ChangedTick = NttWorld.Tick;
                        bodyA.TransformUpdateRequired = true;
                        bodyA.AABBUpdateRequired = true;
                        Game.Grid.Move(in a, ref bodyA);
                    }
                    if (bodyB.Position != bodyB.LastPosition)
                    {
                        bodyB.ChangedTick = NttWorld.Tick;
                        bodyB.TransformUpdateRequired = true;
                        bodyB.AABBUpdateRequired = true;
                        Game.Grid.Move(in b, ref bodyB);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Optimized contact resolution inspired by C++ physics solver.
    /// Returns true if collision was resolved, false otherwise.
    /// </summary>
    private bool SolveContact(in NTT a, in NTT b, ref PhysicsComponent bodyA, ref PhysicsComponent bodyB,
        float aShieldRadius, float bShieldRadius)
    {
        var aRadius = MathF.Max(bodyA.Radius, aShieldRadius);
        var bRadius = MathF.Max(bodyB.Radius, bShieldRadius);

        // Quick circle-circle collision for most common case
        if (bodyA.ShapeType == ShapeType.Circle && bodyB.ShapeType == ShapeType.Circle)
        {
            return SolveCircleContact(in a, in b, ref bodyA, ref bodyB, aRadius, bRadius);
        }

        // Fall back to full collision detection for complex shapes
        if (Collisions.Collide(ref bodyA, ref bodyB, aRadius, bRadius, out Vector2 normal, out float depth))
        {
            return SolveComplexContact(in a, in b, ref bodyA, ref bodyB, aRadius, bRadius, normal, depth);
        }

        return false;
    }

    /// <summary>
    /// Batch process multiple circle collisions using SSE vectorization
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void BatchDetectCollisionsAVX(Span<float> positionsX, Span<float> positionsY,
        Span<float> radii, Span<bool> results, int count)
    {
        // Simplified vectorization using SSE instead of AVX for better compatibility
        for (int i = 0; i < count; i += 2)
        {
            if (i + 1 < count && Sse.IsSupported)
            {
                // Process 2 collision pairs with SSE
                var xA = Vector128.Create(positionsX[i * 2], positionsX[i * 2], 0, 0);
                var yA = Vector128.Create(positionsY[i * 2], positionsY[i * 2], 0, 0);
                var xB = Vector128.Create(positionsX[i * 2 + 1], positionsX[i * 2 + 1], 0, 0);
                var yB = Vector128.Create(positionsY[i * 2 + 1], positionsY[i * 2 + 1], 0, 0);

                var dx = Sse.Subtract(xA, xB);
                var dy = Sse.Subtract(yA, yB);
                var dx2 = Sse.Multiply(dx, dx);
                var dy2 = Sse.Multiply(dy, dy);
                var dist2 = Sse.Add(dx2, dy2);

                var rA = Vector128.Create(radii[i * 2], radii[i * 2], 0, 0);
                var rB = Vector128.Create(radii[i * 2 + 1], radii[i * 2 + 1], 0, 0);
                var rSum = Sse.Add(rA, rB);
                var rSum2 = Sse.Multiply(rSum, rSum);

                var collision = Sse.CompareLessThan(dist2, rSum2);
                results[i] = collision.GetElement(0) != 0;
                if (i + 1 < count)
                    results[i + 1] = collision.GetElement(1) != 0;
            }
            else
            {
                // Fallback to scalar
                for (int idx = 0; idx < count; idx++)
                {
                    var dx = positionsX[idx * 2] - positionsX[idx * 2 + 1];
                    var dy = positionsY[idx * 2] - positionsY[idx * 2 + 1];
                    var dist2 = dx * dx + dy * dy;
                    var rSum = radii[idx * 2] + radii[idx * 2 + 1];
                    results[idx] = dist2 < rSum * rSum;
                }
            }
        }
    }

    /// <summary>
    /// Vectorized circle-circle collision detection using SIMD
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool DetectCircleCollisionSIMD(Vector2 posA, Vector2 posB, float radiusA, float radiusB,
        out Vector2 normal, out float penetrationDepth)
    {
        // Use Vector128 for SIMD operations on position differences
        var posA_vec = Vector128.Create(posA.X, posA.Y, 0, 0);
        var posB_vec = Vector128.Create(posB.X, posB.Y, 0, 0);

        var diff = Sse.Subtract(posA_vec, posB_vec);
        var diff_squared = Sse.Multiply(diff, diff);

        // Sum components for distance squared
        var dist2 = diff_squared.GetElement(0) + diff_squared.GetElement(1);
        var radiusSum = radiusA + radiusB;
        var radiusSumSq = radiusSum * radiusSum;

        if (dist2 < radiusSumSq && dist2 > Epsilon)
        {
            var dist = MathF.Sqrt(dist2);
            normal = new Vector2(diff.GetElement(0) / dist, diff.GetElement(1) / dist);
            penetrationDepth = radiusSum - dist;
            return true;
        }

        normal = Vector2.Zero;
        penetrationDepth = 0;
        return false;
    }

    /// <summary>
    /// Optimized circle-circle collision resolution with proper impulse response
    /// </summary>
    private bool SolveCircleContact(in NTT a, in NTT b, ref PhysicsComponent bodyA, ref PhysicsComponent bodyB, float aRadius, float bRadius)
    {
        // Try vectorized detection first if SSE is available
        if (Sse.IsSupported && DetectCircleCollisionSIMD(bodyA.Position, bodyB.Position, aRadius, bRadius, out var normal, out var penetrationDepth))
        {
            // Continue with resolution
        }
        else
        {
            // Fallback to scalar version
            var o2_o1 = bodyA.Position - bodyB.Position;
            var dist2 = o2_o1.X * o2_o1.X + o2_o1.Y * o2_o1.Y;
            var radiusSum = aRadius + bRadius;

            if (!(dist2 < radiusSum * radiusSum && dist2 > Epsilon))
                return false;

            var dist = MathF.Sqrt(dist2);
            normal = o2_o1 / dist;
            penetrationDepth = radiusSum - dist;
        }

        // Limit maximum correction to prevent instability
        var correctionMagnitude = MathF.Min(penetrationDepth * ResponseCoefficient * 0.5f, MaxPenetration);
        var colVec = normal * correctionMagnitude;

        // Handle parent entities
        var entityA = a;
        var entityB = b;

        bodyA = ref entityA.Get<PhysicsComponent>();
        bodyB = ref entityB.Get<PhysicsComponent>();

        // Apply position corrections based on mass and entity type
        if (bodyA.Static)
            bodyB.Position -= colVec;
        else if (bodyB.Static)
            bodyA.Position += colVec;
        else
        {
            // Mass-based separation (more stable than impulse for position correction)
            var massRatio = bodyA.Mass / (bodyA.Mass + bodyB.Mass);
            bodyA.Position += colVec * (1 - massRatio);
            bodyB.Position -= colVec * massRatio;
        }

        // Calculate contact point for circle-circle collision
        var contactPoint = bodyA.Position + normal * aRadius;

        // Apply impulse with angular momentum and stabilization
        float e = MathF.Min(bodyA.Elasticity, bodyB.Elasticity);
        ApplyCircleImpulse(ref bodyA, ref bodyB, normal, contactPoint, e, penetrationDepth, 1f / 30f);

        return true;
    }

    /// <summary>
    /// Complex collision resolution for non-circle shapes
    /// </summary>
    private bool SolveComplexContact(in NTT a, in NTT b, ref PhysicsComponent bodyA, ref PhysicsComponent bodyB,
        float aRadius, float bRadius, Vector2 normal, float depth)
    {
        Collisions.FindContactPoints(ref bodyA, ref bodyB, aRadius, bRadius, out Vector2 contact1, out Vector2 contact2, out int contactCount);
        var penetration = normal * depth;

        // Handle parent entities
        var entityA = a;
        var entityB = b;

        bodyA = ref entityA.Get<PhysicsComponent>();
        bodyB = ref entityB.Get<PhysicsComponent>();

        // Position separation
        if (bodyA.Static)
        {
            bodyB.Position += penetration;
        }
        else if (bodyB.Static)
        {
            bodyA.Position -= penetration;
        }
        else
        {
            var massRatio = bodyA.Mass / (bodyA.Mass + bodyB.Mass);
            bodyA.Position -= penetration * (1 - massRatio);
            bodyB.Position += penetration * massRatio;
        }

        // Apply impulse for velocity changes with angular momentum
        float e = MathF.Min(bodyA.Elasticity, bodyB.Elasticity);
        ApplyComplexImpulse(ref bodyA, ref bodyB, normal, e, contact1, contact2, contactCount);

        return true;
    }

    /// <summary>
    /// Impulse application with angular momentum and stabilization for circle-circle collisions
    /// </summary>
    private void ApplyCircleImpulse(ref PhysicsComponent bodyA, ref PhysicsComponent bodyB, Vector2 normal, Vector2 contactPoint, float elasticity, float penetrationDepth = 0, float deltaTime = 0.033f)
    {
        // Calculate relative positions from contact point
        var ra = contactPoint - bodyA.Position;
        var rb = contactPoint - bodyB.Position;

        // Calculate perpendicular vectors for angular velocity contribution
        var raPerp = new Vector2(-ra.Y, ra.X);
        var rbPerp = new Vector2(-rb.Y, rb.X);

        // Calculate velocities at contact point including angular components
        var angularVelocityA = raPerp * bodyA.AngularVelocity;
        var angularVelocityB = rbPerp * bodyB.AngularVelocity;

        // Use vectorized calculation if available
        var totalVelA = bodyA.LinearVelocity + angularVelocityA;
        var totalVelB = bodyB.LinearVelocity + angularVelocityB;

        var relativeVelocity = totalVelB - totalVelA;
        var contactVelocityMagnitude = Vector2.Dot(relativeVelocity, normal);

        // Add Baumgarte stabilization bias to help resolve penetration
        var bias = 0f;
        if (penetrationDepth > 0.01f) // Only apply bias for significant penetration
        {
            bias = Baumgarte * penetrationDepth / deltaTime;
        }

        // Skip if bodies are separating fast enough (accounting for bias)
        if (contactVelocityMagnitude > bias)
            return;

        // Calculate denominator including rotational inertia
        var raPerpDotN = Vector2.Dot(raPerp, normal);
        var rbPerpDotN = Vector2.Dot(rbPerp, normal);

        var denom = bodyA.InvMass + bodyB.InvMass +
                   raPerpDotN * raPerpDotN * bodyA.InvInertia +
                   rbPerpDotN * rbPerpDotN * bodyB.InvInertia;

        // Calculate impulse magnitude with bias
        float j = -(1 + elasticity) * contactVelocityMagnitude / denom;

        // Add bias impulse for position correction
        j += bias / denom;

        var impulse = normal * j;

        // Apply impulse to linear and angular velocities with damping
        if (bodyA.Static)
        {
            bodyA.LinearVelocity -= impulse * bodyA.InvMass;
            bodyA.AngularVelocity -= (ra.X * impulse.Y - ra.Y * impulse.X) * bodyA.InvInertia;

            // Apply damping to prevent jitter
            bodyA.LinearVelocity *= VelocityDamping;
            bodyA.AngularVelocity *= AngularDamping;

            // Sleep detection for very slow objects
            if (bodyA.LinearVelocity.LengthSquared() < SleepThreshold * SleepThreshold)
            {
                bodyA.LinearVelocity *= 0.9f; // Extra damping for nearly stopped objects
            }
        }
        if (bodyB.Static)
        {
            bodyB.LinearVelocity += impulse * bodyB.InvMass;
            bodyB.AngularVelocity += (rb.X * impulse.Y - rb.Y * impulse.X) * bodyB.InvInertia;

            // Apply damping to prevent jitter
            bodyB.LinearVelocity *= VelocityDamping;
            bodyB.AngularVelocity *= AngularDamping;

            // Sleep detection for very slow objects
            if (bodyB.LinearVelocity.LengthSquared() < SleepThreshold * SleepThreshold)
            {
                bodyB.LinearVelocity *= 0.9f; // Extra damping for nearly stopped objects
            }
        }
    }

    /// <summary>
    /// Proper impulse application with angular momentum for complex shapes
    /// </summary>
    private void ApplyComplexImpulse(ref PhysicsComponent bodyA, ref PhysicsComponent bodyB,
        Vector2 normal, float elasticity,
        Vector2 contact1, Vector2 contact2, int contactCount)
    {
        var impulseList = stackalloc Vector2[2];
        var raList = stackalloc Vector2[2];
        var rbList = stackalloc Vector2[2];
        var contactList = stackalloc Vector2[2];

        contactList[0] = contact1;
        contactList[1] = contact2;

        // Calculate impulses for each contact point
        for (int i = 0; i < contactCount; i++)
        {
            var ra = contactList[i] - bodyA.Position;
            var rb = contactList[i] - bodyB.Position;

            raList[i] = ra;
            rbList[i] = rb;

            var raPerp = new Vector2(-ra.Y, ra.X);
            var rbPerp = new Vector2(-rb.Y, rb.X);

            var angularVelocityA = raPerp * bodyA.AngularVelocity;
            var angularVelocityB = rbPerp * bodyB.AngularVelocity;

            var relativeVelocity = bodyB.LinearVelocity + angularVelocityB - bodyA.LinearVelocity - angularVelocityA;
            var contactVelocityMagnitude = Vector2.Dot(relativeVelocity, normal);

            if (contactVelocityMagnitude > 0)
                continue;

            var raPerpDotN = Vector2.Dot(raPerp, normal);
            var rbPerpDotN = Vector2.Dot(rbPerp, normal);

            var denom = bodyA.InvMass + bodyB.InvMass +
                       raPerpDotN * raPerpDotN * bodyA.InvInertia +
                       rbPerpDotN * rbPerpDotN * bodyB.InvInertia;

            float j = -(1 + elasticity) * contactVelocityMagnitude / denom;
            j /= contactCount;

            impulseList[i] = normal * j;
        }

        // Apply impulses with proper angular momentum changes
        for (int i = 0; i < contactCount; i++)
        {
            var impulse = impulseList[i];
            var ra = raList[i];
            var rb = rbList[i];

            if (bodyA.Static)
            {
                bodyA.LinearVelocity -= impulse * bodyA.InvMass;
                bodyA.AngularVelocity -= (ra.X * impulse.Y - ra.Y * impulse.X) * bodyA.InvInertia;

                // Apply damping for stability
                bodyA.LinearVelocity *= VelocityDamping;
                bodyA.AngularVelocity *= AngularDamping;
            }
            if (bodyB.Static)
            {
                bodyB.LinearVelocity += impulse * bodyB.InvMass;
                bodyB.AngularVelocity += (rb.X * impulse.Y - rb.Y * impulse.X) * bodyB.InvInertia;

                // Apply damping for stability
                bodyB.LinearVelocity *= VelocityDamping;
                bodyB.AngularVelocity *= AngularDamping;
            }
        }
    }
}