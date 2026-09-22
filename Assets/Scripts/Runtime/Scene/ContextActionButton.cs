using UnityEngine;
using UnityEngine.UI;

namespace KeySlaught.SceneGameplay
{
    [RequireComponent(typeof(Button))]
    public sealed class ContextActionButton : MonoBehaviour
    {
        [SerializeField] private TileContextActionPanel panel;
        [SerializeField, Range(1, 9)] private int oneBasedIndex = 1;

        public void Configure(TileContextActionPanel actionPanel, int index)
        {
            panel = actionPanel;
            oneBasedIndex = Mathf.Clamp(index, 1, 9);
        }

        private void Awake()
        {
            GetComponent<Button>().onClick.AddListener(Select);
        }

        private void Select()
        {
            panel?.SelectAction(oneBasedIndex);
        }
    }
}
