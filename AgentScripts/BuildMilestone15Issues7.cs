using System;
using System.IO;
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

public static class BuildMilestone15Issues7
{
    private const string Sample = "Assets/Scenes/SampleScene.unity";
    private const string Lore1 = "Assets/Scenes/LoreOneLevelOne.unity";
    private const string Lore2 = "Assets/Scenes/LoreOneLevelTwo.unity";
    private const string Lore3 = "Assets/Scenes/LoreOneLevelThree.unity";
    private const string Splash = "Assets/Art/Splash/KeySlaught_Splash_Cartoon.png";
    private const string Door = "Assets/Art/Colorful/UI_Door_128.png";
    private static readonly Color Ink = C("10152B");
    private static readonly Color Plum = C("482143");
    private static readonly Color Gold = C("F4C967");
    private static readonly Color Cream = C("FFF2CF");
    private static readonly Color Teal = C("38B7B0");

    public static string Build()
    {
        if (EditorApplication.isPlaying) throw new InvalidOperationException("Exit Play Mode before authoring Milestone 15.");
        GenerateDoorIcon();
        AssetDatabase.Refresh();
        ConfigureSprite(Door);
        TuneDefinitions();
        foreach (var path in new[] { Sample, Lore1, Lore2, Lore3 })
        {
            var scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
            ConfigureGameplay(path == Sample);
            if (path == Sample) ConfigureShell();
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        EditorSceneManager.OpenScene(Sample, OpenSceneMode.Single);
        return "Built issues7 milestone: delayed launch actions, shared art menus, readable UI, persistent currencies, sequential turret unlocks, result/reward flow, forced tutorial focus, 1x/2x/3x speed, skip-wait control, and balance polish.";
    }

    public static string Audit()
    {
        foreach (var path in new[] { Sample, Lore1, Lore2, Lore3 })
        {
            EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
            var pause = Object.FindFirstObjectByType<PauseMenuController>(FindObjectsInactive.Include) ?? throw new InvalidOperationException($"Pause controller missing in {path}.");
            var pauseWindow = Find("Portrait Gameplay HUD/Pause Overlay/Pause Window") as GameObject;
            if (pauseWindow == null || pauseWindow.GetComponent<RectTransform>().sizeDelta.y > 1200f) throw new InvalidOperationException($"Pause window bounds invalid in {path}.");
            if (Object.FindFirstObjectByType<RunResultPresenter>(FindObjectsInactive.Include) == null) throw new InvalidOperationException($"Result presenter missing in {path}.");
            var skip = Find("Portrait Gameplay HUD/Top 20 Percent/Aligned Stats Bar/Button SKIP WAIT");
            if (skip == null || skip.GetComponent<IntermissionFastForwardButton>() == null) throw new InvalidOperationException($"Skip-wait action missing in {path}.");
            var currency = Object.FindFirstObjectByType<BrainCellEconomy>(FindObjectsInactive.Include);
            var currencyData = new SerializedObject(currency);
            if (currencyData.FindProperty("currencyLabel").objectReferenceValue == null) throw new InvalidOperationException($"Brain-cell number missing in {path}.");
            if (Find("Portrait Gameplay HUD/Top 20 Percent/Aligned Stats Bar/Brain Cell Stat Card") == null) throw new InvalidOperationException($"Brain-cell stat card missing in {path}.");
            if (Find("Portrait Gameplay HUD/Top 20 Percent/Aligned Stats Bar/Time Stat Card") == null) throw new InvalidOperationException($"Time stat card missing in {path}.");
            var context = Object.FindFirstObjectByType<TileContextActionPanel>(FindObjectsInactive.Include);
            var contextData = new SerializedObject(context);
            if (contextData.FindProperty("progression").objectReferenceValue == null) throw new InvalidOperationException($"Turret progression gate missing in {path}.");
        }

        EditorSceneManager.OpenScene(Sample, OpenSceneMode.Single);
        var shell = Object.FindFirstObjectByType<GameShellController>(FindObjectsInactive.Include) ?? throw new InvalidOperationException("Shell missing.");
        var shellData = new SerializedObject(shell);
        if (shellData.FindProperty("launchActionsCanvasGroup").objectReferenceValue == null) throw new InvalidOperationException("Launch fade group missing.");
        if (shellData.FindProperty("exitConfirmationPanel").objectReferenceValue == null) throw new InvalidOperationException("Exit confirmation missing.");
        if (shellData.FindProperty("brainCellsLabel").objectReferenceValue == null) throw new InvalidOperationException("Menu brain-cell balance missing.");
        foreach (var panel in new[] { "Launch Screen", "Main Menu", "Mode Select", "Research", "Credits", "Settings", "Lore Levels" })
        {
            if (Find($"Game Shell Canvas/Game Shell/{panel}/Artwork Background") == null) throw new InvalidOperationException($"Shared artwork missing on {panel}.");
            var shade = Find($"Game Shell Canvas/Game Shell/{panel}/Artwork Shade")?.GetComponent<Image>();
            if (shade == null) throw new InvalidOperationException($"Artwork tint missing on {panel}.");
            if (panel == "Launch Screen" && shade.color.a > .01f) throw new InvalidOperationException("Launch artwork must remain untinted.");
            if (panel != "Launch Screen" && (shade.color.r > .01f || shade.color.g > .01f || shade.color.b > .01f || shade.color.a < .6f)) throw new InvalidOperationException($"Readable black artwork tint invalid on {panel}.");
        }
        if (Find("Game Shell Canvas/Game Shell/Main Menu/Menu Currency Header/Brain Cell Card") == null || Find("Game Shell Canvas/Game Shell/Main Menu/Menu Currency Header/Research Card") == null)
            throw new InvalidOperationException("Separate menu currency cards missing.");
        if (Object.FindFirstObjectByType<TutorialFocusGuide>(FindObjectsInactive.Include) == null) throw new InvalidOperationException("Tutorial focus guide missing.");
        return "Audit passed: all four scenes have bounded pause/result/skip/currency/unlock wiring; the shell has delayed launch actions, exit confirmation, shared artwork, currency header, speed controls, and forced tutorial focus.";
    }

    public static string ReviewLaunch()
    {
        var shell = Object.FindFirstObjectByType<GameShellController>(FindObjectsInactive.Include);
        Find("Game Shell Canvas/Game Shell")?.SetActive(true);
        shell?.ShowLaunch();
        var group = Find("Game Shell Canvas/Game Shell/Launch Screen/Launch Actions")?.GetComponent<CanvasGroup>();
        if (group != null) { group.alpha = 1f; group.interactable = true; group.blocksRaycasts = true; }
        return "Launch review staged with revealed Continue and door actions.";
    }

    public static string ReviewMain()
    {
        Find("Game Shell Canvas/Game Shell")?.SetActive(true);
        Object.FindFirstObjectByType<GameShellController>(FindObjectsInactive.Include)?.ShowMain();
        return "Shared-art main menu staged.";
    }

    public static string ReviewPause()
    {
        Find("Game Shell Canvas/Game Shell")?.SetActive(false);
        var pause = Object.FindFirstObjectByType<PauseMenuController>(FindObjectsInactive.Include);
        if (pause != null && !pause.IsPaused) pause.TogglePause();
        return "Bounded pause menu staged.";
    }

    public static string ReviewResult()
    {
        Find("Game Shell Canvas/Game Shell")?.SetActive(false);
        var pause = Object.FindFirstObjectByType<PauseMenuController>(FindObjectsInactive.Include); if (pause != null && pause.IsPaused) pause.Continue();
        var tutorial = Find("Portrait Gameplay HUD/Tutorial Guidance"); if (tutorial != null) tutorial.SetActive(false);
        var result = Object.FindFirstObjectByType<RunResultPresenter>(FindObjectsInactive.Include);
        result?.gameObject.SetActive(true);
        result?.Present(true, 2, "Engineer", true);
        return "Victory rewards and tower unlock staged.";
    }

    public static string ReviewTutorial()
    {
        Find("Game Shell Canvas/Game Shell")?.SetActive(false);
        var pause = Object.FindFirstObjectByType<PauseMenuController>(FindObjectsInactive.Include); if (pause != null && pause.IsPaused) pause.Continue();
        var result = Find("Portrait Gameplay HUD/Run Result Overlay"); if (result != null) result.SetActive(false);
        Object.FindFirstObjectByType<TutorialDirector>(FindObjectsInactive.Include)?.Begin();
        return "Forced movement tutorial focus staged.";
    }

    public static string ReviewDestroyedFocusShutdown()
    {
        var guide = Object.FindFirstObjectByType<TutorialFocusGuide>(FindObjectsInactive.Include);
        if (guide != null) Object.DestroyImmediate(guide.gameObject);
        var director = Object.FindFirstObjectByType<TutorialDirector>(FindObjectsInactive.Include);
        if (director != null) director.enabled = false;
        return "Destroyed the focus guide before disabling TutorialDirector to reproduce the reported lifecycle order.";
    }

    public static string ReviewTurretChoices()
    {
        Find("Game Shell Canvas/Game Shell")?.SetActive(false);
        var pause = Object.FindFirstObjectByType<PauseMenuController>(FindObjectsInactive.Include); if (pause != null && pause.IsPaused) pause.Continue();
        var result = Find("Portrait Gameplay HUD/Run Result Overlay"); if (result != null) result.SetActive(false);
        var tutorial = Find("Portrait Gameplay HUD/Tutorial Guidance"); if (tutorial != null) tutorial.SetActive(false);
        TutorialInputGate.Clear();
        var guide = Object.FindFirstObjectByType<TutorialFocusGuide>(FindObjectsInactive.Include); if (guide != null) guide.Hide();
        Object.FindFirstObjectByType<TileContextActionPanel>(FindObjectsInactive.Include)?.SelectAction(1);
        return "Readable turret family choices staged.";
    }

    private static void ConfigureGameplay(bool sampleScene)
    {
        var progression = Object.FindFirstObjectByType<ProgressionService>(FindObjectsInactive.Include);
        var economy = Object.FindFirstObjectByType<BrainCellEconomy>(FindObjectsInactive.Include);
        var context = Object.FindFirstObjectByType<TileContextActionPanel>(FindObjectsInactive.Include);
        if (economy != null && progression != null)
        {
            var data = new SerializedObject(economy); data.FindProperty("progression").objectReferenceValue = progression; data.ApplyModifiedPropertiesWithoutUndo(); EditorUtility.SetDirty(economy);
        }
        context?.ConfigureProgression(progression);
        ConfigurePlayer();
        ConfigureHud();
        ConfigureContextButtons();
        ConfigurePause();
        ConfigureSkipButton();
        ConfigureTutorialFocus();
        ConfigureResult(sampleScene);
    }

    private static void ConfigureShell()
    {
        var shell = Object.FindFirstObjectByType<GameShellController>(FindObjectsInactive.Include) ?? throw new InvalidOperationException("Shell missing.");
        var root = Find("Game Shell Canvas/Game Shell")?.transform ?? throw new InvalidOperationException("Shell root missing.");
        foreach (var name in new[] { "Launch Screen", "Main Menu", "Mode Select", "Research", "Credits", "Settings", "Lore Levels" })
        {
            var panel = root.Find(name);
            if (panel == null) continue;
            AddArtworkBackground(panel, name == "Launch Screen");
            var group = panel.GetComponent<CanvasGroup>() ?? panel.gameObject.AddComponent<CanvasGroup>();
            var entrance = panel.GetComponent<UiPanelEntranceAnimator>() ?? panel.gameObject.AddComponent<UiPanelEntranceAnimator>();
            entrance.Configure(.32f, 18f);
            foreach (var text in panel.GetComponentsInChildren<Text>(true))
                if (text.fontSize < 22) text.fontSize = 22;
            foreach (var button in panel.GetComponentsInChildren<Button>(true))
            {
                var label = button.GetComponentInChildren<Text>(true);
                if (label != null && label.fontSize < 24) label.fontSize = 24;
            }
        }
        ConfigureLaunch(shell, root.Find("Launch Screen"));
        ConfigureMenuCurrency(shell, root.Find("Main Menu"));
        ConfigureShellSettings(shell, root.Find("Settings"));
        var mode = root.Find("Mode Select");
        var lore = mode == null ? null : mode.GetComponentsInChildren<Button>(true).FirstOrDefault(b => b.GetComponentInChildren<Text>(true)?.text.Contains("LORE I") == true);
        if (lore != null) { Clear(lore.onClick); UnityEventTools.AddPersistentListener(lore.onClick, shell.ShowLoreLevels); }
    }

    private static void ConfigureLaunch(GameShellController shell, Transform launch)
    {
        if (launch == null) return;
        foreach (var text in launch.GetComponentsInChildren<Text>(true))
            if ((text.text ?? string.Empty).ToUpperInvariant().Contains("DEFEND THE DIVINE LIBRARY")) Object.DestroyImmediate(text.gameObject);
        var actions = launch.Find("Launch Actions") as RectTransform;
        if (actions == null) throw new InvalidOperationException("Launch Actions missing.");
        for (var index = actions.childCount - 1; index >= 0; index--) Object.DestroyImmediate(actions.GetChild(index).gameObject);
        Full(actions);
        var group = actions.GetComponent<CanvasGroup>();
        if (group == null) group = actions.gameObject.AddComponent<CanvasGroup>();
        if (group == null) throw new InvalidOperationException("Could not add launch CanvasGroup.");
        EditorUtility.SetDirty(group);
        var continueButton = TextButton("CONTINUE", actions, 34);
        Rect(continueButton.GetComponent<RectTransform>(), new Vector2(.5f, 0), new Vector2(.5f, 0), new Vector2(0, 210), new Vector2(650, 112));
        continueButton.gameObject.AddComponent<UiBreathingAnimator>().Configure(.018f, .42f);
        UnityEventTools.AddPersistentListener(continueButton.onClick, shell.ContinueFromLaunch);
        var exit = IconButton("EXIT DOOR", actions, AssetDatabase.LoadAssetAtPath<Sprite>(Door));
        Rect(exit.GetComponent<RectTransform>(), new Vector2(1, 1), new Vector2(1, 1), new Vector2(-34, -34), new Vector2(88, 88));
        UnityEventTools.AddPersistentListener(exit.onClick, shell.ExitGame);

        var old = launch.Find("Exit Confirmation"); if (old != null) Object.DestroyImmediate(old.gameObject);
        var confirm = Panel("Exit Confirmation", launch, new Color(.025f, .02f, .05f, .94f)); Full(confirm.rectTransform);
        var card = Panel("Exit Card", confirm.transform, Ink); Rect(card.rectTransform, new Vector2(.5f, .5f), new Vector2(.5f, .5f), Vector2.zero, new Vector2(700, 460));
        var title = Label("LEAVE THE LIBRARY?", card.transform, 34, Gold); CenterTop(title.rectTransform, -72, 610, 80);
        var body = Label("Your saved progress is safe.\nExit KeySlaught now?", card.transform, 26, Cream); CenterTop(body.rectTransform, -180, 600, 110);
        var yes = TextButton("EXIT", card.transform, 26); Rect(yes.GetComponent<RectTransform>(), new Vector2(.5f, 0), new Vector2(.5f, 0), new Vector2(-160, 42), new Vector2(270, 76));
        var no = TextButton("STAY", card.transform, 26); Rect(no.GetComponent<RectTransform>(), new Vector2(.5f, 0), new Vector2(.5f, 0), new Vector2(160, 42), new Vector2(270, 76));
        UnityEventTools.AddPersistentListener(yes.onClick, shell.ConfirmExitGame);
        UnityEventTools.AddPersistentListener(no.onClick, shell.HideExitConfirmation);
        confirm.gameObject.SetActive(false);
        shell.ConfigureLaunchPresentation(group, confirm.gameObject);
        var shellData = new SerializedObject(shell);
        shellData.FindProperty("launchActionsCanvasGroup").objectReferenceValue = group;
        shellData.FindProperty("exitConfirmationPanel").objectReferenceValue = confirm.gameObject;
        shellData.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(shell);
    }

    private static void ConfigureMenuCurrency(GameShellController shell, Transform main)
    {
        if (main == null) return;
        var old = main.Find("Menu Currency Header"); if (old != null) Object.DestroyImmediate(old.gameObject);
        var header = new GameObject("Menu Currency Header", typeof(RectTransform)); header.transform.SetParent(main, false);
        Rect(header.GetComponent<RectTransform>(), new Vector2(.5f, 1), new Vector2(.5f, 1), new Vector2(0, -32), new Vector2(560, 78));
        var brainCard = Panel("Brain Cell Card", header.transform, new Color(0f, 0f, 0f, .78f));
        Rect(brainCard.rectTransform, new Vector2(0, .5f), new Vector2(0, .5f), Vector2.zero, new Vector2(300, 74)); brainCard.raycastTarget = false;
        var brainIcon = ChildImage(brainCard.transform, "Brain Icon", FindSprite("Brain_128"), Teal); Rect(brainIcon.rectTransform, new Vector2(0, .5f), new Vector2(0, .5f), new Vector2(22, 0), new Vector2(42, 42));
        var brain = Label("BRAIN CELLS  0", brainCard.transform, 22, Cream); Rect(brain.rectTransform, new Vector2(0, .5f), new Vector2(0, .5f), new Vector2(54, 0), new Vector2(230, 58)); brain.alignment = TextAnchor.MiddleLeft;
        var researchCard = Panel("Research Card", header.transform, new Color(0f, 0f, 0f, .78f));
        Rect(researchCard.rectTransform, new Vector2(1, .5f), new Vector2(1, .5f), Vector2.zero, new Vector2(240, 74)); researchCard.raycastTarget = false;
        var knowledge = Label("RESEARCH  0", researchCard.transform, 22, Gold); Full(knowledge.rectTransform); knowledge.alignment = TextAnchor.MiddleCenter;
        shell.ConfigureProgressionHeader(brain, knowledge);
    }

    private static void ConfigureShellSettings(GameShellController shell, Transform settings)
    {
        if (settings == null) return;
        var old = settings.Find("Game Speed Controls"); if (old != null) Object.DestroyImmediate(old.gameObject);
        var root = new GameObject("Game Speed Controls", typeof(RectTransform)); root.transform.SetParent(settings, false);
        Rect(root.GetComponent<RectTransform>(), new Vector2(.5f, .5f), new Vector2(.5f, .5f), new Vector2(0, -245), new Vector2(760, 190));
        var title = Label("GAME SPEED", root.transform, 25, Gold); CenterTop(title.rectTransform, 0, 650, 52);
        var one = SmallSpeedButton("1X", root.transform, -230); var two = SmallSpeedButton("2X", root.transform, 0); var three = SmallSpeedButton("3X", root.transform, 230);
        UnityEventTools.AddPersistentListener(one.onClick, shell.SetGameSpeed1); UnityEventTools.AddPersistentListener(two.onClick, shell.SetGameSpeed2); UnityEventTools.AddPersistentListener(three.onClick, shell.SetGameSpeed3);
        shell.ConfigureSpeedLabels(one.GetComponentInChildren<Text>(), two.GetComponentInChildren<Text>(), three.GetComponentInChildren<Text>());
    }

    private static void ConfigurePause()
    {
        var pause = Object.FindFirstObjectByType<PauseMenuController>(FindObjectsInactive.Include); if (pause == null) return;
        var card = pause.transform.Find("Pause Window") as RectTransform;
        if (card != null)
        {
            card.sizeDelta = new Vector2(720, 1120);
            var title = card.GetComponentsInChildren<Text>(true).FirstOrDefault(t => t.text == "PAUSED"); if (title != null) CenterTop(title.rectTransform, -70, 620, 90);
            var details = card.GetComponentsInChildren<Text>(true).FirstOrDefault(t => (t.text ?? string.Empty).Contains("DIVINE LIBRARY")); if (details != null) CenterTop(details.rectTransform, -190, 610, 230);
            var buttons = card.GetComponentsInChildren<Button>(true).Where(b => b.transform.parent == card).ToArray();
            foreach (var button in buttons)
            {
                var label = button.GetComponentInChildren<Text>(true); if (label != null) label.fontSize = Mathf.Max(26, label.fontSize);
                var y = button.name.Contains("CONTINUE") ? 310 : button.name.Contains("CONTROLS") ? 215 : button.name.Contains("SETTINGS") ? 120 : 25;
                Rect(button.GetComponent<RectTransform>(), new Vector2(.5f, 0), new Vector2(.5f, 0), new Vector2(0, y), new Vector2(540, 78));
            }
        }
        var settings = pause.transform.Find("Audio Settings Panel") as RectTransform;
        if (settings == null) return;
        settings.sizeDelta = new Vector2(700, 980);
        var old = settings.Find("Game Speed Controls"); if (old != null) Object.DestroyImmediate(old.gameObject);
        var controls = new GameObject("Game Speed Controls", typeof(RectTransform)); controls.transform.SetParent(settings, false);
        Rect(controls.GetComponent<RectTransform>(), new Vector2(.5f, 1), new Vector2(.5f, 1), new Vector2(0, -570), new Vector2(650, 190));
        var heading = Label("GAME SPEED", controls.transform, 24, Gold); CenterTop(heading.rectTransform, 0, 600, 50);
        var one = SmallSpeedButton("1X", controls.transform, -205); var two = SmallSpeedButton("2X", controls.transform, 0); var three = SmallSpeedButton("3X", controls.transform, 205);
        var selector = controls.AddComponent<GameSpeedSelector>(); selector.Configure(one.GetComponentInChildren<Text>(), two.GetComponentInChildren<Text>(), three.GetComponentInChildren<Text>());
        UnityEventTools.AddPersistentListener(one.onClick, selector.Set1X); UnityEventTools.AddPersistentListener(two.onClick, selector.Set2X); UnityEventTools.AddPersistentListener(three.onClick, selector.Set3X);
        var close = settings.GetComponentsInChildren<Button>(true).FirstOrDefault(b => b.name.Contains("CLOSE")); if (close != null) Rect(close.GetComponent<RectTransform>(), new Vector2(.5f, 0), new Vector2(.5f, 0), new Vector2(0, 35), new Vector2(520, 76));
    }

    private static void ConfigureHud()
    {
        var economy = Object.FindFirstObjectByType<BrainCellEconomy>(FindObjectsInactive.Include); if (economy == null) return;
        var data = new SerializedObject(economy); var label = data.FindProperty("currencyLabel").objectReferenceValue as Text;
        var stats = Find("Portrait Gameplay HUD/Top 20 Percent/Aligned Stats Bar")?.transform;
        if (label == null && stats != null) label = stats.GetComponentsInChildren<Text>(true).FirstOrDefault(t => t.name.Contains("BRAIN") || t.text == "0");
        if (stats != null)
        {
            var oldBrainCard = stats.Find("Brain Cell Stat Card"); if (oldBrainCard != null) Object.DestroyImmediate(oldBrainCard.gameObject);
            var oldTimeCard = stats.Find("Time Stat Card"); if (oldTimeCard != null) Object.DestroyImmediate(oldTimeCard.gameObject);
            var brainCard = Panel("Brain Cell Stat Card", stats, new Color(0f, 0f, 0f, .76f));
            Rect(brainCard.rectTransform, new Vector2(.5f, 1), new Vector2(.5f, 1), new Vector2(-84, -8), new Vector2(218, 66)); brainCard.raycastTarget = false; brainCard.transform.SetAsFirstSibling();
            var timeCard = Panel("Time Stat Card", stats, new Color(0f, 0f, 0f, .76f));
            Rect(timeCard.rectTransform, new Vector2(.5f, 1), new Vector2(.5f, 1), new Vector2(118, -8), new Vector2(172, 66)); timeCard.raycastTarget = false; timeCard.transform.SetSiblingIndex(1);
            var brainIcon = stats.Find("Brain Cell Icon")?.GetComponent<Image>();
            if (brainIcon != null) Rect(brainIcon.rectTransform, new Vector2(.5f, 1), new Vector2(.5f, 1), new Vector2(-166, -20), new Vector2(38, 38));
            var clock = stats.Find("Elegant Time Icon")?.GetComponent<Image>();
            if (clock != null) Rect(clock.rectTransform, new Vector2(.5f, 1), new Vector2(.5f, 1), new Vector2(56, -20), new Vector2(38, 38));
            var time = stats.GetComponentsInChildren<Text>(true).FirstOrDefault(t => (t.text ?? string.Empty).Contains(":"));
            if (time != null) { time.fontSize = Mathf.Max(time.fontSize, 27); time.fontStyle = FontStyle.Bold; Rect(time.rectTransform, new Vector2(.5f, 1), new Vector2(.5f, 1), new Vector2(130, -13), new Vector2(105, 56)); }
        }
        if (label != null)
        {
            label.gameObject.SetActive(true); label.fontSize = 30; label.fontStyle = FontStyle.Bold; label.color = Cream; label.alignment = TextAnchor.MiddleLeft; label.resizeTextForBestFit = false;
            Rect(label.rectTransform, new Vector2(.5f, 1), new Vector2(.5f, 1), new Vector2(-77, -13), new Vector2(120, 58));
            data.FindProperty("currencyLabel").objectReferenceValue = label; data.ApplyModifiedPropertiesWithoutUndo(); EditorUtility.SetDirty(economy);
        }
    }

    private static void ConfigureContextButtons()
    {
        var panel = Find("Portrait Gameplay HUD/Top 20 Percent/Transparent Context Actions")?.GetComponent<RectTransform>(); if (panel == null) return;
        panel.anchoredPosition = new Vector2(0, -58);
        panel.sizeDelta = new Vector2(-44, 270);
        var buttons = panel.GetComponentsInChildren<Button>(true).Where(b => b.GetComponent<ContextActionButton>() != null).OrderBy(b => b.name).ToArray();
        var brain = FindSprite("Brain_128");
        for (var index = 0; index < buttons.Length; index++)
        {
            var row = index / 2; var col = index % 2;
            Rect(buttons[index].GetComponent<RectTransform>(), new Vector2(0, 1), new Vector2(0, 1), new Vector2(20 + col * 418, -52 - row * 78), new Vector2(394, 68));
            var label = buttons[index].GetComponentInChildren<Text>(true); if (label != null) { label.fontSize = 24; label.resizeTextForBestFit = false; label.alignment = TextAnchor.MiddleCenter; label.rectTransform.offsetMin = new Vector2(64, 6); label.rectTransform.offsetMax = new Vector2(-92, -6); }
            var old = buttons[index].transform.Find("Cost Number"); if (old != null) Object.DestroyImmediate(old.gameObject);
            var cost = Label(string.Empty, buttons[index].transform, 22, Gold); cost.gameObject.name = "Cost Number"; cost.fontStyle = FontStyle.Bold; cost.horizontalOverflow = HorizontalWrapMode.Overflow; Rect(cost.rectTransform, new Vector2(1, 1), new Vector2(1, 1), new Vector2(-12, -8), new Vector2(82, 30)); cost.alignment = TextAnchor.UpperRight;
            var icon = buttons[index].transform.Find("Cost Icon")?.GetComponent<Image>(); if (icon != null) Rect(icon.rectTransform, new Vector2(1, 1), new Vector2(1, 1), new Vector2(-36, -42), new Vector2(28, 28));
            var presenter = buttons[index].GetComponent<ContextActionIconPresenter>(); presenter?.ConfigureCostLabel(cost);
        }
    }

    private static void ConfigureSkipButton()
    {
        var stats = Find("Portrait Gameplay HUD/Top 20 Percent/Aligned Stats Bar")?.transform; if (stats == null) return;
        var button = stats.GetComponentsInChildren<Button>(true).FirstOrDefault(b => b.GetComponent<IntermissionFastForwardButton>() != null); if (button == null) return;
        button.gameObject.name = "Button SKIP WAIT";
        var label = button.GetComponentInChildren<Text>(true); if (label != null) { label.text = "SKIP WAIT"; label.fontSize = 19; }
        Rect(button.GetComponent<RectTransform>(), new Vector2(0, 1), new Vector2(0, 1), new Vector2(22, -76), new Vector2(230, 52));
        if (button.GetComponent<UiBreathingAnimator>() == null) button.gameObject.AddComponent<UiBreathingAnimator>().Configure(.012f, .5f);
    }

    private static void ConfigureTutorialFocus()
    {
        var hud = Find("Portrait Gameplay HUD")?.transform; if (hud == null) return;
        var director = Object.FindFirstObjectByType<TutorialDirector>(FindObjectsInactive.Include); if (director == null) return;
        var old = hud.Find("Tutorial Focus Guide"); if (old != null) Object.DestroyImmediate(old.gameObject);
        var image = Panel("Tutorial Focus Guide", hud, Gold); image.color = new Color(Gold.r, Gold.g, Gold.b, .22f); image.raycastTarget = false;
        var outline = image.gameObject.AddComponent<Outline>(); outline.effectColor = new Color(Gold.r, Gold.g, Gold.b, .92f); outline.effectDistance = new Vector2(6f, -6f); outline.useGraphicAlpha = false;
        var guide = image.gameObject.AddComponent<TutorialFocusGuide>(); image.gameObject.SetActive(false); image.transform.SetAsLastSibling();
        var arena = Find("Portrait Gameplay HUD/Tutorial Drag Controller")?.GetComponent<RectTransform>()
            ?? hud.Find("Middle 60 Percent") as RectTransform;
        var input = hud.Find("Bottom 20 Percent") as RectTransform;
        director.ConfigureFocusGuide(guide, arena, input);
    }

    private static void ConfigureResult(bool sampleScene)
    {
        var root = Find("Portrait Gameplay HUD/Run Result Overlay")?.transform; if (root == null) return;
        for (var index = root.childCount - 1; index >= 0; index--) Object.DestroyImmediate(root.GetChild(index).gameObject);
        var card = Panel("Result Window", root, Ink); Rect(card.rectTransform, new Vector2(.5f, .5f), new Vector2(.5f, .5f), Vector2.zero, new Vector2(740, 940));
        var title = Label("LIBRARY DEFENDED", card.transform, 39, Gold); title.fontStyle = FontStyle.Bold; CenterTop(title.rectTransform, -80, 650, 100);
        var rewards = Label("REWARDS", card.transform, 28, Cream); CenterTop(rewards.rectTransform, -235, 620, 180);
        var unlock = Label("PROGRESS SAVED", card.transform, 31, Gold); unlock.fontStyle = FontStyle.Bold; CenterTop(unlock.rectTransform, -440, 620, 150);
        var primary = TextButton("CONTINUE", card.transform, 27); Rect(primary.GetComponent<RectTransform>(), new Vector2(.5f, 0), new Vector2(.5f, 0), new Vector2(0, 145), new Vector2(560, 84));
        var menu = TextButton("MAIN MENU", card.transform, 25); Rect(menu.GetComponent<RectTransform>(), new Vector2(.5f, 0), new Vector2(.5f, 0), new Vector2(0, 45), new Vector2(560, 78));
        card.gameObject.AddComponent<UiPanelEntranceAnimator>().Configure(.4f, 34f);
        var presenter = root.GetComponent<RunResultPresenter>() ?? root.gameObject.AddComponent<RunResultPresenter>();
        presenter.Configure(title, rewards, unlock, primary, primary.GetComponentInChildren<Text>());
        var run = Object.FindFirstObjectByType<WaveRunController>(FindObjectsInactive.Include);
        if (run != null)
        {
            var data = new SerializedObject(run); data.FindProperty("resultRoot").objectReferenceValue = root.gameObject; data.FindProperty("resultLabel").objectReferenceValue = title; data.ApplyModifiedPropertiesWithoutUndo(); EditorUtility.SetDirty(run);
        }
        if (sampleScene)
        {
            var shell = Object.FindFirstObjectByType<GameShellController>(FindObjectsInactive.Include); if (shell != null) { shell.ConfigureResultPresenter(presenter); UnityEventTools.AddPersistentListener(primary.onClick, shell.ContinueFromResult); UnityEventTools.AddPersistentListener(menu.onClick, shell.ReturnToMainFromRun); }
        }
        else
        {
            var flow = Object.FindFirstObjectByType<DedicatedLevelSceneController>(FindObjectsInactive.Include); if (flow != null) { flow.ConfigureResultPresenter(presenter); UnityEventTools.AddPersistentListener(primary.onClick, flow.ContinueFromResult); UnityEventTools.AddPersistentListener(menu.onClick, flow.ReturnToMainMenu); }
        }
        root.gameObject.SetActive(false);
    }

    private static void ConfigurePlayer()
    {
        var player = Object.FindFirstObjectByType<PlayerMover>(FindObjectsInactive.Include); if (player == null) return;
        var data = new SerializedObject(player); data.FindProperty("movementSpeed").floatValue = 5.6f; data.ApplyModifiedPropertiesWithoutUndo(); EditorUtility.SetDirty(player);
    }

    private static void TuneDefinitions()
    {
        var speeds = new[]
        {
            new[] { .68f, .74f, .80f, .87f, .64f },
            new[] { .78f, .85f, .92f, 1.00f, .72f },
            new[] { .88f, .96f, 1.04f, 1.12f, .82f }
        };
        var paths = new[] { "Assets/Data/Levels/LoreOneLevelOne.asset", "Assets/Data/Levels/LoreOneLevelTwo.asset", "Assets/Data/Levels/LoreOneLevelThree.asset" };
        for (var levelIndex = 0; levelIndex < paths.Length; levelIndex++)
        {
            var level = AssetDatabase.LoadAssetAtPath<LevelDefinition>(paths[levelIndex]); if (level == null) continue;
            var data = new SerializedObject(level); data.FindProperty("intermissionSeconds").floatValue = 10f; var waves = data.FindProperty("waves");
            for (var wave = 0; wave < waves.arraySize && wave < speeds[levelIndex].Length; wave++) waves.GetArrayElementAtIndex(wave).FindPropertyRelative("enemyMovementSpeed").floatValue = speeds[levelIndex][wave];
            data.ApplyModifiedPropertiesWithoutUndo(); EditorUtility.SetDirty(level);
        }
        foreach (var item in new[] { ("Teacher", 1.20f), ("Engineer", 1.65f), ("Scientist", 2.10f), ("President", 2.50f) })
        {
            var turret = AssetDatabase.LoadAssetAtPath<TurretDefinition>($"Assets/Data/Turrets/{item.Item1}.asset"); if (turret == null) continue;
            var data = new SerializedObject(turret); data.FindProperty("secondsPerShot").floatValue = item.Item2; data.FindProperty("cadenceMultiplierPerUpgrade").floatValue = .86f; data.ApplyModifiedPropertiesWithoutUndo(); EditorUtility.SetDirty(turret);
        }
    }

    private static void AddArtworkBackground(Transform panel, bool isLaunch)
    {
        var legacyArt = panel.Find("Cartoon Splash"); if (legacyArt != null) Object.DestroyImmediate(legacyArt.gameObject);
        var legacyShade = panel.Find("Splash Readability Shade"); if (legacyShade != null) Object.DestroyImmediate(legacyShade.gameObject);
        var old = panel.Find("Artwork Background"); if (old != null) Object.DestroyImmediate(old.gameObject);
        var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(Splash) ?? throw new InvalidOperationException("Splash artwork missing.");
        var art = ChildImage(panel, "Artwork Background", sprite, Color.white); Full(art.rectTransform); art.preserveAspect = false; art.raycastTarget = false; art.transform.SetAsFirstSibling();
        var shade = panel.Find("Artwork Shade")?.GetComponent<Image>() ?? Panel("Artwork Shade", panel, Color.clear);
        shade.color = isLaunch ? Color.clear : new Color(0f, 0f, 0f, .64f);
        Full(shade.rectTransform); shade.raycastTarget = false; shade.transform.SetSiblingIndex(1);
    }

    private static Button SmallSpeedButton(string text, Transform parent, float x)
    {
        var button = TextButton(text, parent, 20); Rect(button.GetComponent<RectTransform>(), new Vector2(.5f, 0), new Vector2(.5f, 0), new Vector2(x, 10), new Vector2(200, 68)); return button;
    }

    private static Button TextButton(string text, Transform parent, int size)
    {
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/UiTextButton.prefab");
        var go = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent); go.name = "Button " + text;
        var label = go.GetComponentInChildren<Text>(true); label.text = text; label.fontSize = size; label.color = Cream;
        return go.GetComponent<Button>();
    }

