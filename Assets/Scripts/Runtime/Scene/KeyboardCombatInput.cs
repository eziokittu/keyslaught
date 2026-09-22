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
                combatController.TryStartRefresh();
            }

            foreach (var keyControl in keyboard.allKeys)
            {
                var code = keyControl.keyCode;
                if (keyControl.wasPressedThisFrame && code >= Key.A && code <= Key.Z)
                {
                    combatController.TryTypeLetter((char)('A' + code - Key.A));
                }
            }
        }
    }
}
