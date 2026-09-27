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
        [SerializeField, Min(0.1f)] private float movingSize = 10f;
        [SerializeField, Min(0.1f)] private float idleSize = 7f;
        [SerializeField, Min(0f)] private float idleDelay = 1f;
        [SerializeField, Min(0.01f)] private float smoothTime = 0.8f;
        [SerializeField, Min(0f)] private float lookAheadDistance = 2.05f;
        [SerializeField, Min(0.01f)] private float translationSmoothTime = 0.52f;
        [SerializeField, Min(0f)] private float idleBreathAmount = 0.16f;
        [SerializeField, Min(0f)] private float idleBreathSpeed = 0.75f;
        [SerializeField, Min(0f)] private float movementSwayAmount = .16f;
        [SerializeField, Min(0f)] private float movementSwaySpeed = 2.2f;
        [SerializeField, Min(0f)] private float directionalTiltDegrees = 1.4f;

        private float idleSeconds;
        private float currentSize;
        private bool sizeInitialized;
        private Vector3 translationVelocity;
        private Vector3 baseLocalPosition;
        private Quaternion baseLocalRotation;
        private float rotationVelocity;

        public void Configure(PlayerMover mover, Camera camera, Component virtualCamera = null)
        {
            player = mover;
            targetCamera = camera;
            cinemachineCamera = virtualCamera;
            baseLocalPosition = transform.localPosition;
            baseLocalRotation = transform.localRotation;
            InitializeSize();
        }

        private void Awake()
        {
            baseLocalPosition = transform.localPosition;
            baseLocalRotation = transform.localRotation;
            InitializeSize();
        }

        private void LateUpdate()
        {
            var moving = player != null && player.LastMovementInput.sqrMagnitude > 0.01f;
            idleSeconds = moving ? 0f : idleSeconds + Time.unscaledDeltaTime;
            var desired = moving || idleSeconds < idleDelay ? movingSize : idleSize;
            if (!sizeInitialized)
            {
                InitializeSize();
            }

            currentSize = DampSize(currentSize, desired, smoothTime, Time.unscaledDeltaTime);
            WriteSize(currentSize);

            // Issues 11 deliberately limits camera motion to orthographic zoom. Keeping the
            // authored transform fixed prevents look-ahead, sway, or Cinemachine roll jitter.
            transform.localPosition = baseLocalPosition;
            transform.localRotation = baseLocalRotation;
        }

        public static float DampSize(float current, float target, float smoothingSeconds, float deltaTime)
        {
            if (deltaTime <= 0f)
            {
                return current;
            }

            var alpha = 1f - Mathf.Exp(-deltaTime / Mathf.Max(0.01f, smoothingSeconds));
            return Mathf.LerpUnclamped(current, target, alpha);
        }

        private void InitializeSize()
        {
            currentSize = ReadSize();
            sizeInitialized = true;
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
