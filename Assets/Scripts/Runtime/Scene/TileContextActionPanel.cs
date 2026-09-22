using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

namespace KeySlaught.SceneGameplay
{
    public enum TileContextKind { None, Library, BuildableGround, OccupiedGround }

    public sealed class TileContextActionPanel : MonoBehaviour
    {
        private enum MenuDepth { Root, TurretChoice, Confirm }
        private static readonly Key[] NumberKeys =
        {
            Key.Digit1, Key.Digit2, Key.Digit3, Key.Digit4, Key.Digit5,
            Key.Digit6, Key.Digit7, Key.Digit8, Key.Digit9
        };

        [SerializeField] private PlayerMover player;
        [SerializeField] private Tilemap groundTilemap;
        [SerializeField] private Tilemap pathTilemap;
        [SerializeField] private Tilemap blockedTilemap;
        [SerializeField] private LibraryEndpoint library;
        [SerializeField] private Vector3Int libraryCellMin;
        [SerializeField] private Vector3Int libraryCellMax;
        [SerializeField] private SpriteRenderer highlightRenderer;
        [SerializeField] private Transform turretRoot;
        [SerializeField] private Sprite teacherSprite;
        [SerializeField] private Sprite engineerSprite;
        [SerializeField] private Sprite scientistSprite;
        [SerializeField] private BrainCellEconomy economy;
        [SerializeField] private GameObject panelRoot;
        [SerializeField] private Text titleLabel;
        [SerializeField] private Text statusLabel;
        [SerializeField] private Button[] actionButtons;
        [SerializeField] private Text[] actionLabels;

        private readonly Dictionary<Vector3Int, TurretPadController> turrets = new();
        private Vector3Int lastCell = new(int.MinValue, int.MinValue, int.MinValue);
        private MenuDepth menuDepth;
        private TurretPadController activePad;
        private TurretKind previewKind;

        public TileContextKind CurrentContext { get; private set; }
        public TurretPadController ActivePad => activePad;
        public Vector3Int HighlightedCell => lastCell;

        public void ConfigureEconomy(BrainCellEconomy runEconomy)
        {
            economy = runEconomy;
            RefreshLabels();
        }

        public void Configure(
            PlayerMover playerMover, Tilemap ground, Tilemap path, Tilemap blocked,
            LibraryEndpoint libraryEndpoint, Vector3Int libraryMinimum,
            Vector3Int libraryMaximum, SpriteRenderer highlight,
            Transform authoredTurretRoot, Sprite teacher, Sprite engineer, Sprite scientist,
            GameObject root, Text title, Text status, Button[] buttons, Text[] labels)
        {
            player = playerMover;
            groundTilemap = ground;
            pathTilemap = path;
            blockedTilemap = blocked;
            library = libraryEndpoint;
            libraryCellMin = libraryMinimum;
            libraryCellMax = libraryMaximum;
            highlightRenderer = highlight;
            turretRoot = authoredTurretRoot;
            teacherSprite = teacher;
            engineerSprite = engineer;
            scientistSprite = scientist;
            panelRoot = root;
            titleLabel = title;
            statusLabel = status;
            actionButtons = buttons;
            actionLabels = labels;
            lastCell = new Vector3Int(int.MinValue, int.MinValue, int.MinValue);
            menuDepth = MenuDepth.Root;
            ReindexAuthoredTurrets();
            RefreshContext(true);
        }

        public bool SelectAction(int oneBasedIndex)
        {
            if (oneBasedIndex < 1 || oneBasedIndex > 9) return false;
            var changed = CurrentContext switch
            {
                TileContextKind.Library => SelectLibraryAction(oneBasedIndex),
                TileContextKind.BuildableGround => SelectBuildAction(oneBasedIndex),
                TileContextKind.OccupiedGround => SelectOccupiedAction(oneBasedIndex),
                _ => false
            };
            RefreshLabels();
            return changed;
        }

        private void Update()
        {
            RefreshContext(false);
            var keyboard = Keyboard.current;
            if (keyboard == null) return;
            for (var index = 0; index < NumberKeys.Length; index++)
                if (keyboard[NumberKeys[index]].wasPressedThisFrame) SelectAction(index + 1);
        }

