using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace KeySlaught.Gameplay
{
    /// <summary>
    /// Runtime state for wrong or currently unusable typed letters.
    /// </summary>
    public sealed class ErrorRefreshBuffer
    {
        private readonly List<char> occupiedLetters;
        private readonly ReadOnlyCollection<char> occupiedLettersView;
        private float refreshSecondsRemaining;

        public ErrorRefreshBuffer(RefreshBufferSettings settings)
        {
            Settings = settings ?? throw new ArgumentNullException(nameof(settings));
            occupiedLetters = new List<char>(settings.Capacity);
            occupiedLettersView = occupiedLetters.AsReadOnly();
        }

        public RefreshBufferSettings Settings { get; }

        public IReadOnlyList<char> OccupiedLetters => occupiedLettersView;

        public int OccupiedSlotCount => occupiedLetters.Count;

        public bool IsFull => OccupiedSlotCount >= Settings.Capacity;

        public bool IsRefreshing { get; private set; }

        public bool CanAcceptInput => !IsRefreshing && !IsFull;

        public float RefreshSecondsRemaining => refreshSecondsRemaining;

        public bool TryRecordUnusableLetter(char letter)
        {
            var normalizedLetter = EnemyWordState.NormalizeCombatLetter(letter);
            if (!CanAcceptInput || !EnemyWordState.IsCombatLetter(normalizedLetter))
            {
                return false;
            }

            occupiedLetters.Add(normalizedLetter);
            return true;
        }

        public bool TryStartRefresh(out float durationSeconds)
        {
            durationSeconds = 0f;

            if (IsRefreshing || OccupiedSlotCount == 0)
            {
                return false;
            }

            if (!Settings.AllowRefreshBeforeFull && !IsFull)
            {
                return false;
            }

            durationSeconds = OccupiedSlotCount * Settings.SecondsPerOccupiedSlot;
            refreshSecondsRemaining = durationSeconds;

            if (durationSeconds <= 0f)
            {
                CompleteRefresh();
                return true;
            }

            IsRefreshing = true;
            return true;
        }

        /// <summary>
        /// Advances an active refresh and returns true only on the tick that completes it.
        /// </summary>
        public bool AdvanceRefresh(float deltaSeconds)
        {
            if (float.IsNaN(deltaSeconds) ||
                float.IsInfinity(deltaSeconds) ||
                deltaSeconds < 0f)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(deltaSeconds),
                    "Refresh delta must be a finite, non-negative value.");
            }

            if (!IsRefreshing)
            {
                return false;
            }

            refreshSecondsRemaining = Math.Max(0f, refreshSecondsRemaining - deltaSeconds);
            if (refreshSecondsRemaining > 0f)
            {
                return false;
            }

            CompleteRefresh();
            return true;
        }

        private void CompleteRefresh()
        {
            occupiedLetters.Clear();
            refreshSecondsRemaining = 0f;
            IsRefreshing = false;
        }
    }
}
