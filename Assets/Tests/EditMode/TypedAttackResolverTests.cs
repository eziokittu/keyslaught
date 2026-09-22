using KeySlaught.Gameplay;
using NUnit.Framework;

namespace KeySlaught.Tests.EditMode
{
    public sealed class TypedAttackResolverTests
    {
        [Test]
        public void Resolve_MatchingLetterHitsTargetWithoutFillingBuffer()
        {
            var enemy = Target("HELLO", distanceToLibrary: 2f);
            var buffer = Buffer(capacity: 2);

            var result = TypedAttackResolver.Resolve('h', new[] { enemy }, buffer);

            Assert.That(result.Outcome, Is.EqualTo(TypedAttackOutcome.TargetHit));
            Assert.That(result.Target, Is.SameAs(enemy));
            Assert.That(enemy.Word.RemainingWord, Is.EqualTo("ELLO"));
            Assert.That(buffer.OccupiedSlotCount, Is.Zero);
        }

        [Test]
        public void Resolve_NoMatchingTargetAddsLetterToBuffer()
        {
            var buffer = Buffer(capacity: 2);

            var result = TypedAttackResolver.Resolve(
                'x',
                new[] { Target("HELLO", distanceToLibrary: 2f) },
                buffer);

            Assert.That(result.Outcome, Is.EqualTo(TypedAttackOutcome.AddedToErrorBuffer));
            Assert.That(result.Target, Is.Null);
            Assert.That(buffer.OccupiedLetters, Is.EqualTo(new[] { 'X' }));
        }

        [Test]
        public void Resolve_FullBufferBlocksOtherwiseValidHit()
        {
            var enemy = Target("HELLO", distanceToLibrary: 2f);
            var buffer = Buffer(capacity: 1);
            buffer.TryRecordUnusableLetter('X');

            var result = TypedAttackResolver.Resolve('H', new[] { enemy }, buffer);

            Assert.That(result.Outcome, Is.EqualTo(TypedAttackOutcome.InputBlocked));
            Assert.That(enemy.Word.RemainingWord, Is.EqualTo("HELLO"));
        }

        [Test]
        public void Resolve_NonCombatCharacterDoesNotFillBuffer()
        {
            var buffer = Buffer(capacity: 1);

            var result = TypedAttackResolver.Resolve('1', new EnemyTargetSnapshot[0], buffer);

            Assert.That(result.Outcome, Is.EqualTo(TypedAttackOutcome.InvalidLetter));
            Assert.That(buffer.OccupiedSlotCount, Is.Zero);
        }

        private static EnemyTargetSnapshot Target(string word, float distanceToLibrary)
        {
            return new EnemyTargetSnapshot(
                new EnemyWordState(word),
                true,
                distanceToLibrary,
                tieBreakOrder: 1);
        }

        private static ErrorRefreshBuffer Buffer(int capacity)
        {
            return new ErrorRefreshBuffer(
                new RefreshBufferSettings(
                    capacity,
                    secondsPerOccupiedSlot: 0.5f,
                    allowRefreshBeforeFull: false));
        }
    }
}
