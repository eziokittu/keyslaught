using System.Collections;
using KeySlaught.Progression;
using UnityEngine;
using UnityEngine.UI;

namespace KeySlaught.SceneGameplay
{
    public sealed class AbilityTutorialDirector : MonoBehaviour
    {
        private enum Stage { Inactive, MoveToLibrary, WatchHistory, Complete }

        [SerializeField] private WaveRunController run;
        [SerializeField] private EnemySpawner spawner;
        [SerializeField] private BrainCellEconomy economy;
        [SerializeField] private LibraryAbilityController abilities;
        [SerializeField] private GameShellController shell;
        [SerializeField] private GameObject objectiveRoot;
        [SerializeField] private Text heading;
        [SerializeField] private Text body;
        [SerializeField] private GameObject acknowledgementRoot;
        [SerializeField] private Text acknowledgementTitle;
        [SerializeField] private Text acknowledgementBody;

        private Stage stage;
        private bool observedHistory;

        public void Configure(WaveRunController waveRun, EnemySpawner enemySpawner, BrainCellEconomy brainCells,
            LibraryAbilityController abilityController, GameShellController gameShell, GameObject objective,
            Text objectiveHeading, Text objectiveBody, GameObject acknowledgement, Text messageTitle, Text messageBody)
        {
            run = waveRun; spawner = enemySpawner; economy = brainCells; abilities = abilityController; shell = gameShell;
            objectiveRoot = objective; heading = objectiveHeading; body = objectiveBody;
            acknowledgementRoot = acknowledgement; acknowledgementTitle = messageTitle; acknowledgementBody = messageBody;
        }

        public void Begin()
        {
            gameObject.SetActive(true);
            StopAllCoroutines();
            stage = Stage.MoveToLibrary;
            observedHistory = false;
            if (acknowledgementRoot != null) acknowledgementRoot.SetActive(false);
            if (objectiveRoot != null) objectiveRoot.SetActive(true);
            run?.SetGuidanceSuspended(true);
            economy?.ResetState();
            economy?.Credit(8);
            abilities?.ResetState();
            TutorialInputGate.MovementOnly();
            var target = spawner?.SpawnWord("HISTORY", .35f);
            target?.SetMovementMultiplier(1f);
            ShowObjective("USE A LIBRARY ABILITY",
                "Move onto the Library tile. Choose ABILITIES, then HISTORY. The supplied 8 brain cells pay for this training activation.");
        }

        public void ContinueTutorial()
        {
            if (stage != Stage.Complete) return;
            if (acknowledgementRoot != null) acknowledgementRoot.SetActive(false);
            if (objectiveRoot != null) objectiveRoot.SetActive(false);
            TutorialInputGate.Clear();
            run?.SetGuidanceSuspended(false);
            shell?.CompleteAbilityTutorialFromGuide();
            gameObject.SetActive(false);
        }

        private void Update()
        {
            if (stage == Stage.MoveToLibrary && abilities != null && abilities.ActiveAbility == LibraryAbilityKind.History)
            {
                observedHistory = true;
                stage = Stage.WatchHistory;
                TutorialInputGate.DisableAll();
                ShowObjective("HISTORY IS ACTIVE", "Watch the enemy reverse along its authored route until the ability timer finishes.");
            }
            else if (stage == Stage.WatchHistory && observedHistory && abilities != null && abilities.ActiveAbility == null)
            {
                stage = Stage.Complete;
                StartCoroutine(ShowCompletion());
            }
        }

        private IEnumerator ShowCompletion()
        {
            yield return new WaitForSecondsRealtime(.8f);
            if (objectiveRoot != null) objectiveRoot.SetActive(false);
            if (acknowledgementTitle != null) acknowledgementTitle.text = "ABILITY TRAINING COMPLETE";
            if (acknowledgementBody != null)
                acknowledgementBody.text = "You entered the Library, spent run-scoped brain cells, and used History to reverse an enemy. Continue to Lore I, Level 1.";
            if (acknowledgementRoot != null) acknowledgementRoot.SetActive(true);
        }

        private void ShowObjective(string title, string description)
        {
            if (objectiveRoot != null) objectiveRoot.SetActive(true);
            if (heading != null) heading.text = title;
            if (body != null) body.text = description;
        }

        private void OnDisable()
        {
            if (stage != Stage.Inactive) TutorialInputGate.Clear();
        }
    }
}
