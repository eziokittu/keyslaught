using System;
using UnityEngine;

namespace KeySlaught.SceneGameplay
{
    [Serializable]
    public sealed class LevelWordEntry
    {
        [SerializeField] private string word = "BOOK";
        [SerializeField, Min(0f)] private float delayAfterPrevious = 1.5f;

        public string Word
        {
            get
            {
                var normalized = string.IsNullOrWhiteSpace(word) ? "BOOK" : word.Trim().ToUpperInvariant();
                return normalized.Length <= 64 ? normalized : normalized.Substring(0, 64);
            }
        }
        public float DelayAfterPrevious => Mathf.Max(0f, delayAfterPrevious);
        internal void SetDelay(float value) => delayAfterPrevious = Mathf.Max(0f, value);
    }

    [Serializable]
    public sealed class LevelWaveEntry
    {
        [SerializeField] private string title = "WAVE";
        [SerializeField] private LevelWordEntry[] words = Array.Empty<LevelWordEntry>();
        [Tooltip("Toggle once to evenly assign the spare wave time as delays, then it turns itself off so every delay stays editable.")]
        [SerializeField] private bool normalizeDelays;
        [SerializeField, Min(1f)] private float averageTypingWordsPerMinute = 24f;
        [SerializeField, Min(1f)] private float targetWaveEndSeconds = 30f;
        [SerializeField, Min(0.01f)] private float enemyMovementSpeed = 0.8625f;

        public string Title => string.IsNullOrWhiteSpace(title) ? "WAVE" : title;
        public LevelWordEntry[] Words => words;
        public float EnemyMovementSpeed => enemyMovementSpeed;

        public void NormalizeDelaysIfRequested()
        {
            if (!normalizeDelays) return;
            normalizeDelays = false;
            if (words == null || words.Length == 0) return;

            var letters = 0;
            foreach (var entry in words) letters += entry == null ? 0 : entry.Word.Length;
            var estimatedTypingSeconds = letters * (60f / (Mathf.Max(1f, averageTypingWordsPerMinute) * 5f));
            var delay = Mathf.Max(0f, (targetWaveEndSeconds - estimatedTypingSeconds) / words.Length);
            foreach (var entry in words)
            {
                if (entry == null) continue;
                entry.SetDelay(delay);
            }
        }
    }

    [CreateAssetMenu(menuName = "KeySlaught/Level Definition", fileName = "LevelDefinition")]
    public sealed class LevelDefinition : ScriptableObject
    {
        [SerializeField] private string displayName = "LORE LEVEL";
        [SerializeField] private LevelWaveEntry[] waves = Array.Empty<LevelWaveEntry>();
        [SerializeField, Min(0f)] private float intermissionSeconds = 30f;

        public string DisplayName => displayName;
        public LevelWaveEntry[] Waves => waves;
        public float IntermissionSeconds => intermissionSeconds;

        private void OnValidate()
        {
            if (waves == null) return;
            foreach (var wave in waves) wave?.NormalizeDelaysIfRequested();
        }
    }
}
