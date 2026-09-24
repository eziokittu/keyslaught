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
        [SerializeField] private WaveRunController waveRun;
        private float elapsedSeconds;

        public void ResetState()
        {
            elapsedSeconds = 0f;
            Refresh();
        }

        public void Configure(
            GameplaySceneCoordinator sceneCoordinator,
            Text wave,
            Text time,
            Text targetWord,
            WaveRunController runController = null)
        {
            coordinator = sceneCoordinator;
            waveLabel = wave;
            timeLabel = time;
            targetWordLabel = targetWord;
            waveRun = runController;
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
                var number = waveRun == null ? waveNumber : waveRun.CurrentWaveNumber;
                waveLabel.text = waveRun != null && waveRun.Phase == WaveRunPhase.Intermission
                    ? $"NEXT WAVE  {Mathf.CeilToInt(waveRun.IntermissionRemaining)}s"
                    : waveRun != null && waveRun.IsBossWave ? $"BOSS {number}" : $"WAVE {number}";
            }

            if (timeLabel != null)
            {
                var total = Mathf.FloorToInt(elapsedSeconds);
                timeLabel.text = $"{total / 60:00}:{total % 60:00}";
            }

            if (targetWordLabel != null)
            {
                var target = coordinator == null ? null : coordinator.GetPrimaryInRangeEnemy();
                targetWordLabel.text = target == null ? string.Empty : target.WordState.RemainingWord;
            }
        }
    }
}
