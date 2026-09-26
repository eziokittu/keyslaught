using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace KeySlaught.SceneGameplay
{
    public sealed class CorruptionFeedback : MonoBehaviour
    {
        [SerializeField] private GameplayCombatController combat;
        [SerializeField] private PlayerMover player;
        [SerializeField] private CanvasGroup typingArea;
        [SerializeField] private GameObject overlay;
        [SerializeField] private Text countdown;
        private Vector3 origin;
        public void Configure(GameplayCombatController controller, PlayerMover playerMover, CanvasGroup bottom, GameObject root, Text label)
        { combat = controller; player = playerMover; typingArea = bottom; overlay = root; countdown = label; }
        private void OnEnable() { if (combat != null) combat.Corrupted += OnCorrupted; }
        private void OnDisable() { if (combat != null) combat.Corrupted -= OnCorrupted; }
        private void Update()
        {
            var active = combat != null && combat.Corruption.IsCorrupted;
            if (typingArea != null) { typingArea.alpha = active ? .28f : 1f; typingArea.interactable = !active; typingArea.blocksRaycasts = !active; }
            if (overlay != null) overlay.SetActive(active);
            if (active && countdown != null) countdown.text = $"WAND CORRUPTED\n{combat.Corruption.SecondsRemaining:0.0}s";
        }
        private void OnCorrupted(float seconds)
        {
            if (player == null) return; StopAllCoroutines(); StartCoroutine(Knockback());
        }
        private IEnumerator Knockback()
        {
            origin = player.transform.position; var away = (origin - transform.position).normalized;
            if (away.sqrMagnitude < .01f) away = Vector3.down;
            var end = origin + away * .38f; var duration = .22f;
            for (var t = 0f; t < duration; t += Time.unscaledDeltaTime)
            { var p = t / duration; player.transform.position = Vector3.Lerp(origin, end, Mathf.Sin(p * Mathf.PI)); yield return null; }
            player.transform.position = origin;
        }
    }
}
