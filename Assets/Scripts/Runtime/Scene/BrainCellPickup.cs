using UnityEngine;

namespace KeySlaught.SceneGameplay
{
    public sealed class BrainCellPickup : MonoBehaviour
    {
        public int Value { get; private set; }
        public void Initialize(int value) => Value = Mathf.Max(0, value);
        public void Add(int value)
        {
            Value += Mathf.Max(0, value);
            name = $"Brain Cells +{Value}";
            transform.localScale = Vector3.one * Mathf.Min(0.7f, 0.42f + Value * 0.015f);
        }
    }
}
