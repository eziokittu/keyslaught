using System;

namespace KeySlaught.Gameplay
{
    /// <summary>
    /// Mutable runtime progress for one enemy word. Base enemy definitions remain separate data.
    /// </summary>
    public sealed class EnemyWordState
    {
        private int consumedLetterCount;

        public EnemyWordState(string word)
        {
            if (word == null)
            {
                throw new ArgumentNullException(nameof(word));
            }

            if (word.Length == 0)
            {
                throw new ArgumentException("Enemy words cannot be empty.", nameof(word));
            }

            OriginalWord = word.ToUpperInvariant();

            for (var index = 0; index < OriginalWord.Length; index++)
            {
                if (!IsCombatLetter(OriginalWord[index]))
                {
                    throw new ArgumentException(
                        "Enemy words may only contain the letters A through Z.",
                        nameof(word));
                }
            }
        }

        public string OriginalWord { get; }

        public int ConsumedLetterCount => consumedLetterCount;

        public int RemainingLetterCount => OriginalWord.Length - consumedLetterCount;

        public string RemainingWord => OriginalWord.Substring(consumedLetterCount);

        public bool IsDefeated => consumedLetterCount >= OriginalWord.Length;

        public char? NextLetter => IsDefeated ? (char?)null : OriginalWord[consumedLetterCount];

        public bool Requires(char typedLetter)
        {
            return !IsDefeated && NextLetter == NormalizeCombatLetter(typedLetter);
        }

        public bool TryConsume(char typedLetter)
        {
            if (!Requires(typedLetter))
            {
                return false;
            }

            consumedLetterCount++;
            return true;
        }

        internal static bool IsCombatLetter(char letter)
        {
            return letter >= 'A' && letter <= 'Z';
        }

        internal static char NormalizeCombatLetter(char letter)
        {
            return char.ToUpperInvariant(letter);
        }
    }
}
