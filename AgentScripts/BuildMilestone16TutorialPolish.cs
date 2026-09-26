using System;
using System.Linq;
using KeySlaught.Progression;
using KeySlaught.SceneGameplay;
using KeySlaught.UI;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Object = UnityEngine.Object;

public static class BuildMilestone16TutorialPolish
{
    private const string Sample = "Assets/Scenes/SampleScene.unity";
    private const string Lore1 = "Assets/Scenes/LoreOneLevelOne.unity";
    private const string Lore2 = "Assets/Scenes/LoreOneLevelTwo.unity";
    private const string Lore3 = "Assets/Scenes/LoreOneLevelThree.unity";
    private static readonly Color Ink = C("10152B");
    private static readonly Color Gold = C("F4C967");
    private static readonly Color Cream = C("FFF2CF");
    private static readonly Color Teal = C("38B7B0");

    public static string Build()
    {
        if (EditorApplication.isPlaying) throw new InvalidOperationException("Exit Play Mode before authoring tutorial polish.");
        foreach (var path in new[] { Sample, Lore1, Lore2, Lore3 })
        {
            var scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
            PolishAllText(); PolishHud(); PolishLibrary(); BuildLibraryHud(); BuildVisualControls();
            if (path == Sample) { BuildTutorialPresentation(); WireLoreSelection(); }
            if (path == Lore3) BuildDualRoute();
            EditorSceneManager.MarkSceneDirty(scene); EditorSceneManager.SaveScene(scene);
        }
        AssetDatabase.SaveAssets(); AssetDatabase.Refresh();
        EditorSceneManager.OpenScene(Sample, OpenSceneMode.Single);
        return Audit();
    }

    public static string Audit()
    {
        var errors = "";
        foreach (var path in new[] { Sample, Lore1, Lore2, Lore3 })
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(path) == null) errors += $"Missing scene {path}. ";
        var buildScenes = EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToArray();
        foreach (var path in new[] { Sample, Lore1, Lore2, Lore3 }) if (!buildScenes.Contains(path)) errors += $"Scene not enabled: {path}. ";

        EditorSceneManager.OpenScene(Sample, OpenSceneMode.Single);
        var guide = Object.FindFirstObjectByType<TutorialFocusGuide>(FindObjectsInactive.Include);
        if (guide == null || guide.GetComponent<Image>() != null) errors += "Spotlight is missing or still Image-based. ";
        var director = Object.FindFirstObjectByType<TutorialDirector>(FindObjectsInactive.Include);
        if (director == null) errors += "TutorialDirector missing. ";
        var shell = Object.FindFirstObjectByType<GameShellController>(FindObjectsInactive.Include);
        if (shell == null) errors += "Game shell missing. ";
        var levelButtons = Find("Game Shell Canvas/Game Shell/Lore Levels")?.GetComponentsInChildren<Button>(true) ?? Array.Empty<Button>();
        if (levelButtons.Count(b => b.name.Contains("LEVEL")) < 3) errors += "Three Lore level buttons are not authored. ";
        var controls = Find("Portrait Gameplay HUD/Pause Overlay/Controls Panel");
        if (controls == null || controls.transform.Cast<Transform>().Count(t => t.name.StartsWith("Input Hint")) != 4) errors += "Four visual control hints missing. ";
        if (!LibraryLooksReadable()) errors += "Library health treatment missing. ";
        if (Find("Portrait Gameplay HUD/Top 20 Percent/Library Health Card") == null) errors += "Library HUD card missing. ";

