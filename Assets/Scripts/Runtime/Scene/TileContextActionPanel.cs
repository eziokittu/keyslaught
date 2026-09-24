using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

namespace KeySlaught.SceneGameplay
{
    public enum TileContextKind { None, Library, BuildableGround, OccupiedGround, ClearableObstacle }

    public sealed class TileContextActionPanel : MonoBehaviour
    {
        private enum MenuDepth { Root, TurretChoice, Confirm, Abilities }
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
        [SerializeField] private GameplaySceneCoordinator coordinator;
        [SerializeField] private GameplayCombatController combat;
        [SerializeField] private TurretDefinition[] turretDefinitions;
        [SerializeField] private LibraryAbilityController abilityController;
        [SerializeField] private GameObject panelRoot;
        [SerializeField] private Text titleLabel;
        [SerializeField] private Text statusLabel;
        [SerializeField] private Button[] actionButtons;
        [SerializeField] private Text[] actionLabels;

        private readonly Dictionary<Vector3Int, TurretPadController> turrets = new();
        private readonly Dictionary<Vector3Int, TileBase> clearedObstacles = new();
        private Vector3Int lastCell = new(int.MinValue, int.MinValue, int.MinValue);
        private MenuDepth menuDepth;
        private TurretPadController activePad;
        private TurretKind previewKind;
        private bool optionPauseActive;

        public static bool ContinueDuringSelections
        {
            get => PlayerPrefs.GetInt("KeySlaught.ContinueDuringSelections", 0) != 0;
            set => PlayerPrefs.SetInt("KeySlaught.ContinueDuringSelections", value ? 1 : 0);
        }

        public TileContextKind CurrentContext { get; private set; }
        public TurretPadController ActivePad => activePad;
        public Vector3Int HighlightedCell => lastCell;

        public void ConfigureEconomy(BrainCellEconomy runEconomy)
        {
            economy = runEconomy;
            RefreshLabels();
        }

        public void ConfigureRuntimeSystems(GameplaySceneCoordinator sceneCoordinator,
            GameplayCombatController combatController, TurretDefinition[] dataDefinitions,
            LibraryAbilityController libraryAbilities)
        {
            coordinator = sceneCoordinator;
            combat = combatController;
            turretDefinitions = dataDefinitions;
            abilityController = libraryAbilities;
            RefreshLabels();
        }

        public void Configure(
            PlayerMover playerMover, Tilemap ground, Tilemap path, Tilemap blocked,
            LibraryEndpoint libraryEndpoint, Vector3Int libraryMinimum,
            Vector3Int libraryMaximum, SpriteRenderer highlight,
            Transform authoredTurretRoot, Sprite teacher, Sprite engineer, Sprite scientist,
            GameObject root, Text title, Text status, Button[] buttons, Text[] labels,
            GameplaySceneCoordinator sceneCoordinator = null, GameplayCombatController combatController = null,
            TurretDefinition[] dataDefinitions = null, LibraryAbilityController libraryAbilities = null)
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
            coordinator = sceneCoordinator;
            combat = combatController;
            turretDefinitions = dataDefinitions;
            abilityController = libraryAbilities;
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
                TileContextKind.ClearableObstacle => SelectObstacleAction(oneBasedIndex),
                _ => false
            };
            RefreshLabels();
            RefreshOptionPause();
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
            ReleaseOptionPause();
            var isLibrary = cell.x >= libraryCellMin.x && cell.x <= libraryCellMax.x &&
                cell.y >= libraryCellMin.y && cell.y <= libraryCellMax.y;
            var isGround = groundTilemap.HasTile(cell);
            var pathBlocked = pathTilemap != null && pathTilemap.HasTile(cell);
            var obstacleTile = blockedTilemap == null ? null : blockedTilemap.GetTile(cell);
            var blocked = pathBlocked || obstacleTile != null;
            var clearable = obstacleTile != null &&
                (obstacleTile.name.Contains("Tree") || obstacleTile.name.Contains("Boulder"));

            if (isLibrary) SetContext(TileContextKind.Library, null);
            else if (isGround && clearable) SetContext(TileContextKind.ClearableObstacle, null);
            else if (isGround && !blocked)
            {
                turrets.TryGetValue(cell, out var pad);
                SetContext(pad == null || pad.Kind == TurretKind.None
                    ? TileContextKind.BuildableGround : TileContextKind.OccupiedGround, pad);
            }
            else SetContext(TileContextKind.None, null);

