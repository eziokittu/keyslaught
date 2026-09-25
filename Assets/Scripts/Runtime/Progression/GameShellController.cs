using KeySlaught.SceneGameplay;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace KeySlaught.Progression
{
    public enum GameModeSelection { None, Tutorial, LoreOneLevelOne, Endless }

    public sealed class GameShellController : MonoBehaviour
    {
        [SerializeField] private ProgressionService progression;
        [SerializeField] private PermanentUpgradeApplier upgrades;
        [SerializeField] private WaveRunController run;
        [SerializeField] private GameObject shellRoot;
        [SerializeField] private GameObject launchPanel;
        [SerializeField] private GameObject launchActions;
        [SerializeField] private GameObject mainPanel;
        [SerializeField] private GameObject modePanel;
        [SerializeField] private GameObject researchPanel;
        [SerializeField] private GameObject creditsPanel;
        [SerializeField] private GameObject settingsPanel;
        [SerializeField] private GameObject loreLevelsPanel;
        [SerializeField] private Text loreOneLevelOneStarsLabel;
        [SerializeField] private Text tutorialLabel;
        [SerializeField] private Button endlessButton;
        [SerializeField] private Text endlessLabel;
        [SerializeField] private Text knowledgeLabel;
        [SerializeField] private Button[] researchButtons;
        [SerializeField] private Text[] researchLabels;
        [SerializeField] private Text musicLabel;
        [SerializeField] private Text sfxLabel;
        [SerializeField] private Text selectionPauseLabel;
        [SerializeField] private LevelDefinition tutorialLevel;
        [SerializeField] private LevelDefinition loreOneLevelOne;
        [SerializeField] private LevelDefinition endlessLevel;
        [SerializeField] private TutorialDirector tutorialDirector;
        [SerializeField, Min(0f)] private float launchDelay = 2f;

        private float launchTimer;
        private bool musicEnabled = true;
        private bool sfxEnabled = true;

        public GameModeSelection ActiveMode { get; private set; }

        public void Configure(ProgressionService service, PermanentUpgradeApplier applier,
            WaveRunController runController, GameObject root, GameObject launch, GameObject actions,
            GameObject main, GameObject modes, GameObject research, GameObject credits, GameObject settings,
            Text tutorial, Button endless, Text endlessText, Text knowledge,
            Button[] researchActionButtons, Text[] researchActionLabels, Text music, Text sfx)
        {
            progression = service; upgrades = applier; run = runController; shellRoot = root;
            launchPanel = launch; launchActions = actions; mainPanel = main; modePanel = modes;
            researchPanel = research; creditsPanel = credits; settingsPanel = settings;
            tutorialLabel = tutorial; endlessButton = endless; endlessLabel = endlessText;
            knowledgeLabel = knowledge; researchButtons = researchActionButtons; researchLabels = researchActionLabels;
            musicLabel = music; sfxLabel = sfx;
        }

        public void ShowLaunch() { ShowOnly(launchPanel); launchTimer = launchDelay; if (launchActions != null) launchActions.SetActive(false); }
        public void ContinueFromLaunch() { progression?.MarkLaunchSeen(); ShowMain(); }
        public void ShowMain() { ShowOnly(mainPanel); Refresh(); }
        public void ShowModes() { ShowOnly(modePanel); Refresh(); }
        public void ShowResearch() { ShowOnly(researchPanel); Refresh(); }
        public void ShowCredits() => ShowOnly(creditsPanel);
        public void ShowSettings() { ShowOnly(settingsPanel); Refresh(); }
        public void ShowLoreLevels() { ShowOnly(loreLevelsPanel); Refresh(); }
        public void ExitGame() => Application.Quit();
        public void StartTutorial() => BeginRun(GameModeSelection.Tutorial);
        public void StartLoreOneLevelOne()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene("LoreOneLevelOne");
        }
        public void StartEndless() { if (progression != null && progression.Profile.endlessModeUnlocked) BeginRun(GameModeSelection.Endless); }
        public void ReturnToMainFromRun()
        {
            run?.RestartRun();
            run?.SetMenuSuspended(true);
            ActiveMode = GameModeSelection.None;
            if (shellRoot != null) shellRoot.SetActive(true);
            ShowMain();
        }
        public void PurchaseResearch(int index)
        {
            if (progression == null || progression.Definitions == null || index < 0 || index >= progression.Definitions.Length) return;
            progression.TryPurchase(progression.Definitions[index].Id);
            Refresh();
        }
        public void ToggleMusic() { musicEnabled = !musicEnabled; PlayerPrefs.SetInt("KeySlaught.Music", musicEnabled ? 1 : 0); Refresh(); }
        public void ToggleSfx() { sfxEnabled = !sfxEnabled; PlayerPrefs.SetInt("KeySlaught.Sfx", sfxEnabled ? 1 : 0); Refresh(); }
        public void ToggleSelectionPause() { TileContextActionPanel.ConfirmInteractions = !TileContextActionPanel.ConfirmInteractions; Refresh(); }

        public void ConfigureLevelContent(LevelDefinition tutorial, LevelDefinition lore, LevelDefinition endless,
            TutorialDirector director, Text selectionSettingLabel)
        {
            tutorialLevel = tutorial;
            loreOneLevelOne = lore;
            endlessLevel = endless;
            tutorialDirector = director;
            selectionPauseLabel = selectionSettingLabel;
        }

        public void ConfigureLoreSelection(GameObject levelsPanel, Text levelOneStarsLabel)
        {
            loreLevelsPanel = levelsPanel;
            loreOneLevelOneStarsLabel = levelOneStarsLabel;
        }

        private void Start()
        {
            musicEnabled = PlayerPrefs.GetInt("KeySlaught.Music", 1) != 0;
            sfxEnabled = PlayerPrefs.GetInt("KeySlaught.Sfx", 1) != 0;
            if (run != null) run.RunEnded += OnRunEnded;
            run?.SetMenuSuspended(true);
            if (shellRoot != null) shellRoot.SetActive(true);
            if (PlayerPrefs.GetInt("KeySlaught.ReturnToMainMenu", 0) != 0)
            {
                PlayerPrefs.DeleteKey("KeySlaught.ReturnToMainMenu");
                ShowMain();
            }
            else ShowLaunch();
        }

        private void OnDestroy() { if (run != null) run.RunEnded -= OnRunEnded; }

        private void Update()
        {
            if (launchActions == null || launchActions.activeSelf) return;
            launchTimer -= Time.unscaledDeltaTime;
            if (launchTimer <= 0f) launchActions.SetActive(true);
        }

        private void BeginRun(GameModeSelection mode)
        {
            ActiveMode = mode;
            upgrades?.Apply();
            run?.SetLevel(mode switch
            {
                GameModeSelection.Tutorial => tutorialLevel,
                GameModeSelection.LoreOneLevelOne => loreOneLevelOne,
                GameModeSelection.Endless => endlessLevel,
                _ => null
            });
            if (shellRoot != null) shellRoot.SetActive(false);
            run?.SetMenuSuspended(false);
            run?.RestartRun();
            if (mode == GameModeSelection.Tutorial) tutorialDirector?.Begin();
        }

        private void OnRunEnded(bool victory)
        {
            if (!victory || progression == null) return;
            if (ActiveMode == GameModeSelection.Tutorial) progression.CompleteTutorial();
            if (ActiveMode == GameModeSelection.LoreOneLevelOne)
            {
                progression.CompleteLoreOneLevelOne();
                progression.CreditKnowledge(1);
            }
        }

        private void ShowOnly(GameObject target)
        {
            foreach (var panel in new[] { launchPanel, mainPanel, modePanel, researchPanel, creditsPanel, settingsPanel, loreLevelsPanel })
                if (panel != null) panel.SetActive(panel == target);
        }

        private void Refresh()
        {
            if (progression == null || progression.Profile == null) return;
            var profile = progression.Profile;
            if (tutorialLabel != null) tutorialLabel.text = profile.tutorialCompleted ? "TUTORIAL" : "TUTORIAL  •  RECOMMENDED";
            if (endlessButton != null) endlessButton.interactable = profile.endlessModeUnlocked;
            if (endlessLabel != null) endlessLabel.text = profile.endlessModeUnlocked ? "ENDLESS  •  IN DEVELOPMENT" : "ENDLESS  •  LOCKED";
            if (knowledgeLabel != null) knowledgeLabel.text = $"KNOWLEDGE POINTS  {profile.knowledgePoints}";
            if (progression.Definitions != null)
                for (var index = 0; index < progression.Definitions.Length && index < researchLabels.Length; index++)
                {
                    var definition = progression.Definitions[index];
                    var level = profile.GetLevel(definition.Id);
                    var maxed = level >= definition.MaxLevel;
                    researchLabels[index].text = maxed ? $"{definition.DisplayName}  LV {level}  •  MAX" :
                        $"{definition.DisplayName}  LV {level}  •  {definition.CostForLevel(level)}";
                    researchButtons[index].interactable = !maxed && profile.knowledgePoints >= definition.CostForLevel(level);
                }
            if (musicLabel != null) musicLabel.text = $"MUSIC  {(musicEnabled ? "ON" : "OFF")}";
            if (sfxLabel != null) sfxLabel.text = $"SFX  {(sfxEnabled ? "ON" : "OFF")}";
            if (selectionPauseLabel != null) selectionPauseLabel.text = TileContextActionPanel.ConfirmInteractions
                ? "ACTION CONFIRMATIONS  ON" : "ACTION CONFIRMATIONS  OFF";
            if (loreOneLevelOneStarsLabel != null)
            {
                var stars = Mathf.Clamp(profile.loreOneLevelOneStars, 0, 3);
                var rating = new string('★', stars) + new string('☆', 3 - stars);
                loreOneLevelOneStarsLabel.text = profile.loreOneLevelOneCompleted
                    ? $"LEVEL 1  {rating}  BEST {FormatTime(profile.loreOneLevelOneBestSeconds)}"
                    : $"LEVEL 1  {rating}";
            }
        }

        private static string FormatTime(float seconds)
        {
            if (seconds <= 0f) return "--:--";
            var total = Mathf.FloorToInt(seconds);
            return $"{total / 60:00}:{total % 60:00}";
        }
    }
}
