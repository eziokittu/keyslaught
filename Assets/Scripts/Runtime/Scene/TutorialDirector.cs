using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace KeySlaught.SceneGameplay
{
    public sealed class TutorialDirector : MonoBehaviour
    {
        private enum TutorialStage
        {
            Inactive, ExploreMovement, MovementMessage, LoadBuffer, BufferLoadedMessage,
            RefreshBuffer, RefreshMessage, DefeatWord, TypingMessage,
            CollectBrainCells, CollectionMessage, WatchLibrary, LibraryMessage, Complete
        }

        [SerializeField] private GameObject overlay;
        [SerializeField] private Text titleLabel;
        [SerializeField] private Text bodyLabel;
        [SerializeField] private PlayerMover player;
        [SerializeField] private WaveRunController run;
        [SerializeField] private Button continueButton;
        [SerializeField] private GameObject objectiveRoot;
        [SerializeField] private Text objectiveLabel;
        [SerializeField] private EnemySpawner spawner;
        [SerializeField] private GameplayCombatController combat;
        [SerializeField] private LibraryEndpoint library;
        [SerializeField] private BrainCellEconomy economy;
        [SerializeField] private GameObject tutorialJoystickRoot;
        [SerializeField] private Button[] tutorialInputButtons;

        private TutorialStage stage;
        private Vector3 movementStart;
        private int startingBalance;
        private bool refreshObserved;

        public void Configure(GameObject root, Text title, Text body, Button next,
            PlayerMover mover, WaveRunController runController)
        {
            Configure(root, title, body, next, mover, runController, null, null);
        }

        public void Configure(GameObject root, Text title, Text body, Button next,
            PlayerMover mover, WaveRunController runController, GameObject objectiveBanner, Text objective)
        {
            overlay = root; titleLabel = title; bodyLabel = body; continueButton = next;
            player = mover; run = runController; objectiveRoot = objectiveBanner; objectiveLabel = objective;
            spawner = FindFirstObjectByType<EnemySpawner>();
            combat = FindFirstObjectByType<GameplayCombatController>();
            library = FindFirstObjectByType<LibraryEndpoint>();
            economy = FindFirstObjectByType<BrainCellEconomy>();
        }

        public void ConfigureInteractiveVisuals(GameObject joystickRoot, Button[] inputButtons)
        {
            tutorialJoystickRoot = joystickRoot;
            tutorialInputButtons = inputButtons;
        }

        public void Begin()
        {
            StopAllCoroutines();
            Time.timeScale = 1f;
            stage = TutorialStage.ExploreMovement;
            movementStart = player == null ? Vector3.zero : player.transform.position;
            startingBalance = economy == null ? 0 : economy.Balance;
            run?.SetGuidanceSuspended(true);
            if (combat != null)
            {
                combat.enabled = true;
                combat.TargetDefeated -= OnTargetDefeated;
                combat.TargetDefeated += OnTargetDefeated;
            }
            if (overlay != null) overlay.SetActive(false);
            if (tutorialJoystickRoot != null) tutorialJoystickRoot.SetActive(true);
            ShowObjective("DRAG THE CONTROLLER OR SCREEN TO MOVE");
        }

        public void ContinueTutorial()
        {
            Time.timeScale = 1f;
            if (overlay != null) overlay.SetActive(false);
            switch (stage)
            {
                case TutorialStage.MovementMessage:
                    stage = TutorialStage.LoadBuffer;
                    ShowObjective("WAND BUFFER: PRESS  C  A  T");
                    SetButtonHighlights("C", "A", "T");
                    break;
                case TutorialStage.BufferLoadedMessage:
                    stage = TutorialStage.RefreshBuffer;
                    refreshObserved = false;
                    ShowObjective("PRESS REFRESH TO CLEAR THE WAND BUFFER");
                    SetButtonHighlights("REFRESH");
                    break;
                case TutorialStage.RefreshMessage:
                    stage = TutorialStage.DefeatWord;
                    ShowObjective("ENEMY IN RANGE: TYPE  C  A  T  TO FIRE");
                    SetButtonHighlights("C", "A", "T");
                    if (spawner != null && player != null) spawner.SpawnWordNear("CAT", .34f, player.transform.position);
                    break;
                case TutorialStage.TypingMessage:
                    stage = TutorialStage.CollectBrainCells;
                    startingBalance = economy == null ? 0 : economy.Balance;
                    ShowObjective("TRY IT: WALK OVER THE BRAIN CELLS");
                    break;
                case TutorialStage.CollectionMessage:
                    stage = TutorialStage.WatchLibrary;
                    ShowObjective("WATCH: AN ENEMY REACHES THE LIBRARY");
                    StartCoroutine(DemonstrateLibraryAttack());
                    break;
                case TutorialStage.LibraryMessage:
                    CompleteTutorialGuidance();
                    break;
            }
        }

        private void Update()
        {
            if (stage == TutorialStage.ExploreMovement && player != null &&
                Vector2.Distance(movementStart, player.transform.position) >= 1.25f)
            {
                stage = TutorialStage.MovementMessage;
                if (tutorialJoystickRoot != null) tutorialJoystickRoot.SetActive(false);
                ShowMessage("MOVEMENT READY", "Drag the on-screen controller, drag in the arena, use arrow keys, or use a gamepad. Water and mountains stop movement; trees and rocks can be cleared for turrets.");
            }
            else if (stage == TutorialStage.LoadBuffer && combat != null && combat.ErrorBuffer.OccupiedSlotCount >= 3)
            {
                stage = TutorialStage.BufferLoadedMessage;
                ClearButtonHighlights();
                ShowMessage("WAND BUFFER LOADED", "Letters wait in the wand buffer when no matching target is in range. A matching enemy will consume them automatically. Refresh clears the buffer so you can correct or replace its letters.");
            }
            else if (stage == TutorialStage.RefreshBuffer && combat != null)
            {
                if (combat.ErrorBuffer.IsRefreshing) refreshObserved = true;
                if (refreshObserved && !combat.ErrorBuffer.IsRefreshing && combat.ErrorBuffer.OccupiedSlotCount == 0)
                {
                    stage = TutorialStage.RefreshMessage;
                    ClearButtonHighlights();
                    ShowMessage("BUFFER REFRESHED", "The buffer is empty again. Next, a CAT enemy will be placed inside your glowing attack range. Type its visible letters in order to fire immediately.");
                }
            }
            else if (stage == TutorialStage.CollectBrainCells && economy != null && economy.Balance > startingBalance)
            {
                stage = TutorialStage.CollectionMessage;
                ShowMessage("BRAIN CELLS COLLECTED", "Defeated words drop brain cells. Spend them to clear obstacles, place a turret family, upgrade turrets, repair the Library, or activate abilities.");
            }
        }

        private void OnTargetDefeated(EnemyAgent enemy)
        {
            if (stage != TutorialStage.DefeatWord) return;
            stage = TutorialStage.TypingMessage;
            ClearButtonHighlights();
            ShowMessage("WORD DEFEATED", "Correct letters fire immediately when a matching enemy is in range. Its card changes color after every hit to show the remaining word length.");
        }

        private IEnumerator DemonstrateLibraryAttack()
        {
            if (combat != null) combat.enabled = false;
            var demo = spawner == null ? null : spawner.SpawnWord("LOSS", 5f);
            demo?.SetMovementMultiplier(1f);
            var timeout = 12f;
            while (demo != null && !demo.HasArrived && timeout > 0f)
            {
                timeout -= Time.unscaledDeltaTime;
                yield return null;
            }
            stage = TutorialStage.LibraryMessage;
            ShowMessage("PROTECT THE LIBRARY", "An enemy that reaches the Library removes HP equal to its remaining letters. Between waves you get 30 seconds to build, repair, or use Fast Forward.");
        }

        private void CompleteTutorialGuidance()
        {
            stage = TutorialStage.Complete;
            ClearButtonHighlights();
            if (tutorialJoystickRoot != null) tutorialJoystickRoot.SetActive(false);
            if (objectiveRoot != null) objectiveRoot.SetActive(false);
            library?.ResetState();
            if (combat != null) combat.enabled = true;
            run?.RestartRun();
            run?.SetGuidanceSuspended(false);
        }

        private void ShowObjective(string text)
        {
            if (objectiveRoot != null) objectiveRoot.SetActive(true);
            if (objectiveLabel != null) objectiveLabel.text = text;
        }

        private void ShowMessage(string title, string body)
        {
            if (objectiveRoot != null) objectiveRoot.SetActive(false);
            if (overlay != null) overlay.SetActive(true);
            if (titleLabel != null) titleLabel.text = title;
            if (bodyLabel != null) bodyLabel.text = body;
            Time.timeScale = 0f;
        }

        private void SetButtonHighlights(params string[] labels)
        {
            ClearButtonHighlights();
            if (tutorialInputButtons == null) return;
            foreach (var button in tutorialInputButtons)
            {
                if (button == null) continue;
                var label = button.GetComponentInChildren<Text>(true)?.text?.Trim().ToUpperInvariant();
                var isRefresh = button.GetComponent<OnScreenRefreshButton>() != null;
                var match = false;
                foreach (var requested in labels)
                    if (requested == label || (requested == "REFRESH" && isRefresh)) match = true;
                if (!match) continue;
                button.transform.localScale = Vector3.one * 1.14f;
                var image = button.GetComponent<Image>();
                if (image != null) image.color = new Color(1f, .78f, .28f, 1f);
            }
        }

        private void ClearButtonHighlights()
        {
            if (tutorialInputButtons == null) return;
            foreach (var button in tutorialInputButtons)
            {
                if (button == null) continue;
                button.transform.localScale = Vector3.one;
                var image = button.GetComponent<Image>();
                if (image != null) image.color = Color.white;
            }
        }

        private void OnDisable()
        {
            if (combat != null) combat.TargetDefeated -= OnTargetDefeated;
            ClearButtonHighlights();
            if (stage != TutorialStage.Inactive) Time.timeScale = 1f;
        }
    }
}
