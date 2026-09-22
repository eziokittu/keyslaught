using UnityEngine;

namespace KeySlaught.SceneGameplay
{
    public enum TurretKind
    {
        None,
        Teacher,
        Engineer,
        Scientist
    }

    public sealed class TurretPadController : MonoBehaviour
    {
        [SerializeField] private Vector3Int cellPosition;
        [SerializeField] private SpriteRenderer turretRenderer;
        [SerializeField] private Sprite teacherSprite;
        [SerializeField] private Sprite engineerSprite;
        [SerializeField] private Sprite scientistSprite;

        public Vector3Int CellPosition => cellPosition;

        public TurretKind Kind { get; private set; }

        public int Level { get; private set; }

        public void Configure(
            Vector3Int cell,
            SpriteRenderer renderer,
            Sprite teacher,
            Sprite engineer,
            Sprite scientist)
        {
            cellPosition = cell;
            turretRenderer = renderer;
            teacherSprite = teacher;
            engineerSprite = engineer;
            scientistSprite = scientist;
            RefreshPresentation();
        }

        public bool Build(TurretKind kind)
        {
            if (Kind != TurretKind.None || kind == TurretKind.None)
            {
                return false;
            }

            Kind = kind;
            Level = 1;
            RefreshPresentation();
            return true;
        }

        public bool Upgrade()
        {
            if (Kind == TurretKind.None || Level >= 3)
            {
                return false;
            }

            Level++;
            RefreshPresentation();
            return true;
        }

        public void ResetPad()
        {
            Kind = TurretKind.None;
            Level = 0;
            RefreshPresentation();
        }

        private void Awake()
        {
            RefreshPresentation();
        }

        private void RefreshPresentation()
        {
            if (turretRenderer == null)
            {
                return;
            }

            switch (Kind)
            {
                case TurretKind.Teacher:
                    turretRenderer.sprite = teacherSprite;
                    break;
                case TurretKind.Engineer:
                    turretRenderer.sprite = engineerSprite;
                    break;
                case TurretKind.Scientist:
                    turretRenderer.sprite = scientistSprite;
                    break;
                default:
                    turretRenderer.sprite = null;
                    break;
            }

            turretRenderer.transform.localScale = Vector3.one * (1f + Mathf.Max(0, Level - 1) * 0.12f);
        }
    }
}
