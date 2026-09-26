using System.Collections;
using KeySlaught.Progression;
using KeySlaught.UI;
using UnityEngine;
using UnityEngine.UI;

namespace KeySlaught.SceneGameplay
{
    public sealed class TutorialDirector : MonoBehaviour
    {
        private enum TutorialStage
        {
            Inactive, ExploreMovement, MovementMessage, LoadBuffer, BufferLoadedMessage,
            RefreshBuffer, RefreshMessage, ApproachTarget, DefeatWord, TypingMessage,
            CollectBrainCells, CollectionMessage, WatchLibrary, LibraryMessage, RepairMessage, Complete
        }

        [SerializeField] private GameObject overlay;
        [SerializeField] private Text titleLabel;
        [SerializeField] private Text bodyLabel;
        [SerializeField] private PlayerMover player;
        [SerializeField] private WaveRunController run;
        [SerializeField] private Button continueButton;
        [SerializeField] private GameObject objectiveRoot;
        [SerializeField] private Text objectiveLabel;
        [SerializeField] private Text checklistLabel;
        [SerializeField] private Text feedbackLabel;
        [SerializeField] private EnemySpawner spawner;
        [SerializeField] private GameplayCombatController combat;
        [SerializeField] private LibraryEndpoint library;
        [SerializeField] private BrainCellEconomy economy;
        [SerializeField] private GameObject tutorialJoystickRoot;
        [SerializeField] private Button[] tutorialInputButtons;
        [SerializeField] private TutorialFocusGuide focusGuide;
        [SerializeField] private RectTransform arenaFocusTarget;
        [SerializeField] private RectTransform inputFocusTarget;

        private TutorialStage stage;
        private TutorialMovementChecklist movementChecklist;
        private Vector3 previousPlayerPosition;
        private int startingBalance;
        private bool refreshObserved;
        private EnemyAgent tutorialEnemy;

        public void Configure(GameObject root, Text title, Text body, Button next,
            PlayerMover mover, WaveRunController runController) =>
            Configure(root, title, body, next, mover, runController, null, null);

        public void Configure(GameObject root, Text title, Text body, Button next,
            PlayerMover mover, WaveRunController runController, GameObject objectiveBanner, Text objective)
        {
            overlay = root; titleLabel = title; bodyLabel = body; continueButton = next;
            player = mover; run = runController; objectiveRoot = objectiveBanner; objectiveLabel = objective;
            spawner = FindFirstObjectByType<EnemySpawner>(); combat = FindFirstObjectByType<GameplayCombatController>();
            library = FindFirstObjectByType<LibraryEndpoint>(); economy = FindFirstObjectByType<BrainCellEconomy>();
        }

        public void ConfigureTutorialUi(GameObject root, Text heading, Text checklist, Text feedback)
        {
            objectiveRoot = root; objectiveLabel = heading; checklistLabel = checklist; feedbackLabel = feedback;
        }

        public void ConfigureInteractiveVisuals(GameObject joystickRoot, Button[] inputButtons)
        { tutorialJoystickRoot = joystickRoot; tutorialInputButtons = inputButtons; }

        public void ConfigureFocusGuide(TutorialFocusGuide guide, RectTransform arenaTarget, RectTransform inputTarget)
        { focusGuide = guide; arenaFocusTarget = arenaTarget; inputFocusTarget = inputTarget; }

        public void Begin()
        {
            StopAllCoroutines(); GameSpeedSettings.ApplyGameplaySpeed();
            stage = TutorialStage.ExploreMovement;
            movementChecklist = new TutorialMovementChecklist(.7f);
            previousPlayerPosition = player == null ? Vector3.zero : player.transform.position;
            startingBalance = economy == null ? 0 : economy.Balance;
            run?.SetGuidanceSuspended(true);
            TutorialInputGate.Feedback -= OnInputFeedback;
            TutorialInputGate.Feedback += OnInputFeedback;
            if (combat != null)
            {
                combat.enabled = true; combat.ClearMagazine();
                combat.TargetHit -= OnTargetHit; combat.TargetHit += OnTargetHit;
                combat.TargetDefeated -= OnTargetDefeated; combat.TargetDefeated += OnTargetDefeated;
            }
            if (overlay != null) overlay.SetActive(false);
            if (tutorialJoystickRoot != null) tutorialJoystickRoot.SetActive(true);
            TutorialInputGate.MovementOnly();
            if (focusGuide != null) focusGuide.Focus(arenaFocusTarget, new Vector2(20f, 20f));
            ShowObjective("LEARN TO MOVE", movementChecklist.FormatTodos(), "Use arrows, a stick, or drag the controller.");
        }

