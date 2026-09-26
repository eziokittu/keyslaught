using System;
using UnityEngine;

namespace KeySlaught.Progression
{
    public static class GameSpeedSettings
    {
        public const string PlayerPrefsKey = "KeySlaught.GameSpeed";

        public static event Action Changed;

        public static int Multiplier => Mathf.Clamp(PlayerPrefs.GetInt(PlayerPrefsKey, 1), 1, 3);

        public static void SetMultiplier(int value)
        {
            PlayerPrefs.SetInt(PlayerPrefsKey, Mathf.Clamp(value, 1, 3));
            PlayerPrefs.Save();
            Changed?.Invoke();
        }

        public static void ApplyGameplaySpeed() => Time.timeScale = Multiplier;
        public static void ApplyMenuSpeed() => Time.timeScale = 1f;
    }
}
