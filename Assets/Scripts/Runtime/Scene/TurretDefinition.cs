using System;
using UnityEngine;

namespace KeySlaught.SceneGameplay
{
    [Serializable]
    public sealed class TurretVariantDefinition
    {
        [SerializeField] private string displayName = "A-Z";
        [SerializeField] private string coveredLetters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

        public string DisplayName => string.IsNullOrWhiteSpace(displayName) ? coveredLetters : displayName;
        public string CoveredLetters => coveredLetters ?? string.Empty;
        public bool Covers(char letter) => CoveredLetters.IndexOf(char.ToUpperInvariant(letter)) >= 0;
    }

    [CreateAssetMenu(menuName = "KeySlaught/Turret Definition", fileName = "TurretDefinition")]
    public sealed class TurretDefinition : ScriptableObject
    {
        [SerializeField] private TurretKind kind;
        [SerializeField] private string coveredLetters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        [SerializeField] private TurretVariantDefinition[] variants = Array.Empty<TurretVariantDefinition>();
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
        public int VariantCount => variants == null || variants.Length == 0 ? 1 : variants.Length;
        public string VariantName(int index) => variants == null || variants.Length == 0
            ? (string.IsNullOrWhiteSpace(coveredLetters) ? "A-Z" : coveredLetters)
            : variants[Mathf.Clamp(index, 0, variants.Length - 1)].DisplayName;
        public bool Covers(int variantIndex, char letter) => variants == null || variants.Length == 0
            ? coveredLetters != null && coveredLetters.IndexOf(char.ToUpperInvariant(letter)) >= 0
            : variants[Mathf.Clamp(variantIndex, 0, variants.Length - 1)].Covers(letter);
        public bool Covers(char letter) => Covers(0, letter);
    }
}