        public void ContinueTutorial()
        {
            GameSpeedSettings.ApplyGameplaySpeed();
            if (overlay != null) overlay.SetActive(false);
            switch (stage)
            {
                case TutorialStage.MovementMessage:
                    stage = TutorialStage.LoadBuffer; combat?.ClearMagazine();
                    SetRequiredBufferLetter();
                    if (focusGuide != null) focusGuide.Focus(inputFocusTarget, new Vector2(16f, 16f));
                    break;
                case TutorialStage.BufferLoadedMessage:
                    stage = TutorialStage.RefreshBuffer; refreshObserved = false;
                    ShowObjective("CLEAR THE WAND", $"{Tick(false)} PRESS REFRESH\n{Tick(false)} WAIT UNTIL EMPTY", "Refresh lets you correct stored letters.");
                    TutorialInputGate.RefreshOnly(); SetButtonHighlights("REFRESH"); FocusRefreshButton();
                    break;
                case TutorialStage.RefreshMessage:
                    StartRoutedCatExercise();
                    break;
                case TutorialStage.TypingMessage:
                    stage = TutorialStage.CollectBrainCells;
                    startingBalance = economy == null ? 0 : economy.Balance;
                    ShowObjective("COLLECT THE REWARD", $"{Tick(false)} WALK TO THE BRAIN CELLS\n{Tick(false)} COLLECT THEM", "Movement is unlocked again.");
                    TutorialInputGate.MovementOnly();
                    if (tutorialJoystickRoot != null) tutorialJoystickRoot.SetActive(true);
                    if (focusGuide != null) focusGuide.Focus(arenaFocusTarget, new Vector2(20f, 20f));
                    break;
                case TutorialStage.CollectionMessage:
                    stage = TutorialStage.WatchLibrary;
                    if (player != null && library != null) player.transform.position = library.transform.position + new Vector3(-2.4f, -1.5f, 0f);
                    ShowObjective("LIBRARY DAMAGE DEMO", $"{Tick(true)} VIEW MOVED TO LIBRARY\n{Tick(false)} FAST ENEMY FOLLOWS PATH\n{Tick(false)} LIBRARY LOSES HP", "The action now stays visible beside the Library.");
                    TutorialInputGate.DisableAll();
                    if (tutorialJoystickRoot != null) tutorialJoystickRoot.SetActive(false);
                    if (focusGuide != null) focusGuide.Focus(arenaFocusTarget, new Vector2(20f, 20f));
                    StartCoroutine(DemonstrateLibraryAttack());
                    break;
                case TutorialStage.LibraryMessage:
                    var repaired = library == null || library.State == null ? 0 : library.Repair(library.State.MaximumHealth);
                    stage = TutorialStage.RepairMessage;
                    ShowMessage("LIBRARY REPAIRED", $"Repair restored {repaired} HP. During a run, stand near the Library and spend brain cells on REPAIR.");
                    break;
                case TutorialStage.RepairMessage:
                    CompleteTutorialGuidance();
                    break;
            }
        }

        private void Update()
        {
            if (stage == TutorialStage.ExploreMovement && player != null)
            {
                var current = player.transform.position;
                movementChecklist.Advance(current - previousPlayerPosition);
                previousPlayerPosition = current;
                SetChecklist(movementChecklist.FormatTodos());
                if (movementChecklist.IsComplete)
                {
                    stage = TutorialStage.MovementMessage;
                    if (tutorialJoystickRoot != null) tutorialJoystickRoot.SetActive(false);
                    ShowMessage("MOVEMENT COMPLETE", "All four directions are checked. You can use arrow keys, a controller, or the on-screen drag control; blocked terrain still stops movement.");
                }
            }
            else if (stage == TutorialStage.LoadBuffer && combat != null)
            {
                if (combat.ErrorBuffer.OccupiedSlotCount >= 3)
                {
                    stage = TutorialStage.BufferLoadedMessage; ClearButtonHighlights();
                    ShowMessage("WAND BUFFER LOADED", "C, A, and T are stored in order. When a matching word enters range, the wand can use those letters. First, clear them so you can practise firing manually.");
                }
                else SetRequiredBufferLetter();
            }
            else if (stage == TutorialStage.RefreshBuffer && combat != null)
            {
                if (combat.ErrorBuffer.IsRefreshing)
                {
                    refreshObserved = true;
                    SetChecklist($"{Tick(true)} PRESS REFRESH\n{Tick(false)} WAIT UNTIL EMPTY");
                }
                if (refreshObserved && !combat.ErrorBuffer.IsRefreshing && combat.ErrorBuffer.OccupiedSlotCount == 0)
                {
                    stage = TutorialStage.RefreshMessage; ClearButtonHighlights();
                    ShowMessage("BUFFER CLEARED", "Next, CAT will enter from the real enemy route. Move near it, wait for it to enter your attack range, then type its visible letters.");
                }
            }
            else if (stage == TutorialStage.ApproachTarget && tutorialEnemy != null)
            {
                var inRange = combat != null && combat.SceneCoordinator != null && combat.SceneCoordinator.GetPrimaryInRangeEnemy() == tutorialEnemy;
                if (!inRange && tutorialEnemy.DistanceToLibrary <= 5f) tutorialEnemy.SetMovementMultiplier(0f);
                if (inRange)
                {
                    tutorialEnemy.SetMovementMultiplier(0f);
                    combat.ResetCombatState();
                    stage = TutorialStage.DefeatWord;
                    if (tutorialJoystickRoot != null) tutorialJoystickRoot.SetActive(false);
                    ShowObjective("DEFEAT CAT", $"{Tick(true)} CAT FOLLOWED THE PATH\n{Tick(true)} ENTER ATTACK RANGE\n{Tick(false)} TYPE C - A - T", "Only the next correct letter is accepted.");
                    SetRequiredEnemyLetter();
                    if (focusGuide != null) focusGuide.Focus(inputFocusTarget, new Vector2(16f, 16f));
                }
            }
            else if (stage == TutorialStage.CollectBrainCells && economy != null && economy.Balance > startingBalance)
            {
                stage = TutorialStage.CollectionMessage;
                ShowMessage("BRAIN CELLS COLLECTED", "Brain cells pay for clearing obstacles, placing and upgrading specialists, repairing the Library, and activating abilities.");
            }
        }

