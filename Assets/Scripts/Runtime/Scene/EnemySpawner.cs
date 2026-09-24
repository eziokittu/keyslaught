using System;
using System.Collections.Generic;
using UnityEngine;

namespace KeySlaught.SceneGameplay
{
    public sealed class EnemySpawner : MonoBehaviour
    {
        [SerializeField] private EnemyAgent enemyPrefab;
        [SerializeField] private WaypointPath path;
        [SerializeField] private EnemyDefinition[] definitions;
        [SerializeField] private Transform spawnRoot;
        [SerializeField] private bool spawnOnStart = true;
        [SerializeField, Min(0.1f)] private float spawnInterval = 4f;

        private readonly List<EnemyAgent> activeEnemies = new List<EnemyAgent>();
        private float spawnTimer;
        private int definitionIndex;
        private long nextSpawnOrder;
        private float movementMultiplier = 1f;

        public event Action<EnemyAgent> EnemySpawned;

        public IReadOnlyList<EnemyAgent> ActiveEnemies => activeEnemies;

        public void Configure(
            EnemyAgent prefab,
            WaypointPath waypointPath,
            EnemyDefinition[] enemyDefinitions,
            Transform parent = null)
        {
            enemyPrefab = prefab;
            path = waypointPath;
            definitions = enemyDefinitions;
            spawnRoot = parent;
        }

        public void SetAutomaticSpawning(bool enabled)
        {
            spawnOnStart = enabled;
            spawnTimer = enabled ? 0f : spawnInterval;
        }

        public EnemyAgent SpawnNext()
        {
            if (enemyPrefab == null || path == null || definitions == null || definitions.Length == 0)
            {
                throw new InvalidOperationException($"{name} is missing its prefab, path, or definitions.");
            }

            var definition = definitions[definitionIndex % definitions.Length];
            definitionIndex++;

            return Spawn(definition);
        }

        public EnemyAgent Spawn(EnemyDefinition definition)
        {
            if (enemyPrefab == null || path == null || definition == null)
            {
                throw new InvalidOperationException($"{name} is missing its prefab, path, or enemy definition.");
            }

            var enemy = Instantiate(enemyPrefab, path.StartPosition, Quaternion.identity, spawnRoot);
            enemy.name = $"Enemy_{nextSpawnOrder:000}_{definition.Word}";
            enemy.Initialize(definition, path, nextSpawnOrder++);
            enemy.SetMovementMultiplier(movementMultiplier);
            enemy.ArrivedAtLibrary += OnEnemyArrived;
            activeEnemies.Add(enemy);
            EnemySpawned?.Invoke(enemy);
            return enemy;
        }

        public EnemyAgent SpawnWord(string word, float movementSpeed)
        {
            if (enemyPrefab == null || path == null)
                throw new InvalidOperationException($"{name} is missing its prefab or path.");
            var enemy = Instantiate(enemyPrefab, path.StartPosition, Quaternion.identity, spawnRoot);
            enemy.name = $"Enemy_{nextSpawnOrder:000}_{word}";
            enemy.InitializeWord(word, movementSpeed, path, nextSpawnOrder++);
            enemy.SetMovementMultiplier(movementMultiplier);
            enemy.ArrivedAtLibrary += OnEnemyArrived;
            activeEnemies.Add(enemy);
            EnemySpawned?.Invoke(enemy);
            return enemy;
        }

        public void SetMovementMultiplier(float multiplier)
        {
            movementMultiplier = Mathf.Clamp(multiplier, -1f, 1f);
            foreach (var enemy in activeEnemies)
                if (enemy != null) enemy.SetMovementMultiplier(movementMultiplier);
        }

        public void ClearAll()
        {
            for (var index = activeEnemies.Count - 1; index >= 0; index--)
            {
                var enemy = activeEnemies[index];
                if (enemy != null)
                {
                    enemy.ArrivedAtLibrary -= OnEnemyArrived;
                    Destroy(enemy.gameObject);
                }
            }
            activeEnemies.Clear();
            definitionIndex = 0;
            nextSpawnOrder = 0;
            movementMultiplier = 1f;
        }

        public bool Despawn(EnemyAgent enemy)
        {
            if (enemy == null || !activeEnemies.Remove(enemy))
            {
                return false;
            }

            enemy.ArrivedAtLibrary -= OnEnemyArrived;
            Destroy(enemy.gameObject);
            return true;
        }

        private void Start()
        {
            spawnTimer = 0f;
            if (spawnOnStart)
            {
                SpawnNext();
                spawnTimer = spawnInterval;
            }
        }

        private void Update()
        {
            if (!spawnOnStart)
            {
                return;
            }

            spawnTimer -= Time.deltaTime;
            if (spawnTimer <= 0f)
            {
                SpawnNext();
                spawnTimer = spawnInterval;
            }
        }

        private void OnEnemyArrived(EnemyAgent enemy)
        {
            enemy.ArrivedAtLibrary -= OnEnemyArrived;
            activeEnemies.Remove(enemy);
        }
    }
}
