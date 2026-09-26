using KeySlaught.Progression;
using UnityEngine;
using UnityEngine.UI;

namespace KeySlaught.UI
{
    public sealed class GameSpeedSelector : MonoBehaviour
    {
        [SerializeField] private Text oneLabel;
        [SerializeField] private Text twoLabel;
        [SerializeField] private Text threeLabel;

        public void Configure(Text one, Text two, Text three)
        {
            oneLabel = one;
            twoLabel = two;
            threeLabel = three;
            Refresh();
        }

        public void Set1X() => Set(1);
        public void Set2X() => Set(2);
        public void Set3X() => Set(3);

        private void OnEnable() => Refresh();

        private void Set(int multiplier)
        {
            GameSpeedSettings.SetMultiplier(multiplier);
            if (Time.timeScale > 0f) GameSpeedSettings.ApplyGameplaySpeed();
            Refresh();
        }

        private void Refresh()
        {
            var labels = new[] { oneLabel, twoLabel, threeLabel };
            for (var index = 0; index < labels.Length; index++)
                if (labels[index] != null)
                    labels[index].text = GameSpeedSettings.Multiplier == index + 1
                        ? $"{index + 1}X  SELECTED"
                        : $"{index + 1}X";
        }
    }
}
