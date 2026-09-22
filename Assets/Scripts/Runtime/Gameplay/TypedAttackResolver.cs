using System;
using System.Collections.Generic;

namespace KeySlaught.Gameplay
{
    public enum TypedAttackOutcome
    {
        InvalidLetter,
        InputBlocked,
        TargetHit,
        LoadedIntoMagazine,
        AddedToErrorBuffer = LoadedIntoMagazine
    }

    public sealed class TypedAttackResult
    {
        internal TypedAttackResult(
            TypedAttackOutcome outcome,
            EnemyTargetSnapshot target)
        {
            Outcome = outcome;
            Target = target;
        }

        public TypedAttackOutcome Outcome { get; }

        public EnemyTargetSnapshot Target { get; }

        public static TypedAttackResult Blocked()
        {
            return new TypedAttackResult(TypedAttackOutcome.InputBlocked, null);
        }
    }

    /// <summary>
    /// Connects target selection, sequential word progress, and the loaded-letter magazine.
    /// Presentation can react to the returned result without owning gameplay rules.
    /// </summary>
    public static class TypedAttackResolver
    {
        public static TypedAttackResult Resolve(
            char typedLetter,
            IEnumerable<EnemyTargetSnapshot> candidates,
            ErrorRefreshBuffer errorBuffer)
        {
            if (candidates == null)
            {
                throw new ArgumentNullException(nameof(candidates));
            }

            if (errorBuffer == null)
            {
                throw new ArgumentNullException(nameof(errorBuffer));
            }

            var normalizedLetter = EnemyWordState.NormalizeCombatLetter(typedLetter);
            if (!EnemyWordState.IsCombatLetter(normalizedLetter))
            {
                return new TypedAttackResult(TypedAttackOutcome.InvalidLetter, null);
            }

            if (!errorBuffer.CanAcceptInput)
            {
                return new TypedAttackResult(TypedAttackOutcome.InputBlocked, null);
            }

            errorBuffer.TryRecordUnusableLetter(normalizedLetter);
            var target = TargetingRules.SelectClosestMatching(candidates, normalizedLetter);
            if (target != null)
            {
                errorBuffer.TryConsume(normalizedLetter);
                target.Word.TryConsume(normalizedLetter);
                return new TypedAttackResult(TypedAttackOutcome.TargetHit, target);
            }

            return new TypedAttackResult(TypedAttackOutcome.LoadedIntoMagazine, null);
        }

        public static TypedAttackResult ResolveLoadedMagazine(
            IEnumerable<EnemyTargetSnapshot> candidates,
            ErrorRefreshBuffer magazine)
        {
            if (candidates == null)
            {
                throw new ArgumentNullException(nameof(candidates));
            }

            if (magazine == null)
            {
                throw new ArgumentNullException(nameof(magazine));
            }

            if (magazine.IsRefreshing || magazine.OccupiedSlotCount == 0)
            {
                return TypedAttackResult.Blocked();
            }

            EnemyTargetSnapshot best = null;
            char bestLetter = default;
            foreach (var letter in magazine.OccupiedLetters)
            {
                var candidate = TargetingRules.SelectClosestMatching(candidates, letter);
                if (candidate == null)
                {
                    continue;
                }

                if (best == null ||
                    candidate.DistanceToLibrary < best.DistanceToLibrary ||
                    (candidate.DistanceToLibrary == best.DistanceToLibrary &&
                     candidate.TieBreakOrder < best.TieBreakOrder))
                {
                    best = candidate;
                    bestLetter = letter;
                }
            }

            if (best == null || !magazine.TryConsume(bestLetter))
            {
                return new TypedAttackResult(TypedAttackOutcome.LoadedIntoMagazine, null);
            }

            best.Word.TryConsume(bestLetter);
            return new TypedAttackResult(TypedAttackOutcome.TargetHit, best);
        }
    }
}
