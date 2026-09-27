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
        [SerializeField] private Image libraryImage;
        [SerializeField] private Image victoryEmblem;
        [SerializeField] private Image defeatEmblem;
        [SerializeField] private RunResultVisualAnimator visualAnimator;
        [SerializeField] private GameObject rewardRevealRoot;
        [SerializeField] private Text rewardRevealTitle;
        [SerializeField] private Text rewardRevealBody;
        [SerializeField] private Image rewardRevealImage;

        public void Configure(Text title, Text rewards, Text unlock, Button primary, Text primaryText)
        {
            titleLabel = title;
            rewardsLabel = rewards;
            unlockLabel = unlock;
            primaryButton = primary;
            primaryButtonLabel = primaryText;
        }

        public void ConfigureVisuals(Image library, Image shield, Image skull, RunResultVisualAnimator animator)
        {
            libraryImage = library; victoryEmblem = shield; defeatEmblem = skull; visualAnimator = animator;
        }

        public void ConfigureRewardReveal(GameObject root, Text title, Text body, Image image)
        {
            rewardRevealRoot = root; rewardRevealTitle = title; rewardRevealBody = body; rewardRevealImage = image;
            if (rewardRevealRoot != null) rewardRevealRoot.SetActive(false);
        }

        public void Present(bool victory, int researchReward, string unlockedTurret, bool hasNextLevel)
        {
            gameObject.SetActive(true);
            if (rewardRevealRoot != null) rewardRevealRoot.SetActive(false);
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
            if (libraryImage != null) libraryImage.gameObject.SetActive(true);
            if (victoryEmblem != null) victoryEmblem.gameObject.SetActive(victory);
            if (defeatEmblem != null) defeatEmblem.gameObject.SetActive(!victory);
            visualAnimator?.SetVictory(victory);
        }

        public void PresentUnlockReward(string turretName, Sprite turretSprite)
        {
            if (rewardRevealTitle != null) rewardRevealTitle.text = "TUTORIAL REWARD";
            if (rewardRevealBody != null) rewardRevealBody.text = turretName.Contains("Ability")
                ? $"{turretName.ToUpperInvariant()} UNLOCKED\n\nThis Library ability is now available in every run."
                : $"{turretName.ToUpperInvariant()} UNLOCKED\n\nThis specialist is now available on buildable ground in every run.";
            if (rewardRevealImage != null) rewardRevealImage.sprite = turretSprite;
            if (rewardRevealImage != null) rewardRevealImage.gameObject.SetActive(turretSprite != null);
            if (rewardRevealRoot != null) rewardRevealRoot.SetActive(true);
        }

        public void HideRewardReveal()
        {
            if (rewardRevealRoot != null) rewardRevealRoot.SetActive(false);
        }
    }
}
