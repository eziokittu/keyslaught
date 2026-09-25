using KeySlaught.Progression;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace KeySlaught.SceneGameplay
{
    [DefaultExecutionOrder(-100)]
    public sealed class DedicatedLevelSceneController : MonoBehaviour
    {
        [SerializeField] private WaveRunController run;
        [SerializeField] private ProgressionService progression;
        [SerializeField] private PermanentUpgradeApplier upgrades;
        [SerializeField] private LevelDefinition level;
        [SerializeField] private string mainMenuSceneName = "SampleScene";
        private bool completionRecorded;

        public LevelDefinition Level => level;

        public void Configure(WaveRunController runController, ProgressionService progressionService,
            PermanentUpgradeApplier upgradeApplier, LevelDefinition levelDefinition,
            string menuSceneName = "SampleScene")
        {
            run = runController; progression = progressionService; upgrades = upgradeApplier;
            level = levelDefinition; mainMenuSceneName = menuSceneName;
        }

        public void ReturnToMainMenu()
        {
            Time.timeScale = 1f;
            PlayerPrefs.SetInt("KeySlaught.ReturnToMainMenu", 1);
            SceneManager.LoadScene(mainMenuSceneName);
        }

        private void Awake()
        {
            run?.SetLevel(level);
            run?.SetMenuSuspended(false);
        }

        private void Start()
        {
            if (run == null) return;
            run.RunEnded += OnRunEnded;
            upgrades?.Apply();
            run.RestartRun();
        }

        private void OnDestroy()
        {
            if (run != null) run.RunEnded -= OnRunEnded;
        }

        private void OnRunEnded(bool victory)
        {
            RecordCompletion(victory);
        }

        public void RecordCompletion(bool victory)
        {
            if (!victory || completionRecorded || progression == null) return;
            completionRecorded = true;
            progression.CompleteLoreOneLevelOne(
                ProgressionService.CalculateLoreStars(run == null ? 0 : run.LibraryHitCount, run == null ? 0f : run.ElapsedSeconds),
                run == null ? 0f : run.ElapsedSeconds);
            progression.CreditKnowledge(1);
        }
    }
}
