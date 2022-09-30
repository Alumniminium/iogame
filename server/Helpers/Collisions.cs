using System;
using System.Numerics;
using Packets.Enums;
using server.Simulation.Components;

namespace server.Helpers
{
    public static class Collisions
    {
        public static void PointSegmentDistance(Vector2 p, Vector2 a, Vector2 b, out float distanceSquared, out Vector2 cp)
        {
            Vector2 ab = b - a;
            Vector2 ap = p - a;

            float proj = Vector2.Dot(ap, ab);
            float abLenSq = ab.LengthSquared();
            float d = proj / abLenSq;

            if (d <= 0f)
                cp = a;
            else cp = d >= 1f ? b : a + (ab * d);

            distanceSquared = Vector2.DistanceSquared(p, cp);
        }
        public static void FindContactPoints(ref PhysicsComponent bodyA, ref PhysicsComponent bodyB, out Vector2 contact1, out Vector2 contact2, out int contactCount)
        {
            contact1 = Vector2.Zero;
            contact2 = Vector2.Zero;
            contactCount = 0;

            ShapeType shapeTypeA = bodyA.ShapeType;
            ShapeType shapeTypeB = bodyB.ShapeType;

            if (shapeTypeA is ShapeType.Box)
            {
                if (shapeTypeB is ShapeType.Box)
                {
                    Collisions.FindPolygonsContactPoints(ref bodyA,ref bodyB,out contact1, out contact2, out contactCount);
                }
                else if (shapeTypeB is ShapeType.Circle)
                {
                    Collisions.FindCirclePolygonContactPoint(ref bodyB, ref bodyA, out contact1);
                    contactCount = 1;
                }
            }
            else if (shapeTypeA is ShapeType.Circle)
            {
                if (shapeTypeB is ShapeType.Box)
                {
                    Collisions.FindCirclePolygonContactPoint(ref bodyA, ref bodyB, out contact1);
                    contactCount = 1;
                }
                else if (shapeTypeB is ShapeType.Circle)
                {
                    Collisions.FindCirclesContactPoint(ref bodyA, ref bodyB, out contact1);
                    contactCount = 1;
                }
            }
        }

        private static void FindCirclePolygonContactPoint(ref PhysicsComponent bodyA, ref PhysicsComponent bodyB,out Vector2 cp)
        {
            cp = Vector2.Zero;

            float minDistSq = float.MaxValue;

            for (int i = 0; i < bodyB.transformedVertices.Length; i++)
            {
                Vector2 va = bodyB.transformedVertices.Span[i];
                Vector2 vb = bodyB.transformedVertices.Span[(i + 1) % bodyB.transformedVertices.Length];

                PointSegmentDistance(bodyA.Position, va, vb, out float distSq, out Vector2 contact);

                if (distSq < minDistSq)
                {
                    minDistSq = distSq;
                    cp = contact;
                }
            }
        }

        private static void FindCirclesContactPoint(ref PhysicsComponent bodyA, ref PhysicsComponent bodyB, out Vector2 cp)
        {
            Vector2 ab = bodyB.Position - bodyA.Position;
            Vector2 dir = Vector2.Normalize(ab);
            cp = bodyA.Position + dir * bodyA.Radius;
        }

        public static void FindPolygonsContactPoints(ref PhysicsComponent phyA, ref PhysicsComponent phyB, out Vector2 contact1, out Vector2 contact2, out int contactCount)
        {
            var verticesA = phyA.transformedVertices;
            var verticesB = phyB.transformedVertices;

            contact1 = Vector2.Zero;
            contact2 = Vector2.Zero;
            contactCount = 0;

            float minDistSq = float.MaxValue;

            for (int i = 0; i < verticesA.Length; i++)
            {
                var p = verticesA.Span[i];

                for (int j = 0; j < verticesB.Length; j++)
                {
                    var va = verticesB.Span[j];
                    var vb = verticesB.Span[(j + 1) % verticesB.Length];

                    PointSegmentDistance(p, va, vb, out float distSq, out Vector2 cp);

                    if (NearlyEqual(distSq, minDistSq))
                    {
                        if (!NearlyEqual(cp, contact1))
                        {
                            contact2 = cp;
                            contactCount = 2;
                        }
                    }
                    else if (distSq < minDistSq)
                    {
                        minDistSq = distSq;
                        contactCount = 1;
                        contact1 = cp;
                    }
                }
            }

            for (int i = 0; i < verticesB.Length; i++)
            {
                var p = verticesB.Span[i];

                for (int j = 0; j < verticesA.Length; j++)
                {
                    var va = verticesA.Span[j];
                    var vb = verticesA.Span[(j + 1) % verticesA.Length];

                    PointSegmentDistance(p, va, vb, out float distSq, out Vector2 cp);

                    if (NearlyEqual(distSq, minDistSq))
                    {
                        if (!NearlyEqual(cp, contact1))
                        {
                            contact2 = cp;
                            contactCount = 2;
                        }
                    }
                    else if (distSq < minDistSq)
                    {
                        minDistSq = distSq;
                        contactCount = 1;
                        contact1 = cp;
                    }
                }
            }
        }

