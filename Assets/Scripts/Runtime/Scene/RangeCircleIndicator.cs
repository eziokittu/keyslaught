using UnityEngine;

namespace KeySlaught.SceneGameplay
{
    public sealed class RangeCircleIndicator : MonoBehaviour
    {
        [SerializeField] private GameplaySceneCoordinator coordinator;
        [SerializeField] private TurretPadController turret;
        [SerializeField] private PlayerMover player;
        [SerializeField] private bool alwaysVisible;
        [SerializeField, Min(24)] private int segments = 72;

        private LineRenderer circle;
        private Material circleMaterial;

        public void ConfigurePlayer(GameplaySceneCoordinator sceneCoordinator)
        {
            coordinator = sceneCoordinator;
            player = sceneCoordinator == null ? null : sceneCoordinator.Player;
            turret = null;
            alwaysVisible = false;
            EnsureCircle();
            RefreshGeometry();
        }

        public void ConfigureTurret(TurretPadController turretController, PlayerMover playerMover)
        {
            turret = turretController;
            player = playerMover;
            coordinator = null;
            alwaysVisible = false;
            EnsureCircle();
            RefreshGeometry();
        }

        private void Awake() => EnsureCircle();

        private void Update()
        {
            EnsureCircle();
            if (circle == null) return;
            var playerRangeVisible = coordinator != null && player != null && player.LastMovementInput.sqrMagnitude > 0.01f;
            var visible = playerRangeVisible || alwaysVisible || (turret != null && !turret.IsPreview && player != null &&
                Vector2.Distance(player.transform.position, transform.position) <= 0.72f);
            circle.enabled = visible;
            if (!visible) return;
            RefreshGeometry();
            var pulse = 0.34f + (Mathf.Sin(Time.unscaledTime * 2.8f) + 1f) * 0.18f;
            var color = alwaysVisible ? new Color(.20f, .96f, 1f, pulse) : new Color(1f, .78f, .26f, pulse + .12f);
            circle.startColor = color;
            circle.endColor = color;
        }

        private void EnsureCircle()
        {
            if (circle != null) return;
            var child = new GameObject("Attack Range Circle");
            child.transform.SetParent(transform, false);
            circle = child.AddComponent<LineRenderer>();
            circle.useWorldSpace = false;
            circle.loop = true;
            circle.positionCount = Mathf.Max(24, segments);
            circle.startWidth = .025f;
            circle.endWidth = .025f;
            circle.sortingOrder = 8;
            circleMaterial = new Material(Shader.Find("Sprites/Default"));
            circle.material = circleMaterial;
        }

        private void RefreshGeometry()
        {
            if (circle == null) return;
            var radius = coordinator != null ? coordinator.PlayerAttackRange : turret == null ? 0f : turret.CurrentRange;
            for (var index = 0; index < circle.positionCount; index++)
            {
                var angle = index * Mathf.PI * 2f / circle.positionCount;
                circle.SetPosition(index, new Vector3(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius, 0f));
            }
        }

        private void OnDestroy()
        {
            if (circleMaterial != null) Destroy(circleMaterial);
        }
    }
}
