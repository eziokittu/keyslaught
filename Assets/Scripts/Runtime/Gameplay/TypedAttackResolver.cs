using System;
using System.Collections.Generic;

namespace KeySlaught.Gameplay
{
    public enum TypedAttackOutcome
    {
        InvalidLetter,
        InputBlocked,
        TargetHit,
        AddedToErrorBuffer
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
    /// Connects target selection, sequential word progress, and the unusable-letter buffer.
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

            var target = TargetingRules.SelectClosestMatching(candidates, normalizedLetter);
            if (target != null)
            {
                target.Word.TryConsume(normalizedLetter);
                return new TypedAttackResult(TypedAttackOutcome.TargetHit, target);
            }

            errorBuffer.TryRecordUnusableLetter(normalizedLetter);
            return new TypedAttackResult(TypedAttackOutcome.AddedToErrorBuffer, null);
        }
    }
}
