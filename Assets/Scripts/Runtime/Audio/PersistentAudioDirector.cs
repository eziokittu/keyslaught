using UnityEngine;

namespace KeySlaught.Audio
{
    [DefaultExecutionOrder(-500)]
    public sealed class PersistentAudioDirector : MonoBehaviour
    {
        private const string MusicEnabledKey = "KeySlaught.Music";
        private const string SfxEnabledKey = "KeySlaught.Sfx";
        private const string MusicVolumeKey = "KeySlaught.MusicVolume";
        private const string SfxVolumeKey = "KeySlaught.SfxVolume";
        [SerializeField] private KeySlaughtAudioLibrary library;
        [SerializeField] private AudioSource musicSource;
        [SerializeField] private AudioSource sfxSource;
        public static PersistentAudioDirector Instance { get; private set; }
        public bool MusicEnabled => PlayerPrefs.GetInt(MusicEnabledKey, 1) != 0;
        public bool SfxEnabled => PlayerPrefs.GetInt(SfxEnabledKey, 1) != 0;
        public float MusicVolume => PlayerPrefs.GetFloat(MusicVolumeKey, 1f);
        public float SfxVolume => PlayerPrefs.GetFloat(SfxVolumeKey, 1f);

        public void Configure(KeySlaughtAudioLibrary value) { library = value; EnsureSources(); ApplySettings(); }
        public static void Play(KeySlaughtSound cue) => Instance?.PlayCue(cue);
        public void ToggleMusic() { SetMusicEnabled(!MusicEnabled); }
        public void ToggleSfx() { SetSfxEnabled(!SfxEnabled); }
        public void SetMusicEnabled(bool value) { PlayerPrefs.SetInt(MusicEnabledKey, value ? 1 : 0); ApplySettings(); }
        public void SetSfxEnabled(bool value) { PlayerPrefs.SetInt(SfxEnabledKey, value ? 1 : 0); }
        public void SetMusicVolume(float value) { PlayerPrefs.SetFloat(MusicVolumeKey, Mathf.Clamp01(value)); ApplySettings(); }
        public void SetSfxVolume(float value) { PlayerPrefs.SetFloat(SfxVolumeKey, Mathf.Clamp01(value)); }

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this; DontDestroyOnLoad(gameObject); EnsureSources(); ApplySettings();
        }

        private void EnsureSources()
        {
            if (musicSource == null) musicSource = gameObject.AddComponent<AudioSource>();
            if (sfxSource == null) sfxSource = gameObject.AddComponent<AudioSource>();
            musicSource.loop = true; musicSource.playOnAwake = false; sfxSource.playOnAwake = false;
        }

        private void ApplySettings()
        {
            if (library == null || musicSource == null) return;
            musicSource.clip = library.music; musicSource.volume = library.musicVolume * MusicVolume;
            musicSource.mute = !MusicEnabled;
            if (musicSource.clip != null && !musicSource.isPlaying) musicSource.Play();
        }

        private void PlayCue(KeySlaughtSound cue)
        {
            if (!SfxEnabled || library == null || sfxSource == null) return;
            var clip = library.Clip(cue); if (clip != null) sfxSource.PlayOneShot(clip, library.Volume(cue) * SfxVolume);
        }
    }
}
