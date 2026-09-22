using System;

namespace KeySlaught.Gameplay
{
    public static class LibraryDamageRules
    {
        public static int DamageFromArrival(EnemyWordState enemyWord)
        {
            if (enemyWord == null)
            {
                throw new ArgumentNullException(nameof(enemyWord));
            }

            return enemyWord.RemainingLetterCount;
        }
    }

    public sealed class LibraryState
    {
        public LibraryState(int maximumHealth)
        {
            if (maximumHealth <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(maximumHealth),
                    "Maximum library health must be greater than zero.");
            }

            MaximumHealth = maximumHealth;
            CurrentHealth = maximumHealth;
        }

        public int MaximumHealth { get; }

        public int CurrentHealth { get; private set; }

        public bool IsDestroyed => CurrentHealth == 0;

        public int ApplyEnemyArrival(EnemyWordState enemyWord)
        {
            var damage = LibraryDamageRules.DamageFromArrival(enemyWord);
            CurrentHealth = Math.Max(0, CurrentHealth - damage);
            return damage;
        }

        public int Repair(int amount)
        {
            if (amount < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(amount));
            }

            var previousHealth = CurrentHealth;
            CurrentHealth = Math.Min(MaximumHealth, CurrentHealth + amount);
            return CurrentHealth - previousHealth;
        }
    }
}
