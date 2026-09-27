using System;
using UnityEngine;

namespace KeySlaught.Progression
{
    public static class GameSpeedSettings
    {
        public static event Action Changed;
        private static int multiplier = 1;

        public static int Multiplier => multiplier;

        public static void SetMultiplier(int value)
        {
            multiplier = Mathf.Clamp(value, 1, 3);
            Changed?.Invoke();
        }

        public static void ResetForLevel()
        {
            multiplier = 1;
            Time.timeScale = 1f;
            Changed?.Invoke();
        }

        public static void ApplyGameplaySpeed() => Time.timeScale = Multiplier;
        public static void ApplyMenuSpeed() => Time.timeScale = 1f;
    }
}
