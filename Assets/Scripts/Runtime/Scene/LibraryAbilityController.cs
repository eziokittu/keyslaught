using System;
using System.Linq;
using UnityEngine;

namespace KeySlaught.SceneGameplay
{
    public sealed class LibraryAbilityController : MonoBehaviour
    {
        [SerializeField] private EnemySpawner spawner;
        [SerializeField] private GameplayCombatController combat;
        [SerializeField] private BrainCellEconomy economy;
        [SerializeField] private AbilityDefinition[] definitions;

        private AbilityDefinition activeTimedAbility;
        private float remainingSeconds;

        public LibraryAbilityKind? ActiveAbility => activeTimedAbility == null ? null : activeTimedAbility.Kind;
        public float RemainingSeconds => remainingSeconds;

        public void Configure(EnemySpawner enemySpawner, GameplayCombatController combatController,
            BrainCellEconomy runEconomy, AbilityDefinition[] abilityDefinitions)
        {
            spawner = enemySpawner;
            combat = combatController;
            economy = runEconomy;
            definitions = abilityDefinitions;
            ResetState();
        }

        public AbilityDefinition GetDefinition(LibraryAbilityKind kind)
        {
            return definitions == null ? null : Array.Find(definitions, item => item != null && item.Kind == kind);
        }

        public bool TryActivate(LibraryAbilityKind kind)
        {
            var definition = GetDefinition(kind);
            if (definition == null || economy == null || activeTimedAbility != null || !economy.TrySpend(definition.Cost))
                return false;

            if (kind == LibraryAbilityKind.Politics)
            {
                ApplyPolitics(definition.TargetCount);
                return true;
            }

            activeTimedAbility = definition;
            remainingSeconds = definition.Duration;
            ApplyMovementEffect();
            return true;
        }

        public void Tick(float deltaSeconds)
        {
            if (activeTimedAbility == null) return;
            remainingSeconds = Mathf.Max(0f, remainingSeconds - Mathf.Max(0f, deltaSeconds));
            if (remainingSeconds > 0f) return;
            activeTimedAbility = null;
            spawner?.SetMovementMultiplier(1f);
        }

        public void ResetState()
        {
            activeTimedAbility = null;
            remainingSeconds = 0f;
            spawner?.SetMovementMultiplier(1f);
        }

        private void Update() => Tick(Time.deltaTime);

        private void ApplyMovementEffect()
        {
            if (spawner == null || activeTimedAbility == null) return;
            spawner.SetMovementMultiplier(activeTimedAbility.Kind == LibraryAbilityKind.History ? -1f : 0f);
        }

        private void ApplyPolitics(int targetCount)
        {
            if (spawner == null) return;
            var targets = spawner.ActiveEnemies
                .Where(enemy => enemy != null && !enemy.HasArrived && enemy.WordState != null)
                .OrderBy(enemy => enemy.DistanceToLibrary)
                .ThenBy(enemy => enemy.TieBreakOrder)
                .Take(Mathf.Max(1, targetCount))
                .ToArray();
            foreach (var enemy in targets)
            {
                enemy.WordState.ConsumePrefix(Mathf.CeilToInt(enemy.WordState.RemainingLetterCount / 2f));
                combat?.ResolveExternalDamage(enemy);
            }
        }
    }
}
