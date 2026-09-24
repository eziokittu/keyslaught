using UnityEngine;

namespace KeySlaught.SceneGameplay
{
    [CreateAssetMenu(menuName = "KeySlaught/Turret Definition", fileName = "TurretDefinition")]
    public sealed class TurretDefinition : ScriptableObject
    {
        [SerializeField] private TurretKind kind;
        [SerializeField] private string coveredLetters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        [SerializeField, Min(0.1f)] private float range = 3f;
        [SerializeField, Min(0.05f)] private float secondsPerShot = 1f;
        [SerializeField, Min(0)] private int buildCost = 6;
        [SerializeField, Min(0f)] private float rangePerUpgrade = 0.35f;
        [SerializeField, Range(0.1f, 1f)] private float cadenceMultiplierPerUpgrade = 0.85f;

        public TurretKind Kind => kind;
        public string CoveredLetters => coveredLetters;
        public float RangeAtLevel(int level) => range + Mathf.Max(0, level - 1) * rangePerUpgrade;
        public float SecondsPerShotAtLevel(int level) => secondsPerShot * Mathf.Pow(cadenceMultiplierPerUpgrade, Mathf.Max(0, level - 1));
        public int BuildCost => buildCost;
        public bool Covers(char letter) => coveredLetters != null && coveredLetters.IndexOf(char.ToUpperInvariant(letter)) >= 0;
    }
}
