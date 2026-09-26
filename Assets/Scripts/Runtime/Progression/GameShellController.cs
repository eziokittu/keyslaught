using KeySlaught.SceneGameplay;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using KeySlaught.Audio;
using KeySlaught.UI;

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
        [SerializeField] private CanvasGroup launchActionsCanvasGroup;
        [SerializeField] private GameObject exitConfirmationPanel;
        [SerializeField] private GameObject mainPanel;
        [SerializeField] private GameObject modePanel;
        [SerializeField] private GameObject researchPanel;
        [SerializeField] private GameObject creditsPanel;
        [SerializeField] private GameObject settingsPanel;
        [SerializeField] private GameObject loreLevelsPanel;
        [SerializeField] private Text loreOneLevelOneStarsLabel;
        [SerializeField] private Button loreOneLevelTwoButton;
        [SerializeField] private Text loreOneLevelTwoStarsLabel;
        [SerializeField] private Button loreOneLevelThreeButton;
        [SerializeField] private Text loreOneLevelThreeStarsLabel;
        [SerializeField] private Button[] loreLevelButtons;
        [SerializeField] private Text[] loreLevelLabels;
        [SerializeField] private Text tutorialLabel;
        [SerializeField] private Button endlessButton;
        [SerializeField] private Text endlessLabel;
        [SerializeField] private Text knowledgeLabel;
        [SerializeField] private Text researchKnowledgeLabel;
        [SerializeField] private Text brainCellsLabel;
        [SerializeField] private Button[] researchButtons;
        [SerializeField] private Text[] researchLabels;
        [SerializeField] private Text musicLabel;
        [SerializeField] private Text sfxLabel;
        [SerializeField] private Text selectionPauseLabel;
        [SerializeField] private LevelDefinition tutorialLevel;
        [SerializeField] private LevelDefinition loreOneLevelOne;
        [SerializeField] private LevelDefinition endlessLevel;
        [SerializeField] private TutorialDirector tutorialDirector;
        [SerializeField] private RunResultPresenter resultPresenter;
        [SerializeField] private Text[] gameSpeedLabels;
        [SerializeField, Min(0f)] private float launchDelay = 2f;
        [SerializeField, Min(.05f)] private float launchFadeDuration = .5f;

        private float launchTimer;
        private float launchFadeTimer;
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

        public void ShowLaunch()
        {
            GameSpeedSettings.ApplyMenuSpeed();
            ShowOnly(launchPanel);
            launchTimer = launchDelay;
            launchFadeTimer = 0f;
            if (launchActions != null) launchActions.SetActive(true);
            if (launchActionsCanvasGroup != null)
            {
                launchActionsCanvasGroup.alpha = 0f;
                launchActionsCanvasGroup.interactable = false;
                launchActionsCanvasGroup.blocksRaycasts = false;
            }
            if (exitConfirmationPanel != null) exitConfirmationPanel.SetActive(false);
        }
        public void ContinueFromLaunch() { progression?.MarkLaunchSeen(); ShowMain(); }
        public void ShowMain() { ShowOnly(mainPanel); Refresh(); }
        public void ShowModes() { ShowOnly(modePanel); Refresh(); }
        public void ShowResearch() { ShowOnly(researchPanel); Refresh(); }
        public void ShowCredits() => ShowOnly(creditsPanel);
        public void ShowSettings() { ShowOnly(settingsPanel); Refresh(); }
        public void ShowLoreLevels() { ShowOnly(loreLevelsPanel); Refresh(); }
        public void ExitGame()
        {
            if (exitConfirmationPanel != null) exitConfirmationPanel.SetActive(true);
            else ConfirmExitGame();
        }
        public void HideExitConfirmation() { if (exitConfirmationPanel != null) exitConfirmationPanel.SetActive(false); }
        public void ConfirmExitGame() => Application.Quit();
        public void StartTutorial() => BeginRun(GameModeSelection.Tutorial);
        public void StartLoreOneLevelOne()
        {
            ShowLoreLevels();
        }
        public void LoadLoreOneLevelOne()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene("LoreOneLevelOne");
        }
        public void StartLoreOneLevelTwo() { Time.timeScale = 1f; SceneManager.LoadScene("LoreOneLevelTwo"); }
        public void StartLoreOneLevelThree() { Time.timeScale = 1f; SceneManager.LoadScene("LoreOneLevelThree"); }
        public void StartLoreOneLevelFour() { Time.timeScale = 1f; SceneManager.LoadScene("LoreOneLevelFour"); }
        public void StartLoreOneLevelFive() { Time.timeScale = 1f; SceneManager.LoadScene("LoreOneLevelFive"); }
        public void StartLoreOneLevelSix() { Time.timeScale = 1f; SceneManager.LoadScene("LoreOneLevelSix"); }
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
        public void ToggleMusic() { musicEnabled = !musicEnabled; PlayerPrefs.SetInt("KeySlaught.Music", musicEnabled ? 1 : 0); PersistentAudioDirector.Instance?.SetMusicEnabled(musicEnabled); Refresh(); }
        public void ToggleSfx() { sfxEnabled = !sfxEnabled; PlayerPrefs.SetInt("KeySlaught.Sfx", sfxEnabled ? 1 : 0); PersistentAudioDirector.Instance?.SetSfxEnabled(sfxEnabled); Refresh(); }
        public void ToggleSelectionPause() { TileContextActionPanel.ConfirmInteractions = !TileContextActionPanel.ConfirmInteractions; Refresh(); }
        public void SetGameSpeed(int multiplier) { GameSpeedSettings.SetMultiplier(multiplier); Refresh(); }
        public void SetGameSpeed1() => SetGameSpeed(1);
        public void SetGameSpeed2() => SetGameSpeed(2);
        public void SetGameSpeed3() => SetGameSpeed(3);

        public void ConfigureLaunchPresentation(CanvasGroup actionsGroup, GameObject confirmationPanel)
        { launchActionsCanvasGroup = actionsGroup; exitConfirmationPanel = confirmationPanel; }

        public void ConfigureProgressionHeader(Text brainCells, Text knowledge)
        { brainCellsLabel = brainCells; knowledgeLabel = knowledge; }

        public void ConfigureResearchKnowledgeLabel(Text knowledge) => researchKnowledgeLabel = knowledge;

        public void ConfigureSpeedLabels(Text one, Text two, Text three)
        { gameSpeedLabels = new[] { one, two, three }; }

        public void ConfigureResultPresenter(RunResultPresenter presenter) => resultPresenter = presenter;

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

        public void ConfigureLoreSequence(Button levelTwo, Text levelTwoLabel, Button levelThree, Text levelThreeLabel)
        { loreOneLevelTwoButton = levelTwo; loreOneLevelTwoStarsLabel = levelTwoLabel; loreOneLevelThreeButton = levelThree; loreOneLevelThreeStarsLabel = levelThreeLabel; }

        public void ConfigureLoreLevels(Button[] buttons, Text[] labels)
        {
            loreLevelButtons = buttons;
            loreLevelLabels = labels;
        }

        public void ConfigureResearchUi(Button[] buttons, Text[] labels)
        {
            researchButtons = buttons;
            researchLabels = labels;
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
            if (launchActionsCanvasGroup == null || launchActionsCanvasGroup.alpha >= 1f) return;
            if (launchPanel == null || !launchPanel.activeInHierarchy) return;
            launchTimer -= Time.unscaledDeltaTime;
            if (launchTimer > 0f) return;
            launchFadeTimer += Time.unscaledDeltaTime;
            launchActionsCanvasGroup.alpha = Mathf.Clamp01(launchFadeTimer / launchFadeDuration);
            var ready = launchActionsCanvasGroup.alpha >= 1f;
            launchActionsCanvasGroup.interactable = ready;
            launchActionsCanvasGroup.blocksRaycasts = ready;
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
            if (progression == null) return;
            if (!victory)
            {
                resultPresenter?.Present(false, 0, null, false);
                return;
            }
            var previousUnlockCount = progression.UnlockedTurretCount;
            if (ActiveMode == GameModeSelection.Tutorial) progression.CompleteTutorial();
            if (ActiveMode == GameModeSelection.LoreOneLevelOne) progression.CompleteLoreOneLevelOne();
            var reward = 1 + (run == null ? 0 : run.BossesDefeated);
            progression.CreditKnowledge(reward);
            var unlocked = progression.UnlockedTurretCount > previousUnlockCount
                ? ActiveMode == GameModeSelection.Tutorial ? "Teacher" : "Engineer"
                : null;
            resultPresenter?.Present(true, reward, unlocked, ActiveMode == GameModeSelection.Tutorial);
        }

        public void ContinueFromResult()
        {
            if (run != null && run.Phase == WaveRunPhase.Defeat)
            {
                run.RestartRun();
                return;
            }
            if (ActiveMode == GameModeSelection.Tutorial)
            {
                ShowLoreLevels();
                return;
            }
            ReturnToMainFromRun();
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
            if (researchKnowledgeLabel != null) researchKnowledgeLabel.text = $"KNOWLEDGE POINTS  {profile.knowledgePoints}";
            if (brainCellsLabel != null) brainCellsLabel.text = profile.brainCells.ToString();
            if (gameSpeedLabels != null)
                for (var index = 0; index < gameSpeedLabels.Length; index++)
                    if (gameSpeedLabels[index] != null)
                        gameSpeedLabels[index].text = GameSpeedSettings.Multiplier == index + 1 ? $"{index + 1}X  SELECTED" : $"{index + 1}X";
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
            RefreshLoreButton(loreOneLevelTwoButton, loreOneLevelTwoStarsLabel, 2, true,
                profile.loreOneLevelTwoCompleted, profile.loreOneLevelTwoStars, profile.loreOneLevelTwoBestSeconds);
            RefreshLoreButton(loreOneLevelThreeButton, loreOneLevelThreeStarsLabel, 3, true,
                profile.loreOneLevelThreeCompleted, profile.loreOneLevelThreeStars, profile.loreOneLevelThreeBestSeconds);
            if (loreLevelButtons != null && loreLevelLabels != null && loreLevelButtons.Length >= 6 && loreLevelLabels.Length >= 6)
            {
                RefreshLoreButton(loreLevelButtons[0], loreLevelLabels[0], 1, true, profile.loreOneLevelOneCompleted, profile.loreOneLevelOneStars, profile.loreOneLevelOneBestSeconds);
                RefreshLoreButton(loreLevelButtons[1], loreLevelLabels[1], 2, true, profile.loreOneLevelTwoCompleted, profile.loreOneLevelTwoStars, profile.loreOneLevelTwoBestSeconds);
                RefreshLoreButton(loreLevelButtons[2], loreLevelLabels[2], 3, true, profile.loreOneLevelThreeCompleted, profile.loreOneLevelThreeStars, profile.loreOneLevelThreeBestSeconds);
                RefreshLoreButton(loreLevelButtons[3], loreLevelLabels[3], 4, true, profile.loreOneLevelFourCompleted, profile.loreOneLevelFourStars, profile.loreOneLevelFourBestSeconds);
                RefreshLoreButton(loreLevelButtons[4], loreLevelLabels[4], 5, true, profile.loreOneLevelFiveCompleted, profile.loreOneLevelFiveStars, profile.loreOneLevelFiveBestSeconds);
                RefreshLoreButton(loreLevelButtons[5], loreLevelLabels[5], 6, true, profile.loreOneLevelSixCompleted, profile.loreOneLevelSixStars, profile.loreOneLevelSixBestSeconds);
            }
        }

        private static void RefreshLoreButton(Button button, Text label, int level, bool unlocked, bool completed, int stars, float best)
        {
            if (button != null) button.interactable = unlocked;
            if (label == null) return;
            if (!unlocked) { label.text = $"LEVEL {level}  •  LOCKED"; return; }
            var rating = new string('★', Mathf.Clamp(stars, 0, 3)) + new string('☆', 3 - Mathf.Clamp(stars, 0, 3));
            label.text = completed ? $"LEVEL {level}  {rating}  BEST {FormatTime(best)}" : $"LEVEL {level}  {rating}";
        }

        private static string FormatTime(float seconds)
        {
            if (seconds <= 0f) return "--:--";
            var total = Mathf.FloorToInt(seconds);
            return $"{total / 60:00}:{total % 60:00}";
        }
    }
}
