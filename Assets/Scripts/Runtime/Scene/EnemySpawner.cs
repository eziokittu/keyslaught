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

            var enemy = Instantiate(enemyPrefab, path.StartPosition, Quaternion.identity, spawnRoot);
            enemy.name = $"Enemy_{nextSpawnOrder:000}_{definition.Word}";
            enemy.Initialize(definition, path, nextSpawnOrder++);
            enemy.ArrivedAtLibrary += OnEnemyArrived;
            activeEnemies.Add(enemy);
            EnemySpawned?.Invoke(enemy);
            return enemy;
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
