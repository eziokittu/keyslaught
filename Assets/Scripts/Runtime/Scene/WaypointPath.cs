using UnityEngine;

namespace KeySlaught.SceneGameplay
{
    public sealed class WaypointPath : MonoBehaviour
    {
        [SerializeField] private Transform[] waypoints;
        [SerializeField] private LineRenderer pathRenderer;

        public int WaypointCount => waypoints == null ? 0 : waypoints.Length;

        public Transform[] Waypoints => waypoints;

        public float TotalLength => PathMath.CalculateLength(GetPositions());

        public Vector3 StartPosition => EvaluateDistance(0f);

        public Vector3 EndPosition => EvaluateDistance(TotalLength);

        public Vector3 EvaluateDistance(float distance)
        {
            return PathMath.EvaluateDistance(GetPositions(), distance);
        }

        public float DistanceRemaining(float travelledDistance)
        {
            return PathMath.DistanceRemaining(GetPositions(), travelledDistance);
        }

        public float FindClosestDistance(Vector3 worldPosition)
        {
            var positions = GetPositions();
            var bestDistance = 0f;
            var bestSquared = float.PositiveInfinity;
            var accumulated = 0f;
            for (var index = 1; index < positions.Length; index++)
            {
                var start = positions[index - 1];
                var end = positions[index];
                var segment = end - start;
                var length = segment.magnitude;
                if (length <= Mathf.Epsilon) continue;
                var t = Mathf.Clamp01(Vector3.Dot(worldPosition - start, segment) / (length * length));
                var point = start + segment * t;
                var squared = (worldPosition - point).sqrMagnitude;
                if (squared < bestSquared)
                {
                    bestSquared = squared;
                    bestDistance = accumulated + length * t;
                }
                accumulated += length;
            }
            return bestDistance;
        }

        public void Configure(Transform[] orderedWaypoints, LineRenderer renderer = null)
        {
            waypoints = orderedWaypoints;
            pathRenderer = renderer;
            RefreshPreview();
        }

        public void RefreshPreview()
        {
            if (pathRenderer == null || waypoints == null)
            {
                return;
            }

            pathRenderer.positionCount = waypoints.Length;
            for (var index = 0; index < waypoints.Length; index++)
            {
                if (waypoints[index] != null)
                {
                    pathRenderer.SetPosition(index, waypoints[index].position);
                }
            }
        }

        private Vector3[] GetPositions()
        {
            if (waypoints == null || waypoints.Length < 2)
            {
                throw new System.InvalidOperationException(
                    $"{name} needs at least two waypoint references.");
            }

            var positions = new Vector3[waypoints.Length];
            for (var index = 0; index < waypoints.Length; index++)
            {
                if (waypoints[index] == null)
                {
                    throw new System.InvalidOperationException(
                        $"{name} has an empty waypoint reference at index {index}.");
                }

                positions[index] = waypoints[index].position;
            }

            return positions;
        }

        public Vector3[] CopyPositions() => GetPositions();

        private void OnValidate()
        {
            RefreshPreview();
        }

        private void OnDrawGizmos()
        {
            if (waypoints == null || waypoints.Length < 2)
            {
                return;
            }

            Gizmos.color = new Color(0.95f, 0.75f, 0.25f, 0.8f);
            for (var index = 1; index < waypoints.Length; index++)
            {
                if (waypoints[index - 1] != null && waypoints[index] != null)
                {
                    Gizmos.DrawLine(waypoints[index - 1].position, waypoints[index].position);
                }
            }
        }
    }
}
