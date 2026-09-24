using UnityEngine;

namespace KeySlaught.SceneGameplay
{
    public static class EnemyLengthPalette
    {
        private static readonly Color[] Stops =
        {
            new(0.96f, 0.96f, 0.91f), // 0
            new(1.00f, 0.84f, 0.24f), // 20 yellow
            new(1.00f, 0.50f, 0.18f), // 30 orange
            new(0.90f, 0.16f, 0.20f), // 40 red
            new(0.96f, 0.28f, 0.57f), // 50 pink
            new(0.19f, 0.04f, 0.31f)  // 64 deep purple
        };
        private static readonly int[] Lengths = { 0, 20, 30, 40, 50, 64 };

        public static Color Evaluate(int remainingCharacters)
        {
            var length = Mathf.Clamp(remainingCharacters, 0, 64);
            for (var index = 1; index < Lengths.Length; index++)
            {
                if (length > Lengths[index]) continue;
                var t = Mathf.InverseLerp(Lengths[index - 1], Lengths[index], length);
                return Color.Lerp(Stops[index - 1], Stops[index], t);
            }
            return Stops[^1];
        }

        public static bool UseDarkText(int remainingCharacters) => remainingCharacters <= 30;
    }
}
