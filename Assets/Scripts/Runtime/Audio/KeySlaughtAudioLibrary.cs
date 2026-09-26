using UnityEngine;

namespace KeySlaught.Audio
{
    public enum KeySlaughtSound
    {
        EnemyHit, LibraryHit, PlayerCorrupted, TurretPlaced, TurretFired,
        TurretSold, TurretUpgraded, UiClick, GameWin, GameLost, RoundStart
    }

    [CreateAssetMenu(menuName = "KeySlaught/Audio Library", fileName = "KeySlaughtAudioLibrary")]
    public sealed class KeySlaughtAudioLibrary : ScriptableObject
    {
        [Header("Persistent music")]
        public AudioClip music;
        [Range(0f, 1f)] public float musicVolume = .34f;
        [Header("Subtle instrument-inspired SFX and individual mix")]
        public AudioClip enemyHit; [Range(0f, 1f)] public float enemyHitVolume = .42f;
        public AudioClip libraryHit; [Range(0f, 1f)] public float libraryHitVolume = .55f;
        public AudioClip playerCorrupted; [Range(0f, 1f)] public float playerCorruptedVolume = .58f;
        public AudioClip turretPlaced; [Range(0f, 1f)] public float turretPlacedVolume = .48f;
        public AudioClip turretFired; [Range(0f, 1f)] public float turretFiredVolume = .24f;
        public AudioClip turretSold; [Range(0f, 1f)] public float turretSoldVolume = .42f;
        public AudioClip turretUpgraded; [Range(0f, 1f)] public float turretUpgradedVolume = .5f;
        public AudioClip uiClick; [Range(0f, 1f)] public float uiClickVolume = .28f;
        public AudioClip gameWin; [Range(0f, 1f)] public float gameWinVolume = .64f;
        public AudioClip gameLost; [Range(0f, 1f)] public float gameLostVolume = .58f;
        public AudioClip roundStart; [Range(0f, 1f)] public float roundStartVolume = .48f;

        public AudioClip Clip(KeySlaughtSound cue) => cue switch
        {
            KeySlaughtSound.EnemyHit => enemyHit, KeySlaughtSound.LibraryHit => libraryHit,
            KeySlaughtSound.PlayerCorrupted => playerCorrupted, KeySlaughtSound.TurretPlaced => turretPlaced,
            KeySlaughtSound.TurretFired => turretFired, KeySlaughtSound.TurretSold => turretSold,
            KeySlaughtSound.TurretUpgraded => turretUpgraded, KeySlaughtSound.UiClick => uiClick,
            KeySlaughtSound.GameWin => gameWin, KeySlaughtSound.GameLost => gameLost,
            KeySlaughtSound.RoundStart => roundStart, _ => null
        };

        public float Volume(KeySlaughtSound cue) => cue switch
        {
            KeySlaughtSound.EnemyHit => enemyHitVolume, KeySlaughtSound.LibraryHit => libraryHitVolume,
            KeySlaughtSound.PlayerCorrupted => playerCorruptedVolume, KeySlaughtSound.TurretPlaced => turretPlacedVolume,
            KeySlaughtSound.TurretFired => turretFiredVolume, KeySlaughtSound.TurretSold => turretSoldVolume,
            KeySlaughtSound.TurretUpgraded => turretUpgradedVolume, KeySlaughtSound.UiClick => uiClickVolume,
            KeySlaughtSound.GameWin => gameWinVolume, KeySlaughtSound.GameLost => gameLostVolume,
            KeySlaughtSound.RoundStart => roundStartVolume, _ => 1f
        };
    }
}
