using System;
using System.Collections.Generic;
using KeySlaught.Gameplay;
using UnityEngine;

namespace KeySlaught.SceneGameplay
{
    public sealed class GameplayCombatController : MonoBehaviour
    {
        [SerializeField] private GameplaySceneCoordinator sceneCoordinator;
        [SerializeField] private TextMesh bufferLabel;
        [SerializeField] private TextMesh statusLabel;
        [SerializeField] private LineRenderer shotLine;
        [SerializeField, Min(1)] private int bufferCapacity = 4;
        [SerializeField, Min(0f)] private float secondsPerOccupiedSlot = 0.5f;
        [SerializeField] private bool allowRefreshBeforeFull = true;
        [SerializeField, Min(0f)] private float corruptionDuration = 2.5f;
        [SerializeField, Min(0f)] private float corruptionContactDistance = 0.8f;
        [SerializeField, Min(0f)] private float shotVisibleSeconds = 0.1f;

        private readonly HashSet<EnemyAgent> touchingEnemies = new HashSet<EnemyAgent>();
        private ErrorRefreshBuffer errorBuffer;
        private GunCorruptionState corruption;
        private float activeRefreshDuration;
        private float shotSecondsRemaining;

        public event Action<EnemyAgent> TargetHit;

        public ErrorRefreshBuffer ErrorBuffer
        {
            get
            {
                EnsureRuntimeState();
                return errorBuffer;
            }
        }

        public GunCorruptionState Corruption
        {
            get
            {
                EnsureRuntimeState();
                return corruption;
            }
        }

        public bool CanType => !Corruption.IsCorrupted && ErrorBuffer.CanAcceptInput;

        public GameplaySceneCoordinator SceneCoordinator => sceneCoordinator;

        public void Configure(
            GameplaySceneCoordinator coordinator,
            TextMesh errorBufferLabel,
            TextMesh combatStatusLabel,
            LineRenderer directShotLine,
            int capacity = 4,
            float secondsPerSlot = 0.5f,
            bool allowEarlyRefresh = true,
            float contactCorruptionDuration = 2.5f,
            float contactDistance = 0.8f,
            float directShotSeconds = 0.1f)
        {
            sceneCoordinator = coordinator;
            bufferLabel = errorBufferLabel;
            statusLabel = combatStatusLabel;
            shotLine = directShotLine;
            bufferCapacity = Mathf.Max(1, capacity);
            secondsPerOccupiedSlot = Mathf.Max(0f, secondsPerSlot);
            allowRefreshBeforeFull = allowEarlyRefresh;
            corruptionDuration = Mathf.Max(0f, contactCorruptionDuration);
            corruptionContactDistance = Mathf.Max(0f, contactDistance);
            shotVisibleSeconds = Mathf.Max(0f, directShotSeconds);
            ResetRuntimeState();
        }

        public TypedAttackResult TryTypeLetter(char letter)
        {
            EnsureRuntimeState();
            if (corruption.IsCorrupted)
            {
                RefreshHud();
                return TypedAttackResult.Blocked();
            }

            var result = TypedAttackResolver.Resolve(
                letter,
                sceneCoordinator == null
                    ? Array.Empty<EnemyTargetSnapshot>()
                    : sceneCoordinator.CreateTargetSnapshots(),
                errorBuffer);

            if (result.Outcome == TypedAttackOutcome.TargetHit && sceneCoordinator != null)
            {
                var enemy = sceneCoordinator.FindEnemy(result.Target.Word);
                if (enemy != null)
                {
                    enemy.RefreshLabel();
                    ShowShot(enemy);
                    TargetHit?.Invoke(enemy);
                    if (enemy.WordState.IsDefeated)
                    {
                        touchingEnemies.Remove(enemy);
                        sceneCoordinator.RemoveDefeatedEnemy(enemy);
                    }
                }
            }

            RefreshHud();
            return result;
        }

        public bool TryStartRefresh()
        {
            EnsureRuntimeState();
            if (!errorBuffer.TryStartRefresh(out activeRefreshDuration))
            {
                RefreshHud();
                return false;
            }

            RefreshHud();
            return true;
        }

        public void CorruptFor(float durationSeconds)
        {
            Corruption.CorruptFor(durationSeconds);
            RefreshHud();
        }

        public void Tick(float deltaSeconds)
        {
            EnsureRuntimeState();
            corruption.Advance(deltaSeconds);
            errorBuffer.AdvanceRefresh(deltaSeconds);
            TickShot(deltaSeconds);
            DetectEnemyContact();
            RefreshHud();
        }