        private void RefreshContext(bool force)
        {
            if (player == null || groundTilemap == null)
            {
                SetContext(TileContextKind.None, null);
                return;
            }

            var cell = groundTilemap.WorldToCell(player.transform.position);
            if (!force && cell == lastCell) return;
            CancelPreview();
            lastCell = cell;
            menuDepth = MenuDepth.Root;
            var isLibrary = cell.x >= libraryCellMin.x && cell.x <= libraryCellMax.x &&
                cell.y >= libraryCellMin.y && cell.y <= libraryCellMax.y;
            var isGround = groundTilemap.HasTile(cell);
            var blocked = (pathTilemap != null && pathTilemap.HasTile(cell)) ||
                (blockedTilemap != null && blockedTilemap.HasTile(cell));

            if (isLibrary) SetContext(TileContextKind.Library, null);
            else if (isGround && !blocked)
            {
                turrets.TryGetValue(cell, out var pad);
                SetContext(pad == null || pad.Kind == TurretKind.None
                    ? TileContextKind.BuildableGround : TileContextKind.OccupiedGround, pad);
            }
            else SetContext(TileContextKind.None, null);

            if (highlightRenderer != null)
            {
                highlightRenderer.gameObject.SetActive(CurrentContext is TileContextKind.BuildableGround or TileContextKind.OccupiedGround);
                highlightRenderer.transform.position = groundTilemap.GetCellCenterWorld(cell);
            }
        }

        private void SetContext(TileContextKind context, TurretPadController pad)
        {
            CurrentContext = context;
            activePad = pad;
            if (panelRoot != null) panelRoot.SetActive(context != TileContextKind.None);
            RefreshLabels();
        }

        private bool SelectLibraryAction(int index)
        {
            if (index == 1 && library != null)
            {
                if (library.State != null && library.State.CurrentHealth >= library.State.MaximumHealth)
                {
                    SetStatus("HEALTH ALREADY FULL");
                    return false;
                }
                if (economy == null || !economy.TrySpend(5))
                {
                    SetStatus("NEED 5 BRAIN CELLS");
                    return false;
                }
                var amount = library.Repair(5);
                SetStatus(amount > 0 ? $"REPAIRED +{amount}" : "HEALTH ALREADY FULL");
                return amount > 0;
            }
            if (index == 2) SetStatus("ABILITIES UNLOCK IN A LATER MILESTONE");
            return false;
        }

        private bool SelectBuildAction(int index)
        {
            if (menuDepth == MenuDepth.Root)
            {
                if (index != 1) return false;
                menuDepth = MenuDepth.TurretChoice;
                return true;
            }
            if (menuDepth == MenuDepth.TurretChoice)
            {
                var kind = index switch
                {
                    1 => TurretKind.Teacher, 2 => TurretKind.Engineer,
                    3 => TurretKind.Scientist, _ => TurretKind.None
                };
                if (kind == TurretKind.None) return false;
                activePad = CreatePreview(kind);
                previewKind = kind;
                menuDepth = MenuDepth.Confirm;
                return true;
            }
            if (index == 1 && activePad != null)
            {
                var cost = CostFor(previewKind);
                if (economy == null || !economy.TrySpend(cost))
                {
                    SetStatus($"NEED {cost} BRAIN CELLS");
                    return false;
                }
                activePad.GetComponent<SpriteRenderer>().color = Color.white;
                turrets[lastCell] = activePad;
                previewKind = TurretKind.None;
                menuDepth = MenuDepth.Root;
                CurrentContext = TileContextKind.OccupiedGround;
                SetStatus("TURRET PLACED");
                return true;
            }
            if (index == 2)
            {
                CancelPreview();
                menuDepth = MenuDepth.TurretChoice;
                SetStatus("PLACEMENT CANCELLED");
                return true;
            }
            return false;
        }

        private bool SelectOccupiedAction(int index)
        {
            if (activePad == null) return false;
            if (index == 1)
            {
                var cost = 5 + activePad.Level * 3;
                if (activePad.Level >= 3) { SetStatus("MAX LEVEL"); return false; }
                if (economy == null || !economy.TrySpend(cost)) { SetStatus($"NEED {cost} BRAIN CELLS"); return false; }
                var upgraded = activePad.Upgrade();
                SetStatus(upgraded ? $"UPGRADED TO LEVEL {activePad.Level}" : "MAX LEVEL");
                return upgraded;
            }
            if (index == 2)
            {
                economy?.Credit(3 + activePad.Level * 2);
                turrets.Remove(lastCell);
                Destroy(activePad.gameObject);
                activePad = null;
                CurrentContext = TileContextKind.BuildableGround;
                SetStatus("TURRET SOLD");
                return true;
            }
            return false;
        }

