using System;

namespace KeySlaught.Gameplay
{
    /// <summary>
    /// A mechanics-only view of an enemy at the instant a target is selected.
    /// </summary>
    public sealed class EnemyTargetSnapshot
    {
        public EnemyTargetSnapshot(
            EnemyWordState word,
            bool isInRange,
            float distanceToLibrary,
            long tieBreakOrder)
        {
            Word = word ?? throw new ArgumentNullException(nameof(word));

            if (float.IsNaN(distanceToLibrary) ||
                float.IsInfinity(distanceToLibrary) ||
                distanceToLibrary < 0f)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(distanceToLibrary),
                    "Distance to the library must be a finite, non-negative value.");
            }

            IsInRange = isInRange;
            DistanceToLibrary = distanceToLibrary;
            TieBreakOrder = tieBreakOrder;
        }

        public EnemyWordState Word { get; }

        public bool IsInRange { get; }

        public float DistanceToLibrary { get; }

        /// <summary>
        /// Stable authored/runtime order used only when two valid targets are exactly tied.
        /// </summary>
        public long TieBreakOrder { get; }
    }
}
