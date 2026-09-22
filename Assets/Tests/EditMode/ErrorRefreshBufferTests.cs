using System;
using KeySlaught.Gameplay;
using NUnit.Framework;

namespace KeySlaught.Tests.EditMode
{
    public sealed class ErrorRefreshBufferTests
    {
        [Test]
        public void TryRecordUnusableLetter_FillsCapacityThenLocksInput()
        {
            var buffer = Buffer(capacity: 2);

            Assert.That(buffer.TryRecordUnusableLetter('x'), Is.True);
            Assert.That(buffer.TryRecordUnusableLetter('Y'), Is.True);
            Assert.That(buffer.IsFull, Is.True);
            Assert.That(buffer.CanAcceptInput, Is.False);
            Assert.That(buffer.TryRecordUnusableLetter('Z'), Is.False);
            Assert.That(buffer.OccupiedLetters, Is.EqualTo(new[] { 'X', 'Y' }));
        }

        [Test]
        public void TryStartRefresh_WhenEarlyRefreshDisabled_RequiresFullBuffer()
        {
            var buffer = Buffer(capacity: 3, allowRefreshBeforeFull: false);
            buffer.TryRecordUnusableLetter('A');

            Assert.That(buffer.TryStartRefresh(out var duration), Is.False);
            Assert.That(duration, Is.Zero);

            buffer.TryRecordUnusableLetter('B');
            buffer.TryRecordUnusableLetter('C');

            Assert.That(buffer.TryStartRefresh(out duration), Is.True);
            Assert.That(duration, Is.EqualTo(1.5f));
        }

        [Test]
        public void TryStartRefresh_WhenEarlyRefreshEnabled_UsesOccupiedSlotsOnly()
        {
            var buffer = Buffer(
                capacity: 4,
                secondsPerSlot: 0.25f,
                allowRefreshBeforeFull: true);
            buffer.TryRecordUnusableLetter('A');
            buffer.TryRecordUnusableLetter('B');

            Assert.That(buffer.TryStartRefresh(out var duration), Is.True);
            Assert.That(duration, Is.EqualTo(0.5f));
            Assert.That(buffer.IsRefreshing, Is.True);
            Assert.That(buffer.CanAcceptInput, Is.False);
        }

        [Test]
        public void AdvanceRefresh_ClearsOnlyWhenConfiguredDurationCompletes()
        {
            var buffer = Buffer(capacity: 2, secondsPerSlot: 0.5f);
            buffer.TryRecordUnusableLetter('A');
            buffer.TryRecordUnusableLetter('B');
            buffer.TryStartRefresh(out _);

            Assert.That(buffer.AdvanceRefresh(0.75f), Is.False);
            Assert.That(buffer.RefreshSecondsRemaining, Is.EqualTo(0.25f));
            Assert.That(buffer.OccupiedSlotCount, Is.EqualTo(2));

            Assert.That(buffer.AdvanceRefresh(0.25f), Is.True);
            Assert.That(buffer.IsRefreshing, Is.False);
            Assert.That(buffer.OccupiedSlotCount, Is.Zero);
            Assert.That(buffer.CanAcceptInput, Is.True);
        }

        [Test]
        public void TryStartRefresh_RejectsEmptyBuffer()
        {
            var buffer = Buffer(capacity: 2);

            Assert.That(buffer.TryStartRefresh(out var duration), Is.False);
            Assert.That(duration, Is.Zero);
        }

        [Test]
        public void ZeroDurationRefresh_ClearsImmediately()
        {
            var buffer = Buffer(capacity: 1, secondsPerSlot: 0f);
            buffer.TryRecordUnusableLetter('A');

            Assert.That(buffer.TryStartRefresh(out var duration), Is.True);
            Assert.That(duration, Is.Zero);
            Assert.That(buffer.IsRefreshing, Is.False);
            Assert.That(buffer.OccupiedSlotCount, Is.Zero);
        }

        [Test]
        public void AdvanceRefresh_RejectsNegativeDelta()
        {
            var buffer = Buffer(capacity: 1);

            Assert.That(
                () => buffer.AdvanceRefresh(-0.1f),
                Throws.TypeOf<ArgumentOutOfRangeException>());
        }

        private static ErrorRefreshBuffer Buffer(
            int capacity,
            float secondsPerSlot = 0.5f,
            bool allowRefreshBeforeFull = false)
        {
            return new ErrorRefreshBuffer(
                new RefreshBufferSettings(
                    capacity,
                    secondsPerSlot,
                    allowRefreshBeforeFull));
        }
    }
}
