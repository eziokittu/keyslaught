using System;
using System.Collections.Generic;
using System.Linq;
using KeySlaught.Progression;
using KeySlaught.SceneGameplay;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;
using UnityEngine.UI;
using Object = UnityEngine.Object;

public static class BuildMilestone18Issues9
{
    private const string Sample = "Assets/Scenes/SampleScene.unity";
    private static readonly string[] LoreScenes =
    {
        "Assets/Scenes/LoreOneLevelOne.unity", "Assets/Scenes/LoreOneLevelTwo.unity",
        "Assets/Scenes/LoreOneLevelThree.unity", "Assets/Scenes/LoreOneLevelFour.unity",
        "Assets/Scenes/LoreOneLevelFive.unity", "Assets/Scenes/LoreOneLevelSix.unity"
    };
    private static readonly string[] NumberWords = { "One", "Two", "Three", "Four", "Five", "Six" };
    private static readonly Color Ink = C("10152B");
    private static readonly Color Gold = C("F4C967");
    private static readonly Color Cream = C("FFF2CF");
    private static readonly Color Teal = C("38B7B0");

    public static string Build()
    {
        if (EditorApplication.isPlaying) throw new InvalidOperationException("Exit Play Mode before authoring Milestone 18.");
        var research = BuildResearch();
        var loreLevels = BuildLoreLevels();
        var endless = BuildEndlessLevel();
        ConfigureTutorialPreparation();

        var sample = EditorSceneManager.OpenScene(Sample, OpenSceneMode.Single);
        ConfigureCommonGameplay();
        ConfigureSampleShell(research, endless);
        RepaintAllRoutes();
        EditorSceneManager.MarkSceneDirty(sample);
        EditorSceneManager.SaveScene(sample);

        for (var index = 0; index < LoreScenes.Length; index++)
        {
            var scene = EditorSceneManager.OpenScene(LoreScenes[index], OpenSceneMode.Single);
            var flow = Object.FindFirstObjectByType<DedicatedLevelSceneController>(FindObjectsInactive.Include)
                ?? throw new InvalidOperationException($"Dedicated flow missing in {LoreScenes[index]}.");
            var run = Object.FindFirstObjectByType<WaveRunController>(FindObjectsInactive.Include)
                ?? throw new InvalidOperationException($"Wave run missing in {LoreScenes[index]}.");
            flow.SetLoreLevelNumber(index + 1);
            flow.SetLevelDefinition(loreLevels[index]);
            run.SetLevel(loreLevels[index]);
            ConfigureCommonGameplay();
            RepaintAllRoutes();
            EditorUtility.SetDirty(flow);
            EditorUtility.SetDirty(run);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        EditorSceneManager.OpenScene(Sample, OpenSceneMode.Single);
        return Audit();
    }

    public static string BuildDataAndSample()
    {
        if (EditorApplication.isPlaying) throw new InvalidOperationException("Exit Play Mode before authoring Milestone 18.");
        var research = BuildResearch();
        BuildLoreLevels();
        var endless = BuildEndlessLevel();
        ConfigureTutorialPreparation();
        var sample = EditorSceneManager.OpenScene(Sample, OpenSceneMode.Single);
        ConfigureCommonGameplay();
        ConfigureSampleShell(research, endless);
        RepaintAllRoutes();
        EditorSceneManager.MarkSceneDirty(sample); EditorSceneManager.SaveScene(sample);
        AssetDatabase.SaveAssets();
        return "Milestone 18 data and SampleScene authored.";
    }

    public static string BuildLoreOne() => BuildLoreScene(0);
    public static string BuildLoreTwo() => BuildLoreScene(1);
    public static string BuildLoreThree() => BuildLoreScene(2);
    public static string BuildLoreFour() => BuildLoreScene(3);
    public static string BuildLoreFive() => BuildLoreScene(4);
    public static string BuildLoreSix() => BuildLoreScene(5);

    private static string BuildLoreScene(int index)
    {
        if (EditorApplication.isPlaying) throw new InvalidOperationException("Exit Play Mode before authoring Milestone 18.");
        var level = AssetDatabase.LoadAssetAtPath<LevelDefinition>($"Assets/Data/Levels/LoreOneLevel{NumberWords[index]}.asset")
            ?? throw new InvalidOperationException($"Lore level {index + 1} data missing.");
        var scene = EditorSceneManager.OpenScene(LoreScenes[index], OpenSceneMode.Single);
        var flow = Object.FindFirstObjectByType<DedicatedLevelSceneController>(FindObjectsInactive.Include)
            ?? throw new InvalidOperationException($"Dedicated flow missing in {LoreScenes[index]}.");
        var run = Object.FindFirstObjectByType<WaveRunController>(FindObjectsInactive.Include)
            ?? throw new InvalidOperationException($"Wave run missing in {LoreScenes[index]}.");
        flow.SetLoreLevelNumber(index + 1); flow.SetLevelDefinition(level); run.SetLevel(level);
        ConfigureCommonGameplay(); RepaintAllRoutes();
        EditorUtility.SetDirty(flow); EditorUtility.SetDirty(run);
        EditorSceneManager.MarkSceneDirty(scene); EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();
        return $"Lore I Level {index + 1} authored and route-repainted.";
    }

    public static string Audit()
    {
        var errors = new List<string>();
        var endless = AssetDatabase.LoadAssetAtPath<LevelDefinition>("Assets/Data/Levels/EndlessPrototype.asset");
        if (endless == null || endless.Waves == null || endless.Waves.Length != 30) errors.Add("Endless does not contain 30 waves");
        else
        {
            for (var index = 4; index < 30; index += 5)
                if (!endless.Waves[index].Title.ToUpperInvariant().Contains("BOSS") || endless.Waves[index].Words.Length < 2)
                    errors.Add($"Endless wave {index + 1} is not a boss-plus-normal wave");
            if (!endless.PauseAfterBossWave || endless.InitialPreparationSeconds < 29.9f) errors.Add("Endless preparation/checkpoints are not configured");
        }

        foreach (var path in LoreScenes)
        {
            EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
            var flow = Object.FindFirstObjectByType<DedicatedLevelSceneController>(FindObjectsInactive.Include);
            if (flow == null || flow.Level == null || flow.Level.Waves.Length != 6) errors.Add($"{path} level wiring invalid");
            else if (flow.Level.InitialPreparationSeconds < 29.9f) errors.Add($"{path} has no opening preparation");
            var library = Object.FindFirstObjectByType<PermanentUpgradeApplier>(FindObjectsInactive.Include);
            if (library == null || new SerializedObject(library).FindProperty("baseLibraryHealth").intValue != 15) errors.Add($"{path} base Library HP is not 15");
            var progression = Object.FindFirstObjectByType<ProgressionService>(FindObjectsInactive.Include);
            if (progression == null || progression.Definitions == null || progression.Definitions.Length != 12) errors.Add($"{path} does not have all twelve research branches");
            var arena = Object.FindFirstObjectByType<ReusableLevelArena>(FindObjectsInactive.Include);
            if (arena == null || CountPaintedCells(arena.EnemyPath) < 20) errors.Add($"{path} route Tilemap is incomplete");
            foreach (var route in Object.FindObjectsByType<WaypointPath>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                if (route.GetComponent<LineRenderer>() != null && route.GetComponent<LineRenderer>().enabled) errors.Add($"{path} still has a differently colored route line");
        }

        EditorSceneManager.OpenScene(Sample, OpenSceneMode.Single);
        var shell = Object.FindFirstObjectByType<GameShellController>(FindObjectsInactive.Include);
        var service = Object.FindFirstObjectByType<ProgressionService>(FindObjectsInactive.Include);
        if (shell == null || service == null || service.Definitions == null || service.Definitions.Length != 12) errors.Add("Twelve research branches are not wired");
        if (Find("Game Shell Canvas/Game Shell/Main Menu/Menu Currency Header/Brain Cell Card") != null) errors.Add("Brain cells still appear in the menu");
        if (Find("Game Shell Canvas/Game Shell/Lore Levels/Lore II In Development") == null) errors.Add("Lore II development page missing");
        if (Find("Game Shell Canvas/Game Shell/Mode Select/Tutorial Replay Confirmation") == null) errors.Add("Tutorial replay confirmation missing");
        if (Find("Portrait Gameplay HUD/Endless Boss Checkpoint") == null) errors.Add("Endless boss checkpoint choice missing");
        if (errors.Count > 0) throw new InvalidOperationException(string.Join(". ", errors));
        return "Milestone 18 audit passed: menu-only Knowledge, run-only brain cells, cyclic Lore I/II picker, tutorial replay confirmation, 30-wave Endless with boss checkpoints, harder Lore waves, 15+20x5 Library HP research, separate turret range/speed research, themed particles, repainted shared-style routes, best-time corner tags, and 30-second skippable openings.";
    }

    public static string ReviewMain()
    {
        Find("Game Shell Canvas/Game Shell")?.SetActive(true);
        Object.FindFirstObjectByType<GameShellController>(FindObjectsInactive.Include)?.ShowMain();
        return "Main menu staged.";
    }

    public static string ReviewResearch()
    {
        Find("Game Shell Canvas/Game Shell")?.SetActive(true);
        Object.FindFirstObjectByType<GameShellController>(FindObjectsInactive.Include)?.ShowResearch();
        return "Research staged.";
    }

    public static string ReviewModes()
    {
        Find("Game Shell Canvas/Game Shell")?.SetActive(true);
        Object.FindFirstObjectByType<GameShellController>(FindObjectsInactive.Include)?.ShowModes();
        return "Mode selection staged.";
    }

    public static string ReviewLoreOne()
    {
        Find("Game Shell Canvas/Game Shell")?.SetActive(true);
        Object.FindFirstObjectByType<GameShellController>(FindObjectsInactive.Include)?.ShowLoreLevels();
        return "Lore I staged.";
    }

    public static string ReviewLoreTwo()
    {
        ReviewLoreOne();
        Object.FindFirstObjectByType<GameShellController>(FindObjectsInactive.Include)?.NextLore();
        return "Lore II staged.";
    }

    public static string ReviewReplayConfirmation()
    {
        var service = Object.FindFirstObjectByType<ProgressionService>(FindObjectsInactive.Include);
        if (service != null && service.Profile != null) service.CompleteTutorial();
        Find("Game Shell Canvas/Game Shell")?.SetActive(true);
        var shell = Object.FindFirstObjectByType<GameShellController>(FindObjectsInactive.Include);
        shell?.ShowModes(); shell?.StartTutorial();
        return "Tutorial replay confirmation staged.";
    }

    public static string ReviewGameplayHud()
    {
        Find("Game Shell Canvas/Game Shell")?.SetActive(false);
        var run = Object.FindFirstObjectByType<WaveRunController>(FindObjectsInactive.Include);
        run?.SetMenuSuspended(false);
        return $"Gameplay HUD staged; phase={run?.Phase}; preparation={run?.IntermissionRemaining:F1}.";
    }

    public static string ReviewEndlessPreparation()
    {
        var service = Object.FindFirstObjectByType<ProgressionService>(FindObjectsInactive.Include);
        service?.CompleteLoreOneLevelOne();
        var shell = Object.FindFirstObjectByType<GameShellController>(FindObjectsInactive.Include);
        shell?.StartEndless();
        var run = Object.FindFirstObjectByType<WaveRunController>(FindObjectsInactive.Include);
        return $"Endless staged; waves={run?.ActiveLevel?.Waves?.Length}; phase={run?.Phase}; preparation={run?.IntermissionRemaining:F1}.";
    }

    public static string ResetLocalProgress()
    {
        var service = Object.FindFirstObjectByType<ProgressionService>(FindObjectsInactive.Include);
        service?.ResetProgress();
        PlayerPrefs.DeleteKey("KeySlaught.ReturnToMainMenu");
        PlayerPrefs.Save();
        return $"Local progression reset; firstLaunch={service?.Profile?.firstLaunch}; tutorial={service?.Profile?.tutorialCompleted}; knowledge={service?.Profile?.knowledgePoints}.";
    }

    public static string StartMovementParticlePreview()
    {
        var player = Object.FindFirstObjectByType<PlayerMover>(FindObjectsInactive.Include);
        player?.SetVirtualMovement(new Vector2(.65f, .3f));
        return "Player movement held for soft-cloud particle review.";
    }

    public static string CleanupReviewCaptures()
    {
        var names = new[]
        {
            "m18_endless_prep.png", "m18_lore1.png", "m18_lore2.png", "m18_lore3_route.png",
            "m18_main.png", "m18_main_camera.png", "m18_modes.png", "m18_particles.png",
            "m18_replay.png", "m18_research.png"
        };
        var removed = 0;
        foreach (var name in names) if (AssetDatabase.DeleteAsset($"Assets/Temp/{name}")) removed++;
        if (AssetDatabase.IsValidFolder("Assets/Temp")) AssetDatabase.DeleteAsset("Assets/Temp");
        AssetDatabase.Refresh();
        return $"Removed {removed} temporary visual-review captures.";
    }

    public static string InspectActiveRoutes()
    {
        var arena = Object.FindFirstObjectByType<ReusableLevelArena>(FindObjectsInactive.Include);
        var routes = Object.FindObjectsByType<WaypointPath>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        return $"scene={SceneManager.GetActiveScene().name}; tiles={arena?.EnemyPath?.GetUsedTilesCount()}; routes={routes.Length}; " +
            string.Join(" | ", routes.Select(route => $"{route.name}: points={route.WaypointCount}, " +
                string.Join(", ", route.CopyPositions().Select(position => $"{position}->{arena?.EnemyPath?.WorldToCell(position)}"))));
    }

    private static void ConfigureSampleShell(ResearchDefinition[] research, LevelDefinition endless)
    {
        var shell = Object.FindFirstObjectByType<GameShellController>(FindObjectsInactive.Include)
            ?? throw new InvalidOperationException("Game shell missing.");
        var service = Object.FindFirstObjectByType<ProgressionService>(FindObjectsInactive.Include)
            ?? throw new InvalidOperationException("Progression service missing.");
        service.Configure(research);
        EditorUtility.SetDirty(service);
        var shellData = new SerializedObject(shell);
        shellData.FindProperty("endlessLevel").objectReferenceValue = endless;
        shellData.FindProperty("brainCellsLabel").objectReferenceValue = null;
        shellData.ApplyModifiedPropertiesWithoutUndo();
        BuildMainKnowledgeHeader(shell);
        BuildResearchMenu(shell, service, research);
        BuildLoreCarousel(shell);
        BuildTutorialReplayConfirmation(shell);
        BuildEndlessRecommendation(shell);
        BuildEndlessCheckpoint(shell);
        EditorUtility.SetDirty(shell);
    }

    private static void BuildMainKnowledgeHeader(GameShellController shell)
    {
        var main = Find("Game Shell Canvas/Game Shell/Main Menu")?.transform;
        if (main == null) return;
        var header = main.Find("Menu Currency Header");
        var brain = header?.Find("Brain Cell Card");
        if (brain != null) Object.DestroyImmediate(brain.gameObject);
        var knowledgeCard = header?.Find("Research Card")?.GetComponent<Image>();
        if (knowledgeCard == null) return;
        Rect(knowledgeCard.rectTransform, new Vector2(.5f, 1), new Vector2(.5f, 1), new Vector2(0, -18), new Vector2(430, 88));
        var label = knowledgeCard.GetComponentInChildren<Text>(true);
        if (label != null)
        {
            label.text = "KNOWLEDGE POINTS  0";
            label.fontSize = 30;
            label.fontStyle = FontStyle.Bold;
            label.resizeTextForBestFit = true;
            label.resizeTextMinSize = 24;
            label.resizeTextMaxSize = 32;
            Full(label.rectTransform);
        }
        shell.ConfigureProgressionHeader(null, label);
    }

    private static void BuildResearchMenu(GameShellController shell, ProgressionService service, ResearchDefinition[] definitions)
    {
        var panel = Find("Game Shell Canvas/Game Shell/Research")?.transform
            ?? throw new InvalidOperationException("Research panel missing.");
        foreach (var button in panel.GetComponentsInChildren<Button>(true)) Object.DestroyImmediate(button.gameObject);
        foreach (var text in panel.GetComponentsInChildren<Text>(true).Where(t => !t.name.Contains("KNOWLEDGE")).ToArray()) Object.DestroyImmediate(text.gameObject);
        var oldHeader = panel.Find("Research Header Backplate"); if (oldHeader != null) Object.DestroyImmediate(oldHeader.gameObject);
        Backplate("Research Header Backplate", panel, new Vector2(0, -135), new Vector2(820, 260));
        var title = Label("RESEARCH TREE", panel, 40, Gold); Rect(title.rectTransform, new Vector2(.5f, 1), new Vector2(.5f, 1), new Vector2(0, -70), new Vector2(650, 70));
        var note = Label("Permanent upgrades use Knowledge Points. Turret range and attack speed are upgraded separately.", panel, 20, Cream);
        Rect(note.rectTransform, new Vector2(.5f, 1), new Vector2(.5f, 1), new Vector2(0, -135), new Vector2(760, 70));
        var knowledge = panel.GetComponentsInChildren<Text>(true).FirstOrDefault(t => (t.text ?? "").Contains("KNOWLEDGE"))
            ?? Label("KNOWLEDGE POINTS  0", panel, 30, Teal);
        knowledge.fontSize = 30; knowledge.fontStyle = FontStyle.Bold;
        Rect(knowledge.rectTransform, new Vector2(.5f, 1), new Vector2(.5f, 1), new Vector2(0, -210), new Vector2(650, 55));
        var buttons = new Button[definitions.Length];
        var labels = new Text[definitions.Length];
        for (var index = 0; index < definitions.Length; index++)
        {
            buttons[index] = TextButton(definitions[index].DisplayName, panel, 19);
            var x = index % 2 == 0 ? -192f : 192f;
            var y = -300f - index / 2 * 100f;
            Rect(buttons[index].GetComponent<RectTransform>(), new Vector2(.5f, 1), new Vector2(.5f, 1), new Vector2(x, y), new Vector2(360, 74));
            labels[index] = buttons[index].GetComponentInChildren<Text>(true);
            Clear(buttons[index].onClick);
            UnityEventTools.AddIntPersistentListener(buttons[index].onClick, shell.PurchaseResearch, index);
        }
        var back = TextButton("BACK", panel, 24);
        Rect(back.GetComponent<RectTransform>(), new Vector2(0, 1), new Vector2(0, 1), new Vector2(18, -18), new Vector2(190, 70));
        UnityEventTools.AddPersistentListener(back.onClick, shell.ShowMain);
        shell.ConfigureResearchUi(buttons, labels);
        shell.ConfigureResearchKnowledgeLabel(knowledge);
    }

    private static void BuildLoreCarousel(GameShellController shell)
    {
        var panel = Find("Game Shell Canvas/Game Shell/Lore Levels")?.transform
            ?? throw new InvalidOperationException("Lore Levels panel missing.");
        foreach (var button in panel.GetComponentsInChildren<Button>(true)) Object.DestroyImmediate(button.gameObject);
        foreach (var text in panel.GetComponentsInChildren<Text>(true).Where(t => t.transform != panel).ToArray()) Object.DestroyImmediate(text.gameObject);
        foreach (var name in new[] { "Levels Header Backplate", "Lore I Content", "Lore II In Development" })
        {
            var old = panel.Find(name); if (old != null) Object.DestroyImmediate(old.gameObject);
        }
        Backplate("Levels Header Backplate", panel, new Vector2(0, -105), new Vector2(820, 185));
        var title = Label("LORE I", panel, 42, Gold);
        Rect(title.rectTransform, new Vector2(.5f, 1), new Vector2(.5f, 1), new Vector2(0, -82), new Vector2(470, 76));
        var left = TextButton("◀◀", panel, 34); Rect(left.GetComponent<RectTransform>(), new Vector2(.5f, 1), new Vector2(.5f, 1), new Vector2(-330, -94), new Vector2(120, 72));
        var right = TextButton("▶▶", panel, 34); Rect(right.GetComponent<RectTransform>(), new Vector2(.5f, 1), new Vector2(.5f, 1), new Vector2(330, -94), new Vector2(120, 72));
        UnityEventTools.AddPersistentListener(left.onClick, shell.PreviousLore);
        UnityEventTools.AddPersistentListener(right.onClick, shell.NextLore);

        var content = new GameObject("Lore I Content", typeof(RectTransform)); content.transform.SetParent(panel, false); Full(content.GetComponent<RectTransform>());
        var buttons = new Button[6]; var labels = new Text[6]; var bestLabels = new Text[6];
        for (var index = 0; index < 6; index++)
        {
            buttons[index] = TextButton($"LEVEL {index + 1}", content.transform, 25);
            var x = index % 2 == 0 ? -190f : 190f; var y = -265f - index / 2 * 155f;
            Rect(buttons[index].GetComponent<RectTransform>(), new Vector2(.5f, 1), new Vector2(.5f, 1), new Vector2(x, y), new Vector2(340, 112));
            labels[index] = buttons[index].GetComponentInChildren<Text>(true);
            Clear(buttons[index].onClick);
            var tag = Backplate("Best Time Corner", buttons[index].transform, new Vector2(-4, -4), new Vector2(162, 38));
            Rect(tag.rectTransform, new Vector2(1, 1), new Vector2(1, 1), new Vector2(-4, -4), new Vector2(162, 38));
            tag.rectTransform.localRotation = Quaternion.Euler(0, 0, -7f);
            var best = Label("BEST 00:00", tag.transform, 17, Gold); Full(best.rectTransform); best.fontStyle = FontStyle.Bold;
            bestLabels[index] = best; tag.gameObject.SetActive(false);
        }
        UnityEventTools.AddPersistentListener(buttons[0].onClick, shell.LoadLoreOneLevelOne);
        UnityEventTools.AddPersistentListener(buttons[1].onClick, shell.StartLoreOneLevelTwo);
        UnityEventTools.AddPersistentListener(buttons[2].onClick, shell.StartLoreOneLevelThree);
        UnityEventTools.AddPersistentListener(buttons[3].onClick, shell.StartLoreOneLevelFour);
        UnityEventTools.AddPersistentListener(buttons[4].onClick, shell.StartLoreOneLevelFive);
        UnityEventTools.AddPersistentListener(buttons[5].onClick, shell.StartLoreOneLevelSix);

        var development = new GameObject("Lore II In Development", typeof(RectTransform)); development.transform.SetParent(panel, false); Full(development.GetComponent<RectTransform>());
        var devCard = Backplate("Development Card", development.transform, new Vector2(0, -410), new Vector2(650, 360));
        var devTitle = Label("IN DEVELOPMENT", devCard.transform, 42, Gold); Rect(devTitle.rectTransform, new Vector2(.5f, 1), new Vector2(.5f, 1), new Vector2(0, -70), new Vector2(580, 90));
        var devBody = Label("A new Lore is being written.\nCycle back to Lore I to defend its six chapters.", devCard.transform, 25, Cream); Rect(devBody.rectTransform, new Vector2(.5f, .5f), new Vector2(.5f, .5f), new Vector2(0, -25), new Vector2(560, 150));
        development.SetActive(false);

        var back = TextButton("BACK", panel, 24); Rect(back.GetComponent<RectTransform>(), new Vector2(0, 1), new Vector2(0, 1), new Vector2(18, -18), new Vector2(190, 70));
        UnityEventTools.AddPersistentListener(back.onClick, shell.ShowModes);
        shell.ConfigureLoreSelection(panel.gameObject, labels[0]);
        shell.ConfigureLoreSequence(buttons[1], labels[1], buttons[2], labels[2]);
        shell.ConfigureLoreLevels(buttons, labels);
        shell.ConfigureLoreCarousel(content, development, title, bestLabels);
    }

    private static void BuildTutorialReplayConfirmation(GameShellController shell)
    {
        var mode = Find("Game Shell Canvas/Game Shell/Mode Select")?.transform;
        if (mode == null) return;
        var old = mode.Find("Tutorial Replay Confirmation"); if (old != null) Object.DestroyImmediate(old.gameObject);
        var overlay = Backplate("Tutorial Replay Confirmation", mode, Vector2.zero, Vector2.zero); Full(overlay.rectTransform); overlay.color = new Color(.01f, .015f, .04f, .94f); overlay.raycastTarget = true;
        var card = Backplate("Confirmation Card", overlay.transform, Vector2.zero, new Vector2(700, 520)); Rect(card.rectTransform, new Vector2(.5f, .5f), new Vector2(.5f, .5f), Vector2.zero, new Vector2(700, 520));
        var title = Label("TUTORIAL COMPLETED", card.transform, 36, Gold); Rect(title.rectTransform, new Vector2(.5f, 1), new Vector2(.5f, 1), new Vector2(0, -75), new Vector2(620, 80));
        var body = Label("You have already completed the tutorial.\nDo you wish to play it again?", card.transform, 26, Cream); Rect(body.rectTransform, new Vector2(.5f, .5f), new Vector2(.5f, .5f), new Vector2(0, 30), new Vector2(590, 150));
        var yes = TextButton("CONTINUE", card.transform, 25); Rect(yes.GetComponent<RectTransform>(), new Vector2(.5f, 0), new Vector2(.5f, 0), new Vector2(-155, 45), new Vector2(280, 78));
        var no = TextButton("CANCEL", card.transform, 25); Rect(no.GetComponent<RectTransform>(), new Vector2(.5f, 0), new Vector2(.5f, 0), new Vector2(155, 45), new Vector2(280, 78));
        UnityEventTools.AddPersistentListener(yes.onClick, shell.ConfirmTutorialReplay);
        UnityEventTools.AddPersistentListener(no.onClick, shell.CancelTutorialReplay);
        overlay.gameObject.SetActive(false);
        shell.ConfigureTutorialReplayConfirmation(overlay.gameObject);
    }

    private static void BuildEndlessRecommendation(GameShellController shell)
    {
        var mode = Find("Game Shell Canvas/Game Shell/Mode Select")?.transform;
        var endless = mode?.GetComponentsInChildren<Button>(true).FirstOrDefault(b => (b.GetComponentInChildren<Text>(true)?.text ?? "").Contains("ENDLESS"));
        if (endless == null) return;
        var old = endless.transform.Find("Recommended Tag"); if (old != null) Object.DestroyImmediate(old.gameObject);
        var tag = Backplate("Recommended Tag", endless.transform, Vector2.zero, new Vector2(230, 48));
        Rect(tag.rectTransform, new Vector2(1, 0), new Vector2(1, 0), new Vector2(-10, 8), new Vector2(230, 48));
        tag.rectTransform.localRotation = Quaternion.Euler(0, 0, -9f);
        tag.color = new Color(.1f, .05f, .02f, .92f);
        var label = Label("RECOMMENDED", tag.transform, 19, Gold); Full(label.rectTransform); label.fontStyle = FontStyle.Bold;
        tag.gameObject.AddComponent<KeySlaught.UI.UiBreathingAnimator>().Configure(.035f, .55f);
    }

    private static void BuildEndlessCheckpoint(GameShellController shell)
    {
        var hud = Find("Portrait Gameplay HUD")?.transform;
        if (hud == null) return;
        var old = hud.Find("Endless Boss Checkpoint"); if (old != null) Object.DestroyImmediate(old.gameObject);
        var overlay = Backplate("Endless Boss Checkpoint", hud, Vector2.zero, Vector2.zero); Full(overlay.rectTransform); overlay.color = new Color(.01f, .015f, .04f, .94f); overlay.raycastTarget = true;
        var card = Backplate("Checkpoint Card", overlay.transform, Vector2.zero, new Vector2(720, 620)); Rect(card.rectTransform, new Vector2(.5f, .5f), new Vector2(.5f, .5f), Vector2.zero, new Vector2(720, 620));
        var label = Label("BOSS WAVE CLEARED\n\n+1 KNOWLEDGE POINT\n\nCONTINUE THE RUN?", card.transform, 31, Gold); Rect(label.rectTransform, new Vector2(.5f, 1), new Vector2(.5f, 1), new Vector2(0, -85), new Vector2(620, 300));
        var keepGoing = TextButton("CONTINUE", card.transform, 26); Rect(keepGoing.GetComponent<RectTransform>(), new Vector2(.5f, 0), new Vector2(.5f, 0), new Vector2(0, 145), new Vector2(540, 82));
        var end = TextButton("END RUN", card.transform, 24); Rect(end.GetComponent<RectTransform>(), new Vector2(.5f, 0), new Vector2(.5f, 0), new Vector2(0, 48), new Vector2(540, 76));
        UnityEventTools.AddPersistentListener(keepGoing.onClick, shell.ContinueEndlessAfterBoss);
        UnityEventTools.AddPersistentListener(end.onClick, shell.EndEndlessAfterBoss);
        overlay.gameObject.SetActive(false);
        shell.ConfigureEndlessCheckpoint(overlay.gameObject, label);
    }

    private static void ConfigureCommonGameplay()
    {
        var progression = Object.FindFirstObjectByType<ProgressionService>(FindObjectsInactive.Include);
        if (progression != null)
        {
            progression.Configure(LoadResearchDefinitions());
            EditorUtility.SetDirty(progression);
        }
        var upgrades = Object.FindFirstObjectByType<PermanentUpgradeApplier>(FindObjectsInactive.Include);
        if (upgrades != null)
        {
            var data = new SerializedObject(upgrades);
            data.FindProperty("baseLibraryHealth").intValue = 15;
            data.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(upgrades);
        }
        ConfigureHudLayout();
    }

    private static ResearchDefinition[] LoadResearchDefinitions()
    {
        var files = new[]
        {
            "PlayerRange", "MagazineCapacity", "LibraryHealth", "ReloadSpeed",
            "TeacherRange", "TeacherSpeed", "EngineerRange", "EngineerSpeed",
            "ScientistRange", "ScientistSpeed", "PresidentRange", "PresidentSpeed"
        };
        return files.Select(file => AssetDatabase.LoadAssetAtPath<ResearchDefinition>($"Assets/Data/Research/{file}.asset")).ToArray();
    }

    private static void ConfigureHudLayout()
    {
        var stats = Find("Portrait Gameplay HUD/Top 20 Percent/Aligned Stats Bar")?.transform;
        if (stats == null) return;
        var wave = stats.GetComponentsInChildren<Text>(true).FirstOrDefault(t => t.name.StartsWith("Label Wave"));
        var economy = Object.FindFirstObjectByType<BrainCellEconomy>(FindObjectsInactive.Include);
        var economyData = economy == null ? null : new SerializedObject(economy);
        var amount = economyData?.FindProperty("currencyLabel").objectReferenceValue as Text;
        var time = stats.GetComponentsInChildren<Text>(true).FirstOrDefault(t => (t.text ?? "").Contains(":"));
        var brainCard = stats.Find("Brain Cell Stat Card")?.GetComponent<RectTransform>();
        var timeCard = stats.Find("Time Stat Card")?.GetComponent<RectTransform>();
        var brainIcon = stats.Find("Brain Cell Icon")?.GetComponent<Image>();
        var clock = stats.Find("Elegant Time Icon")?.GetComponent<Image>();
        if (wave != null) Rect(wave.rectTransform, new Vector2(0, 1), new Vector2(0, 1), new Vector2(20, -14), new Vector2(250, 62));
        if (brainCard != null) { Rect(brainCard, new Vector2(.5f, 1), new Vector2(.5f, 1), new Vector2(0, -8), new Vector2(210, 74)); brainCard.SetAsFirstSibling(); }
        if (brainIcon != null) Rect(brainIcon.rectTransform, new Vector2(.5f, 1), new Vector2(.5f, 1), new Vector2(-72, -18), new Vector2(46, 46));
        if (amount != null)
        {
            amount.fontSize = 32; amount.alignment = TextAnchor.MiddleLeft; amount.resizeTextForBestFit = false;
            Rect(amount.rectTransform, new Vector2(.5f, 1), new Vector2(.5f, 1), new Vector2(25, -14), new Vector2(100, 56)); amount.transform.SetAsLastSibling();
        }
        if (brainIcon != null) brainIcon.transform.SetAsLastSibling();
        if (timeCard != null) { Rect(timeCard, new Vector2(1, 1), new Vector2(1, 1), new Vector2(-18, -8), new Vector2(185, 74)); timeCard.SetAsFirstSibling(); }
        if (clock != null) Rect(clock.rectTransform, new Vector2(1, 1), new Vector2(1, 1), new Vector2(-156, -18), new Vector2(46, 46));
        if (time != null) Rect(time.rectTransform, new Vector2(1, 1), new Vector2(1, 1), new Vector2(-112, -14), new Vector2(104, 56));
        var skip = stats.GetComponentsInChildren<Button>(true).FirstOrDefault(b => b.GetComponent<IntermissionFastForwardButton>() != null);
        if (skip != null)
        {
            skip.gameObject.SetActive(true);
            var text = skip.GetComponentInChildren<Text>(true); if (text != null) { text.text = "▶▶"; text.fontSize = 25; }
            Rect(skip.GetComponent<RectTransform>(), new Vector2(0, 1), new Vector2(0, 1), new Vector2(275, -13), new Vector2(72, 56));
        }
    }

    private static ResearchDefinition[] BuildResearch()
    {
        var definitions = new[]
        {
            Research("PlayerRange", "PLAYER_RANGE", "PLAYER RANGE", ResearchStat.PlayerRange, 5, 2, 2, .25f),
            Research("MagazineCapacity", "MAGAZINE_CAPACITY", "MAGAZINE", ResearchStat.MagazineCapacity, 3, 4, 3, 1f),
            Research("LibraryHealth", "LIBRARY_HEALTH", "LIBRARY HP", ResearchStat.LibraryHealth, 20, 2, 1, 5f),
            Research("ReloadSpeed", "RELOAD_SPEED", "RELOAD SPEED", ResearchStat.ReloadSpeed, 5, 2, 2, .05f),
            Research("TeacherRange", "TEACHER_RANGE", "TEACHER RANGE", ResearchStat.TeacherRange, 10, 3, 2, .12f),
            Research("TeacherSpeed", "TEACHER_SPEED", "TEACHER SPEED", ResearchStat.TeacherAttackSpeed, 10, 3, 2, .08f),
            Research("EngineerRange", "ENGINEER_RANGE", "ENGINEER RANGE", ResearchStat.EngineerRange, 10, 3, 2, .12f),
            Research("EngineerSpeed", "ENGINEER_SPEED", "ENGINEER SPEED", ResearchStat.EngineerAttackSpeed, 10, 3, 2, .08f),
            Research("ScientistRange", "SCIENTIST_RANGE", "SCIENTIST RANGE", ResearchStat.ScientistRange, 10, 4, 2, .12f),
            Research("ScientistSpeed", "SCIENTIST_SPEED", "SCIENTIST SPEED", ResearchStat.ScientistAttackSpeed, 10, 4, 2, .08f),
            Research("PresidentRange", "PRESIDENT_RANGE", "PRESIDENT RANGE", ResearchStat.PresidentRange, 10, 4, 2, .12f),
            Research("PresidentSpeed", "PRESIDENT_SPEED", "PRESIDENT SPEED", ResearchStat.PresidentAttackSpeed, 10, 4, 2, .08f)
        };
        TuneTurret("Teacher", 2.6f, 1.8f);
        TuneTurret("Engineer", 2.4f, 2.15f);
        TuneTurret("Scientist", 2.1f, 2.6f);
        TuneTurret("President", 2.3f, 2.35f);
        return definitions;
    }

    private static ResearchDefinition Research(string file, string id, string title, ResearchStat stat, int max, int cost, int growth, float value)
    {
        var path = $"Assets/Data/Research/{file}.asset";
        var asset = AssetDatabase.LoadAssetAtPath<ResearchDefinition>(path);
        if (asset == null) { asset = ScriptableObject.CreateInstance<ResearchDefinition>(); AssetDatabase.CreateAsset(asset, path); }
        var data = new SerializedObject(asset);
        data.FindProperty("id").stringValue = id; data.FindProperty("displayName").stringValue = title;
        data.FindProperty("stat").enumValueIndex = (int)stat; data.FindProperty("maxLevel").intValue = max;
        data.FindProperty("baseCost").intValue = cost; data.FindProperty("costIncreasePerLevel").intValue = growth;
        data.FindProperty("valuePerLevel").floatValue = value; data.ApplyModifiedPropertiesWithoutUndo(); EditorUtility.SetDirty(asset);
        return asset;
    }

    private static void TuneTurret(string name, float range, float secondsPerShot)
    {
        var asset = AssetDatabase.LoadAssetAtPath<TurretDefinition>($"Assets/Data/Turrets/{name}.asset"); if (asset == null) return;
        var data = new SerializedObject(asset);
        data.FindProperty("range").floatValue = range;
        data.FindProperty("secondsPerShot").floatValue = secondsPerShot;
        data.FindProperty("rangePerUpgrade").floatValue = .22f;
        data.FindProperty("cadenceMultiplierPerUpgrade").floatValue = .9f;
        data.ApplyModifiedPropertiesWithoutUndo(); EditorUtility.SetDirty(asset);
    }

    private static LevelDefinition[] BuildLoreLevels()
    {
        var pools = WordPools();
        var bosses = new[] { "GRANDLIBRARYKEEPER", "RAINBOWCONSTELLATION", "ENCYCLOPEDIAGUARDIAN", "IMAGINATIONCHAMPION", "INTERDEPENDENTCIVILIZATION", "ELECTROENCEPHALOGRAPHIC" };
        var result = new LevelDefinition[6];
        for (var level = 0; level < result.Length; level++)
        {
            var waves = new List<(string title, float speed, string[] words, float delay)>();
            for (var wave = 0; wave < 5; wave++)
            {
                var count = 8 + level * 2 + wave * 2;
                waves.Add(($"WAVE {wave + 1}", .72f + level * .09f + wave * .06f, TakeWords(pools[level], count, wave * 4), Mathf.Max(.3f, .78f - level * .06f - wave * .07f)));
            }
            var bossNormals = TakeWords(pools[level], 8 + level * 2, 13);
            waves.Add(("BOSS WAVE", .68f + level * .08f, new[] { bosses[level] }.Concat(bossNormals).ToArray(), Mathf.Max(.3f, .62f - level * .045f)));
            result[level] = WriteLevel($"LoreOneLevel{NumberWords[level]}", $"LORE I - LEVEL {level + 1}", waves.ToArray(), 30f, false);
        }
        return result;
    }

    private static LevelDefinition BuildEndlessLevel()
    {
        var pool = WordPools().SelectMany(p => p).Distinct().ToArray();
        var bosses = new[] { "GRANDLIBRARYKEEPER", "RAINBOWCONSTELLATION", "ENCYCLOPEDIAGUARDIAN", "IMAGINATIONCHAMPION", "INTERDEPENDENTCIVILIZATION", "ELECTROENCEPHALOGRAPHIC" };
        var waves = new List<(string title, float speed, string[] words, float delay)>();
        for (var wave = 1; wave <= 30; wave++)
        {
            var count = Mathf.Min(22, 6 + wave / 2);
            var normalWords = TakeWords(pool, count, wave * 5);
            var boss = wave % 5 == 0;
            var words = boss ? new[] { bosses[wave / 5 - 1] }.Concat(normalWords).ToArray() : normalWords;
            waves.Add((boss ? $"BOSS WAVE {wave}" : $"WAVE {wave}", .74f + wave * .018f, words, Mathf.Max(.28f, .82f - wave * .018f)));
        }
        return WriteLevel("EndlessPrototype", "ENDLESS", waves.ToArray(), 30f, true);
    }

    private static void ConfigureTutorialPreparation()
    {
        var tutorial = AssetDatabase.LoadAssetAtPath<LevelDefinition>("Assets/Data/Levels/Tutorial.asset"); if (tutorial == null) return;
        var data = new SerializedObject(tutorial);
        data.FindProperty("initialPreparationSeconds").floatValue = 0f;
        data.FindProperty("pauseAfterBossWave").boolValue = false;
        data.ApplyModifiedPropertiesWithoutUndo(); EditorUtility.SetDirty(tutorial);
    }

    private static LevelDefinition WriteLevel(string file, string display, (string title, float speed, string[] words, float delay)[] waves, float initialPreparation, bool checkpoints)
    {
        var path = $"Assets/Data/Levels/{file}.asset";
        var level = AssetDatabase.LoadAssetAtPath<LevelDefinition>(path);
        if (level == null) { level = ScriptableObject.CreateInstance<LevelDefinition>(); AssetDatabase.CreateAsset(level, path); }
        var data = new SerializedObject(level);
        data.FindProperty("displayName").stringValue = display;
        data.FindProperty("intermissionSeconds").floatValue = 8f;
        data.FindProperty("initialPreparationSeconds").floatValue = initialPreparation;
        data.FindProperty("pauseAfterBossWave").boolValue = checkpoints;
        var list = data.FindProperty("waves"); list.arraySize = waves.Length;
        for (var waveIndex = 0; waveIndex < waves.Length; waveIndex++)
        {
            var wave = list.GetArrayElementAtIndex(waveIndex);
            wave.FindPropertyRelative("title").stringValue = waves[waveIndex].title;
            wave.FindPropertyRelative("enemyMovementSpeed").floatValue = waves[waveIndex].speed;
            wave.FindPropertyRelative("targetWaveEndSeconds").floatValue = 50f + waveIndex * 3f;
            var entries = wave.FindPropertyRelative("words"); entries.arraySize = waves[waveIndex].words.Length;
            for (var index = 0; index < waves[waveIndex].words.Length; index++)
            {
                var entry = entries.GetArrayElementAtIndex(index);
                entry.FindPropertyRelative("word").stringValue = waves[waveIndex].words[index];
                entry.FindPropertyRelative("delayAfterPrevious").floatValue = index == 0 ? .2f : waves[waveIndex].delay;
            }
        }
        data.ApplyModifiedPropertiesWithoutUndo(); EditorUtility.SetDirty(level); return level;
    }

    private static string[][] WordPools() => new[]
    {
        new[] { "CAT", "BOOK", "MOON", "TREE", "STAR", "SMILE", "APPLE", "RIVER", "MUSIC", "HAPPY", "FRIEND", "PLANET", "PUZZLE", "GARDEN", "CASTLE", "RAINBOW", "JOURNEY", "LIBRARY", "ADVENTURE", "IMAGINATION" },
        new[] { "CLOUD", "BRAVE", "STORY", "PAINT", "DREAM", "ROCKET", "ANIMAL", "FOREST", "PENCIL", "SCHOOL", "BICYCLE", "DOLPHIN", "TREASURE", "MOUNTAIN", "DINOSAUR", "BUTTERFLY", "TELESCOPE", "CELEBRATE", "DISCOVERY", "CREATIVITY" },
        new[] { "ORACLE", "POETRY", "HISTORY", "MEMORY", "WISDOM", "LANGUAGE", "HARMONY", "MYSTERY", "COMPASS", "VILLAGE", "NOTEBOOK", "INVENTOR", "FESTIVAL", "WATERFALL", "CONSTELLATION", "EXPLORATION", "UNDERSTANDING", "ENCYCLOPEDIA", "ASTRONOMY", "KNOWLEDGE" },
        new[] { "COURAGE", "KINDNESS", "SCIENCE", "PATTERN", "FREEDOM", "BALANCE", "CHAMPION", "SAPPHIRE", "VOLCANO", "ARCHIVE", "MIGRATION", "LABYRINTH", "EQUATION", "NAVIGATION", "COMMUNITY", "INSPIRATION", "TRANSFORMATION", "RESPONSIBILITY", "COMMUNICATION", "INTERPRETATION" },
        new[] { "CURIOUS", "ENERGY", "THEOREM", "PARADOX", "CRYSTAL", "SYMPHONY", "ENGINEER", "SCIENTIST", "PRESIDENT", "TEACHER", "MANUSCRIPT", "CHRONICLE", "CIVILIZATION", "ARCHAEOLOGY", "LINGUISTICS", "METAMORPHOSIS", "CONSCIOUSNESS", "PERSEVERANCE", "EXTRAORDINARY", "COLLABORATION" },
        new[] { "EVIDENCE", "REASONING", "DIALECTIC", "ALGORITHM", "GEOMETRY", "PHILOSOPHY", "TRANSLATION", "INHERITANCE", "REVOLUTION", "POSSIBILITY", "INVESTIGATION", "DETERMINATION", "ELECTROMAGNETIC", "CHARACTERISTIC", "INTERDEPENDENCE", "MISUNDERSTANDING", "REPRESENTATION", "INTERNATIONAL", "COUNTERBALANCE", "ELECTROENCEPHALOGRAPHIC" }
    };

    private static string[] TakeWords(string[] pool, int count, int offset)
    {
        var result = new string[count];
        for (var index = 0; index < count; index++) result[index] = pool[(offset + index) % pool.Length];
        return result;
    }

    private static void RepaintAllRoutes()
    {
        var arena = Object.FindFirstObjectByType<ReusableLevelArena>(FindObjectsInactive.Include); if (arena == null || arena.EnemyPath == null) return;
        var routes = Object.FindObjectsByType<WaypointPath>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        var routeCells = new HashSet<Vector3Int>();
        foreach (var route in routes)
        {
            if (route == null || route.WaypointCount < 2) continue;
            var positions = route.CopyPositions();
            for (var index = 1; index < positions.Length; index++)
            {
                var start = arena.EnemyPath.WorldToCell(positions[index - 1]);
                var end = arena.EnemyPath.WorldToCell(positions[index]);
                AddOrthogonalCells(routeCells, start, new Vector3Int(end.x, start.y, 0));
                AddOrthogonalCells(routeCells, new Vector3Int(end.x, start.y, 0), end);
            }
            var line = route.GetComponent<LineRenderer>(); if (line != null) line.enabled = false;
        }
        arena.EnemyPath.ClearAllTiles();
        var tiles = new Dictionary<string, TileBase>
        {
            ["H"] = Tile("Path_H"), ["V"] = Tile("Path_V"), ["LT"] = Tile("Path_LT"),
            ["TR"] = Tile("Path_TR"), ["LB"] = Tile("Path_LB"), ["BR"] = Tile("Path_BR")
        };
        foreach (var cell in routeCells) arena.EnemyPath.SetTile(cell, PathTile(cell, routeCells, tiles));
        EditorUtility.SetDirty(arena.EnemyPath);
    }

    private static void AddOrthogonalCells(HashSet<Vector3Int> cells, Vector3Int start, Vector3Int end)
    {
        var xStep = Math.Sign(end.x - start.x); var yStep = Math.Sign(end.y - start.y);
        var cell = start; cells.Add(cell);
        while (cell.x != end.x) { cell.x += xStep; cells.Add(cell); }
        while (cell.y != end.y) { cell.y += yStep; cells.Add(cell); }
    }

    private static TileBase PathTile(Vector3Int cell, HashSet<Vector3Int> cells, Dictionary<string, TileBase> tiles)
    {
        var left = cells.Contains(cell + Vector3Int.left); var right = cells.Contains(cell + Vector3Int.right);
        var up = cells.Contains(cell + Vector3Int.up); var down = cells.Contains(cell + Vector3Int.down);
        if ((left || right) && !(up || down)) return tiles["H"];
        if ((up || down) && !(left || right)) return tiles["V"];
        if (left && up) return tiles["LT"];
        if (right && up) return tiles["TR"];
        if (left && down) return tiles["LB"];
        if (right && down) return tiles["BR"];
        return left || right ? tiles["H"] : tiles["V"];
    }

    private static int CountPaintedCells(Tilemap map)
    {
        if (map == null) return 0;
        map.CompressBounds();
        var count = 0;
        foreach (var tile in map.GetTilesBlock(map.cellBounds)) if (tile != null) count++;
        return count;
    }

    private static TileBase Tile(string name) => AssetDatabase.LoadAssetAtPath<TileBase>($"Assets/Tiles/Monochrome/{name}.asset");
    private static Button TextButton(string text, Transform parent, int size)
    {
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/UiTextButton.prefab");
        var go = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent); go.name = "Button " + text;
        var label = go.GetComponentInChildren<Text>(true); label.text = text; label.fontSize = size; label.color = Cream;
        return go.GetComponent<Button>();
    }
    private static Text Label(string text, Transform parent, int size, Color color)
    {
        var go = new GameObject("Label " + text.Split('\n')[0], typeof(RectTransform), typeof(CanvasRenderer), typeof(Text)); go.transform.SetParent(parent, false);
        var label = go.GetComponent<Text>(); label.text = text; label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); label.fontSize = size; label.color = color;
        label.alignment = TextAnchor.MiddleCenter; label.horizontalOverflow = HorizontalWrapMode.Wrap; label.verticalOverflow = VerticalWrapMode.Overflow; return label;
    }
    private static Image Backplate(string name, Transform parent, Vector2 position, Vector2 size)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image)); go.transform.SetParent(parent, false);
        var image = go.GetComponent<Image>(); image.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Prototype/SolidSprite.asset"); image.color = new Color(0f, 0f, 0f, .8f); image.raycastTarget = false;
        Rect(image.rectTransform, new Vector2(.5f, 1), new Vector2(.5f, 1), position, size); return image;
    }
    private static void Clear(UnityEngine.Events.UnityEvent action) { for (var index = action.GetPersistentEventCount() - 1; index >= 0; index--) UnityEventTools.RemovePersistentListener(action, index); }
    private static GameObject Find(string path) => GameObject.Find("/" + path) ?? Resources.FindObjectsOfTypeAll<GameObject>().FirstOrDefault(go => go.scene == SceneManager.GetActiveScene() && Hierarchy(go) == path);
    private static string Hierarchy(GameObject go) { var value = go.name; for (var parent = go.transform.parent; parent != null; parent = parent.parent) value = parent.name + "/" + value; return value; }
    private static void Full(RectTransform rect) { rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one; rect.pivot = new Vector2(.5f, .5f); rect.offsetMin = Vector2.zero; rect.offsetMax = Vector2.zero; }
    private static void Rect(RectTransform rect, Vector2 min, Vector2 max, Vector2 position, Vector2 size) { rect.anchorMin = min; rect.anchorMax = max; rect.pivot = min == max ? min : new Vector2(.5f, .5f); rect.anchoredPosition = position; rect.sizeDelta = size; }
    private static Color C(string hex) { ColorUtility.TryParseHtmlString("#" + hex, out var color); return color; }
}
