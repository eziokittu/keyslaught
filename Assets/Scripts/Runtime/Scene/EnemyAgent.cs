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
        private float movementMultiplier = 1f;
        private float movementSpeed;
        private SpriteRenderer cardRenderer;
        private bool pinnedToWorldPosition;

        public event Action<EnemyAgent> ArrivedAtLibrary;

        public EnemyWordState WordState { get; private set; }

        public EnemyDefinition Definition => definition;

        public float TravelledDistance => travelledDistance;

        public float DistanceToLibrary => path == null ? float.PositiveInfinity : path.DistanceRemaining(travelledDistance);

        public bool HasArrived => hasArrived;

        public long TieBreakOrder => tieBreakOrder;

        public void SetMovementMultiplier(float multiplier)
        {
            movementMultiplier = Mathf.Clamp(multiplier, -1f, 1f);
        }

        public void Initialize(EnemyDefinition enemyDefinition, WaypointPath waypointPath, long order)
        {
            definition = enemyDefinition ?? throw new ArgumentNullException(nameof(enemyDefinition));
            path = waypointPath ?? throw new ArgumentNullException(nameof(waypointPath));
            tieBreakOrder = order;
            WordState = new EnemyWordState(definition.Word);
            movementSpeed = definition.MovementSpeed;
            travelledDistance = 0f;
            hasArrived = false;
            initialized = true;
            pinnedToWorldPosition = false;
            transform.position = path.StartPosition;
            RefreshLabel();
        }

        public void InitializeWord(string word, float speed, WaypointPath waypointPath, long order)
        {
            path = waypointPath ?? throw new ArgumentNullException(nameof(waypointPath));
            tieBreakOrder = order;
            WordState = new EnemyWordState(NormalizeWord(word));
            movementSpeed = Mathf.Max(0.01f, speed);
            travelledDistance = 0f;
            hasArrived = false;
            initialized = true;
            pinnedToWorldPosition = false;
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

            if (pinnedToWorldPosition) return false;

            travelledDistance = Mathf.Clamp(
                travelledDistance + movementSpeed * movementMultiplier * deltaSeconds,
                0f,
                path.TotalLength);
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
                var remainingLength = WordState.RemainingWord.Length;
                var cardColor = EnemyLengthPalette.Evaluate(remainingLength);
                if (cardRenderer == null) cardRenderer = GetComponent<SpriteRenderer>();
                if (cardRenderer != null) cardRenderer.color = cardColor;
                wordLabel.color = EnemyLengthPalette.UseDarkText(remainingLength) ? new Color(.12f,.07f,.13f) : new Color(1f,.96f,.88f);
            }
        }

        public void PlaceAtPathDistance(float distance)
        {
            if (path == null) return;
            travelledDistance = Mathf.Clamp(distance, 0f, path.TotalLength);
            transform.position = path.EvaluateDistance(travelledDistance);
        }

        public void PinNearWorldPosition(Vector3 worldPosition)
        {
            if (path == null) return;
            travelledDistance = path.FindClosestDistance(worldPosition);
            transform.position = worldPosition;
            pinnedToWorldPosition = true;
        }

        public void SetBossPresentation()
        {
            transform.localScale = Vector3.one * 1.55f;
            if (cardRenderer == null) cardRenderer = GetComponent<SpriteRenderer>();
            if (cardRenderer != null) cardRenderer.color = new Color(1f, .33f, .42f, 1f);
            if (wordLabel != null)
            {
                wordLabel.fontSize = Mathf.Max(wordLabel.fontSize, 110);
                wordLabel.fontStyle = FontStyle.Bold;
            }
        }

        private static string NormalizeWord(string word)
        {
            var normalized = string.IsNullOrWhiteSpace(word) ? "BOOK" : word.Trim().ToUpperInvariant();
            return normalized.Length <= 64 ? normalized : normalized.Substring(0, 64);
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
