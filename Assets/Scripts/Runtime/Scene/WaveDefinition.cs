using UnityEngine;

namespace KeySlaught.SceneGameplay
{
    [CreateAssetMenu(menuName = "KeySlaught/Wave Definition", fileName = "WaveDefinition")]
    public sealed class WaveDefinition : ScriptableObject
    {
        [SerializeField] private EnemyDefinition[] enemies;
        [SerializeField, Min(0.05f)] private float spawnInterval = 1.25f;
        [SerializeField] private bool bossWave;

        public EnemyDefinition[] Enemies => enemies;
        public float SpawnInterval => spawnInterval;
        public bool IsBossWave => bossWave;
    }
}
