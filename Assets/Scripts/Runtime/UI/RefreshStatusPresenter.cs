using KeySlaught.SceneGameplay;
using UnityEngine;
using UnityEngine.UI;

namespace KeySlaught.UI
{
    public sealed class RefreshStatusPresenter : MonoBehaviour
    {
        [SerializeField] private GameplayCombatController combat;
        [SerializeField] private GameObject panel;
        [SerializeField] private Text label;

        public void Configure(GameplayCombatController controller, GameObject root, Text status)
        {
            combat = controller;
            panel = root;
            label = status;
            Refresh();
        }

        private void Update() => Refresh();

        private void Refresh()
        {
            var visible = combat != null && combat.IsRefreshing;
            if (panel != null) panel.SetActive(visible);
            if (visible && label != null)
                label.text = $"WAND BUFFER RESETTING  {combat.RefreshSecondsRemaining:0.0}s";
        }
    }
}
