using UnityEngine;
using UnityEngine.UI;

namespace KeySlaught.SceneGameplay
{
    public sealed class GameplayTopHud : MonoBehaviour
    {
        [SerializeField] private GameplaySceneCoordinator coordinator;
        [SerializeField] private Text waveLabel;
        [SerializeField] private Text timeLabel;
        [SerializeField] private Text targetWordLabel;
        [SerializeField] private int waveNumber = 1;
        private float elapsedSeconds;

        public void Configure(
            GameplaySceneCoordinator sceneCoordinator,
            Text wave,
            Text time,
            Text targetWord)
        {
            coordinator = sceneCoordinator;
            waveLabel = wave;
            timeLabel = time;
            targetWordLabel = targetWord;
            Refresh();
        }

        private void Update()
        {
            elapsedSeconds += Time.deltaTime;
            Refresh();
        }

        private void Refresh()
        {
            if (waveLabel != null)
            {
                waveLabel.text = $"Wave {waveNumber}";
            }

            if (timeLabel != null)
            {
                var total = Mathf.FloorToInt(elapsedSeconds);
                timeLabel.text = $"{total / 60:00}:{total % 60:00}";
            }

            if (targetWordLabel != null)
            {
                var target = coordinator == null ? null : coordinator.GetPrimaryInRangeEnemy();
                targetWordLabel.text = target == null ? "NO TARGET IN RANGE" : target.WordState.RemainingWord;
            }
        }
    }
}
