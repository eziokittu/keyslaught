using UnityEngine;

namespace KeySlaught.SceneGameplay
{
    public sealed class TutorialMovementChecklist
    {
        private readonly float requiredDistance;
        private float up, down, left, right;

        public TutorialMovementChecklist(float distancePerDirection = .7f) => requiredDistance = Mathf.Max(.1f, distancePerDirection);
        public bool UpComplete => up >= requiredDistance;
        public bool DownComplete => down >= requiredDistance;
        public bool LeftComplete => left >= requiredDistance;
        public bool RightComplete => right >= requiredDistance;
        public bool IsComplete => UpComplete && DownComplete && LeftComplete && RightComplete;

        public void Advance(Vector2 displacement)
        {
            if (displacement.y > 0f) up += displacement.y; else down -= displacement.y;
            if (displacement.x > 0f) right += displacement.x; else left -= displacement.x;
        }

        public string FormatTodos() =>
            $"{Tick(UpComplete)} MOVE UP\n{Tick(DownComplete)} MOVE DOWN\n{Tick(LeftComplete)} MOVE LEFT\n{Tick(RightComplete)} MOVE RIGHT";
        private static string Tick(bool complete) => complete
            ? "<color=#62F0A7>\u2713</color>"
            : "<color=#A9B4C8>\u25A1</color>";
    }
}
