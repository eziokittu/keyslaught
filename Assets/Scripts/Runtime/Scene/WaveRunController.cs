using System;
using UnityEngine;
using UnityEngine.UI;

namespace KeySlaught.SceneGameplay
{
    public enum WaveRunPhase { Waiting, Spawning, Fighting, Intermission, Victory, Defeat }

    public sealed class WaveRunController : MonoBehaviour
    {
        [SerializeField] private EnemySpawner spawner;
        [SerializeField] private LibraryEndpoint library;
        [SerializeField] private BrainCellEconomy economy;
        [SerializeField] private GameplayCombatController combat;
        [SerializeField] private PlayerMover player;
        [SerializeField] private Transform turretRoot;
        [SerializeField] private LibraryAbilityController abilities;
        [SerializeField] private TileContextActionPanel contextActions;
        [SerializeField] private GameplayTopHud topHud;
        [SerializeField] private WaveDefinition[] waves;
        [SerializeField] private LevelDefinition activeLevel;
        [SerializeField, Min(0f)] private float intermissionSeconds = 30f;
        [SerializeField] private GameObject resultRoot;
        [SerializeField] private Text resultLabel;

        private int waveIndex = -1;
        private int nextEnemyIndex;
        private float timer;
        [SerializeField] private Vector3 playerStartPosition;
        private bool menuSuspended;
        private bool guidanceSuspended;
        private float elapsedSeconds;
        private int libraryHitCount;

        public event Action<int, bool> WaveStarted;
        public event Action<int> WaveCompleted;
        public event Action<bool> RunEnded;
        public WaveRunPhase Phase { get; private set; } = WaveRunPhase.Waiting;
        public int CurrentWaveNumber => Mathf.Max(0, waveIndex + 1);
        public bool IsBossWave => waveIndex >= 0 && waves != null && waveIndex < waves.Length && waves[waveIndex] != null && waves[waveIndex].IsBossWave;
        public float IntermissionRemaining => Phase == WaveRunPhase.Intermission ? Mathf.Max(0f, timer) : 0f;
        public LevelDefinition ActiveLevel => activeLevel;
        public float ElapsedSeconds => elapsedSeconds;
        public int LibraryHitCount => libraryHitCount;

        public void Configure(EnemySpawner enemySpawner, LibraryEndpoint endpoint, BrainCellEconomy runEconomy,
            GameplayCombatController combatController, PlayerMover playerMover, Transform placedTurrets,
            LibraryAbilityController abilityController, WaveDefinition[] waveDefinitions,
            TileContextActionPanel tileContext = null,
            GameObject runResultRoot = null, Text runResultLabel = null,
            GameplayTopHud gameplayHud = null)
        {
            if (isActiveAndEnabled && library != null) library.EnemyDamageReceived -= OnLibraryDamaged;
            spawner = enemySpawner;
            library = endpoint;
            economy = runEconomy;
            combat = combatController;
            player = playerMover;
            turretRoot = placedTurrets;
            abilities = abilityController;
            contextActions = tileContext;
            waves = waveDefinitions;
            resultRoot = runResultRoot;
            resultLabel = runResultLabel;
            topHud = gameplayHud;
            if (player != null) playerStartPosition = player.transform.position;
            spawner?.SetAutomaticSpawning(false);
            if (isActiveAndEnabled && library != null) library.EnemyDamageReceived += OnLibraryDamaged;
        }

        public void StartRun()
        {
            elapsedSeconds = 0f;
            libraryHitCount = 0;
            waveIndex = -1;
            BeginNextWave();
        }

        public void SetLevel(LevelDefinition level)
        {
            activeLevel = level;
            if (activeLevel != null) intermissionSeconds = activeLevel.IntermissionSeconds;
        }

        public void SkipIntermission()
        {
            if (Phase != WaveRunPhase.Intermission) return;
            timer = 0f;
            BeginNextWave();
        }

        public void Tick(float deltaSeconds)
        {
            if (menuSuspended || guidanceSuspended) return;
            if (Phase is WaveRunPhase.Victory or WaveRunPhase.Defeat) return;
            elapsedSeconds += Mathf.Max(0f, deltaSeconds);
            if (library != null && library.State != null && library.State.IsDestroyed)
            {
                EndRun(false);
                return;
            }

            timer -= Mathf.Max(0f, deltaSeconds);
            if (Phase == WaveRunPhase.Spawning && timer <= 0f) SpawnNextEnemy();
            if (Phase == WaveRunPhase.Fighting && spawner != null && spawner.ActiveEnemies.Count == 0) CompleteWave();
            if (Phase == WaveRunPhase.Intermission && timer <= 0f) BeginNextWave();
        }

