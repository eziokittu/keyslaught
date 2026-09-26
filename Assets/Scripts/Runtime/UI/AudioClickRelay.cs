using KeySlaught.Audio;
using UnityEngine;

namespace KeySlaught.UI
{
    public sealed class AudioClickRelay : MonoBehaviour
    {
        public void PlayClick() => PersistentAudioDirector.Play(KeySlaughtSound.UiClick);
    }
}
