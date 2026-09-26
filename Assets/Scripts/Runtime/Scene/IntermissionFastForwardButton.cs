using UnityEngine;
using UnityEngine.UI;

namespace KeySlaught.SceneGameplay
{
    public sealed class IntermissionFastForwardButton : MonoBehaviour
    {
        [SerializeField] private WaveRunController run;
        [SerializeField] private Button button;
        private CanvasGroup group;

        public void Configure(WaveRunController controller, Button target)
        {
            run = controller;
            button = target;
            EnsureGroup();
            Refresh();
        }

        private void Awake() => EnsureGroup();
        private void OnEnable() => Refresh();
        private void Update() => Refresh();

        private void EnsureGroup()
        {
            if (button == null) button = GetComponent<Button>();
            group = GetComponent<CanvasGroup>();
            if (group == null) group = gameObject.AddComponent<CanvasGroup>();
        }

        private void Refresh()
        {
            EnsureGroup();
            var visible = run != null && run.Phase == WaveRunPhase.Intermission;
            group.alpha = visible ? 1f : 0f;
            group.interactable = visible;
            group.blocksRaycasts = visible;
        }
    }
}
