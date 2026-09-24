using KeySlaught.SceneGameplay;
using UnityEngine;

namespace KeySlaught.Progression
{
    public sealed class PermanentUpgradeApplier : MonoBehaviour
    {
        [SerializeField] private ProgressionService progression;
        [SerializeField] private GameplaySceneCoordinator coordinator;
        [SerializeField] private GameplayCombatController combat;
        [SerializeField] private LibraryEndpoint library;
        [SerializeField] private float basePlayerRange = 4f;
        [SerializeField] private int baseMagazineCapacity = 4;
        [SerializeField] private float baseReloadSecondsPerSlot = 0.5f;
        [SerializeField] private int baseLibraryHealth = 30;

        public void Configure(ProgressionService service, GameplaySceneCoordinator sceneCoordinator,
            GameplayCombatController combatController, LibraryEndpoint endpoint)
        {
            progression = service; coordinator = sceneCoordinator; combat = combatController; library = endpoint;
        }

        public void Apply()
        {
            if (progression == null) return;
            coordinator?.SetPlayerAttackRange(basePlayerRange + progression.BonusFor(ResearchStat.PlayerRange));
            combat?.ApplyPermanentStats(
                baseMagazineCapacity + Mathf.RoundToInt(progression.BonusFor(ResearchStat.MagazineCapacity)),
                Mathf.Max(0.1f, baseReloadSecondsPerSlot - progression.BonusFor(ResearchStat.ReloadSpeed)));
            library?.SetMaximumHealth(baseLibraryHealth + Mathf.RoundToInt(progression.BonusFor(ResearchStat.LibraryHealth)));
        }
    }
}
