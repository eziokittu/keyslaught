using System;
using KeySlaught.Gameplay;
using NUnit.Framework;

namespace KeySlaught.Tests.EditMode
{
    public sealed class GunCorruptionStateTests
    {
        [Test]
        public void CorruptFor_DisablesUntilDurationExpires()
        {
            var state = new GunCorruptionState();

            state.CorruptFor(1.5f);
            Assert.That(state.IsCorrupted, Is.True);
            Assert.That(state.Advance(1f), Is.False);
            Assert.That(state.SecondsRemaining, Is.EqualTo(0.5f).Within(0.0001f));
            Assert.That(state.Advance(0.5f), Is.True);
            Assert.That(state.IsCorrupted, Is.False);
        }

        [Test]
        public void CorruptFor_DoesNotShortenExistingCorruption()
        {
            var state = new GunCorruptionState();

            state.CorruptFor(3f);
            state.Advance(1f);
            state.CorruptFor(0.5f);

            Assert.That(state.SecondsRemaining, Is.EqualTo(2f).Within(0.0001f));
        }

        [TestCase(float.NaN)]
        [TestCase(float.PositiveInfinity)]
        [TestCase(-0.01f)]
        public void InvalidDurations_AreRejected(float duration)
        {
            var state = new GunCorruptionState();

            Assert.Throws<ArgumentOutOfRangeException>(() => state.CorruptFor(duration));
            Assert.Throws<ArgumentOutOfRangeException>(() => state.Advance(duration));
        }
    }
}
