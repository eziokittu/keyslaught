using System.Collections;
using System.Reflection;
using KeySlaught.Gameplay;
using KeySlaught.SceneGameplay;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace KeySlaught.Tests.PlayMode
{
    public sealed class SceneVerticalSliceTests
    {
        [UnityTest]
        public IEnumerator PlayerMovement_IsTransformDrivenAndClampedToArena()
        {
            var playerObject = new GameObject("Test Player");
            var mover = playerObject.AddComponent<PlayerMover>();
            mover.Configure(4f, new Vector2(-1f, -1f), new Vector2(1f, 1f));

            mover.ApplyMovement(Vector2.right, 0.125f);
            Assert.That(playerObject.transform.position.x, Is.EqualTo(0.5f).Within(0.0001f));

            mover.ApplyMovement(Vector2.right, 10f);
            Assert.That(playerObject.transform.position.x, Is.EqualTo(1f).Within(0.0001f));

            Object.Destroy(playerObject);
            yield return null;
        }

        [UnityTest]
        public IEnumerator PlayerMovement_VirtualInputIsClampedAndResettable()
        {
            var playerObject = new GameObject("Virtual Player");
            var mover = playerObject.AddComponent<PlayerMover>();

            mover.SetVirtualMovement(new Vector2(4f, 0f));
            Assert.That(mover.VirtualMovement, Is.EqualTo(Vector2.right));
            mover.SetVirtualMovement(Vector2.zero);
            Assert.That(mover.VirtualMovement, Is.EqualTo(Vector2.zero));

            Object.Destroy(playerObject);
            yield return null;
        }

        [UnityTest]
        public IEnumerator EnemyTraversal_FollowsAuthoredPathAndRaisesArrivalOnce()
        {
            var path = CreatePath(Vector3.zero, new Vector3(2f, 0f, 0f), new Vector3(2f, 2f, 0f));
            var definition = CreateDefinition("BOOK", 2f);
            var enemyObject = new GameObject("Test Enemy");
            var enemy = enemyObject.AddComponent<EnemyAgent>();
            var arrivals = 0;
            enemy.ArrivedAtLibrary += _ => arrivals++;
            enemy.Initialize(definition, path, 7);

            Assert.That(enemy.Advance(1f), Is.False);
            Assert.That(enemyObject.transform.position, Is.EqualTo(new Vector3(2f, 0f, 0f)));
            Assert.That(enemy.DistanceToLibrary, Is.EqualTo(2f).Within(0.0001f));

            Assert.That(enemy.Advance(1f), Is.True);
            Assert.That(enemy.HasArrived, Is.True);
            Assert.That(enemyObject.transform.position, Is.EqualTo(new Vector3(2f, 2f, 0f)));
            Assert.That(enemy.Advance(1f), Is.False);
            Assert.That(arrivals, Is.EqualTo(1));

            Object.Destroy(enemyObject);
            Object.Destroy(definition);
            Object.Destroy(path.gameObject);
            yield return null;
        }

        [UnityTest]
        public IEnumerator Spawner_InstantiatesConfiguredEnemyAtPathStart()
        {
            var path = CreatePath(new Vector3(-2f, 1f, 0f), Vector3.zero);
            var definition = CreateDefinition("INK", 1f);
            var prefabObject = new GameObject("Enemy Template");
            var prefab = prefabObject.AddComponent<EnemyAgent>();
            var spawnerObject = new GameObject("Test Spawner");
            var spawner = spawnerObject.AddComponent<EnemySpawner>();
            spawner.Configure(prefab, path, new[] { definition });
            spawner.SetAutomaticSpawning(false);

            var spawned = spawner.SpawnNext();

            Assert.That(spawned, Is.Not.SameAs(prefab));
            Assert.That(spawned.transform.position, Is.EqualTo(path.StartPosition));
            Assert.That(spawned.WordState.OriginalWord, Is.EqualTo("INK"));
            Assert.That(spawner.ActiveEnemies, Has.Count.EqualTo(1));

            Object.Destroy(spawned.gameObject);
            Object.Destroy(spawnerObject);
            Object.Destroy(prefabObject);
            Object.Destroy(definition);
            Object.Destroy(path.gameObject);
            yield return null;
        }

        [UnityTest]
        public IEnumerator Coordinator_AppliesArrivalDamageAndRemovesEnemy()
        {
            var path = CreatePath(Vector3.zero, Vector3.right);
            var definition = CreateDefinition("BOOK", 10f);
            var prefabObject = new GameObject("Enemy Template");
            var prefab = prefabObject.AddComponent<EnemyAgent>();
            var spawnerObject = new GameObject("Test Spawner");
            var spawner = spawnerObject.AddComponent<EnemySpawner>();
            spawner.Configure(prefab, path, new[] { definition });
            spawner.SetAutomaticSpawning(false);

            var playerObject = new GameObject("Test Player");
            var player = playerObject.AddComponent<PlayerMover>();
            var libraryObject = new GameObject("Test Library");
            var library = libraryObject.AddComponent<LibraryEndpoint>();
            library.Initialize(10);
            var coordinatorObject = new GameObject("Test Coordinator");
            var coordinator = coordinatorObject.AddComponent<GameplaySceneCoordinator>();
            coordinator.Configure(player, spawner, library, 3f);

            var enemy = spawner.SpawnNext();
            Assert.That(coordinator.CreateTargetSnapshots(), Has.Count.EqualTo(1));

            enemy.Advance(1f);
            yield return null;

            Assert.That(library.State.CurrentHealth, Is.EqualTo(6));
            Assert.That(spawner.ActiveEnemies, Is.Empty);
            Assert.That(enemy == null, Is.True);

            Object.Destroy(coordinatorObject);
            Object.Destroy(libraryObject);
            Object.Destroy(playerObject);
            Object.Destroy(spawnerObject);
            Object.Destroy(prefabObject);
            Object.Destroy(definition);
            Object.Destroy(path.gameObject);
            yield return null;
        }

        [UnityTest]
        public IEnumerator CombatController_HitsSequentiallyAndRemovesDefeatedEnemy()
        {
            var fixture = CreateCombatFixture("AB", 2);

            var first = fixture.Combat.TryTypeLetter('a');
            Assert.That(first.Outcome, Is.EqualTo(TypedAttackOutcome.TargetHit));
            Assert.That(fixture.Enemy.WordState.RemainingWord, Is.EqualTo("B"));

            var second = fixture.Combat.TryTypeLetter('B');
            Assert.That(second.Outcome, Is.EqualTo(TypedAttackOutcome.TargetHit));
            Assert.That(fixture.Spawner.ActiveEnemies, Is.Empty);

            fixture.Destroy();
            yield return null;
        }

        [UnityTest]
        public IEnumerator CombatController_LocksFullBufferAndRefreshesByOccupiedSlots()
        {
            var fixture = CreateCombatFixture("BOOK", 2, spawnEnemy: false);

            Assert.That(
                fixture.Combat.TryTypeLetter('X').Outcome,
                Is.EqualTo(TypedAttackOutcome.AddedToErrorBuffer));
            Assert.That(
                fixture.Combat.TryTypeLetter('Y').Outcome,
                Is.EqualTo(TypedAttackOutcome.AddedToErrorBuffer));
            Assert.That(fixture.Combat.ErrorBuffer.IsFull, Is.True);
            Assert.That(
                fixture.Combat.TryTypeLetter('Z').Outcome,
                Is.EqualTo(TypedAttackOutcome.InputBlocked));

            Assert.That(fixture.Combat.TryStartRefresh(), Is.True);
            Assert.That(fixture.Combat.ErrorBuffer.IsRefreshing, Is.True);
            fixture.Combat.Tick(1f);
            Assert.That(fixture.Combat.ErrorBuffer.OccupiedSlotCount, Is.Zero);
            Assert.That(fixture.Combat.ErrorBuffer.IsRefreshing, Is.False);

            fixture.Destroy();
            yield return null;
        }

        [UnityTest]
        public IEnumerator CombatController_CorruptionBlocksThenRestoresTypedAttacks()
        {
            var fixture = CreateCombatFixture("BOOK", 4);

            fixture.Combat.CorruptFor(1f);
            Assert.That(
                fixture.Combat.TryTypeLetter('B').Outcome,
                Is.EqualTo(TypedAttackOutcome.InputBlocked));
            Assert.That(fixture.Enemy.WordState.RemainingWord, Is.EqualTo("BOOK"));

            fixture.Combat.Tick(1f);
            Assert.That(fixture.Combat.Corruption.IsCorrupted, Is.False);
            Assert.That(
                fixture.Combat.TryTypeLetter('B').Outcome,
                Is.EqualTo(TypedAttackOutcome.TargetHit));
            Assert.That(fixture.Enemy.WordState.RemainingWord, Is.EqualTo("OOK"));

            fixture.Destroy();
            yield return null;
        }

        [UnityTest]
        public IEnumerator OnScreenLetterButton_RoutesThroughSameCombatController()
        {
            var fixture = CreateCombatFixture("BOOK", 4);
            var buttonObject = new GameObject("B Button");
            var button = buttonObject.AddComponent<Button>();
            var letterButton = buttonObject.AddComponent<OnScreenLetterButton>();
            letterButton.Configure(fixture.Combat, 'B');

            button.onClick.Invoke();

            Assert.That(fixture.Enemy.WordState.RemainingWord, Is.EqualTo("OOK"));
            Object.Destroy(buttonObject);
            fixture.Destroy();
            yield return null;
        }

        [UnityTest]
        public IEnumerator TurretPad_BuildsUpgradesAndResetsAuthoredChoice()
        {
            var padObject = new GameObject("Test Turret Pad");
            var renderer = padObject.AddComponent<SpriteRenderer>();
            var pad = padObject.AddComponent<TurretPadController>();
            pad.Configure(new Vector3Int(2, 3, 0), renderer, null, null, null);

            Assert.That(pad.Build(TurretKind.Engineer), Is.True);
            Assert.That(pad.Kind, Is.EqualTo(TurretKind.Engineer));
            Assert.That(pad.Level, Is.EqualTo(1));
            Assert.That(pad.Upgrade(), Is.True);
            Assert.That(pad.Upgrade(), Is.True);
            Assert.That(pad.Upgrade(), Is.False);
            Assert.That(pad.Level, Is.EqualTo(3));
            pad.ResetPad();
            Assert.That(pad.Kind, Is.EqualTo(TurretKind.None));
            Assert.That(pad.Level, Is.Zero);

            Object.Destroy(padObject);
            yield return null;
        }

        private static WaypointPath CreatePath(params Vector3[] positions)
        {
            var pathObject = new GameObject("Test Path");
            var path = pathObject.AddComponent<WaypointPath>();
            var waypoints = new Transform[positions.Length];

            for (var index = 0; index < positions.Length; index++)
            {
                var waypoint = new GameObject($"Waypoint {index}").transform;
                waypoint.SetParent(pathObject.transform);
                waypoint.position = positions[index];
                waypoints[index] = waypoint;
            }

            path.Configure(waypoints);
            return path;
        }

        private static EnemyDefinition CreateDefinition(string word, float speed)
        {
            var definition = ScriptableObject.CreateInstance<EnemyDefinition>();
            SetPrivateField(definition, "word", word);
            SetPrivateField(definition, "movementSpeed", speed);
            return definition;
        }

        private static CombatFixture CreateCombatFixture(
            string word,
            int capacity,
            bool spawnEnemy = true)
        {
            var path = CreatePath(Vector3.right, new Vector3(10f, 0f, 0f));
            var definition = CreateDefinition(word, 0f);
            var prefabObject = new GameObject("Enemy Template");
            var prefab = prefabObject.AddComponent<EnemyAgent>();
            var spawnerObject = new GameObject("Test Spawner");
            var spawner = spawnerObject.AddComponent<EnemySpawner>();
            spawner.Configure(prefab, path, new[] { definition });
            spawner.SetAutomaticSpawning(false);

            var playerObject = new GameObject("Test Player");
            var player = playerObject.AddComponent<PlayerMover>();
            var libraryObject = new GameObject("Test Library");
            var library = libraryObject.AddComponent<LibraryEndpoint>();
            library.Initialize(10);
            var coordinatorObject = new GameObject("Test Coordinator");
            var coordinator = coordinatorObject.AddComponent<GameplaySceneCoordinator>();
            coordinator.Configure(player, spawner, library, 3f);

            var combatObject = new GameObject("Test Combat");
            var combat = combatObject.AddComponent<GameplayCombatController>();
            combat.Configure(coordinator, null, null, null, capacity, 0.5f, true, 1f, 0f);
            combat.enabled = false;

            return new CombatFixture(
                path,
                definition,
                prefabObject,
                spawnerObject,
                spawner,
                playerObject,
                libraryObject,
                coordinatorObject,
                combatObject,
                combat,
                spawnEnemy ? spawner.SpawnNext() : null);
        }

        private sealed class CombatFixture
        {
            private readonly WaypointPath path;
            private readonly EnemyDefinition definition;
            private readonly GameObject prefabObject;
            private readonly GameObject spawnerObject;
            private readonly GameObject playerObject;
            private readonly GameObject libraryObject;
            private readonly GameObject coordinatorObject;
            private readonly GameObject combatObject;

            public CombatFixture(
                WaypointPath fixturePath,
                EnemyDefinition fixtureDefinition,
                GameObject fixturePrefabObject,
                GameObject fixtureSpawnerObject,
                EnemySpawner fixtureSpawner,
                GameObject fixturePlayerObject,
                GameObject fixtureLibraryObject,
                GameObject fixtureCoordinatorObject,
                GameObject fixtureCombatObject,
                GameplayCombatController fixtureCombat,
                EnemyAgent fixtureEnemy)
            {
                path = fixturePath;
                definition = fixtureDefinition;
                prefabObject = fixturePrefabObject;
                spawnerObject = fixtureSpawnerObject;
                Spawner = fixtureSpawner;
                playerObject = fixturePlayerObject;
                libraryObject = fixtureLibraryObject;
                coordinatorObject = fixtureCoordinatorObject;
                combatObject = fixtureCombatObject;
                Combat = fixtureCombat;
                Enemy = fixtureEnemy;
            }

            public EnemySpawner Spawner { get; }

            public GameplayCombatController Combat { get; }

            public EnemyAgent Enemy { get; }

            public void Destroy()
            {
                if (Enemy != null)
                {
                    Object.Destroy(Enemy.gameObject);
                }

                Object.Destroy(combatObject);
                Object.Destroy(coordinatorObject);
                Object.Destroy(libraryObject);
                Object.Destroy(playerObject);
                Object.Destroy(spawnerObject);
                Object.Destroy(prefabObject);
                Object.Destroy(definition);
                Object.Destroy(path.gameObject);
            }
        }

        private static void SetPrivateField(object target, string fieldName, object value)
        {
            var field = target.GetType().GetField(
                fieldName,
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(field, Is.Not.Null, $"Expected private field {fieldName}.");
            field.SetValue(target, value);
        }
    }
}
