using UnityEngine;
using UnityEngine.UI;

namespace KeySlaught.SceneGameplay
{
    public sealed class IntermissionFastForwardButton : MonoBehaviour
    {
        [SerializeField] private WaveRunController run;
        [SerializeField] private Button button;

        public void Configure(WaveRunController controller, Button target)
        {
            run = controller;
            button = target;
        }

        private void Update()
        {
            if (button != null) button.gameObject.SetActive(run != null && run.Phase == WaveRunPhase.Intermission);
        }
    }
}
