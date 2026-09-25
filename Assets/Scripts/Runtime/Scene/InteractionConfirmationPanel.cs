using System;
using UnityEngine;
using UnityEngine.UI;

namespace KeySlaught.SceneGameplay
{
    public sealed class InteractionConfirmationPanel : MonoBehaviour
    {
        [SerializeField] private GameObject root;
        [SerializeField] private Text titleLabel;
        [SerializeField] private Text bodyLabel;
        [SerializeField] private Text costLabel;

        private Action keepAction;
        private Action cancelAction;

        public bool IsOpen => root != null && root.activeSelf;

        public void Configure(GameObject panelRoot, Text title, Text body, Text cost)
        {
            root = panelRoot;
            titleLabel = title;
            bodyLabel = body;
            costLabel = cost;
            Hide(false);
        }

        public void Show(string title, string body, int brainCellCost, Action keep, Action cancel = null)
        {
            keepAction = keep;
            cancelAction = cancel;
            if (titleLabel != null) titleLabel.text = title;
            if (bodyLabel != null) bodyLabel.text = body;
            if (costLabel != null) costLabel.text = $"REQUIRED  {brainCellCost} BRAIN CELLS";
            if (root != null) root.SetActive(true);
            Time.timeScale = 0f;
        }

        public void Keep()
        {
            var action = keepAction;
            Hide(true);
            action?.Invoke();
        }

        public void Cancel()
        {
            var action = cancelAction;
            Hide(true);
            action?.Invoke();
        }

        public void CloseImmediately() => Hide(true);

        private void Hide(bool restoreTime)
        {
            keepAction = null;
            cancelAction = null;
            if (root != null) root.SetActive(false);
            if (restoreTime) Time.timeScale = 1f;
        }

        private void OnDisable()
        {
            keepAction = null;
            cancelAction = null;
        }
    }
}