        public void RestartRun()
        {
            Time.timeScale = 1f;
            spawner?.ClearAll();
            library?.ResetState();
            economy?.ResetState();
            combat?.ResetCombatState();
            abilities?.ResetState();
            contextActions?.ResetState();
            topHud?.ResetState();
            if (player != null) player.transform.position = playerStartPosition;
            if (turretRoot != null)
            {
                for (var index = turretRoot.childCount - 1; index >= 0; index--)
                    Destroy(turretRoot.GetChild(index).gameObject);
            }
            if (resultRoot != null) resultRoot.SetActive(false);
            StartRun();
        }

        public void SetMenuSuspended(bool suspended)
        {
            menuSuspended = suspended;
            if (player != null) player.enabled = !suspended;
            if (combat != null) combat.enabled = !suspended;
            spawner?.SetMovementMultiplier(suspended ? 0f : 1f);
        }

        public void SetGuidanceSuspended(bool suspended)
        {
            guidanceSuspended = suspended;
            spawner?.SetMovementMultiplier(suspended ? 0f : 1f);
        }

        private void Start()
        {
            if (Phase == WaveRunPhase.Waiting) StartRun();
        }

        private void Update() => Tick(Time.deltaTime);

        private void OnEnable()
        {
            if (library != null) library.EnemyDamageReceived += OnLibraryDamaged;
        }

        private void OnDisable()
        {
            if (library != null) library.EnemyDamageReceived -= OnLibraryDamaged;
        }

        private void OnLibraryDamaged(int damage)
        {
            if (damage > 0) libraryHitCount++;
        }

        private void BeginNextWave()
        {
            waveIndex++;
            if (waveIndex >= WaveCount)
            {
                EndRun(true);
                return;
            }
            nextEnemyIndex = 0;
            timer = 0f;
            Phase = WaveRunPhase.Spawning;
            WaveStarted?.Invoke(CurrentWaveNumber, IsBossWave);
        }

        private void SpawnNextEnemy()
        {
            if (activeLevel != null)
            {
                var authoredWave = activeLevel.Waves[waveIndex];
                if (authoredWave == null || authoredWave.Words == null || nextEnemyIndex >= authoredWave.Words.Length)
                {
                    Phase = WaveRunPhase.Fighting;
                    return;
                }
                var entry = authoredWave.Words[nextEnemyIndex++];
                if (entry != null) spawner.SpawnWord(entry.Word, authoredWave.EnemyMovementSpeed);
                timer = entry == null ? 0f : entry.DelayAfterPrevious;
                if (nextEnemyIndex >= authoredWave.Words.Length) Phase = WaveRunPhase.Fighting;
                return;
            }
            var wave = waves[waveIndex];
            if (wave == null || wave.Enemies == null || nextEnemyIndex >= wave.Enemies.Length)
            {
                Phase = WaveRunPhase.Fighting;
                return;
            }
            spawner.Spawn(wave.Enemies[nextEnemyIndex++]);
            timer = wave.SpawnInterval;
            if (nextEnemyIndex >= wave.Enemies.Length) Phase = WaveRunPhase.Fighting;
        }

        private void CompleteWave()
        {
            economy?.CollectAll();
            WaveCompleted?.Invoke(CurrentWaveNumber);
            if (waveIndex >= WaveCount - 1) EndRun(true);
            else
            {
                Phase = WaveRunPhase.Intermission;
                timer = intermissionSeconds;
            }
        }

        private int WaveCount => activeLevel != null && activeLevel.Waves != null
            ? activeLevel.Waves.Length
            : waves?.Length ?? 0;

        private void EndRun(bool victory)
        {
            Phase = victory ? WaveRunPhase.Victory : WaveRunPhase.Defeat;
            spawner?.SetMovementMultiplier(0f);
            if (combat != null) combat.enabled = false;
            if (resultLabel != null) resultLabel.text = victory ? "LIBRARY DEFENDED" : "LIBRARY LOST";
            if (resultRoot != null) resultRoot.SetActive(true);
            RunEnded?.Invoke(victory);
        }
    }
}
