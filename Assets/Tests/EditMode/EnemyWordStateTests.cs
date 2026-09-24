using System;
using KeySlaught.Gameplay;
using NUnit.Framework;

namespace KeySlaught.Tests.EditMode
{
    public sealed class EnemyWordStateTests
    {
        [Test]
        public void ConsumePrefix_RemovesRequestedLettersAndClampsAtEnd()
        {
            var state = new EnemyWordState("HELLO");

            Assert.That(state.ConsumePrefix(3), Is.EqualTo(3));
            Assert.That(state.RemainingWord, Is.EqualTo("LO"));
            Assert.That(state.ConsumePrefix(99), Is.EqualTo(2));
            Assert.That(state.IsDefeated, Is.True);
        }

        [Test]
        public void Constructor_NormalizesWordToUppercase()
        {
            var word = new EnemyWordState("Hello");

            Assert.That(word.OriginalWord, Is.EqualTo("HELLO"));
            Assert.That(word.RemainingWord, Is.EqualTo("HELLO"));
            Assert.That(word.NextLetter, Is.EqualTo('H'));
        }

        [Test]
        public void TryConsume_AdvancesOnlyInSequence()
        {
            var word = new EnemyWordState("Hello");

            Assert.That(word.TryConsume('x'), Is.False);
            Assert.That(word.RemainingWord, Is.EqualTo("HELLO"));

            Assert.That(word.TryConsume('h'), Is.True);
            Assert.That(word.RemainingWord, Is.EqualTo("ELLO"));
            Assert.That(word.TryConsume('E'), Is.True);
            Assert.That(word.RemainingWord, Is.EqualTo("LLO"));
        }

        [Test]
        public void TryConsume_FinalLetterMarksWordDefeated()
        {
            var word = new EnemyWordState("A");

            Assert.That(word.TryConsume('a'), Is.True);
            Assert.That(word.IsDefeated, Is.True);
            Assert.That(word.RemainingLetterCount, Is.Zero);
            Assert.That(word.RemainingWord, Is.Empty);
            Assert.That(word.NextLetter, Is.Null);
            Assert.That(word.TryConsume('A'), Is.False);
        }

        [TestCase("")]
        [TestCase("HEL LO")]
        [TestCase("123")]
        public void Constructor_RejectsInvalidCombatWords(string value)
        {
            Assert.That(() => new EnemyWordState(value), Throws.ArgumentException);
        }

        [Test]
        public void Constructor_RejectsNullWord()
        {
            Assert.That(() => new EnemyWordState(null), Throws.TypeOf<ArgumentNullException>());
        }
    }
}