        EditorSceneManager.OpenScene(Lore3, OpenSceneMode.Single);
        var spawner = Object.FindFirstObjectByType<EnemySpawner>(FindObjectsInactive.Include);
        if (spawner == null || spawner.RouteCount != 2) errors += "Lore I Level 3 does not have two routes. ";
        var paths = Object.FindObjectsByType<WaypointPath>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        if (paths.Length < 2 || Vector2.Distance(paths[0].StartPosition, paths[1].StartPosition) < 1f ||
            Vector2.Distance(paths[0].EndPosition, paths[1].EndPosition) > .05f) errors += "Level 3 route starts/end are invalid. ";
        EditorSceneManager.OpenScene(Sample, OpenSceneMode.Single);
        if (errors.Length > 0) throw new InvalidOperationException(errors);
        return "Milestone 16 audit passed: circular black spotlight, objective feedback, readable HUD/library/menu text, visual animated controls, Lore level picker, and two converging Level 3 routes.";
    }

    public static string ReviewTutorial()
    {
        Find("Game Shell Canvas/Game Shell")?.SetActive(false);
        Object.FindFirstObjectByType<TutorialDirector>(FindObjectsInactive.Include)?.Begin();
        return "Tutorial movement checklist and circular spotlight staged.";
    }

    public static string ReviewControls()
    {
        Find("Game Shell Canvas/Game Shell")?.SetActive(false);
        var pause = Object.FindFirstObjectByType<PauseMenuController>(FindObjectsInactive.Include);
        if (pause != null && !pause.IsPaused) pause.TogglePause();
        pause?.ToggleControls();
        return "Animated visual control guide staged.";
    }

    public static string ReviewLoreMenu()
    {
        var shellRoot = Find("Game Shell Canvas/Game Shell"); shellRoot?.SetActive(true);
        Object.FindFirstObjectByType<GameShellController>(FindObjectsInactive.Include)?.ShowLoreLevels();
        return "Lore I level-selection menu staged.";
    }

    public static string ReviewLevel3Routes()
    {
        if (SceneManager.GetActiveScene().path != Lore3) EditorSceneManager.OpenScene(Lore3, OpenSceneMode.Single);
        return "Lore I Level 3 dual converging routes staged.";
    }

    public static string InspectRoutes()
    {
        var paths = Object.FindObjectsByType<WaypointPath>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        return string.Join(" | ", paths.Select(path => $"{path.name}: {path.StartPosition} -> {path.EndPosition}"));
    }

    public static string InspectTutorialState()
    {
        var guide = Object.FindFirstObjectByType<TutorialFocusGuide>(FindObjectsInactive.Include);
        var objective = Find("Portrait Gameplay HUD/Tutorial Objective Banner");
        var hud = Find("Portrait Gameplay HUD");
        var target = guide?.Target;
        return $"HUD={hud?.activeInHierarchy}; Objective={objective?.activeSelf}/{objective?.activeInHierarchy}; Guide={guide?.gameObject.activeSelf}/{guide?.gameObject.activeInHierarchy}; Target={target?.name} size={target?.rect.size}; Canvas={hud?.GetComponent<Canvas>()?.renderMode}";
    }

    public static string CaptureTutorialOverlay()
    {
        ScreenCapture.CaptureScreenshot("Temp/tutorial_overlay_runtime.png", 1);
        return "Requested an overlay-inclusive runtime capture.";
    }

    public static string CaptureControlsOverlay()
    {
        ScreenCapture.CaptureScreenshot("Temp/controls_overlay_runtime.png", 1);
        return "Requested controls overlay capture.";
    }

    public static string ReviewHud()
    {
        Find("Game Shell Canvas/Game Shell")?.SetActive(false);
        var pause = Object.FindFirstObjectByType<PauseMenuController>(FindObjectsInactive.Include); if (pause != null && pause.IsPaused) pause.Continue();
        Find("Portrait Gameplay HUD/Tutorial Objective Banner")?.SetActive(false);
        Find("Portrait Gameplay HUD/Tutorial Guidance")?.SetActive(false);
        var guide = Object.FindFirstObjectByType<TutorialFocusGuide>(FindObjectsInactive.Include); if (guide != null) guide.Hide();
        TutorialInputGate.Clear();
        return "Readable gameplay HUD staged.";
    }

    public static string CaptureHud()
    {
        ScreenCapture.CaptureScreenshot("Temp/hud_runtime.png", 1); return "Requested HUD capture.";
    }

    public static string CaptureLoreMenu()
    {
        ScreenCapture.CaptureScreenshot("Temp/lore_menu_runtime.png", 1); return "Requested Lore menu capture.";
    }

    public static string ReviewMainMenu()
    {
        Find("Game Shell Canvas/Game Shell")?.SetActive(true);
        Object.FindFirstObjectByType<GameShellController>(FindObjectsInactive.Include)?.ShowMain();
        ScreenCapture.CaptureScreenshot("Temp/main_menu_runtime.png", 1);
        return "Requested main menu capture.";
    }

    public static string DriveUp() => Drive(Vector2.up);
    public static string DriveDown() => Drive(Vector2.down);
    public static string DriveLeft() => Drive(Vector2.left);
    public static string DriveRight() => Drive(Vector2.right);
    public static string StopDrive() => Drive(Vector2.zero);
    public static string ContinueTutorialLive() { Object.FindFirstObjectByType<TutorialDirector>(FindObjectsInactive.Include)?.ContinueTutorial(); return InspectTutorialFlow(); }
    public static string StartCatProbe()
    {
        var director = Object.FindFirstObjectByType<TutorialDirector>(FindObjectsInactive.Include);
        director?.Begin();
        var field = typeof(TutorialDirector).GetField("stage", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
        if (director != null && field != null) field.SetValue(director, Enum.Parse(field.FieldType, "RefreshMessage"));
        director?.ContinueTutorial();
        return InspectTutorialFlow();
    }
    public static string PressC() => PressTutorialLetter('C');
    public static string PressA() => PressTutorialLetter('A');
    public static string PressT() => PressTutorialLetter('T');
    public static string PressRefresh()
    {
        var combat = Object.FindFirstObjectByType<GameplayCombatController>(FindObjectsInactive.Include);
        var accepted = TutorialInputGate.TryAllowRefresh() && combat != null && combat.TryStartRefresh();
        return $"Refresh={accepted}; {InspectTutorialFlow()}";
    }
    public static string MovePlayerToTutorialEnemy()
    {
        var player = Object.FindFirstObjectByType<PlayerMover>(FindObjectsInactive.Include);
        var enemy = Object.FindObjectsByType<EnemyAgent>(FindObjectsInactive.Include, FindObjectsSortMode.None).FirstOrDefault(e => e.WordState?.OriginalWord == "CAT");
        if (player != null && enemy != null) player.transform.position = enemy.transform.position + Vector3.left * 2f;
        return InspectTutorialFlow();
    }
    public static string InspectTutorialFlow()
    {
        var director = Object.FindFirstObjectByType<TutorialDirector>(FindObjectsInactive.Include);
        var stage = director == null ? "missing" : typeof(TutorialDirector).GetField("stage", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)?.GetValue(director)?.ToString();
        var enemy = Object.FindObjectsByType<EnemyAgent>(FindObjectsInactive.Include, FindObjectsSortMode.None).FirstOrDefault(e => e.WordState?.OriginalWord == "CAT");
        var player = Object.FindFirstObjectByType<PlayerMover>(FindObjectsInactive.Include);
        var list = Find("Portrait Gameplay HUD/Tutorial Objective Banner")?.GetComponentsInChildren<Text>(true).FirstOrDefault(t => (t.text ?? "").Contains("MOVE UP"));
        return $"Stage={stage}; movement={TutorialInputGate.AllowMovement}; required={TutorialInputGate.RequiredLetter}; player={player?.transform.position}; todos={list?.text}; CAT={(enemy == null ? "none" : $"travel={enemy.TravelledDistance:F2} remaining={enemy.WordState.RemainingWord} pos={enemy.transform.position}")}";
    }

    private static string Drive(Vector2 direction)
    {
        var mover = Object.FindFirstObjectByType<PlayerMover>(FindObjectsInactive.Include);
        if (mover != null)
        {
            mover.SetVirtualMovement(direction);
            if (direction != Vector2.zero) mover.ApplyMovement(direction, .25f);
        }
        return direction.ToString();
    }
    private static string PressTutorialLetter(char letter)
    {
        var combat = Object.FindFirstObjectByType<GameplayCombatController>(FindObjectsInactive.Include);
        var accepted = TutorialInputGate.TryAllowLetter(letter) && combat != null;
        var outcome = accepted ? combat.TryTypeLetter(letter).Outcome.ToString() : "Rejected";
        return $"{letter}={accepted}/{outcome}; {InspectTutorialFlow()}";
    }

    private static void BuildTutorialPresentation()
    {
        var hud = Find("Portrait Gameplay HUD")?.transform ?? throw new InvalidOperationException("Gameplay HUD missing.");
        var director = Object.FindFirstObjectByType<TutorialDirector>(FindObjectsInactive.Include) ?? throw new InvalidOperationException("TutorialDirector missing.");
        var oldGuide = hud.Find("Tutorial Focus Guide"); if (oldGuide != null) Object.DestroyImmediate(oldGuide.gameObject);
        var guideObject = new GameObject("Tutorial Focus Guide", typeof(RectTransform), typeof(CanvasRenderer), typeof(TutorialFocusGuide));
        guideObject.transform.SetParent(hud, false); var guide = guideObject.GetComponent<TutorialFocusGuide>(); guide.color = new Color(0f, 0f, 0f, .82f);
        Full(guide.rectTransform); guide.raycastTarget = false; guideObject.SetActive(false);

        var oldObjective = hud.Find("Tutorial Objective Banner"); if (oldObjective != null) Object.DestroyImmediate(oldObjective.gameObject);
        var objective = Panel("Tutorial Objective Banner", hud, new Color(.025f, .035f, .07f, .96f));
        Rect(objective.rectTransform, new Vector2(.5f, 1), new Vector2(.5f, 1), new Vector2(0, -34), new Vector2(760, 330));
        var title = Label("LEARN TO MOVE", objective.transform, 35, Gold); title.fontStyle = FontStyle.Bold; Rect(title.rectTransform, new Vector2(.5f, 1), new Vector2(.5f, 1), new Vector2(0, -28), new Vector2(680, 58));
        var checklist = Label("[ ] MOVE UP\n[ ] MOVE DOWN\n[ ] MOVE LEFT\n[ ] MOVE RIGHT", objective.transform, 29, Cream); checklist.alignment = TextAnchor.UpperLeft;
        Rect(checklist.rectTransform, new Vector2(.5f, 1), new Vector2(.5f, 1), new Vector2(0, -94), new Vector2(650, 150));
        var feedback = Label("Use arrows, a stick, or drag the controller.", objective.transform, 25, Teal);
        Rect(feedback.rectTransform, new Vector2(.5f, 0), new Vector2(.5f, 0), new Vector2(0, 24), new Vector2(680, 58));
        objective.gameObject.AddComponent<UiPanelEntranceAnimator>().Configure(.25f, 18f);

        var arena = Find("Portrait Gameplay HUD/Tutorial Drag Controller")?.GetComponent<RectTransform>() ?? hud.Find("Middle 60 Percent") as RectTransform;
        var input = hud.Find("Bottom 20 Percent") as RectTransform;
        director.ConfigureFocusGuide(guide, arena, input); director.ConfigureTutorialUi(objective.gameObject, title, checklist, feedback);
        guide.transform.SetAsLastSibling(); objective.transform.SetAsLastSibling();
        var message = hud.Find("Tutorial Guidance"); if (message != null) message.SetAsLastSibling();
        var pauseOverlay = hud.Find("Pause Overlay"); if (pauseOverlay != null) pauseOverlay.SetAsLastSibling();
        objective.gameObject.SetActive(false);
    }

    private static void WireLoreSelection()
    {
        var shell = Object.FindFirstObjectByType<GameShellController>(FindObjectsInactive.Include) ?? throw new InvalidOperationException("GameShellController missing.");
        var root = Find("Game Shell Canvas/Game Shell")?.transform ?? throw new InvalidOperationException("Game Shell missing.");
        var mode = root.Find("Mode Select");
        var loreMode = mode?.GetComponentsInChildren<Button>(true).FirstOrDefault(b => (b.GetComponentInChildren<Text>(true)?.text ?? "").Contains("LORE I"));
        if (loreMode != null) { Clear(loreMode.onClick); UnityEventTools.AddPersistentListener(loreMode.onClick, shell.ShowLoreLevels); }
        var panel = root.Find("Lore Levels") ?? throw new InvalidOperationException("Lore Levels panel missing.");
        var buttons = panel.GetComponentsInChildren<Button>(true).Where(b => b.name.Contains("LEVEL")).OrderBy(b => b.name).ToArray();
        if (buttons.Length < 3) throw new InvalidOperationException("Lore Levels needs three buttons.");
        for (var i = 0; i < 3; i++) { buttons[i].interactable = true; Clear(buttons[i].onClick); }
        UnityEventTools.AddPersistentListener(buttons[0].onClick, shell.LoadLoreOneLevelOne);
        UnityEventTools.AddPersistentListener(buttons[1].onClick, shell.StartLoreOneLevelTwo);
        UnityEventTools.AddPersistentListener(buttons[2].onClick, shell.StartLoreOneLevelThree);
        shell.ConfigureLoreSelection(panel.gameObject, buttons[0].GetComponentInChildren<Text>(true));
        shell.ConfigureLoreSequence(buttons[1], buttons[1].GetComponentInChildren<Text>(true), buttons[2], buttons[2].GetComponentInChildren<Text>(true));
    }

    private static void PolishHud()
    {
        var stats = Find("Portrait Gameplay HUD/Top 20 Percent/Aligned Stats Bar")?.transform;
        if (stats != null)
        {
            var brainCard = stats.Find("Brain Cell Stat Card")?.GetComponent<RectTransform>();
            var timeCard = stats.Find("Time Stat Card")?.GetComponent<RectTransform>();
            if (brainCard != null) Rect(brainCard, new Vector2(.5f, 1), new Vector2(.5f, 1), new Vector2(-105, -6), new Vector2(260, 84));
            if (timeCard != null) Rect(timeCard, new Vector2(.5f, 1), new Vector2(.5f, 1), new Vector2(130, -6), new Vector2(200, 84));
            var brainIcon = stats.Find("Brain Cell Icon")?.GetComponent<Image>(); if (brainIcon != null) Rect(brainIcon.rectTransform, new Vector2(.5f, 1), new Vector2(.5f, 1), new Vector2(-205, -18), new Vector2(54, 54));
            var clock = stats.Find("Elegant Time Icon")?.GetComponent<Image>(); if (clock != null) Rect(clock.rectTransform, new Vector2(.5f, 1), new Vector2(.5f, 1), new Vector2(55, -18), new Vector2(54, 54));
            foreach (var text in stats.GetComponentsInChildren<Text>(true))
                if ((text.text ?? "").Contains(":") || text.name.Contains("BRAIN") || text.text == "0") { text.fontSize = 36; text.fontStyle = FontStyle.Bold; text.resizeTextForBestFit = false; AddShadow(text); }
        }
        foreach (var image in Object.FindObjectsByType<Image>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            if (image.name.Contains("Brain") && image.rectTransform.sizeDelta.x < 48) image.rectTransform.sizeDelta = new Vector2(50, 50);
    }

    private static void PolishLibrary()
    {
        var library = Object.FindFirstObjectByType<LibraryEndpoint>(FindObjectsInactive.Include); if (library == null) return;
        var label = library.GetComponentInChildren<TextMesh>(true); if (label == null) return;
        label.fontSize = 90; label.fontStyle = FontStyle.Bold; label.characterSize = .085f; label.anchor = TextAnchor.MiddleCenter; label.alignment = TextAlignment.Center;
        label.transform.localScale = Vector3.one; label.transform.localPosition = new Vector3(0f, 1.55f, -.2f);
        var renderer = label.GetComponent<MeshRenderer>(); if (renderer != null) renderer.sortingOrder = 31;
        var old = library.transform.Find("Health Backplate"); if (old != null) Object.DestroyImmediate(old.gameObject);
        var plate = new GameObject("Health Backplate", typeof(SpriteRenderer)); plate.transform.SetParent(library.transform, false);
        plate.transform.localPosition = new Vector3(0f, 1.55f, 0f); plate.transform.localScale = new Vector3(3.8f, .8f, 1f);
        var spriteRenderer = plate.GetComponent<SpriteRenderer>(); spriteRenderer.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Prototype/SolidSprite.asset"); spriteRenderer.color = new Color(0f, 0f, 0f, .86f); spriteRenderer.sortingOrder = 30;
    }

    private static bool LibraryLooksReadable()
    {
        var library = Object.FindFirstObjectByType<LibraryEndpoint>(FindObjectsInactive.Include);
        var label = library == null ? null : library.GetComponentInChildren<TextMesh>(true);
        return label != null && label.fontSize >= 80 && library.transform.Find("Health Backplate") != null;
    }

    private static void BuildLibraryHud()
    {
        var library = Object.FindFirstObjectByType<LibraryEndpoint>(FindObjectsInactive.Include); if (library == null) return;
        var top = Find("Portrait Gameplay HUD/Top 20 Percent")?.transform; if (top == null) return;
        var old = top.Find("Library Health Card"); if (old != null) Object.DestroyImmediate(old.gameObject);
        var card = Panel("Library Health Card", top, new Color(0f, 0f, 0f, .84f));
        Rect(card.rectTransform, new Vector2(1, 1), new Vector2(1, 1), new Vector2(-20, -82), new Vector2(290, 66)); card.raycastTarget = false;
        var label = Label("LIBRARY HP  30/30", card.transform, 28, Cream); label.fontStyle = FontStyle.Bold; Full(label.rectTransform);
        library.ConfigureUi(label); EditorUtility.SetDirty(library);
    }

    private static void BuildVisualControls()
    {
        var pause = Object.FindFirstObjectByType<PauseMenuController>(FindObjectsInactive.Include); if (pause == null) return;
        var panel = Find("Portrait Gameplay HUD/Pause Overlay/Controls Panel")?.GetComponent<RectTransform>(); if (panel == null) return;
        for (var i = panel.childCount - 1; i >= 0; i--) Object.DestroyImmediate(panel.GetChild(i).gameObject);
        panel.sizeDelta = new Vector2(760, 1050);
        var title = Label("CONTROLS", panel, 42, Gold); title.fontStyle = FontStyle.Bold; Rect(title.rectTransform, new Vector2(.5f, 1), new Vector2(.5f, 1), new Vector2(0, -48), new Vector2(650, 80));
        BuildInputHint(panel, 0, "KEYBOARD", "A  B  C\nX  Y  Z", "TYPE WORDS", Gold);
        BuildInputHint(panel, 1, "ARROW KEYS", "  ↑\n← ↓ →", "MOVE", Teal);
        BuildInputHint(panel, 2, "CONTROLLER", "◯  ●  ●\n   ◉", "STICK + BUTTONS", Gold);
        BuildInputHint(panel, 3, "ON-SCREEN KEYS", "Q W E R\nA S D F", "TAP OR DRAG", Teal);
        var close = TextButton("CLOSE", panel, 30); Rect(close.GetComponent<RectTransform>(), new Vector2(.5f, 0), new Vector2(.5f, 0), new Vector2(0, 38), new Vector2(560, 82));
        UnityEventTools.AddPersistentListener(close.onClick, pause.HideControls);
    }

    private static void BuildInputHint(Transform parent, int index, string heading, string glyph, string caption, Color accent)
    {
        var x = index % 2 == 0 ? -185f : 185f; var y = index < 2 ? -190f : -555f;
        var card = Panel($"Input Hint {index + 1} {heading}", parent, new Color(.02f, .03f, .07f, .94f));
        Rect(card.rectTransform, new Vector2(.5f, 1), new Vector2(.5f, 1), new Vector2(x, y), new Vector2(340, 320));
        var label = Label(heading, card.transform, 28, Cream); label.fontStyle = FontStyle.Bold; Rect(label.rectTransform, new Vector2(.5f, 1), new Vector2(.5f, 1), new Vector2(0, -28), new Vector2(300, 52));
        var iconPlate = Panel("Animated Icon", card.transform, new Color(accent.r, accent.g, accent.b, .18f)); Rect(iconPlate.rectTransform, new Vector2(.5f, .5f), new Vector2(.5f, .5f), new Vector2(0, -5), new Vector2(250, 150));
        var icon = Label(glyph, iconPlate.transform, 40, accent); icon.fontStyle = FontStyle.Bold; Full(icon.rectTransform);
        iconPlate.gameObject.AddComponent<UiBreathingAnimator>().Configure(.055f, .65f, index * .25f);
        var bottom = Label(caption, card.transform, 24, accent); Rect(bottom.rectTransform, new Vector2(.5f, 0), new Vector2(.5f, 0), new Vector2(0, 20), new Vector2(300, 48));
    }

    private static void BuildDualRoute()
    {
        var spawner = Object.FindFirstObjectByType<EnemySpawner>(FindObjectsInactive.Include) ?? throw new InvalidOperationException("Level 3 spawner missing.");
        var paths = Object.FindObjectsByType<WaypointPath>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        var primary = paths.FirstOrDefault(p => p.name != "Secondary Enemy Path") ?? throw new InvalidOperationException("Primary path missing.");
        var old = primary.transform.parent?.Find("Secondary Enemy Path"); if (old != null) Object.DestroyImmediate(old.gameObject);
        var positions = primary.CopyPositions(); var endX = positions[positions.Length - 1].x;
        var copy = new GameObject("Secondary Enemy Path", typeof(WaypointPath), typeof(LineRenderer)); copy.transform.SetParent(primary.transform.parent, false);
        var secondary = copy.GetComponent<WaypointPath>(); var points = new Transform[positions.Length];
        for (var i = 0; i < points.Length; i++)
        {
            var point = new GameObject($"Secondary Waypoint {i + 1:00}"); point.transform.SetParent(copy.transform, false);
            var original = positions[i]; point.transform.position = i == points.Length - 1 ? original : new Vector3(2f * endX - original.x, original.y, original.z);
            points[i] = point.transform;
        }
        if (Vector2.Distance(primary.StartPosition, points[0].position) < 1f) points[0].position += Vector3.left * 6f;
        var line = copy.GetComponent<LineRenderer>(); var sourceLine = primary.GetComponent<LineRenderer>();
        line.useWorldSpace = true; line.widthMultiplier = .72f; line.startColor = new Color(.35f, .9f, .85f, .58f); line.endColor = new Color(.95f, .78f, .35f, .58f);
        if (sourceLine != null) { line.sharedMaterial = sourceLine.sharedMaterial; line.sortingLayerID = sourceLine.sortingLayerID; line.sortingOrder = sourceLine.sortingOrder; }
        line.enabled = true; secondary.Configure(points, line); secondary.RefreshPreview();
        spawner.ConfigureRoutes(new[] { primary, secondary }); EditorUtility.SetDirty(spawner); EditorUtility.SetDirty(secondary);
    }

    private static void PolishAllText()
    {
        foreach (var text in Object.FindObjectsByType<Text>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            var isButton = text.GetComponentInParent<Button>() != null;
            var minimum = isButton ? 28 : 24;
            if (text.fontSize < minimum) text.fontSize = minimum;
            text.fontStyle = FontStyle.Bold; AddShadow(text);
            if (text.resizeTextForBestFit) { text.resizeTextMinSize = Mathf.Max(20, text.resizeTextMinSize); text.resizeTextMaxSize = Mathf.Max(minimum, text.resizeTextMaxSize); }
        }
        var main = Find("Game Shell Canvas/Game Shell/Main Menu")?.transform;
        var card = main?.Find("Menu Currency Header/Brain Cell Card");
        if (card != null)
        {
            var icon = card.Find("Brain Icon")?.GetComponent<Image>(); if (icon != null) Rect(icon.rectTransform, new Vector2(0, .5f), new Vector2(0, .5f), new Vector2(28, 0), new Vector2(58, 58));
            var amount = card.GetComponentsInChildren<Text>(true).FirstOrDefault(); if (amount != null) { amount.text = "0"; amount.fontSize = 36; Rect(amount.rectTransform, new Vector2(0, .5f), new Vector2(0, .5f), new Vector2(72, 0), new Vector2(160, 66)); }
        }
    }

    private static void AddShadow(Text text)
    {
        var shadow = text.GetComponent<Shadow>() ?? text.gameObject.AddComponent<Shadow>();
        shadow.effectColor = new Color(0f, 0f, 0f, .88f); shadow.effectDistance = new Vector2(2f, -2f); shadow.useGraphicAlpha = true;
    }

    private static Button TextButton(string text, Transform parent, int size)
    {
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/UiTextButton.prefab");
        var go = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent); go.name = "Button " + text;
        var label = go.GetComponentInChildren<Text>(true); label.text = text; label.fontSize = size; label.color = Cream; AddShadow(label);
        return go.GetComponent<Button>();
    }

    private static Text Label(string text, Transform parent, int size, Color color)
    {
        var go = new GameObject("Label " + text.Split('\n')[0], typeof(RectTransform), typeof(CanvasRenderer), typeof(Text)); go.transform.SetParent(parent, false);
        var label = go.GetComponent<Text>(); label.text = text; label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); label.fontSize = size; label.color = color; label.alignment = TextAnchor.MiddleCenter; label.horizontalOverflow = HorizontalWrapMode.Wrap; label.verticalOverflow = VerticalWrapMode.Overflow; AddShadow(label); return label;
    }
    private static Image Panel(string name, Transform parent, Color color)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image)); go.transform.SetParent(parent, false);
        var image = go.GetComponent<Image>(); image.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Prototype/SolidSprite.asset"); image.type = Image.Type.Sliced; image.color = color; return image;
    }
    private static void Clear(UnityEngine.Events.UnityEvent action) { for (var i = action.GetPersistentEventCount() - 1; i >= 0; i--) UnityEventTools.RemovePersistentListener(action, i); }
    private static GameObject Find(string path) => GameObject.Find("/" + path) ?? Resources.FindObjectsOfTypeAll<GameObject>().FirstOrDefault(go => go.scene == SceneManager.GetActiveScene() && Hierarchy(go) == path);
    private static string Hierarchy(GameObject go) { var value = go.name; for (var parent = go.transform.parent; parent != null; parent = parent.parent) value = parent.name + "/" + value; return value; }
    private static void Full(RectTransform rect) { rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one; rect.offsetMin = Vector2.zero; rect.offsetMax = Vector2.zero; }
    private static void Rect(RectTransform rect, Vector2 min, Vector2 max, Vector2 position, Vector2 size) { rect.anchorMin = min; rect.anchorMax = max; rect.pivot = min == max ? min : new Vector2(.5f, .5f); rect.anchoredPosition = position; rect.sizeDelta = size; }
    private static Color C(string hex) { ColorUtility.TryParseHtmlString("#" + hex, out var color); return color; }
}
