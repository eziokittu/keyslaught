using System;

namespace KeySlaught.SceneGameplay
{
    public static class TutorialInputGate
    {
        private static string allowedLetters = string.Empty;
        private static char? requiredLetter;

        public static event Action<string, bool> Feedback;
        public static bool Active { get; private set; }
        public static bool AllowMovement { get; private set; } = true;
        public static bool AllowRefresh { get; private set; } = true;
        public static char? RequiredLetter => requiredLetter;

        public static bool AllowsLetter(char letter)
        {
            if (!Active) return true;
            var normalized = char.ToUpperInvariant(letter);
            return requiredLetter.HasValue ? normalized == requiredLetter.Value : allowedLetters.IndexOf(normalized) >= 0;
        }

        public static bool TryAllowLetter(char letter)
        {
            var normalized = char.ToUpperInvariant(letter);
            var allowed = AllowsLetter(normalized);
            if (Active)
                Feedback?.Invoke(allowed ? $"CORRECT — {normalized}" :
                    requiredLetter.HasValue ? $"TRY AGAIN — PRESS {requiredLetter.Value}" : "THAT KEY IS NOT NEEDED YET", allowed);
            return allowed;
        }

        public static bool TryAllowRefresh()
        {
            var allowed = !Active || AllowRefresh;
            if (Active) Feedback?.Invoke(allowed ? "CORRECT — REFRESHING" : "REFRESH IS NOT NEEDED YET", allowed);
            return allowed;
        }

        public static void MovementOnly() { Activate(true, false, string.Empty, null); }
        public static void LettersOnly(string letters) { Activate(false, false, (letters ?? string.Empty).ToUpperInvariant(), null); }
        public static void RequireLetter(char letter)
        {
            var normalized = char.ToUpperInvariant(letter);
            Activate(false, false, normalized.ToString(), normalized);
        }
        public static void RefreshOnly() { Activate(false, true, string.Empty, null); }
        public static void DisableAll() { Activate(false, false, string.Empty, null); }

        public static void Clear()
        {
            Active = false; AllowMovement = true; AllowRefresh = true;
            allowedLetters = string.Empty; requiredLetter = null;
        }

        private static void Activate(bool movement, bool refresh, string letters, char? required)
        {
            Active = true; AllowMovement = movement; AllowRefresh = refresh;
            allowedLetters = letters; requiredLetter = required;
        }
    }
}
