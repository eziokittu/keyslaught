using UnityEngine;
using UnityEngine.UI;

namespace KeySlaught.UI
{
    /// <summary>Full-screen dimmer with a softly feathered circular opening around the active control.</summary>
    [RequireComponent(typeof(RectTransform), typeof(CanvasRenderer))]
    public sealed class TutorialFocusGuide : MaskableGraphic
    {
        [SerializeField] private RectTransform target;
        [SerializeField] private Vector2 padding = new(24f, 24f);
        [SerializeField, Range(12, 128)] private int segmentCount = 64;
        [SerializeField, Min(0f)] private float feather = 18f;

        private Vector2 holeCenter;
        private float holeRadius;
        private Vector3[] targetCorners;

        public RectTransform Target => target;

        protected override void Awake()
        {
            base.Awake();
            raycastTarget = false;
            color = new Color(0f, 0f, 0f, .82f);
            targetCorners = new Vector3[4];
        }

        public void Focus(RectTransform value, Vector2 extraPadding)
        {
            if (this == null) return;
            target = value;
            padding = extraPadding;
            gameObject.SetActive(target != null);
            RefreshHole();
        }

        public void Hide()
        {
            if (this == null) return;
            target = null;
            gameObject.SetActive(false);
        }

        private void LateUpdate() => RefreshHole();

        private void RefreshHole()
        {
            if (target == null) return;
            targetCorners ??= new Vector3[4];
            target.GetWorldCorners(targetCorners);
            var min = (Vector2)rectTransform.InverseTransformPoint(targetCorners[0]);
            var max = (Vector2)rectTransform.InverseTransformPoint(targetCorners[2]);
            var nextCenter = (min + max) * .5f;
            var half = (max - min) * .5f + padding;
            var nextRadius = Mathf.Max(half.x, half.y, 28f);
            if ((nextCenter - holeCenter).sqrMagnitude < .01f && Mathf.Abs(nextRadius - holeRadius) < .05f) return;
            holeCenter = nextCenter;
            holeRadius = nextRadius;
            SetVerticesDirty();
        }

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
            if (target == null) return;
            var bounds = rectTransform.rect;
            var segments = Mathf.Clamp(segmentCount, 12, 128);
            var innerColor = color; innerColor.a = 0f;
            for (var index = 0; index < segments; index++)
            {
                var a0 = Mathf.PI * 2f * index / segments;
                var a1 = Mathf.PI * 2f * (index + 1) / segments;
                var d0 = new Vector2(Mathf.Cos(a0), Mathf.Sin(a0));
                var d1 = new Vector2(Mathf.Cos(a1), Mathf.Sin(a1));
                var inner0 = holeCenter + d0 * holeRadius;
                var inner1 = holeCenter + d1 * holeRadius;
                var soft0 = holeCenter + d0 * (holeRadius + feather);
                var soft1 = holeCenter + d1 * (holeRadius + feather);
                var outer0 = RayToBounds(holeCenter, d0, bounds);
                var outer1 = RayToBounds(holeCenter, d1, bounds);
                AddQuad(vh, inner0, inner1, soft1, soft0, innerColor, innerColor, color, color);
                AddQuad(vh, soft0, soft1, outer1, outer0, color, color, color, color);
            }
        }

        private static Vector2 RayToBounds(Vector2 origin, Vector2 direction, Rect bounds)
        {
            var tx = direction.x > .0001f ? (bounds.xMax - origin.x) / direction.x :
                direction.x < -.0001f ? (bounds.xMin - origin.x) / direction.x : float.PositiveInfinity;
            var ty = direction.y > .0001f ? (bounds.yMax - origin.y) / direction.y :
                direction.y < -.0001f ? (bounds.yMin - origin.y) / direction.y : float.PositiveInfinity;
            return origin + direction * Mathf.Max(0f, Mathf.Min(tx, ty));
        }

        private static void AddQuad(VertexHelper vh, Vector2 a, Vector2 b, Vector2 c, Vector2 d,
            Color colorA, Color colorB, Color colorC, Color colorD)
        {
            var start = vh.currentVertCount;
            vh.AddVert(a, colorA, Vector2.zero); vh.AddVert(b, colorB, Vector2.zero);
            vh.AddVert(c, colorC, Vector2.zero); vh.AddVert(d, colorD, Vector2.zero);
            vh.AddTriangle(start, start + 1, start + 2); vh.AddTriangle(start, start + 2, start + 3);
        }
    }
}
