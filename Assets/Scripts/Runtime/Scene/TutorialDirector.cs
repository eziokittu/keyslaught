using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace KeySlaught.SceneGameplay
{
    public sealed class TutorialDirector : MonoBehaviour
    {
        [SerializeField] private GameObject overlay;
        [SerializeField] private Text titleLabel;
        [SerializeField] private Text bodyLabel;
        [SerializeField] private PlayerMover player;
        [SerializeField] private WaveRunController run;
        [SerializeField] private Button continueButton;
        [SerializeField] private EnemySpawner spawner;
        [SerializeField] private GameplayCombatController combat;
        [SerializeField] private LibraryEndpoint library;
        private int step;
        private Vector3 movementStart;

        public void Configure(GameObject root, Text title, Text body, Button next, PlayerMover mover, WaveRunController runController)
        {
            overlay = root; titleLabel = title; bodyLabel = body; continueButton = next; player = mover; run = runController;
            spawner = FindFirstObjectByType<EnemySpawner>();
            combat = FindFirstObjectByType<GameplayCombatController>();
            library = FindFirstObjectByType<LibraryEndpoint>();
        }

        public void Begin()
        {
            step = 0;
            movementStart = player == null ? Vector3.zero : player.transform.position;
            run?.SetGuidanceSuspended(true);
            Show("MOVE YOUR LIBRARIAN", "Use arrow keys, a controller, or drag anywhere in the arena. Water and crystal mountains block movement; paths, trees, rocks, ground, and the Library remain approachable. Move a little, then continue.");
        }

        public void ContinueTutorial()
        {
            step++;
            switch (step)
            {
                case 1:
                    Show("TYPE TO DEFEND", "Enemies carry words. Type the highlighted first letter to load it into your wand; matching letters fire automatically when an enemy enters range. This beginner run uses only short, slow words.");
                    break;
                case 2:
                    Show("PROTECT THE LIBRARY", "Watch enemies travel toward the Divine Library. Any enemy that reaches it removes HP equal to its remaining letters. The first wave demonstrates the danger at a forgiving pace.");
                    break;
                case 3:
                    StartCoroutine(DemonstrateLibraryAttack());
                    break;
                case 4:
                    Show("PREPARE BETWEEN WAVES", "Every wave is followed by a 30-second preparation timer. Build, repair, or press FAST FORWARD when you are ready for the next wave.");
                    break;
                default:
                    if (overlay != null) overlay.SetActive(false);
                    run?.SetGuidanceSuspended(false);
                    break;
            }
        }

        private IEnumerator DemonstrateLibraryAttack()
        {
            if (overlay != null) overlay.SetActive(false);
            if (combat != null) combat.enabled = false;
            var demo = spawner == null ? null : spawner.SpawnWord("LOSS", 5f);
            demo?.SetMovementMultiplier(1f);
            var timeout = 12f;
            while (demo != null && !demo.HasArrived && timeout > 0f)
            {
                timeout -= Time.unscaledDeltaTime;
                yield return null;
            }
            library?.ResetState();
            if (combat != null) combat.enabled = true;
            Show("COLLECT BRAIN CELLS", "That enemy reached the Library and removed HP equal to its remaining letters. Defeated words instead scatter bright cyan brain cells beside the path. Walk over them to raise the BRAIN CELLS counter and buy upgrades.");
        }

        private void Show(string title, string body)
        {
            if (overlay != null) overlay.SetActive(true);
            if (titleLabel != null) titleLabel.text = title;
            if (bodyLabel != null) bodyLabel.text = body;
        }
    }
}
