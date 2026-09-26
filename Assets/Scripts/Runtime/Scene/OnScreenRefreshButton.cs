using UnityEngine;
using UnityEngine.UI;

namespace KeySlaught.SceneGameplay
{
    [RequireComponent(typeof(Button))]
    public sealed class OnScreenRefreshButton : MonoBehaviour
    {
        [SerializeField] private GameplayCombatController combatController;

        public void Configure(GameplayCombatController controller)
        {
            combatController = controller;
        }

        private void Awake()
        {
            GetComponent<Button>().onClick.AddListener(Refresh);
        }

        private void Refresh()
        {
            if (TutorialInputGate.TryAllowRefresh()) combatController?.TryStartRefresh();
        }
    }
}
