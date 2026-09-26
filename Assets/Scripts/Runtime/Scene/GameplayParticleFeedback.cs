using UnityEngine;

namespace KeySlaught.SceneGameplay
{
    public sealed class GameplayParticleFeedback : MonoBehaviour
    {
        [SerializeField] private PlayerMover player;
        [SerializeField] private GameplayCombatController combat;
        [SerializeField] private LibraryEndpoint library;
        [SerializeField] private WaveRunController run;
        private ParticleSystem cloudParticles;
        private ParticleSystem impactParticles;
        private static Texture2D cloudTexture;
        private static Texture2D splinterTexture;
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
            Emit(cloudParticles, player.transform.position - (Vector3)player.LastMovementInput.normalized * .28f,
                new Color(.72f, .78f, .82f, .58f), 2, .13f, .105f, .34f);
        }

        private void OnTargetHit(EnemyAgent enemy)
        {
            if (enemy != null) Emit(impactParticles, enemy.transform.position, new Color(1f, .78f, .3f, .9f), 7, .62f, .075f, .38f);
        }

        private void OnLibraryDamaged(int damage)
        {
            if (damage > 0 && library != null) Emit(impactParticles, library.transform.position + Vector3.up * .45f,
                new Color(.55f, .28f, .11f, .92f), 13, .78f, .09f, .52f);
        }

        private void OnRunEnded(bool victory)
        {
            var origin = player == null ? transform.position : player.transform.position;
            Emit(cloudParticles, origin, victory ? new Color(.45f, .95f, .69f, .82f) : new Color(.66f, .42f, .72f, .76f), 42, 1.8f, .16f, .8f);
        }

        private void Emit(ParticleSystem system, Vector3 position, Color color, int count, float speed, float size, float lifetime)
        {
            EnsureSystem();
            if (system == null) return;
            system.transform.position = position;
            var emit = new ParticleSystem.EmitParams { startColor = color, startSize = size, startLifetime = lifetime, velocity = Vector3.zero };
            for (var i = 0; i < count; i++)
            {
                var direction = Random.insideUnitCircle.normalized;
                emit.velocity = new Vector3(direction.x, direction.y, 0f) * Random.Range(speed * .45f, speed);
                emit.rotation = Random.Range(-180f, 180f);
                system.Emit(emit, 1);
            }
        }

        private void EnsureSystem()
        {
            if (cloudParticles != null && impactParticles != null) return;
            cloudTexture ??= CreateCloudTexture();
            splinterTexture ??= CreateSplinterTexture();
            cloudParticles = CreateSystem("Soft Movement Clouds", cloudTexture, 60);
            impactParticles = CreateSystem("Themed Impact Flecks", splinterTexture, 61);
        }

        private ParticleSystem CreateSystem(string objectName, Texture2D texture, int sortingOrder)
        {
            var existing = transform.Find(objectName);
            var child = existing == null ? new GameObject(objectName) : existing.gameObject;
            child.transform.SetParent(transform, false);
            var system = child.GetComponent<ParticleSystem>();
            if (system == null) system = child.AddComponent<ParticleSystem>();
            var main = system.main;
            main.playOnAwake = false;
            main.loop = false;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.startLifetime = .55f;
            main.startSpeed = 0f;
            main.maxParticles = 300;
            main.startSize3D = false;
            var emission = system.emission;
            emission.enabled = false;
            var renderer = system.GetComponent<ParticleSystemRenderer>();
            renderer.sortingOrder = sortingOrder;
            renderer.renderMode = ParticleSystemRenderMode.Billboard;
            var material = new Material(Shader.Find("Sprites/Default"));
            material.mainTexture = texture;
            renderer.material = material;
            return system;
        }

        private static Texture2D CreateCloudTexture()
        {
            const int size = 32;
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false) { name = "KeySlaught Soft Cloud Particle" };
            texture.hideFlags = HideFlags.HideAndDontSave;
            var centers = new[] { new Vector2(11f, 17f), new Vector2(17f, 19f), new Vector2(21f, 15f), new Vector2(15f, 13f) };
            for (var y = 0; y < size; y++)
            for (var x = 0; x < size; x++)
            {
                var alpha = 0f;
                foreach (var center in centers)
                {
                    var normalized = Mathf.Clamp01(1f - Vector2.Distance(new Vector2(x, y), center) / 8f);
                    alpha = Mathf.Max(alpha, normalized * normalized);
                }
                texture.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
            }
            texture.Apply();
            return texture;
        }

        private static Texture2D CreateSplinterTexture()
        {
            const int size = 32;
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false) { name = "KeySlaught Wood Fleck Particle" };
            texture.hideFlags = HideFlags.HideAndDontSave;
            for (var y = 0; y < size; y++)
            for (var x = 0; x < size; x++)
            {
                var dx = Mathf.Abs(x - 15.5f) / 3.2f;
                var dy = Mathf.Abs(y - 15.5f) / 11f;
                var alpha = Mathf.Clamp01(1f - Mathf.Max(dx, dy));
                texture.SetPixel(x, y, new Color(1f, 1f, 1f, alpha * alpha));
            }
            texture.Apply();
            return texture;
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
