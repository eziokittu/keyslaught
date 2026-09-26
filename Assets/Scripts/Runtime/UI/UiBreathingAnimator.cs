using UnityEngine;

namespace KeySlaught.UI
{
    public sealed class UiBreathingAnimator : MonoBehaviour
    {
        [SerializeField] private float scaleAmount = .025f;
        [SerializeField] private float cyclesPerSecond = .55f;
        [SerializeField] private float phase;
        private Vector3 baseScale;
        public void Configure(float amount, float speed, float offset = 0f) { scaleAmount = amount; cyclesPerSecond = speed; phase = offset; }
        private void Awake() => baseScale = transform.localScale;
        private void OnEnable() { if (baseScale == Vector3.zero) baseScale = transform.localScale; }
        private void Update() { var pulse = 1f + Mathf.Sin((Time.unscaledTime + phase) * cyclesPerSecond * Mathf.PI * 2f) * scaleAmount; transform.localScale = baseScale * pulse; }
        private void OnDisable() { if (baseScale != Vector3.zero) transform.localScale = baseScale; }
    }
}