    private static Button IconButton(string name, Transform parent, Sprite sprite)
    {
        var go = new GameObject("Button " + name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button)); go.transform.SetParent(parent, false);
        var image = go.GetComponent<Image>(); image.sprite = sprite; image.color = Cream; image.preserveAspect = true;
        var button = go.GetComponent<Button>(); button.targetGraphic = image; return button;
    }

    private static Text Label(string text, Transform parent, int size, Color color)
    {
        var go = new GameObject("Label " + text.Split('\n')[0], typeof(RectTransform), typeof(CanvasRenderer), typeof(Text)); go.transform.SetParent(parent, false);
        var label = go.GetComponent<Text>(); label.text = text; label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); label.fontSize = size; label.color = color; label.alignment = TextAnchor.MiddleCenter; label.horizontalOverflow = HorizontalWrapMode.Wrap; label.verticalOverflow = VerticalWrapMode.Overflow; return label;
    }

    private static Image Panel(string name, Transform parent, Color color)
    {
        var image = ChildImage(parent, name, AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Prototype/SolidSprite.asset"), color); image.type = Image.Type.Sliced; return image;
    }

    private static Image ChildImage(Transform parent, string name, Sprite sprite, Color color)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image)); go.transform.SetParent(parent, false); var image = go.GetComponent<Image>(); image.sprite = sprite; image.color = color; image.preserveAspect = true; return image;
    }

    private static Sprite FindSprite(string name)
    {
        var path = AssetDatabase.FindAssets($"{name} t:Sprite").Select(AssetDatabase.GUIDToAssetPath).FirstOrDefault(); return AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }

    private static void GenerateDoorIcon()
    {
        if (File.Exists(Door)) return;
        Directory.CreateDirectory(Path.GetDirectoryName(Door)); const int size = 128; var texture = new Texture2D(size, size, TextureFormat.RGBA32, false); var pixels = Enumerable.Repeat(new Color32(0, 0, 0, 0), size * size).ToArray();
        void Box(int x, int y, int width, int height, Color32 color) { for (var yy = y; yy < y + height; yy++) for (var xx = x; xx < x + width; xx++) if (xx >= 0 && xx < size && yy >= 0 && yy < size) pixels[yy * size + xx] = color; }
        var cream = (Color32)Cream; var gold = (Color32)Gold; var dark = (Color32)Ink;
        Box(24, 12, 78, 104, cream); Box(33, 20, 60, 88, dark); Box(38, 24, 49, 80, (Color32)Plum); Box(79, 61, 9, 9, gold); Box(15, 8, 10, 112, gold); Box(102, 8, 10, 112, gold); Box(15, 110, 97, 10, gold);
        texture.SetPixels32(pixels); texture.Apply(); File.WriteAllBytes(Door, texture.EncodeToPNG()); Object.DestroyImmediate(texture);
    }

    private static void ConfigureSprite(string path)
    {
        if (AssetImporter.GetAtPath(path) is not TextureImporter importer) return; importer.textureType = TextureImporterType.Sprite; importer.spriteImportMode = SpriteImportMode.Single; importer.mipmapEnabled = false; importer.alphaIsTransparency = true; importer.filterMode = FilterMode.Point; importer.SaveAndReimport();
    }

    private static void Clear(UnityEngine.Events.UnityEvent action) { for (var index = action.GetPersistentEventCount() - 1; index >= 0; index--) UnityEventTools.RemovePersistentListener(action, index); }
    private static GameObject Find(string path) => GameObject.Find("/" + path) ?? Resources.FindObjectsOfTypeAll<GameObject>().FirstOrDefault(go => go.scene == SceneManager.GetActiveScene() && Hierarchy(go) == path);
    private static string Hierarchy(GameObject go) { var value = go.name; for (var parent = go.transform.parent; parent != null; parent = parent.parent) value = parent.name + "/" + value; return value; }
    private static void Full(RectTransform rect) { rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one; rect.offsetMin = Vector2.zero; rect.offsetMax = Vector2.zero; }
    private static void CenterTop(RectTransform rect, float y, float width, float height) => Rect(rect, new Vector2(.5f, 1), new Vector2(.5f, 1), new Vector2(0, y), new Vector2(width, height));
    private static void Rect(RectTransform rect, Vector2 min, Vector2 max, Vector2 position, Vector2 size) { rect.anchorMin = min; rect.anchorMax = max; rect.pivot = min == max ? min : new Vector2(.5f, .5f); rect.anchoredPosition = position; rect.sizeDelta = size; }
    private static Color C(string hex) { ColorUtility.TryParseHtmlString("#" + hex, out var color); return color; }
}
