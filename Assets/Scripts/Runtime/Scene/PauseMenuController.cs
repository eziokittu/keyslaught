using UnityEngine;
using KeySlaught.Progression;

namespace KeySlaught.SceneGameplay
{
    public sealed class PauseMenuController : MonoBehaviour
    {
        [SerializeField] private GameObject pauseOverlay;
        [SerializeField] private GameObject menuConfirmation;
        [SerializeField] private GameObject controlsPanel;
        [SerializeField] private GameShellController gameShell;
        [SerializeField] private DedicatedLevelSceneController dedicatedLevel;
        [SerializeField] private GameObject modalDimmer;
        [SerializeField] private GameObject settingsPanel;

        public bool IsPaused { get; private set; }

        public void Configure(GameObject overlay, GameObject confirmation, GameObject controls)
        {
            pauseOverlay = overlay;
            menuConfirmation = confirmation;
            controlsPanel = controls;
            SetPaused(false);
        }

        public void ConfigureModalDimmer(GameObject dimmer) => modalDimmer = dimmer;
        public void ConfigureSettings(GameObject settings) => settingsPanel = settings;

        public void TogglePause() => SetPaused(!IsPaused);

        public void Continue() => SetPaused(false);

        public void ShowMenuConfirmation()
        {
            if (menuConfirmation != null)
            {
                menuConfirmation.SetActive(true);
                SetModalDimmed(true);
            }
        }

        public void HideMenuConfirmation()
        {
            if (menuConfirmation != null)
            {
                menuConfirmation.SetActive(false);
                SetModalDimmed(controlsPanel != null && controlsPanel.activeSelf);
            }
        }

        public void ConfigureShell(GameShellController shell) => gameShell = shell;

        public void ConfigureDedicatedLevel(DedicatedLevelSceneController level) => dedicatedLevel = level;

        public void ConfirmBackToMenu()
        {
            SetPaused(false);
            if (dedicatedLevel != null) dedicatedLevel.ReturnToMainMenu();
            else gameShell?.ReturnToMainFromRun();
        }

        public void ToggleControls()
        {
            if (controlsPanel != null)
            {
                controlsPanel.SetActive(!controlsPanel.activeSelf);
                SetModalDimmed(controlsPanel.activeSelf);
            }
        }

        public void HideControls()
        {
            if (controlsPanel != null) controlsPanel.SetActive(false);
            SetModalDimmed(menuConfirmation != null && menuConfirmation.activeSelf);
        }

        public void ToggleSettings()
        {
            if (settingsPanel == null) return;
            settingsPanel.SetActive(!settingsPanel.activeSelf);
            SetModalDimmed(settingsPanel.activeSelf);
        }

        public void HideSettings()
        {
            if (settingsPanel != null) settingsPanel.SetActive(false);
            SetModalDimmed(menuConfirmation != null && menuConfirmation.activeSelf);
        }

        private void SetPaused(bool paused)
        {
            IsPaused = paused;
            if (paused) Time.timeScale = 0f;
            else GameSpeedSettings.ApplyGameplaySpeed();
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
                if (settingsPanel != null) settingsPanel.SetActive(false);
                SetModalDimmed(false);
            }
        }

        private void SetModalDimmed(bool visible)
        {
            if (modalDimmer != null) modalDimmer.SetActive(visible);
        }

        private void OnDisable()
        {
            if (IsPaused)
            {
                GameSpeedSettings.ApplyGameplaySpeed();
                IsPaused = false;
            }
        }
    }
}
