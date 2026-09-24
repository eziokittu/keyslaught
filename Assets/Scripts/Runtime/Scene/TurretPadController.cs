using UnityEngine;

namespace KeySlaught.SceneGameplay
{
    public enum TurretKind
    {
        None,
        Teacher,
        Engineer,
        Scientist
    }

    public sealed class TurretPadController : MonoBehaviour
    {
        [SerializeField] private Vector3Int cellPosition;
        [SerializeField] private SpriteRenderer turretRenderer;
        [SerializeField] private Sprite teacherSprite;
        [SerializeField] private Sprite engineerSprite;
        [SerializeField] private Sprite scientistSprite;
        [SerializeField] private GameplaySceneCoordinator coordinator;
        [SerializeField] private GameplayCombatController combat;
        [SerializeField] private TurretDefinition[] definitions;
        [SerializeField] private LineRenderer shotLine;
        [SerializeField] private bool preview;

        private float attackTimer;
        private float shotTimer;

        public Vector3Int CellPosition => cellPosition;

        public TurretKind Kind { get; private set; }

        public int Level { get; private set; }
        public bool IsPreview => preview;

        public void SetPreview(bool value) => preview = value;

        public void Configure(
            Vector3Int cell,
            SpriteRenderer renderer,
            Sprite teacher,
            Sprite engineer,
            Sprite scientist,
            GameplaySceneCoordinator runtimeCoordinator = null,
            GameplayCombatController combatController = null,
            TurretDefinition[] turretDefinitions = null)
        {
            cellPosition = cell;
            turretRenderer = renderer;
            teacherSprite = teacher;
            engineerSprite = engineer;
            scientistSprite = scientist;
            coordinator = runtimeCoordinator;
            combat = combatController;
            definitions = turretDefinitions;
            RefreshPresentation();
        }

        public bool Build(TurretKind kind)
        {
            if (Kind != TurretKind.None || kind == TurretKind.None)
            {
                return false;
            }

            Kind = kind;
            Level = 1;
            attackTimer = 0f;
            RefreshPresentation();
            return true;
        }

        public bool Upgrade()
        {
            if (Kind == TurretKind.None || Level >= 3)
            {
                return false;
            }

            Level++;
            RefreshPresentation();
            return true;
        }

        public void ResetPad()
        {
            Kind = TurretKind.None;
            Level = 0;
            RefreshPresentation();
        }

        private void Awake()
        {
            RefreshPresentation();
        }

        public void Tick(float deltaSeconds)
        {
            if (shotLine != null && shotLine.enabled)
            {
                shotTimer -= Mathf.Max(0f, deltaSeconds);
                if (shotTimer <= 0f) shotLine.enabled = false;
            }
            if (preview || Kind == TurretKind.None || coordinator == null || coordinator.Spawner == null || combat == null) return;
            var definition = GetDefinition();
            if (definition == null) return;
            attackTimer -= Mathf.Max(0f, deltaSeconds);
            if (attackTimer > 0f) return;

            EnemyAgent best = null;
            foreach (var enemy in coordinator.Spawner.ActiveEnemies)
            {
                if (enemy == null || enemy.HasArrived || enemy.WordState == null || enemy.WordState.IsDefeated ||
                    enemy.WordState.NextLetter == null || !definition.Covers(enemy.WordState.NextLetter.Value) ||
                    Vector2.Distance(transform.position, enemy.transform.position) > definition.RangeAtLevel(Level)) continue;
                if (best == null || enemy.DistanceToLibrary < best.DistanceToLibrary ||
                    (Mathf.Approximately(enemy.DistanceToLibrary, best.DistanceToLibrary) && enemy.TieBreakOrder < best.TieBreakOrder)) best = enemy;
            }
            if (best == null) return;
            best.WordState.TryConsume(best.WordState.NextLetter.Value);
            ShowShot(best);
            combat.ResolveExternalDamage(best);
            attackTimer = definition.SecondsPerShotAtLevel(Level);
        }

        private void Update() => Tick(Time.deltaTime);

        private TurretDefinition GetDefinition()
        {
            if (definitions == null) return null;
            foreach (var definition in definitions)
                if (definition != null && definition.Kind == Kind) return definition;
            return null;
        }

        private void ShowShot(EnemyAgent enemy)
        {
            if (shotLine == null)
            {
                shotLine = gameObject.AddComponent<LineRenderer>();
                shotLine.positionCount = 2;
                shotLine.startWidth = 0.055f;
                shotLine.endWidth = 0.02f;
                shotLine.material = new Material(Shader.Find("Sprites/Default"));
                shotLine.startColor = Color.white;
                shotLine.endColor = new Color(1f, 1f, 1f, 0.1f);
                shotLine.sortingOrder = 30;
            }
            shotLine.SetPosition(0, transform.position);
            shotLine.SetPosition(1, enemy.transform.position);
            shotLine.enabled = true;
            shotTimer = 0.08f;
        }

        private void RefreshPresentation()
        {
            if (turretRenderer == null)
            {
                return;
            }

            switch (Kind)
            {
                case TurretKind.Teacher:
                    turretRenderer.sprite = teacherSprite;
                    break;
                case TurretKind.Engineer:
                    turretRenderer.sprite = engineerSprite;
                    break;
                case TurretKind.Scientist:
                    turretRenderer.sprite = scientistSprite;
                    break;
                default:
                    turretRenderer.sprite = null;
                    break;
            }

            turretRenderer.transform.localScale = Vector3.one * (1f + Mathf.Max(0, Level - 1) * 0.12f);
        }
    }
}
