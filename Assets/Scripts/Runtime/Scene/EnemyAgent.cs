using System;
using KeySlaught.Gameplay;
using UnityEngine;

namespace KeySlaught.SceneGameplay
{
    public sealed class EnemyAgent : MonoBehaviour
    {
        [SerializeField] private EnemyDefinition definition;
        [SerializeField] private WaypointPath path;
        [SerializeField] private TextMesh wordLabel;

        private bool initialized;
        private bool hasArrived;
        private float travelledDistance;
        private long tieBreakOrder;

        public event Action<EnemyAgent> ArrivedAtLibrary;

        public EnemyWordState WordState { get; private set; }

        public EnemyDefinition Definition => definition;

        public float TravelledDistance => travelledDistance;

        public float DistanceToLibrary => path == null ? float.PositiveInfinity : path.DistanceRemaining(travelledDistance);

        public bool HasArrived => hasArrived;

        public long TieBreakOrder => tieBreakOrder;

        public void Initialize(EnemyDefinition enemyDefinition, WaypointPath waypointPath, long order)
        {
            definition = enemyDefinition ?? throw new ArgumentNullException(nameof(enemyDefinition));
            path = waypointPath ?? throw new ArgumentNullException(nameof(waypointPath));
            tieBreakOrder = order;
            WordState = new EnemyWordState(definition.Word);
            travelledDistance = 0f;
            hasArrived = false;
            initialized = true;
            transform.position = path.StartPosition;
            RefreshLabel();
        }

        public bool Advance(float deltaSeconds)
        {
            if (!initialized || hasArrived)
            {
                return false;
            }

            if (deltaSeconds < 0f || float.IsNaN(deltaSeconds) || float.IsInfinity(deltaSeconds))
            {
                throw new ArgumentOutOfRangeException(nameof(deltaSeconds));
            }

            travelledDistance = Mathf.Min(
                path.TotalLength,
                travelledDistance + definition.MovementSpeed * deltaSeconds);
            transform.position = path.EvaluateDistance(travelledDistance);

            if (travelledDistance < path.TotalLength)
            {
                return false;
            }

            hasArrived = true;
            ArrivedAtLibrary?.Invoke(this);
            return true;
        }

        public EnemyTargetSnapshot CreateTargetSnapshot(Vector3 playerPosition, float attackRange)
        {
            var inRange = Vector2.Distance(playerPosition, transform.position) <= attackRange;
            return new EnemyTargetSnapshot(
                WordState,
                inRange,
                DistanceToLibrary,
                tieBreakOrder);
        }

        public void RefreshLabel()
        {
            if (wordLabel != null && WordState != null)
            {
                wordLabel.text = WordState.NextLetter?.ToString() ?? string.Empty;
            }
        }

        private void Start()
        {
            if (!initialized && definition != null && path != null)
            {
                Initialize(definition, path, tieBreakOrder);
            }
        }

        private void Update()
        {
            Advance(Time.deltaTime);
        }
    }
}
