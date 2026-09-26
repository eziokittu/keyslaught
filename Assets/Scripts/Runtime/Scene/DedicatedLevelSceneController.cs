using KeySlaught.Progression;
using KeySlaught.UI;
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
        [SerializeField, Min(1)] private int loreLevelNumber = 1;
        [SerializeField] private RunResultPresenter resultPresenter;
        private bool completionRecorded;

        public LevelDefinition Level => level;
        public int LoreLevelNumber => loreLevelNumber;

        public void Configure(WaveRunController runController, ProgressionService progressionService,
            PermanentUpgradeApplier upgradeApplier, LevelDefinition levelDefinition,
            string menuSceneName = "SampleScene")
        {
            run = runController; progression = progressionService; upgrades = upgradeApplier;
            level = levelDefinition; mainMenuSceneName = menuSceneName;
        }

        public void SetLoreLevelNumber(int value) => loreLevelNumber = Mathf.Max(1, value);
        public void SetLevelDefinition(LevelDefinition value) => level = value;
        public void ConfigureResultPresenter(RunResultPresenter presenter) => resultPresenter = presenter;

        public void ReturnToMainMenu()
        {
            GameSpeedSettings.ApplyMenuSpeed();
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
            run.WaveStarted += OnWaveStarted;
            upgrades?.Apply();
            run.RestartRun();
        }

        private void OnDestroy()
        {
            if (run != null) run.RunEnded -= OnRunEnded;
            if (run != null) run.WaveStarted -= OnWaveStarted;
        }

        private void OnRunEnded(bool victory)
        {
            RecordCompletion(victory);
        }

        public void RecordCompletion(bool victory)
        {
            if (completionRecorded || progression == null) return;
            completionRecorded = true;
            if (!victory)
            {
                resultPresenter?.Present(false, 0, null, false);
                return;
            }
            var wasUnlocked = progression.UnlockedTurretCount;
            progression.CompleteLoreLevel(loreLevelNumber,
                ProgressionService.CalculateLoreStars(run == null ? 0 : run.LibraryHitCount, run == null ? 0f : run.ElapsedSeconds),
                run == null ? 0f : run.ElapsedSeconds);
            var reward = 1 + (run == null ? 0 : run.BossesDefeated);
            progression.CreditKnowledge(reward);
            var unlocked = progression.UnlockedTurretCount > wasUnlocked ? loreLevelNumber switch
            {
                1 => "Engineer",
                2 => "Scientist",
                _ => "President"
            } : null;
            resultPresenter?.Present(true, reward, unlocked, loreLevelNumber < 6);
        }

        public void ContinueFromResult()
        {
            if (run != null && run.Phase == WaveRunPhase.Defeat)
            {
                completionRecorded = false;
                run.RestartRun();
                return;
            }
            if (loreLevelNumber < 6)
            {
                GameSpeedSettings.ApplyGameplaySpeed();
                SceneManager.LoadScene(NextSceneName(loreLevelNumber + 1));
            }
            else ReturnToMainMenu();
        }

        private static string NextSceneName(int level) => level switch
        {
            2 => "LoreOneLevelTwo",
            3 => "LoreOneLevelThree",
            4 => "LoreOneLevelFour",
            5 => "LoreOneLevelFive",
            _ => "LoreOneLevelSix"
        };

        private void OnWaveStarted(int number, bool boss)
        {
            if (number == 1) completionRecorded = false;
        }
    }
}
