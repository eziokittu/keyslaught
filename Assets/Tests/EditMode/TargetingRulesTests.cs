using System;
using KeySlaught.Gameplay;
using NUnit.Framework;

namespace KeySlaught.Tests.EditMode
{
    public sealed class TargetingRulesTests
    {
        [Test]
        public void SelectClosestMatching_ChoosesClosestValidEnemyToLibrary()
        {
            var farther = Target("ALPHA", true, 6f, 1);
            var closer = Target("APPLE", true, 2f, 2);
            var wrongLetter = Target("BETA", true, 1f, 3);

            var selected = TargetingRules.SelectClosestMatching(
                new[] { farther, closer, wrongLetter },
                'a');

            Assert.That(selected, Is.SameAs(closer));
        }

        [Test]
        public void SelectClosestMatching_IgnoresMatchingEnemyOutsideRange()
        {
            var outsideRange = Target("ALPHA", false, 1f, 1);
            var inRange = Target("APPLE", true, 5f, 2);

            var selected = TargetingRules.SelectClosestMatching(
                new[] { outsideRange, inRange },
                'A');

            Assert.That(selected, Is.SameAs(inRange));
        }

        [Test]
        public void SelectClosestMatching_UsesStableOrderForExactTie()
        {
            var later = Target("ALPHA", true, 3f, 20);
            var earlier = Target("APPLE", true, 3f, 10);

            var selected = TargetingRules.SelectClosestMatching(
                new[] { later, earlier },
                'A');

            Assert.That(selected, Is.SameAs(earlier));
        }

        [Test]
        public void SelectClosestMatching_ReturnsNullForEmptySet()
        {
            var selected = TargetingRules.SelectClosestMatching(
                Array.Empty<EnemyTargetSnapshot>(),
                'A');

            Assert.That(selected, Is.Null);
        }

        [Test]
        public void SelectClosestMatching_ReturnsNullWhenNoEnemyRequiresLetter()
        {
            var selected = TargetingRules.SelectClosestMatching(
                new[] { Target("BETA", true, 1f, 1) },
                'A');

            Assert.That(selected, Is.Null);
        }

        [Test]
        public void SelectClosestMatching_IgnoresDefeatedEnemy()
        {
            var defeated = Target("A", true, 1f, 1);
            defeated.Word.TryConsume('A');

            var selected = TargetingRules.SelectClosestMatching(
                new[] { defeated },
                'A');

            Assert.That(selected, Is.Null);
        }

        [Test]
        public void TargetSnapshot_RejectsInvalidDistance()
        {
            Assert.That(
                () => Target("A", true, float.NaN, 1),
                Throws.TypeOf<ArgumentOutOfRangeException>());
        }

        private static EnemyTargetSnapshot Target(
            string word,
            bool inRange,
            float distanceToLibrary,
            long tieBreakOrder)
        {
            return new EnemyTargetSnapshot(
                new EnemyWordState(word),
                inRange,
                distanceToLibrary,
                tieBreakOrder);
        }
    }
}
