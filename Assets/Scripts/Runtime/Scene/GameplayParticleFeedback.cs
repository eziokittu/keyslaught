using UnityEngine;

namespace KeySlaught.SceneGameplay
{
    public sealed class GameplayParticleFeedback : MonoBehaviour
    {
        [SerializeField] private PlayerMover player;
        [SerializeField] private GameplayCombatController combat;
        [SerializeField] private LibraryEndpoint library;
        [SerializeField] private WaveRunController run;
        private ParticleSystem particles;
        private float movementTimer;

        public void Configure(PlayerMover mover, GameplayCombatController controller, LibraryEndpoint endpoint, WaveRunController waveRun)
        {
            Unsubscribe();
            player = mover; combat = controller; library = endpoint; run = waveRun;
            EnsureSystem();
            Subscribe();
        }

        private void Awake() => EnsureSystem();
        private void OnEnable() => Subscribe();
        private void OnDisable() => Unsubscribe();

        private void Update()
        {
            if (player == null || player.LastMovementInput.sqrMagnitude < .02f) return;
            movementTimer -= Time.deltaTime;
            if (movementTimer > 0f) return;
            movementTimer = .055f;
            Emit(player.transform.position - (Vector3)player.LastMovementInput.normalized * .28f, new Color(.24f, .9f, 1f, .9f), 2, .12f, .22f);
        }

        private void OnTargetHit(EnemyAgent enemy)
        {
            if (enemy != null) Emit(enemy.transform.position, new Color(1f, .82f, .28f, 1f), 10, .7f, .16f);
        }

        private void OnLibraryDamaged(int damage)
        {
            if (damage > 0 && library != null) Emit(library.transform.position + Vector3.up * .5f, new Color(1f, .22f, .2f, 1f), 18, 1.05f, .24f);
        }

        private void OnRunEnded(bool victory)
        {
            var origin = player == null ? transform.position : player.transform.position;
            Emit(origin, victory ? new Color(.32f, 1f, .58f, 1f) : new Color(.75f, .24f, 1f, 1f), 55, 2.4f, .3f);
        }

        private void Emit(Vector3 position, Color color, int count, float speed, float size)
        {
            EnsureSystem();
            particles.transform.position = position;
            var emit = new ParticleSystem.EmitParams { startColor = color, startSize = size, startLifetime = .55f, velocity = Vector3.zero };
            for (var i = 0; i < count; i++)
            {
                var direction = Random.insideUnitCircle.normalized;
                emit.velocity = new Vector3(direction.x, direction.y, 0f) * Random.Range(speed * .45f, speed);
                particles.Emit(emit, 1);
            }
        }

        private void EnsureSystem()
        {
            if (particles != null) return;
            var child = new GameObject("Gameplay Particles");
            child.transform.SetParent(transform, false);
            particles = child.AddComponent<ParticleSystem>();
            var main = particles.main;
            main.playOnAwake = false;
            main.loop = false;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.startLifetime = .55f;
            main.startSpeed = 0f;
            main.maxParticles = 300;
            var emission = particles.emission;
            emission.enabled = false;
            var renderer = particles.GetComponent<ParticleSystemRenderer>();
            renderer.sortingOrder = 60;
            renderer.material = new Material(Shader.Find("Sprites/Default"));
        }

        private void Subscribe()
        {
            if (combat != null) { combat.TargetHit -= OnTargetHit; combat.TargetHit += OnTargetHit; }
            if (library != null) { library.EnemyDamageReceived -= OnLibraryDamaged; library.EnemyDamageReceived += OnLibraryDamaged; }
            if (run != null) { run.RunEnded -= OnRunEnded; run.RunEnded += OnRunEnded; }
        }

        private void Unsubscribe()
        {
            if (combat != null) combat.TargetHit -= OnTargetHit;
            if (library != null) library.EnemyDamageReceived -= OnLibraryDamaged;
            if (run != null) run.RunEnded -= OnRunEnded;
        }
    }
}
