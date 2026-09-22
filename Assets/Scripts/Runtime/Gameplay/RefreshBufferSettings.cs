using System;

namespace KeySlaught.Gameplay
{
    public sealed class RefreshBufferSettings
    {
        public RefreshBufferSettings(
            int capacity,
            float secondsPerOccupiedSlot,
            bool allowRefreshBeforeFull)
        {
            if (capacity <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(capacity),
                    "Buffer capacity must be greater than zero.");
            }

            if (float.IsNaN(secondsPerOccupiedSlot) ||
                float.IsInfinity(secondsPerOccupiedSlot) ||
                secondsPerOccupiedSlot < 0f)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(secondsPerOccupiedSlot),
                    "Seconds per occupied slot must be a finite, non-negative value.");
            }

            Capacity = capacity;
            SecondsPerOccupiedSlot = secondsPerOccupiedSlot;
            AllowRefreshBeforeFull = allowRefreshBeforeFull;
        }

        public int Capacity { get; }

        public float SecondsPerOccupiedSlot { get; }

        public bool AllowRefreshBeforeFull { get; }
    }
}
