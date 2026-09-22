using UnityEngine;

namespace KeySlaught.SceneGameplay
{
    public sealed class PlayerPixelAnimator : MonoBehaviour
    {
        [SerializeField] private PlayerMover player;
        [SerializeField] private SpriteRenderer targetRenderer;
        [SerializeField] private Sprite[] walkFrames;
        [SerializeField, Min(1f)] private float framesPerSecond = 7f;

        private float frameTime;

        public void Configure(PlayerMover mover, SpriteRenderer renderer, Sprite[] frames)
        {
            player = mover;
            targetRenderer = renderer;
            walkFrames = frames;
        }

        private void Update()
        {
            if (targetRenderer == null || walkFrames == null || walkFrames.Length == 0)
            {
                return;
            }

            if (player == null || player.LastMovementInput.sqrMagnitude < 0.001f)
            {
                targetRenderer.sprite = walkFrames[0];
                frameTime = 0f;
                return;
            }

            frameTime += Time.deltaTime * framesPerSecond;
            targetRenderer.sprite = walkFrames[Mathf.FloorToInt(frameTime) % walkFrames.Length];
            if (Mathf.Abs(player.LastMovementInput.x) > 0.05f)
            {
                targetRenderer.flipX = player.LastMovementInput.x < 0f;
            }
        }
    }
}
