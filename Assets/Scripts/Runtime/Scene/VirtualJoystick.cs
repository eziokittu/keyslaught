using UnityEngine;
using UnityEngine.EventSystems;

namespace KeySlaught.SceneGameplay
{
    public sealed class VirtualJoystick : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        [SerializeField] private PlayerMover player;
        [SerializeField] private RectTransform activationSurface;
        [SerializeField] private RectTransform baseRect;
        [SerializeField] private RectTransform handleRect;
        [SerializeField, Range(0f, 0.9f)] private float deadZone = 0.15f;

        private int activePointerId = int.MinValue;

        public Vector2 Value { get; private set; }

        public void Configure(PlayerMover playerMover, RectTransform joystickBase, RectTransform handle)
        {
            Configure(playerMover, joystickBase, joystickBase, handle);
        }

        public void Configure(
            PlayerMover playerMover,
            RectTransform surface,
            RectTransform joystickBase,
            RectTransform handle)
        {
            player = playerMover;
            activationSurface = surface;
            baseRect = joystickBase;
            handleRect = handle;
            if (baseRect != null && activationSurface != baseRect)
            {
                baseRect.gameObject.SetActive(false);
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (activePointerId == int.MinValue)
            {
                activePointerId = eventData.pointerId;
                if (baseRect != null && activationSurface != null && activationSurface != baseRect)
                {
                    RectTransformUtility.ScreenPointToLocalPointInRectangle(
                        activationSurface,
                        eventData.position,
                        eventData.pressEventCamera,
                        out var spawnPoint);
                    baseRect.anchoredPosition = spawnPoint;
                    baseRect.gameObject.SetActive(true);
                }
                UpdateValue(eventData);
            }
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (eventData.pointerId == activePointerId)
            {
                UpdateValue(eventData);
            }
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (eventData.pointerId == activePointerId)
            {
                ResetJoystick();
            }
        }

        private void OnDisable()
        {
            ResetJoystick();
        }

        private void UpdateValue(PointerEventData eventData)
        {
            if (baseRect == null)
            {
                return;
            }

            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                baseRect,
                eventData.position,
                eventData.pressEventCamera,
                out var localPoint);
            var radius = Mathf.Max(1f, Mathf.Min(baseRect.rect.width, baseRect.rect.height) * 0.5f);
            var normalized = Vector2.ClampMagnitude(localPoint / radius, 1f);
            Value = normalized.magnitude < deadZone ? Vector2.zero : normalized;
            if (handleRect != null)
            {
                handleRect.anchoredPosition = Value * radius * 0.55f;
            }

            player?.SetVirtualMovement(Value);
        }

        private void ResetJoystick()
        {
            activePointerId = int.MinValue;
            Value = Vector2.zero;
            if (handleRect != null)
            {
                handleRect.anchoredPosition = Vector2.zero;
            }

            player?.SetVirtualMovement(Vector2.zero);
            if (baseRect != null && activationSurface != null && activationSurface != baseRect)
            {
                baseRect.gameObject.SetActive(false);
            }
        }
    }
}