        private static bool NearlyEqual(float a, float b) => MathF.Abs(a - b) < 0.0005f;
        public static bool NearlyEqual(Vector2 a, Vector2 b) => Vector2.DistanceSquared(a, b) < 0.0005f * 0.0005f;

        public static bool Collide(ref PhysicsComponent bodyA, ref PhysicsComponent bodyB, float aShieldRadius, float bShieldRadius, out Vector2 normal, out float depth)
        {
            normal = Vector2.Zero;
            depth = 0f;

            ShapeType shapeTypeA = bodyA.ShapeType;
            ShapeType shapeTypeB = bodyB.ShapeType;
            var aRadius = aShieldRadius == 0 ? bodyA.Radius : aShieldRadius;
            var bRadius = bShieldRadius == 0 ? bodyB.Radius : bShieldRadius;

            if (shapeTypeA is ShapeType.Box && aRadius == 0)
            {
                if (shapeTypeB is ShapeType.Box && bShieldRadius == 0)
                    return IntersectPolygons(bodyA.Position, bodyA.GetTransformedVertices(), bodyB.Position, bodyB.GetTransformedVertices(), out normal, out depth);
                else if (shapeTypeB is ShapeType.Circle || bRadius > 0)
                {
                    bool result = IntersectCirclePolygon(bodyB.Position, bRadius, bodyA.Position, bodyA.GetTransformedVertices(), out normal, out depth);
                    normal = -normal;
                    return result;
                }
            }
            else if (shapeTypeA is ShapeType.Circle || aRadius > 0)
            {
                if (shapeTypeB is ShapeType.Box && bRadius == 0)
                    return IntersectCirclePolygon(bodyA.Position, aRadius, bodyB.Position, bodyB.GetTransformedVertices(), out normal, out depth);
                else if (shapeTypeB is ShapeType.Circle || bRadius > 0)
                    return IntersectCircles(bodyA.Position, aRadius, bodyB.Position, bRadius, out normal, out depth);
            }

            return false;
        }

        public static bool IntersectCirclePolygon(Vector2 circleCenter, float circleRadius, Vector2 polygonCenter, Memory<Vector2> vertices, out Vector2 normal, out float depth)
        {
            normal = Vector2.Zero;
            depth = float.MaxValue;

            Vector2 axis;
            float axisDepth;
            float minA, maxA, minB, maxB;

            for (int i = 0; i < vertices.Length; i++)
            {
                Vector2 va = vertices.Span[i];
                Vector2 vb = vertices.Span[(i + 1) % vertices.Length];

                Vector2 edge = vb - va;
                axis = new Vector2(-edge.Y, edge.X);
                axis = Vector2.Normalize(axis);

                ProjectVertices(vertices, axis, out minA, out maxA);
                ProjectCircle(circleCenter, circleRadius, axis, out minB, out maxB);

                if (minA >= maxB || minB >= maxA)
                    return false;

                axisDepth = MathF.Min(maxB - minA, maxA - minB);

                if (axisDepth < depth)
                {
                    depth = axisDepth;
                    normal = axis;
                }
            }

            var cpIndex = FindClosestPointOnPolygon(circleCenter, vertices);
            if (cpIndex == -1)
                return false;

            var cp = vertices.Span[cpIndex];

            axis = cp - circleCenter;
            axis = Vector2.Normalize(axis);

            ProjectVertices(vertices, axis, out minA, out maxA);
            ProjectCircle(circleCenter, circleRadius, axis, out minB, out maxB);

            if (minA >= maxB || minB >= maxA)
                return false;

            axisDepth = MathF.Min(maxB - minA, maxA - minB);

            if (axisDepth < depth)
            {
                depth = axisDepth;
                normal = axis;
            }

            var direction = polygonCenter - circleCenter;

            if (Vector2.Dot(direction, normal) < 0f)
                normal = -normal;

            return true;
        }

