using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;

namespace KeySlaught.SceneGameplay
{
    public sealed class PlayerMover : MonoBehaviour
    {
        [SerializeField] private InputActionAsset inputActions;
        [SerializeField] private string moveActionName = "Player/Move";
        [SerializeField, Min(0f)] private float movementSpeed = 5f;
        [SerializeField] private Vector2 minimumBounds = new Vector2(-7f, -8f);
        [SerializeField] private Vector2 maximumBounds = new Vector2(7f, 8f);
        [SerializeField] private Tilemap blockedTerrain;

        private InputAction resolvedMoveAction;
        private Vector2 virtualMovement;

        public Vector2 LastMovementInput { get; private set; }

        public float MovementSpeed => movementSpeed;

        public Vector2 VirtualMovement => virtualMovement;

        public void ApplyMovement(Vector2 input, float deltaSeconds)
        {
            var direction = Vector2.ClampMagnitude(input, 1f);
            var next = (Vector2)transform.position + direction * movementSpeed * deltaSeconds;
            next.x = Mathf.Clamp(next.x, minimumBounds.x, maximumBounds.x);
            next.y = Mathf.Clamp(next.y, minimumBounds.y, maximumBounds.y);
            var current = (Vector2)transform.position;
            var resolved = current;
            var xCandidate = new Vector2(next.x, current.y);
            if (!IsBlocked(xCandidate)) resolved.x = xCandidate.x;
            var yCandidate = new Vector2(resolved.x, next.y);
            if (!IsBlocked(yCandidate)) resolved.y = yCandidate.y;
            transform.position = new Vector3(resolved.x, resolved.y, transform.position.z);
        }

        public void ConfigureBlockedTerrain(Tilemap blocked) => blockedTerrain = blocked;

        private bool IsBlocked(Vector2 position) => blockedTerrain != null &&
            blockedTerrain.HasTile(blockedTerrain.WorldToCell(position));

        public void Configure(float speed, Vector2 minBounds, Vector2 maxBounds)
        {
            movementSpeed = Mathf.Max(0f, speed);
            minimumBounds = minBounds;
            maximumBounds = maxBounds;
        }

        public void SetVirtualMovement(Vector2 input)
        {
            virtualMovement = Vector2.ClampMagnitude(input, 1f);
        }

        private void OnEnable()
        {
            resolvedMoveAction = inputActions == null
                ? null
                : inputActions.FindAction(moveActionName, throwIfNotFound: false);
            resolvedMoveAction?.Enable();
        }

        private void OnDisable()
        {
            resolvedMoveAction?.Disable();
            resolvedMoveAction = null;
            virtualMovement = Vector2.zero;
            LastMovementInput = Vector2.zero;
        }

        private void Update()
        {
            var movement = ReadArrowAndControllerMovement();
            LastMovementInput = Vector2.ClampMagnitude(movement + virtualMovement, 1f);
            ApplyMovement(LastMovementInput, Time.deltaTime);
        }

        private static Vector2 ReadArrowAndControllerMovement()
        {
            var movement = Vector2.zero;
            var keyboard = Keyboard.current;
            if (keyboard != null)
            {
                movement.x = (keyboard.rightArrowKey.isPressed ? 1f : 0f) -
                    (keyboard.leftArrowKey.isPressed ? 1f : 0f);
                movement.y = (keyboard.upArrowKey.isPressed ? 1f : 0f) -
                    (keyboard.downArrowKey.isPressed ? 1f : 0f);
            }

            if (Gamepad.current != null)
            {
                movement += Gamepad.current.leftStick.ReadValue();
            }

            if (Joystick.current != null)
            {
                movement += Joystick.current.stick.ReadValue();
            }

            return Vector2.ClampMagnitude(movement, 1f);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(0.2f, 0.9f, 1f, 0.35f);
            Gizmos.DrawWireCube(
                (minimumBounds + maximumBounds) * 0.5f,
                maximumBounds - minimumBounds);
        }
    }
}
