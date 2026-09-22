using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

namespace KeySlaught.SceneGameplay
{
    public enum TileContextKind
    {
        None,
        Library,
        TurretPad
    }

    public sealed class TileContextActionPanel : MonoBehaviour
    {
        private static readonly Key[] NumberKeys =
        {
            Key.Digit1,
            Key.Digit2,
            Key.Digit3,
            Key.Digit4,
            Key.Digit5,
            Key.Digit6,
            Key.Digit7,
            Key.Digit8,
            Key.Digit9
        };

        [SerializeField] private PlayerMover player;
        [SerializeField] private Tilemap contextTilemap;
        [SerializeField] private TileBase libraryTile;
        [SerializeField] private TileBase turretPadTile;
        [SerializeField] private LibraryEndpoint library;
        [SerializeField] private TurretPadController[] turretPads;
        [SerializeField] private GameObject panelRoot;
        [SerializeField] private Text titleLabel;
        [SerializeField] private Text statusLabel;
        [SerializeField] private Button[] actionButtons;
        [SerializeField] private Text[] actionLabels;

        private Vector3Int lastCell = new Vector3Int(int.MinValue, int.MinValue, int.MinValue);
        private TurretPadController activePad;

        public TileContextKind CurrentContext { get; private set; }

        public TurretPadController ActivePad => activePad;

        public void Configure(
            PlayerMover playerMover,
            Tilemap tilemap,
            TileBase authoredLibraryTile,
            TileBase authoredTurretPadTile,
            LibraryEndpoint libraryEndpoint,
            TurretPadController[] pads,
            GameObject root,
            Text title,
            Text status,
            Button[] buttons,
            Text[] labels)
        {
            player = playerMover;
            contextTilemap = tilemap;
            libraryTile = authoredLibraryTile;
            turretPadTile = authoredTurretPadTile;
            library = libraryEndpoint;
            turretPads = pads;
            panelRoot = root;
            titleLabel = title;
            statusLabel = status;
            actionButtons = buttons;
            actionLabels = labels;
            lastCell = new Vector3Int(int.MinValue, int.MinValue, int.MinValue);
            RefreshContext(force: true);
        }

        public bool SelectAction(int oneBasedIndex)
        {
            if (oneBasedIndex < 1 || oneBasedIndex > 9)
            {
                return false;
            }

            var result = CurrentContext switch
            {
                TileContextKind.Library => SelectLibraryAction(oneBasedIndex),
                TileContextKind.TurretPad => SelectTurretAction(oneBasedIndex),
                _ => false
            };
            RefreshLabels();
            return result;
        }

        private void Update()
        {
            RefreshContext(force: false);
            var keyboard = Keyboard.current;
            if (keyboard == null)
            {
                return;
            }

            for (var index = 0; index < NumberKeys.Length; index++)
            {
                if (keyboard[NumberKeys[index]].wasPressedThisFrame)
                {
                    SelectAction(index + 1);
                }
            }
        }

        private void RefreshContext(bool force)
        {
            if (player == null || contextTilemap == null)
            {
                SetContext(TileContextKind.None, null);
                return;
            }

            var cell = contextTilemap.WorldToCell(player.transform.position);
            if (!force && cell == lastCell)
            {
                return;
            }

            lastCell = cell;
            var tile = contextTilemap.GetTile(cell);
            if (tile == libraryTile)
            {
                SetContext(TileContextKind.Library, null);
            }
            else if (tile == turretPadTile)
            {
                SetContext(TileContextKind.TurretPad, FindPad(cell));
            }
            else
            {
                SetContext(TileContextKind.None, null);
            }
        }

        private TurretPadController FindPad(Vector3Int cell)
        {
            if (turretPads == null)
            {
                return null;
            }

            foreach (var pad in turretPads)
            {
                if (pad != null && pad.CellPosition == cell)
                {
                    return pad;
                }
            }

            return null;
        }

