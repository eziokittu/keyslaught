using System;
using System.Collections.Generic;

namespace KeySlaught.Gameplay
{
    public static class TargetingRules
    {
        public static EnemyTargetSnapshot SelectClosestMatching(
            IEnumerable<EnemyTargetSnapshot> candidates,
            char typedLetter)
        {
            if (candidates == null)
            {
                throw new ArgumentNullException(nameof(candidates));
            }

            var normalizedLetter = EnemyWordState.NormalizeCombatLetter(typedLetter);
            if (!EnemyWordState.IsCombatLetter(normalizedLetter))
            {
                return null;
            }

            EnemyTargetSnapshot best = null;

            foreach (var candidate in candidates)
            {
                if (candidate == null ||
                    !candidate.IsInRange ||
                    !candidate.Word.Requires(normalizedLetter))
                {
                    continue;
                }

                if (best == null ||
                    candidate.DistanceToLibrary < best.DistanceToLibrary ||
                    (candidate.DistanceToLibrary == best.DistanceToLibrary &&
                     candidate.TieBreakOrder < best.TieBreakOrder))
                {
                    best = candidate;
                }
            }

            return best;
        }
    }
}
