using System.Numerics;
using server.Simulation.Components;

namespace server.Helpers
{
    public class Polygon
    {
        public struct PolygonCollisionResult
        {
            // Are the polygons going to intersect forward in time?
            public bool WillIntersect;
            // Are the polygons currently intersecting?
            public bool Intersect;
            // The translation to apply to the first polygon to push the polygons apart.
            public Vector2 MinimumTranslationVector;
        }

        public static PolygonCollisionResult PolygonCollision(ref PolygonComponent polygonA, ref PolygonComponent polygonB, Vector2 velocity)
        {
            PolygonCollisionResult result = new()
            {
                Intersect = true,
                WillIntersect = true
            };

            var edgeCountA = polygonA.Edges.Count;
            var edgeCountB = polygonB.Edges.Count;
            var minIntervalDistance = float.PositiveInfinity;

            Vector2 translationAxis = new();

            // Loop through all the edges of both polygons
            for (var edgeIndex = 0; edgeIndex < edgeCountA + edgeCountB; edgeIndex++)
            {
                Vector2 edge = edgeIndex < edgeCountA ? polygonA.Edges[edgeIndex] : polygonB.Edges[edgeIndex - edgeCountA];

                // ===== 1. Find if the polygons are currently intersecting =====

                // Find the axis perpendicular to the current edge
                var axis = Vector2.Normalize(new Vector2(-edge.Y, edge.X));

                // Find the projection of the polygon on the current axis
                float minA = 0;
                float minB = 0;
                float maxA = 0;
                float maxB = 0;
                ProjectPolygon(axis, ref polygonA, ref minA, ref maxA);
                ProjectPolygon(axis, ref polygonB, ref minB, ref maxB);

                // Check if the polygon projections are currentlty intersecting
                if (IntervalDistance(minA, maxA, minB, maxB) > 0)
                    result.Intersect = false;

                // ===== 2. Now find if the polygons *will* intersect =====

                // Project the velocity on the current axis
                var velocityProjection = Vector2.Dot(axis, velocity);

                // Get the projection of polygon A during the movement
                if (velocityProjection < 0)
                    minA += velocityProjection;
                else
                    maxA += velocityProjection;

                // Do the same test as above for the new projection
                var intervalDistance = IntervalDistance(minA, maxA, minB, maxB);
                if (intervalDistance > 0) result.WillIntersect = false;

                // If the polygons are not intersecting and won't intersect, exit the loop
                if (!result.Intersect && !result.WillIntersect) break;

                // Check if the current interval distance is the minimum one. If so store
                // the interval distance and the current distance.
                // This will be used to calculate the minimum translation vector
                intervalDistance = Math.Abs(intervalDistance);
                if (intervalDistance < minIntervalDistance)
                {
                    minIntervalDistance = intervalDistance;
                    translationAxis = axis;

                    var d = polygonA.Center() - polygonB.Center();
                    if (Vector2.Dot(d, translationAxis) < 0)
                        translationAxis = -translationAxis;
                }
            }

            // The minimum translation vector can be used to push the polygons appart.
            // First moves the polygons by their velocity
            // then move polygonA by MinimumTranslationVector.
            if (result.WillIntersect)
                result.MinimumTranslationVector = translationAxis * minIntervalDistance;

            return result;
        }

        // Calculate the distance between [minA, maxA] and [minB, maxB]
        // The distance will be negative if the intervals overlap
        public static float IntervalDistance(float minA, float maxA, float minB, float maxB)
        {
            return (minA < minB) ? minB - maxA : minA - maxB;
        }

        // Calculate the projection of a polygon on an axis and returns it as a [min, max] interval
        public static void ProjectPolygon(Vector2 axis, ref PolygonComponent polygon, ref float min, ref float max)
        {
            // To project a point on an axis use the dot product
            var d = Vector2.Dot(axis, polygon.Points[0]);
            min = d;
            max = d;
            for (var i = 0; i < polygon.Points.Count; i++)
            {
                d = Vector2.Dot(polygon.Points[i], axis);

                if (d <= max)
                    continue;

                if (d >= min)
                    max = d;
                else
                    min = d;
            }
        }
    }

}