        private static int FindClosestPointOnPolygon(Vector2 circleCenter, Memory<Vector2> vertices)
        {
            int result = -1;
            float minDistance = float.MaxValue;

            for (int i = 0; i < vertices.Length; i++)
            {
                Vector2 v = vertices.Span[i];
                float distance = Vector2.Distance(v, circleCenter);

                if (distance < minDistance)
                {
                    minDistance = distance;
                    result = i;
                }
            }

            return result;
        }

        private static void ProjectCircle(Vector2 center, float radius, Vector2 axis, out float min, out float max)
        {
            Vector2 direction = Vector2.Normalize(axis);
            Vector2 directionAndRadius = direction * radius;

            Vector2 p1 = center + directionAndRadius;
            Vector2 p2 = center - directionAndRadius;

            min = Vector2.Dot(p1, axis);
            max = Vector2.Dot(p2, axis);

            if (min > max)
                (max, min) = (min, max);
        }

        public static bool IntersectPolygons(Vector2 centerA, Memory<Vector2> verticesA, Vector2 centerB, Memory<Vector2> verticesB, out Vector2 normal, out float depth)
        {
            normal = Vector2.Zero;
            depth = float.MaxValue;

            for (int i = 0; i < verticesA.Length; i++)
            {
                Vector2 va = verticesA.Span[i];
                Vector2 vb = verticesA.Span[(i + 1) % verticesA.Length];

                Vector2 edge = vb - va;
                Vector2 axis = new(-edge.Y, edge.X);
                axis = Vector2.Normalize(axis);

                ProjectVertices(verticesA, axis, out float minA, out float maxA);
                ProjectVertices(verticesB, axis, out float minB, out float maxB);

                if (minA >= maxB || minB >= maxA)
                    return false;

                float axisDepth = MathF.Min(maxB - minA, maxA - minB);

                if (axisDepth < depth)
                {
                    depth = axisDepth;
                    normal = axis;
                }
            }

            for (int i = 0; i < verticesB.Length; i++)
            {
                Vector2 va = verticesB.Span[i];
                Vector2 vb = verticesB.Span[(i + 1) % verticesB.Length];

                Vector2 edge = vb - va;
                Vector2 axis = new(-edge.Y, edge.X);
                axis = Vector2.Normalize(axis);

                ProjectVertices(verticesA, axis, out float minA, out float maxA);
                ProjectVertices(verticesB, axis, out float minB, out float maxB);

                if (minA >= maxB || minB >= maxA)
                    return false;

                float axisDepth = MathF.Min(maxB - minA, maxA - minB);

                if (axisDepth < depth)
                {
                    depth = axisDepth;
                    normal = axis;
                }
            }

            Vector2 direction = centerB - centerA;

            if (Vector2.Dot(direction, normal) < 0f)
                normal = -normal;

            return true;
        }

        private static void ProjectVertices(Memory<Vector2> vertices, Vector2 axis, out float min, out float max)
        {
            min = float.MaxValue;
            max = float.MinValue;

            for (int i = 0; i < vertices.Length; i++)
            {
                Vector2 v = vertices.Span[i];
                float proj = Vector2.Dot(v, axis);

                if (proj < min) { min = proj; }
                if (proj > max) { max = proj; }
            }
        }

        public static bool IntersectCircles(Vector2 centerA, float radiusA, Vector2 centerB, float radiusB, out Vector2 normal, out float depth)
        {
            normal = Vector2.Zero;
            depth = 0f;

            float distance = Vector2.Distance(centerA, centerB);
            float radii = radiusA + radiusB;

            if (distance >= radii)
                return false;

            normal = Vector2.Normalize(centerB - centerA);
            depth = radii - distance;

            return true;
        }
    }
}