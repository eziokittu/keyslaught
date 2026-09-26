using UnityEngine;
using UnityEngine.UI;

namespace KeySlaught.UI
{
    public sealed class RunResultPresenter : MonoBehaviour
    {
        [SerializeField] private Text titleLabel;
        [SerializeField] private Text rewardsLabel;
        [SerializeField] private Text unlockLabel;
        [SerializeField] private Button primaryButton;
        [SerializeField] private Text primaryButtonLabel;

        public void Configure(Text title, Text rewards, Text unlock, Button primary, Text primaryText)
        {
            titleLabel = title;
            rewardsLabel = rewards;
            unlockLabel = unlock;
            primaryButton = primary;
            primaryButtonLabel = primaryText;
        }

        public void Present(bool victory, int researchReward, string unlockedTurret, bool hasNextLevel)
        {
            if (titleLabel != null) titleLabel.text = victory ? "LIBRARY DEFENDED" : "THE LIBRARY FELL";
            if (rewardsLabel != null) rewardsLabel.text = victory
                ? $"REWARDS\n+{researchReward} RESEARCH POINT{(researchReward == 1 ? string.Empty : "S")}\nBRAIN CELLS SAVED"
                : "REWARDS\nNO RESEARCH POINTS\nBRAIN CELLS SAVED";
            if (unlockLabel != null)
            {
                unlockLabel.text = string.IsNullOrWhiteSpace(unlockedTurret)
                    ? (victory ? "PROGRESS SAVED" : "REBUILD. RETYPE. RETURN.")
                    : $"NEW TOWER UNLOCKED\n{unlockedTurret.ToUpperInvariant()}";
                unlockLabel.color = string.IsNullOrWhiteSpace(unlockedTurret)
                    ? new Color(.78f, .82f, .88f)
                    : new Color(1f, .79f, .36f);
            }
            if (primaryButtonLabel != null) primaryButtonLabel.text = victory
                ? (hasNextLevel ? "NEXT LEVEL" : "CONTINUE")
                : "TRY AGAIN";
            if (primaryButton != null) primaryButton.interactable = true;
        }
    }
}
