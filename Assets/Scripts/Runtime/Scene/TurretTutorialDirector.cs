using System.Collections;
using KeySlaught.Progression;
using UnityEngine;
using UnityEngine.UI;

namespace KeySlaught.SceneGameplay
{
    public sealed class TurretTutorialDirector : MonoBehaviour
    {
        private enum Stage { Inactive, DefeatForCells, CollectCells, PlaceTeacher, WatchTeacher, Complete }

        [SerializeField] private WaveRunController run;
        [SerializeField] private EnemySpawner spawner;
        [SerializeField] private GameplayCombatController combat;
        [SerializeField] private BrainCellEconomy economy;
        [SerializeField] private Transform turretRoot;
        [SerializeField] private GameShellController shell;
        [SerializeField] private GameObject objectiveRoot;
        [SerializeField] private Text heading;
        [SerializeField] private Text body;
        [SerializeField] private GameObject acknowledgementRoot;
        [SerializeField] private Text acknowledgementTitle;
        [SerializeField] private Text acknowledgementBody;

        private Stage stage;
        private EnemyAgent target;
        private TurretPadController placedTeacher;

        public void Configure(WaveRunController waveRun, EnemySpawner enemySpawner, GameplayCombatController combatController,
            BrainCellEconomy brainCells, Transform placedTurrets, GameShellController gameShell,
            GameObject objective, Text objectiveHeading, Text objectiveBody,
            GameObject acknowledgement, Text messageTitle, Text messageBody)
        {
            run = waveRun; spawner = enemySpawner; combat = combatController; economy = brainCells;
            turretRoot = placedTurrets; shell = gameShell; objectiveRoot = objective;
            heading = objectiveHeading; body = objectiveBody; acknowledgementRoot = acknowledgement;
            acknowledgementTitle = messageTitle; acknowledgementBody = messageBody;
        }

        public void Begin()
        {
            gameObject.SetActive(true);
            StopAllCoroutines();
            stage = Stage.DefeatForCells;
            if (acknowledgementRoot != null) acknowledgementRoot.SetActive(false);
            if (objectiveRoot != null) objectiveRoot.SetActive(true);
            run?.SetGuidanceSuspended(true);
            economy?.ResetState();
            combat?.ClearMagazine();
            if (combat != null)
            {
                combat.enabled = true;
                combat.TargetDefeated -= OnTargetDefeated;
                combat.TargetDefeated += OnTargetDefeated;
            }
            TutorialInputGate.Clear();
            ShowObjective("EARN BRAIN CELLS", "Intercept and type TEACHER. Collect all seven brain cells it drops.");
            target = spawner?.SpawnWord("TEACHER", .48f);
        }

        public void ContinueTutorial()
        {
            if (stage != Stage.Complete) return;
            if (acknowledgementRoot != null) acknowledgementRoot.SetActive(false);
            if (objectiveRoot != null) objectiveRoot.SetActive(false);
            TutorialInputGate.Clear();
            run?.SetGuidanceSuspended(false);
            if (combat != null) combat.enabled = true;
            shell?.CompleteTurretTutorialFromGuide();
            gameObject.SetActive(false);
        }

        private void Update()
        {
            if (stage == Stage.CollectCells && economy != null && economy.Balance >= 7)
            {
                stage = Stage.PlaceTeacher;
                ShowObjective("PLACE THE TEACHER", "Move onto empty ground beside the route. Choose TURRETS, TEACHER, a letter group, then confirm placement.");
            }
            else if (stage == Stage.PlaceTeacher)
            {
                placedTeacher = FindPlacedTeacher();
                if (placedTeacher == null) return;
                stage = Stage.WatchTeacher;
                TutorialInputGate.DisableAll();
                if (combat != null) combat.ClearMagazine();
                var word = placedTeacher.VariantIndex switch
                {
                    0 => "FACE",
                    1 => "MILK",
                    2 => "ROOT",
                    _ => "WXYZ"
                };
                target = spawner?.SpawnWord(word, 0f);
                if (target != null)
                {
                    target.SetMovementMultiplier(0f);
                    target.PinNearWorldPosition(placedTeacher.transform.position + Vector3.right * Mathf.Max(.6f, placedTeacher.CurrentRange * .55f));
                }
                ShowObjective("LET THE TEACHER FIRE", $"Typing is paused. Watch your {placedTeacher.VariantName} Teacher destroy {word} inside its range.");
            }
        }

        private void OnTargetDefeated(EnemyAgent enemy)
        {
            if (enemy != target) return;
            if (stage == Stage.DefeatForCells)
            {
                stage = Stage.CollectCells;
                ShowObjective("COLLECT THE DROP", "Walk over the glowing brain cells. They are run-only currency used to build the turret.");
            }
            else if (stage == Stage.WatchTeacher)
            {
                stage = Stage.Complete;
                StartCoroutine(ShowCompletion());
            }
        }

        private IEnumerator ShowCompletion()
        {
            yield return new WaitForSecondsRealtime(.8f);
            if (objectiveRoot != null) objectiveRoot.SetActive(false);
            if (acknowledgementTitle != null) acknowledgementTitle.text = "TURRET TRAINING COMPLETE";
            if (acknowledgementBody != null) acknowledgementBody.text = "You earned brain cells, chose a route-side position, placed a Teacher, and watched it attack the letters covered by its specialization.";
            if (acknowledgementRoot != null) acknowledgementRoot.SetActive(true);
        }

        private TurretPadController FindPlacedTeacher()
        {
            if (turretRoot == null) return null;
            foreach (var turret in turretRoot.GetComponentsInChildren<TurretPadController>(true))
                if (turret != null && !turret.IsPreview && turret.Kind == TurretKind.Teacher) return turret;
            return null;
        }

        private void ShowObjective(string title, string description)
        {
            if (objectiveRoot != null) objectiveRoot.SetActive(true);
            if (heading != null) heading.text = title;
            if (body != null) body.text = description;
        }

        private void OnDisable()
        {
            if (combat != null) combat.TargetDefeated -= OnTargetDefeated;
        }
    }
}
