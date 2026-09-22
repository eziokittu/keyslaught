using System;
using System.Collections.Generic;
using UnityEngine;

namespace KeySlaught.SceneGameplay
{
    public static class PathMath
    {
        public static float CalculateLength(IReadOnlyList<Vector3> points)
        {
            Validate(points);

            var length = 0f;
            for (var index = 1; index < points.Count; index++)
            {
                length += Vector3.Distance(points[index - 1], points[index]);
            }

            return length;
        }

        public static Vector3 EvaluateDistance(IReadOnlyList<Vector3> points, float distance)
        {
            Validate(points);

            if (float.IsNaN(distance) || float.IsInfinity(distance))
            {
                throw new ArgumentOutOfRangeException(nameof(distance));
            }

            if (distance <= 0f)
            {
                return points[0];
            }

            var remaining = distance;
            for (var index = 1; index < points.Count; index++)
            {
                var start = points[index - 1];
                var end = points[index];
                var segmentLength = Vector3.Distance(start, end);

                if (segmentLength <= Mathf.Epsilon)
                {
                    continue;
                }

                if (remaining <= segmentLength)
                {
                    return Vector3.Lerp(start, end, remaining / segmentLength);
                }

                remaining -= segmentLength;
            }

            return points[points.Count - 1];
        }

        public static float DistanceRemaining(IReadOnlyList<Vector3> points, float travelledDistance)
        {
            return Mathf.Max(0f, CalculateLength(points) - Mathf.Max(0f, travelledDistance));
        }

        private static void Validate(IReadOnlyList<Vector3> points)
        {
            if (points == null)
            {
                throw new ArgumentNullException(nameof(points));
            }

            if (points.Count < 2)
            {
                throw new ArgumentException("A path needs at least two points.", nameof(points));
            }
        }
    }
}