            if (highlightRenderer != null)
            {
                highlightRenderer.gameObject.SetActive(CurrentContext is TileContextKind.BuildableGround or TileContextKind.OccupiedGround or TileContextKind.ClearableObstacle);
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
            if (menuDepth == MenuDepth.Abilities && index >= 1 && index <= 3 && abilityController != null)
            {
                var kind = (LibraryAbilityKind)(index - 1);
                var definition = abilityController.GetDefinition(kind);
                var activated = abilityController.TryActivate(kind);
                SetStatus(activated ? $"{FormatAbility(kind)} ACTIVE" : $"NEED {definition?.Cost ?? 0} BRAIN CELLS OR ABILITY BUSY");
                if (activated) menuDepth = MenuDepth.Root;
                return activated;
            }
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
            if (index == 2)
            {
                menuDepth = MenuDepth.Abilities;
                return true;
            }
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
                activePad.SetPreview(false);
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
            pad.Configure(lastCell, renderer, teacherSprite, engineerSprite, scientistSprite,
                coordinator, combat, turretDefinitions);
            pad.Build(kind);
            pad.SetPreview(true);
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
            ReleaseOptionPause();
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
                    if (menuDepth == MenuDepth.Abilities)
                    {
                        SetTitle("LIBRARY ABILITIES");
                        SetAbilityAction(1, LibraryAbilityKind.History, "HISTORY");
                        SetAbilityAction(2, LibraryAbilityKind.SocialMediaInfluence, "SOCIAL INFLUENCE");
                        SetAbilityAction(3, LibraryAbilityKind.Politics, "POLITICS");
                    }
                    else
                    {
                        SetTitle("LIBRARY");
                        var canRepair = economy != null && economy.Balance >= 5 && library != null && library.State != null && library.State.CurrentHealth < library.State.MaximumHealth;
                        SetAction(1, "1  REPAIR +5  •  5", canRepair);
                        SetAction(2, "2  ABILITIES");
                    }
                    break;
                case TileContextKind.BuildableGround:
                    if (menuDepth == MenuDepth.Root) { SetTitle("EMPTY GROUND"); SetAction(1, "1  TURRETS"); }
                    else if (menuDepth == MenuDepth.TurretChoice)
                    { SetTitle("CHOOSE TURRET"); SetAction(1, $"1  TEACHER • {CostFor(TurretKind.Teacher)}", CanAfford(TurretKind.Teacher)); SetAction(2, $"2  ENGINEER • {CostFor(TurretKind.Engineer)}", CanAfford(TurretKind.Engineer)); SetAction(3, $"3  SCIENTIST • {CostFor(TurretKind.Scientist)}", CanAfford(TurretKind.Scientist)); }
                    else { SetTitle($"{previewKind.ToString().ToUpperInvariant()} PREVIEW"); SetAction(1, "1  KEEP"); SetAction(2, "2  CANCEL"); }
                    break;
                case TileContextKind.OccupiedGround:
                    SetTitle($"{activePad.Kind.ToString().ToUpperInvariant()}  LV {activePad.Level}");
                    var upgradeCost = 5 + activePad.Level * 3;
                    SetAction(1, activePad.Level >= 3 ? "1  MAX LEVEL" : $"1  UPGRADE • {upgradeCost}", activePad.Level < 3 && economy != null && economy.Balance >= upgradeCost); SetAction(2, "2  SELL"); break;
                case TileContextKind.ClearableObstacle:
                    SetTitle("BLOCKED GROUND");
                    var clearCost = ObstacleClearCost();
                    SetAction(1, $"1  CLEAR • {clearCost}", economy != null && economy.Balance >= clearCost);
                    break;
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
        private int CostFor(TurretKind kind)
        {
            if (turretDefinitions != null)
                foreach (var definition in turretDefinitions)
                    if (definition != null && definition.Kind == kind) return definition.BuildCost;
            return kind switch { TurretKind.Teacher => 6, TurretKind.Engineer => 8, TurretKind.Scientist => 10, _ => int.MaxValue };
        }

        public void ResetState()
        {
            CancelPreview();
            if (blockedTilemap != null)
                foreach (var pair in clearedObstacles) blockedTilemap.SetTile(pair.Key, pair.Value);
            clearedObstacles.Clear();
            ReindexAuthoredTurrets();
            lastCell = new Vector3Int(int.MinValue, int.MinValue, int.MinValue);
            menuDepth = MenuDepth.Root;
            RefreshContext(true);
            ReleaseOptionPause();
        }

        private bool SelectObstacleAction(int index)
        {
            if (index != 1 || blockedTilemap == null) return false;
            var cost = ObstacleClearCost();
            if (economy == null || !economy.TrySpend(cost)) { SetStatus($"NEED {cost} BRAIN CELLS"); return false; }
            var tile = blockedTilemap.GetTile(lastCell);
            if (tile != null) clearedObstacles[lastCell] = tile;
            blockedTilemap.SetTile(lastCell, null);
            SetStatus("OBSTACLE CLEARED");
            SetContext(TileContextKind.BuildableGround, null);
            return true;
        }

        private int ObstacleClearCost()
        {
            var tile = blockedTilemap == null ? null : blockedTilemap.GetTile(lastCell);
            return tile != null && tile.name.Contains("Boulder") ? 6 : 4;
        }

        private bool CanAfford(TurretKind kind) => economy != null && economy.Balance >= CostFor(kind);

        private void SetAbilityAction(int index, LibraryAbilityKind kind, string label)
        {
            var definition = abilityController == null ? null : abilityController.GetDefinition(kind);
            var cost = definition == null ? 0 : definition.Cost;
            SetAction(index, $"{index}  {label} • {cost}", definition != null && economy != null && economy.Balance >= cost && abilityController.ActiveAbility == null);
        }

        private static string FormatAbility(LibraryAbilityKind kind) => kind switch
        {
            LibraryAbilityKind.SocialMediaInfluence => "SOCIAL INFLUENCE",
            _ => kind.ToString().ToUpperInvariant()
        };

        private void RefreshOptionPause()
        {
            var shouldPause = !ContinueDuringSelections && menuDepth != MenuDepth.Root;
            if (shouldPause == optionPauseActive) return;
            optionPauseActive = shouldPause;
            Time.timeScale = shouldPause ? 0f : 1f;
        }

        private void ReleaseOptionPause()
        {
            if (!optionPauseActive) return;
            optionPauseActive = false;
            Time.timeScale = 1f;
        }

        private void OnDisable() => ReleaseOptionPause();
    }
}