        private void Awake()
        {
            EnsureRuntimeState();
            if (shotLine != null)
            {
                shotLine.enabled = false;
            }

            RefreshHud();
        }

        private void Update()
        {
            Tick(Time.deltaTime);
        }

        private void EnsureRuntimeState()
        {
            if (errorBuffer == null || corruption == null)
            {
                ResetRuntimeState();
            }
        }

        private void ResetRuntimeState()
        {
            errorBuffer = new ErrorRefreshBuffer(
                new RefreshBufferSettings(
                    Mathf.Max(1, bufferCapacity),
                    Mathf.Max(0f, secondsPerOccupiedSlot),
                    allowRefreshBeforeFull));
            corruption = new GunCorruptionState();
            activeRefreshDuration = 0f;
            shotSecondsRemaining = 0f;
            touchingEnemies.Clear();
            if (shotLine != null)
            {
                shotLine.enabled = false;
            }

            RefreshHud();
        }

        private void DetectEnemyContact()
        {
            if (sceneCoordinator == null || sceneCoordinator.Player == null || sceneCoordinator.Spawner == null)
            {
                touchingEnemies.Clear();
                return;
            }

            var currentContacts = new HashSet<EnemyAgent>();
            foreach (var enemy in sceneCoordinator.Spawner.ActiveEnemies)
            {
                if (enemy == null || enemy.HasArrived || enemy.WordState == null)
                {
                    continue;
                }

                if (Vector2.Distance(
                        sceneCoordinator.Player.transform.position,
                        enemy.transform.position) > corruptionContactDistance)
                {
                    continue;
                }

                currentContacts.Add(enemy);
                if (!touchingEnemies.Contains(enemy))
                {
                    corruption.CorruptFor(corruptionDuration);
                }
            }

            touchingEnemies.RemoveWhere(enemy => enemy == null || !currentContacts.Contains(enemy));
            touchingEnemies.UnionWith(currentContacts);
        }

        private void ShowShot(EnemyAgent enemy)
        {
            if (shotLine == null || sceneCoordinator.Player == null)
            {
                return;
            }

            shotLine.positionCount = 2;
            shotLine.SetPosition(0, sceneCoordinator.Player.transform.position);
            shotLine.SetPosition(1, enemy.transform.position);
            shotLine.enabled = true;
            shotSecondsRemaining = shotVisibleSeconds;
        }

        private void TickShot(float deltaSeconds)
        {
            if (shotLine == null || !shotLine.enabled)
            {
                return;
            }

            shotSecondsRemaining = Mathf.Max(0f, shotSecondsRemaining - deltaSeconds);
            if (shotSecondsRemaining <= 0f)
            {
                shotLine.enabled = false;
            }
        }

        private void RefreshHud()
        {
            if (bufferLabel != null && errorBuffer != null)
            {
                var slots = new string[errorBuffer.Settings.Capacity];
                for (var index = 0; index < slots.Length; index++)
                {
                    slots[index] = index < errorBuffer.OccupiedSlotCount
                        ? $"[{errorBuffer.OccupiedLetters[index]}]"
                        : "[ ]";
                }

                bufferLabel.text = $"ERROR BUFFER  {string.Join(" ", slots)}";
            }

            if (statusLabel == null || errorBuffer == null || corruption == null)
            {
                return;
            }

            if (corruption.IsCorrupted)
            {
                statusLabel.text = $"GUN CORRUPTED  {corruption.SecondsRemaining:0.0}s  //  INPUT DISABLED";
                statusLabel.color = new Color(1f, 0.32f, 0.42f, 1f);
            }
            else if (errorBuffer.IsRefreshing)
            {
                var progress = activeRefreshDuration <= 0f
                    ? 1f
                    : 1f - errorBuffer.RefreshSecondsRemaining / activeRefreshDuration;
                statusLabel.text = $"REFRESHING  {progress * 100f:0}%  //  {errorBuffer.RefreshSecondsRemaining:0.0}s";
                statusLabel.color = new Color(0.3f, 0.85f, 1f, 1f);
            }
            else if (errorBuffer.IsFull)
            {
                statusLabel.text = "BUFFER LOCKED  //  SPACE TO REFRESH";
                statusLabel.color = new Color(1f, 0.68f, 0.18f, 1f);
            }
            else
            {
                statusLabel.text = allowRefreshBeforeFull
                    ? "TYPE A-Z TO FIRE  //  SPACE REFRESH (EARLY ON)"
                    : "TYPE A-Z TO FIRE  //  REFRESH WHEN FULL";
                statusLabel.color = new Color(0.68f, 0.82f, 0.95f, 1f);
            }
        }
    }
}
