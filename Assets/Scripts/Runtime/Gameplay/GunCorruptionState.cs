using System;

namespace KeySlaught.Gameplay
{
    /// <summary>
    /// Runtime timer that disables typed attacks after enemy contact.
    /// </summary>
    public sealed class GunCorruptionState
    {
        public bool IsCorrupted => SecondsRemaining > 0f;

        public float SecondsRemaining { get; private set; }

        public void CorruptFor(float durationSeconds)
        {
            ValidateSeconds(durationSeconds, nameof(durationSeconds));
            SecondsRemaining = Math.Max(SecondsRemaining, durationSeconds);
        }

        public bool Advance(float deltaSeconds)
        {
            ValidateSeconds(deltaSeconds, nameof(deltaSeconds));
            if (!IsCorrupted)
            {
                return false;
            }

            SecondsRemaining = Math.Max(0f, SecondsRemaining - deltaSeconds);
            return !IsCorrupted;
        }

        private static void ValidateSeconds(float seconds, string parameterName)
        {
            if (float.IsNaN(seconds) || float.IsInfinity(seconds) || seconds < 0f)
            {
                throw new ArgumentOutOfRangeException(
                    parameterName,
                    "Duration must be a finite, non-negative value.");
            }
        }
    }
}
