using System;
using System.Reflection;
using UnityEngine;

namespace KeySlaught.SceneGameplay
{
    [DefaultExecutionOrder(1000)]
    public sealed class CameraMotionZoom : MonoBehaviour
    {
        [SerializeField] private PlayerMover player;
        [SerializeField] private Camera targetCamera;
        [SerializeField] private Component cinemachineCamera;
        [SerializeField, Min(0.1f)] private float movingSize = 10.5f;
        [SerializeField, Min(0.1f)] private float idleSize = 7.25f;
        [SerializeField, Min(0f)] private float idleDelay = 0.8f;
        [SerializeField, Min(0.01f)] private float smoothTime = 0.65f;
        [SerializeField, Min(0f)] private float lookAheadDistance = 1.35f;
        [SerializeField, Min(0.01f)] private float translationSmoothTime = 0.38f;
        [SerializeField, Min(0f)] private float idleBreathAmount = 0.16f;
        [SerializeField, Min(0f)] private float idleBreathSpeed = 0.75f;

        private float idleSeconds;
        private float velocity;
        private Vector3 translationVelocity;
        private Vector3 baseLocalPosition;

        public void Configure(PlayerMover mover, Camera camera, Component virtualCamera = null)
        {
            player = mover;
            targetCamera = camera;
            cinemachineCamera = virtualCamera;
            baseLocalPosition = transform.localPosition;
        }

        private void LateUpdate()
        {
            var moving = player != null && player.LastMovementInput.sqrMagnitude > 0.01f;
            idleSeconds = moving ? 0f : idleSeconds + Time.unscaledDeltaTime;
            var breath = moving ? 0f : Mathf.Sin(Time.unscaledTime * idleBreathSpeed * Mathf.PI * 2f) * idleBreathAmount;
            var desired = (moving || idleSeconds < idleDelay ? movingSize : idleSize) + breath;
            var current = ReadSize();
            var next = Mathf.SmoothDamp(current, desired, ref velocity, smoothTime, Mathf.Infinity, Time.unscaledDeltaTime);
            WriteSize(next);
            var input = player == null ? Vector2.zero : player.LastMovementInput;
            var desiredOffset = baseLocalPosition + (Vector3)(input * lookAheadDistance);
            transform.localPosition = Vector3.SmoothDamp(transform.localPosition, desiredOffset,
                ref translationVelocity, translationSmoothTime, Mathf.Infinity, Time.unscaledDeltaTime);
        }

        private float ReadSize()
        {
            if (TryGetLens(out var lens, out var lensProperty, out var sizeField))
            {
                return Convert.ToSingle(sizeField.GetValue(lens));
            }

            return targetCamera == null ? idleSize : targetCamera.orthographicSize;
        }

        private void WriteSize(float size)
        {
            if (TryGetLens(out var lens, out var lensProperty, out var sizeField))
            {
                sizeField.SetValue(lens, size);
                lensProperty.SetValue(cinemachineCamera, lens);
            }
            else if (targetCamera != null)
            {
                targetCamera.orthographicSize = size;
            }
        }

        private bool TryGetLens(out object lens, out PropertyInfo lensProperty, out FieldInfo sizeField)
        {
            lens = null;
            lensProperty = null;
            sizeField = null;
            if (cinemachineCamera == null)
            {
                return false;
            }

            lensProperty = cinemachineCamera.GetType().GetProperty("Lens");
            lens = lensProperty?.GetValue(cinemachineCamera);
            sizeField = lens?.GetType().GetField("OrthographicSize");
            return lensProperty != null && lensProperty.CanWrite && sizeField != null;
        }
    }
}
