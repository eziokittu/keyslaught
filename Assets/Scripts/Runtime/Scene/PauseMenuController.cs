using UnityEngine;

namespace KeySlaught.SceneGameplay
{
    public sealed class PauseMenuController : MonoBehaviour
    {
        [SerializeField] private GameObject pauseOverlay;
        [SerializeField] private GameObject menuConfirmation;
        [SerializeField] private GameObject controlsPanel;

        public bool IsPaused { get; private set; }

        public void Configure(GameObject overlay, GameObject confirmation, GameObject controls)
        {
            pauseOverlay = overlay;
            menuConfirmation = confirmation;
            controlsPanel = controls;
            SetPaused(false);
        }

        public void TogglePause() => SetPaused(!IsPaused);

        public void Continue() => SetPaused(false);

        public void ShowMenuConfirmation()
        {
            if (menuConfirmation != null)
            {
                menuConfirmation.SetActive(true);
            }
        }

        public void HideMenuConfirmation()
        {
            if (menuConfirmation != null)
            {
                menuConfirmation.SetActive(false);
            }
        }

        public void ToggleControls()
        {
            if (controlsPanel != null)
            {
                controlsPanel.SetActive(!controlsPanel.activeSelf);
            }
        }

        private void SetPaused(bool paused)
        {
            IsPaused = paused;
            Time.timeScale = paused ? 0f : 1f;
            if (pauseOverlay != null)
            {
                pauseOverlay.SetActive(paused);
            }

            if (!paused)
            {
                HideMenuConfirmation();
                if (controlsPanel != null)
                {
                    controlsPanel.SetActive(false);
                }
            }
        }

        private void OnDisable()
        {
            if (IsPaused)
            {
                Time.timeScale = 1f;
                IsPaused = false;
            }
        }
    }
}
