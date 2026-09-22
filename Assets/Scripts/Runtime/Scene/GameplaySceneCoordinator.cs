using System.Collections.Generic;
using KeySlaught.Gameplay;
using UnityEngine;

namespace KeySlaught.SceneGameplay
{
    public sealed class GameplaySceneCoordinator : MonoBehaviour
    {
        [SerializeField] private PlayerMover player;
        [SerializeField] private EnemySpawner spawner;
        [SerializeField] private LibraryEndpoint library;
        [SerializeField, Min(0f)] private float playerAttackRange = 4f;

        public PlayerMover Player => player;

        public EnemySpawner Spawner => spawner;

        public LibraryEndpoint Library => library;

        public float PlayerAttackRange => playerAttackRange;

        public void Configure(
            PlayerMover playerMover,
            EnemySpawner enemySpawner,
            LibraryEndpoint libraryEndpoint,
            float attackRange)
        {
            if (isActiveAndEnabled && spawner != null)
            {
                spawner.EnemySpawned -= OnEnemySpawned;
            }

            player = playerMover;
            spawner = enemySpawner;
            library = libraryEndpoint;
            playerAttackRange = Mathf.Max(0f, attackRange);

            if (isActiveAndEnabled && spawner != null)
            {
                spawner.EnemySpawned += OnEnemySpawned;
            }
        }

        public List<EnemyTargetSnapshot> CreateTargetSnapshots()
        {
            var snapshots = new List<EnemyTargetSnapshot>();
            if (spawner == null || player == null)
            {
                return snapshots;
            }

            foreach (var enemy in spawner.ActiveEnemies)
            {
                if (enemy != null && !enemy.HasArrived)
                {
                    snapshots.Add(enemy.CreateTargetSnapshot(player.transform.position, playerAttackRange));
                }
            }

            return snapshots;
        }

        public EnemyAgent FindEnemy(EnemyWordState wordState)
        {
            if (wordState == null || spawner == null)
            {
                return null;
            }

            foreach (var enemy in spawner.ActiveEnemies)
            {
                if (enemy != null && ReferenceEquals(enemy.WordState, wordState))
                {
                    return enemy;
                }
            }

            return null;
        }

        public bool RemoveDefeatedEnemy(EnemyAgent enemy)
        {
            if (enemy == null || enemy.WordState == null || !enemy.WordState.IsDefeated)
            {
                return false;
            }

            enemy.ArrivedAtLibrary -= OnEnemyArrived;
            return spawner != null && spawner.Despawn(enemy);
        }

        private void OnEnable()
        {
            if (spawner != null)
            {
                spawner.EnemySpawned += OnEnemySpawned;
            }
        }

        private void OnDisable()
        {
            if (spawner != null)
            {
                spawner.EnemySpawned -= OnEnemySpawned;
            }
        }

        private void OnEnemySpawned(EnemyAgent enemy)
        {
            enemy.ArrivedAtLibrary += OnEnemyArrived;
        }

        private void OnEnemyArrived(EnemyAgent enemy)
        {
            enemy.ArrivedAtLibrary -= OnEnemyArrived;
            library.ReceiveEnemy(enemy);
            Destroy(enemy.gameObject);
        }

        private void OnDrawGizmosSelected()
        {
            if (player == null)
            {
                return;
            }

            Gizmos.color = new Color(0.15f, 0.9f, 1f, 0.55f);
            Gizmos.DrawWireSphere(player.transform.position, playerAttackRange);
        }
    }
}
