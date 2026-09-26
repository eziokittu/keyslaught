using UnityEngine;

namespace KeySlaught.UI
{
    [RequireComponent(typeof(CanvasGroup))]
    public sealed class UiPanelEntranceAnimator : MonoBehaviour
    {
        [SerializeField, Min(.05f)] private float duration = .35f;
        [SerializeField] private float riseDistance = 28f;
        private CanvasGroup group;
        private RectTransform rect;
        private Vector2 destination;
        private float elapsed;

        public void Configure(float seconds, float rise)
        {
            duration = Mathf.Max(.05f, seconds);
            riseDistance = rise;
        }

        private void Awake()
        {
            group = GetComponent<CanvasGroup>();
            rect = transform as RectTransform;
        }

        private void OnEnable()
        {
            group ??= GetComponent<CanvasGroup>();
            rect ??= transform as RectTransform;
            destination = rect == null ? Vector2.zero : rect.anchoredPosition;
            elapsed = 0f;
            group.alpha = 0f;
            group.interactable = false;
            if (rect != null) rect.anchoredPosition = destination - Vector2.up * riseDistance;
        }

        private void Update()
        {
            if (group == null || group.alpha >= 1f) return;
            elapsed += Time.unscaledDeltaTime;
            var t = Mathf.Clamp01(elapsed / duration);
            t = 1f - Mathf.Pow(1f - t, 3f);
            group.alpha = t;
            group.interactable = t >= 1f;
            if (rect != null) rect.anchoredPosition = Vector2.Lerp(destination - Vector2.up * riseDistance, destination, t);
        }

        private void OnDisable()
        {
            if (group != null) { group.alpha = 1f; group.interactable = true; }
            if (rect != null) rect.anchoredPosition = destination;
        }
    }
}
