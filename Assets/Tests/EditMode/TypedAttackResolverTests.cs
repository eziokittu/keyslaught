using System;
using KeySlaught.Gameplay;
using KeySlaught.SceneGameplay;
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

        [Test]
        public void ResolveLoadedMagazine_FiresLetterThatWasStoredBeforeTargetEnteredRange()
        {
            var magazine = Buffer(capacity: 4);
            Assert.That(
                TypedAttackResolver.Resolve('B', Array.Empty<EnemyTargetSnapshot>(), magazine).Outcome,
                Is.EqualTo(TypedAttackOutcome.LoadedIntoMagazine));
            var book = Target("BOOK", distanceToLibrary: 1f);

            var result = TypedAttackResolver.ResolveLoadedMagazine(new[] { book }, magazine);

            Assert.That(result.Outcome, Is.EqualTo(TypedAttackOutcome.TargetHit));
            Assert.That(book.Word.RemainingWord, Is.EqualTo("OOK"));
            Assert.That(magazine.OccupiedSlotCount, Is.Zero);
        }

        [Test]
        public void ResolveLoadedMagazine_PrioritizesEnemyClosestToLibraryAcrossLoadedLetters()
        {
            var magazine = Buffer(capacity: 4);
            TypedAttackResolver.Resolve('H', Array.Empty<EnemyTargetSnapshot>(), magazine);
            TypedAttackResolver.Resolve('B', Array.Empty<EnemyTargetSnapshot>(), magazine);
            var history = Target("HISTORY", distanceToLibrary: 4f);
            var book = Target("BOOK", distanceToLibrary: 1f);

            var result = TypedAttackResolver.ResolveLoadedMagazine(new[] { history, book }, magazine);

            Assert.That(result.Target, Is.SameAs(book));
            Assert.That(book.Word.RemainingWord, Is.EqualTo("OOK"));
            Assert.That(history.Word.RemainingWord, Is.EqualTo("HISTORY"));
            Assert.That(magazine.OccupiedLetters, Is.EqualTo(new[] { 'H' }));
        }

        [Test]
        public void BrainCellReward_EqualsOriginalWordLength()
        {
            Assert.That(BrainCellEconomy.CalculateReward(new EnemyWordState("BOOK")), Is.EqualTo(4));
            Assert.That(BrainCellEconomy.CalculateReward(new EnemyWordState("BORING")), Is.EqualTo(6));
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