        private void SetContext(TileContextKind kind, TurretPadController pad)
        {
            CurrentContext = kind;
            activePad = pad;
            if (panelRoot != null)
            {
                panelRoot.SetActive(kind != TileContextKind.None);
            }

            RefreshLabels();
        }

        private bool SelectLibraryAction(int index)
        {
            if (index == 1 && library != null)
            {
                var amount = library.Repair(5);
                SetStatus(amount > 0 ? $"REPAIRED +{amount}" : "LIBRARY AT FULL HEALTH");
                return amount > 0;
            }

            if (index >= 2 && index <= 4)
            {
                SetStatus("ABILITY LOCKED FOR A LATER MILESTONE");
            }

            return false;
        }

        private bool SelectTurretAction(int index)
        {
            if (activePad == null)
            {
                SetStatus("NO AUTHORED TURRET PAD FOUND");
                return false;
            }

            if (activePad.Kind == TurretKind.None)
            {
                var kind = index switch
                {
                    1 => TurretKind.Teacher,
                    2 => TurretKind.Engineer,
                    3 => TurretKind.Scientist,
                    _ => TurretKind.None
                };
                var built = activePad.Build(kind);
                if (built)
                {
                    SetStatus($"BUILT {kind.ToString().ToUpperInvariant()} TURRET");
                }

                return built;
            }

            if (index == 1)
            {
                var upgraded = activePad.Upgrade();
                SetStatus(upgraded
                    ? $"UPGRADED TO LEVEL {activePad.Level}"
                    : "MAX LEVEL REACHED");
                return upgraded;
            }

            return false;
        }

        private void RefreshLabels()
        {
            ClearActions();
            switch (CurrentContext)
            {
                case TileContextKind.Library:
                    SetTitle("DIVINE LIBRARY");
                    SetAction(1, "1  REPAIR +5");
                    SetAction(2, "2  HISTORY [LOCKED]");
                    SetAction(3, "3  INFLUENCE [LOCKED]");
                    SetAction(4, "4  POLITICS [LOCKED]");
                    break;
                case TileContextKind.TurretPad:
                    SetTitle(activePad == null
                        ? "TURRET PAD"
                        : activePad.Kind == TurretKind.None
                            ? "BUILD TURRET"
                            : $"{activePad.Kind.ToString().ToUpperInvariant()}  LV {activePad.Level}");
                    if (activePad == null || activePad.Kind == TurretKind.None)
                    {
                        SetAction(1, "1  TEACHER");
                        SetAction(2, "2  ENGINEER");
                        SetAction(3, "3  SCIENTIST");
                    }
                    else
                    {
                        SetAction(1, activePad.Level >= 3 ? "1  MAX LEVEL" : "1  UPGRADE");
                    }
                    break;
                default:
                    SetTitle(string.Empty);
                    SetStatus(string.Empty);
                    break;
            }
        }

        private void ClearActions()
        {
            if (actionButtons == null || actionLabels == null)
            {
                return;
            }

            for (var index = 0; index < actionButtons.Length; index++)
            {
                actionButtons[index].gameObject.SetActive(false);
                if (index < actionLabels.Length && actionLabels[index] != null)
                {
                    actionLabels[index].text = string.Empty;
                }
            }
        }

        private void SetAction(int oneBasedIndex, string label)
        {
            var index = oneBasedIndex - 1;
            if (index < 0 || actionButtons == null || index >= actionButtons.Length)
            {
                return;
            }

            actionButtons[index].gameObject.SetActive(true);
            if (actionLabels != null && index < actionLabels.Length && actionLabels[index] != null)
            {
                actionLabels[index].text = label;
            }
        }

        private void SetTitle(string text)
        {
            if (titleLabel != null)
            {
                titleLabel.text = text;
            }
        }

        private void SetStatus(string text)
        {
            if (statusLabel != null)
            {
                statusLabel.text = text;
            }
        }
    }
}
