using UnityEngine;
using UnityEngine.InputSystem;

namespace KeySlaught.SceneGameplay
{
    public sealed class KeyboardCombatInput : MonoBehaviour
    {
        [SerializeField] private GameplayCombatController combatController;

        public void Configure(GameplayCombatController controller)
        {
            combatController = controller;
        }

        private void Update()
        {
            var keyboard = Keyboard.current;
            if (keyboard == null || combatController == null)
            {
                return;
            }

            if (keyboard.spaceKey.wasPressedThisFrame)
            {
                if (TutorialInputGate.TryAllowRefresh()) combatController.TryStartRefresh();
            }

            foreach (var keyControl in keyboard.allKeys)
            {
                var code = keyControl.keyCode;
                if (keyControl.wasPressedThisFrame && code >= Key.A && code <= Key.Z)
                {
                    var letter = (char)('A' + code - Key.A);
                    if (TutorialInputGate.TryAllowLetter(letter)) combatController.TryTypeLetter(letter);
                }
            }
        }
    }
}
