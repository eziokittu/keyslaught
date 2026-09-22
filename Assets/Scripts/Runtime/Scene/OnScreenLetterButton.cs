using UnityEngine;
using UnityEngine.UI;

namespace KeySlaught.SceneGameplay
{
    [RequireComponent(typeof(Button))]
    public sealed class OnScreenLetterButton : MonoBehaviour
    {
        [SerializeField] private GameplayCombatController combatController;
        [SerializeField] private char letter = 'A';

        private Button button;

        public char Letter => letter;

        public void Configure(GameplayCombatController controller, char combatLetter)
        {
            combatController = controller;
            letter = char.ToUpperInvariant(combatLetter);
        }

        private void Awake()
        {
            button = GetComponent<Button>();
            button.onClick.AddListener(SubmitLetter);
        }

        private void OnDestroy()
        {
            button?.onClick.RemoveListener(SubmitLetter);
        }

        private void SubmitLetter()
        {
            combatController?.TryTypeLetter(letter);
        }
    }
}
