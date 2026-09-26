using UnityEngine;

namespace KeySlaught.UI
{
    [ExecuteAlways]
    [RequireComponent(typeof(RectTransform))]
    public sealed class SafeAreaPadding : MonoBehaviour
    {
        [SerializeField, Min(0f)] private float minimumTopPixels = 20f;
        [SerializeField, Min(0f)] private float minimumBottomPixels = 20f;
        private Rect lastSafeArea;
        private Vector2Int lastScreen;
        private void OnEnable() => Apply();
        private void Update() { if (lastSafeArea != Screen.safeArea || lastScreen.x != Screen.width || lastScreen.y != Screen.height) Apply(); }
        public void Configure(float top, float bottom) { minimumTopPixels = top; minimumBottomPixels = bottom; Apply(); }
        public void Apply()
        {
            var rect = (RectTransform)transform; var safe = Screen.safeArea;
            var bottom = Mathf.Max(minimumBottomPixels, safe.yMin);
            var top = Mathf.Max(minimumTopPixels, Screen.height - safe.yMax);
            rect.offsetMin = new Vector2(rect.offsetMin.x, bottom);
            rect.offsetMax = new Vector2(rect.offsetMax.x, -top);
            lastSafeArea = safe; lastScreen = new Vector2Int(Screen.width, Screen.height);
        }
    }
}
