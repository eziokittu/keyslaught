using KeySlaught.Audio;
using UnityEngine;
using UnityEngine.UI;

namespace KeySlaught.UI
{
    public sealed class AudioSettingsPanel : MonoBehaviour
    {
        [SerializeField] private Text musicState;
        [SerializeField] private Text sfxState;
        [SerializeField] private Slider musicVolume;
        [SerializeField] private Slider sfxVolume;
        public void Configure(Text music, Text sfx, Slider musicSlider, Slider sfxSlider)
        { musicState = music; sfxState = sfx; musicVolume = musicSlider; sfxVolume = sfxSlider; Refresh(); }
        public void ToggleMusic() { PersistentAudioDirector.Instance?.ToggleMusic(); Refresh(); }
        public void ToggleSfx() { PersistentAudioDirector.Instance?.ToggleSfx(); Refresh(); }
        public void SetMusicVolume(float value) { PersistentAudioDirector.Instance?.SetMusicVolume(value); Refresh(); }
        public void SetSfxVolume(float value) { PersistentAudioDirector.Instance?.SetSfxVolume(value); Refresh(); }
        private void OnEnable() => Refresh();
        private void Refresh()
        {
            var audio = PersistentAudioDirector.Instance; if (audio == null) return;
            if (musicState != null) musicState.text = audio.MusicEnabled ? "MUSIC  ON" : "MUSIC  OFF";
            if (sfxState != null) sfxState.text = audio.SfxEnabled ? "SFX  ON" : "SFX  OFF";
            if (musicVolume != null) musicVolume.SetValueWithoutNotify(audio.MusicVolume);
            if (sfxVolume != null) sfxVolume.SetValueWithoutNotify(audio.SfxVolume);
        }
    }
}