        private void StartRoutedCatExercise()
        {
            stage = TutorialStage.ApproachTarget;
            combat?.ClearMagazine();
            tutorialEnemy = spawner == null ? null : spawner.SpawnWord("CAT", .65f);
            tutorialEnemy?.SetMovementMultiplier(1f);
            TutorialInputGate.MovementOnly();
            if (tutorialJoystickRoot != null) tutorialJoystickRoot.SetActive(true);
            ShowObjective("INTERCEPT CAT", $"{Tick(true)} CAT SPAWNED ON THE PATH\n{Tick(false)} MOVE INTO ATTACK RANGE\n{Tick(false)} TYPE C - A - T", "Follow the route and get close enough to attack.");
            if (focusGuide != null) focusGuide.Focus(arenaFocusTarget, new Vector2(20f, 20f));
        }

        private void SetRequiredBufferLetter()
        {
            if (combat == null) return;
            var count = Mathf.Clamp(combat.ErrorBuffer.OccupiedSlotCount, 0, 2);
            var letters = "CAT"; var next = letters[count];
            TutorialInputGate.RequireLetter(next); SetButtonHighlights(next.ToString());
            ShowObjective("LOAD C - A - T", $"{Tick(count > 0)} LOAD C\n{Tick(count > 1)} LOAD A\n{Tick(count > 2)} LOAD T", $"Next key: {next}");
            FocusLetterButton(next);
        }

        private void SetRequiredEnemyLetter()
        {
            if (tutorialEnemy == null || tutorialEnemy.WordState == null || tutorialEnemy.WordState.IsDefeated) return;
            var remaining = tutorialEnemy.WordState.RemainingWord;
            var completed = 3 - remaining.Length;
            var next = tutorialEnemy.WordState.NextLetter.Value;
            TutorialInputGate.RequireLetter(next); SetButtonHighlights(next.ToString());
            SetChecklist($"{Tick(true)} CAT FOLLOWED THE PATH\n{Tick(true)} ENTER ATTACK RANGE\n{Tick(completed >= 3)} TYPE C - A - T\n    {Tick(completed >= 1)} C   {Tick(completed >= 2)} A   {Tick(completed >= 3)} T");
            SetFeedback($"NEXT: {next}", true);
            FocusLetterButton(next);
        }

        private void OnTargetHit(EnemyAgent enemy)
        {
            if (stage == TutorialStage.DefeatWord && enemy == tutorialEnemy) SetRequiredEnemyLetter();
        }

        private void OnTargetDefeated(EnemyAgent enemy)
        {
            if (stage != TutorialStage.DefeatWord || enemy != tutorialEnemy) return;
            stage = TutorialStage.TypingMessage; ClearButtonHighlights();
            ShowMessage("WORD DEFEATED", "You stopped CAT on the route and fired each correct letter from attack range. Its card changed after every hit to show the remaining word.");
        }