        private TurretPadController CreatePreview(TurretKind kind)
        {
            var gameObject = new GameObject($"Turret {lastCell.x},{lastCell.y}");
            gameObject.transform.SetParent(turretRoot == null ? transform : turretRoot, false);
            gameObject.transform.position = groundTilemap.GetCellCenterWorld(lastCell);
            var renderer = gameObject.AddComponent<SpriteRenderer>();
            renderer.sortingOrder = 12;
            renderer.color = new Color(1f, 1f, 1f, 0.65f);
            var pad = gameObject.AddComponent<TurretPadController>();
            pad.Configure(lastCell, renderer, teacherSprite, engineerSprite, scientistSprite);
            pad.Build(kind);
            return pad;
        }

        private void CancelPreview()
        {
            if (previewKind == TurretKind.None || activePad == null)
            {
                previewKind = TurretKind.None;
                return;
            }
            Destroy(activePad.gameObject);
            activePad = null;
            previewKind = TurretKind.None;
        }

        private void ReindexAuthoredTurrets()
        {
            turrets.Clear();
            if (turretRoot == null) return;
            foreach (var pad in turretRoot.GetComponentsInChildren<TurretPadController>(true))
                if (pad != null && pad.Kind != TurretKind.None) turrets[pad.CellPosition] = pad;
        }

        private void RefreshLabels()
        {
            ClearActions();
            switch (CurrentContext)
            {
                case TileContextKind.Library:
                    SetTitle("LIBRARY");
                    var canRepair = economy != null && economy.Balance >= 5 && library != null && library.State != null && library.State.CurrentHealth < library.State.MaximumHealth;
                    SetAction(1, "1  REPAIR +5  •  5", canRepair); SetAction(2, "2  ABILITIES"); break;
                case TileContextKind.BuildableGround:
                    if (menuDepth == MenuDepth.Root) { SetTitle("EMPTY GROUND"); SetAction(1, "1  TURRETS"); }
                    else if (menuDepth == MenuDepth.TurretChoice)
                    { SetTitle("CHOOSE TURRET"); SetAction(1, "1  TEACHER • 6", economy != null && economy.Balance >= 6); SetAction(2, "2  ENGINEER • 8", economy != null && economy.Balance >= 8); SetAction(3, "3  SCIENTIST • 10", economy != null && economy.Balance >= 10); }
                    else { SetTitle($"{previewKind.ToString().ToUpperInvariant()} PREVIEW"); SetAction(1, "1  KEEP"); SetAction(2, "2  CANCEL"); }
                    break;
                case TileContextKind.OccupiedGround:
                    SetTitle($"{activePad.Kind.ToString().ToUpperInvariant()}  LV {activePad.Level}");
                    var upgradeCost = 5 + activePad.Level * 3;
                    SetAction(1, activePad.Level >= 3 ? "1  MAX LEVEL" : $"1  UPGRADE • {upgradeCost}", activePad.Level < 3 && economy != null && economy.Balance >= upgradeCost); SetAction(2, "2  SELL"); break;
                default: SetTitle(string.Empty); SetStatus(string.Empty); break;
            }
        }

        private void ClearActions()
        {
            if (actionButtons == null) return;
            for (var index = 0; index < actionButtons.Length; index++)
            {
                actionButtons[index].gameObject.SetActive(false);
                if (actionLabels != null && index < actionLabels.Length && actionLabels[index] != null) actionLabels[index].text = string.Empty;
            }
        }

        private void SetAction(int oneBasedIndex, string label, bool interactable = true)
        {
            var index = oneBasedIndex - 1;
            if (index < 0 || actionButtons == null || index >= actionButtons.Length) return;
            actionButtons[index].gameObject.SetActive(true);
            actionButtons[index].interactable = interactable;
            if (actionLabels != null && index < actionLabels.Length && actionLabels[index] != null) actionLabels[index].text = label;
        }
        private void SetTitle(string text) { if (titleLabel != null) titleLabel.text = text; }
        private void SetStatus(string text) { if (statusLabel != null) statusLabel.text = text; }
        private static int CostFor(TurretKind kind) => kind switch
        {
            TurretKind.Teacher => 6,
            TurretKind.Engineer => 8,
            TurretKind.Scientist => 10,
            _ => int.MaxValue
        };
    }
}