        private IEnumerator DemonstrateLibraryAttack()
        {
            if (combat != null) { combat.ClearMagazine(); combat.enabled = false; }
            var demo = spawner == null ? null : spawner.SpawnWord("LOSS", 1.85f);
            demo?.SetMovementMultiplier(1f);
            var timeout = 45f;
            while (demo != null && !demo.HasArrived && timeout > 0f)
            { timeout -= Time.unscaledDeltaTime; yield return null; }
            stage = TutorialStage.LibraryMessage;
            ShowMessage("LIBRARY DAMAGED", "LOSS reached the Library at demonstration speed. Every remaining letter removed one HP. Continue to practise repairing it.");
        }

        private void CompleteTutorialGuidance()
        {
            stage = TutorialStage.Complete; ClearButtonHighlights(); TutorialInputGate.Clear();
            if (focusGuide != null) focusGuide.Hide();
            if (tutorialJoystickRoot != null) tutorialJoystickRoot.SetActive(false);
            if (objectiveRoot != null) objectiveRoot.SetActive(false);
            library?.ResetState(); if (combat != null) combat.enabled = true;
            run?.RestartRun(); run?.SetGuidanceSuspended(false);
        }

        private void ShowObjective(string title, string todos, string feedback)
        {
            if (objectiveRoot != null) objectiveRoot.SetActive(true);
            if (objectiveLabel != null) objectiveLabel.text = title;
            SetChecklist(todos); SetFeedback(feedback, true);
        }

        private void SetChecklist(string text) { if (checklistLabel != null) checklistLabel.text = text; }
        private void SetFeedback(string text, bool positive)
        {
            if (feedbackLabel == null) return;
            feedbackLabel.text = text;
            feedbackLabel.color = positive ? new Color(.45f, 1f, .76f) : new Color(1f, .45f, .4f);
        }

        private void OnInputFeedback(string text, bool positive) => SetFeedback(text, positive);

        private void ShowMessage(string title, string body)
        {
            if (objectiveRoot != null) objectiveRoot.SetActive(false);
            if (overlay != null) overlay.SetActive(true);
            if (titleLabel != null) titleLabel.text = title;
            if (bodyLabel != null) bodyLabel.text = body;
            TutorialInputGate.DisableAll();
            if (focusGuide != null) focusGuide.Focus(continueButton == null ? null : continueButton.transform as RectTransform, new Vector2(18f, 18f));
            Time.timeScale = 0f;
        }

        private void SetButtonHighlights(params string[] labels)
        {
            ClearButtonHighlights(); if (tutorialInputButtons == null) return;
            foreach (var button in tutorialInputButtons)
            {
                if (button == null) continue;
                var label = button.GetComponentInChildren<Text>(true)?.text?.Trim().ToUpperInvariant();
                var isRefresh = button.GetComponent<OnScreenRefreshButton>() != null;
                var match = false;
                foreach (var requested in labels) if (requested == label || (requested == "REFRESH" && isRefresh)) match = true;
                button.interactable = match;
                if (!match) continue;
                button.transform.localScale = Vector3.one * 1.12f;
                var image = button.GetComponent<Image>(); if (image != null) image.color = new Color(.35f, .95f, .75f, 1f);
            }
        }

        private void ClearButtonHighlights()
        {
            if (tutorialInputButtons == null) return;
            foreach (var button in tutorialInputButtons)
            {
                if (button == null) continue; button.interactable = true; button.transform.localScale = Vector3.one;
                var image = button.GetComponent<Image>(); if (image != null) image.color = Color.white;
            }
        }

        private void OnDisable()
        {
            TutorialInputGate.Feedback -= OnInputFeedback;
            if (combat != null) { combat.TargetHit -= OnTargetHit; combat.TargetDefeated -= OnTargetDefeated; }
            ClearButtonHighlights(); TutorialInputGate.Clear();
            if (focusGuide != null) focusGuide.Hide();
            if (stage != TutorialStage.Inactive) GameSpeedSettings.ApplyGameplaySpeed();
        }

        private void FocusRefreshButton()
        {
            if (tutorialInputButtons == null) return;
            foreach (var button in tutorialInputButtons)
                if (button != null && button.GetComponent<OnScreenRefreshButton>() != null)
                { if (focusGuide != null) focusGuide.Focus(button.transform as RectTransform, new Vector2(22f, 22f)); return; }
        }

        private void FocusLetterButton(char letter)
        {
            if (tutorialInputButtons == null) return;
            foreach (var button in tutorialInputButtons)
            {
                if (button == null || button.GetComponent<OnScreenLetterButton>()?.Letter != letter) continue;
                if (focusGuide != null) focusGuide.Focus(button.transform as RectTransform, new Vector2(22f, 22f));
                return;
            }
            if (focusGuide != null) focusGuide.Focus(inputFocusTarget, new Vector2(16f, 16f));
        }

        private static string Tick(bool complete) => complete
            ? "<color=#62F0A7>\u2713</color>"
            : "<color=#A9B4C8>\u25A1</color>";
    }
}
