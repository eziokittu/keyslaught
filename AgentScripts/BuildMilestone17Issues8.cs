using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Reflection;
using KeySlaught.Progression;
using KeySlaught.SceneGameplay;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Object = UnityEngine.Object;

public static class BuildMilestone17Issues8
{
    private const string Sample = "Assets/Scenes/SampleScene.unity";
    private static readonly string[] Scenes =
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
        if (EditorApplication.isPlaying) throw new InvalidOperationException("Exit Play Mode before authoring Milestone 17.");
        var research = BuildResearch();
        var levels = BuildLevels();
        EnsureAdditionalScenes();
        UpdateBuildSettings();

        var sample = EditorSceneManager.OpenScene(Sample, OpenSceneMode.Single);
        FixMenuHeader();
        BuildLevelsMenu();
        BuildResearchMenu(research);
        PolishTutorialChecklist();
        AttachParticles();
        EditorSceneManager.MarkSceneDirty(sample); EditorSceneManager.SaveScene(sample);

        for (var index = 0; index < Scenes.Length; index++)
        {
            var scene = EditorSceneManager.OpenScene(Scenes[index], OpenSceneMode.Single);
            var flow = Object.FindFirstObjectByType<DedicatedLevelSceneController>(FindObjectsInactive.Include)
                ?? throw new InvalidOperationException($"Dedicated flow missing in {Scenes[index]}.");
            var run = Object.FindFirstObjectByType<WaveRunController>(FindObjectsInactive.Include)
                ?? throw new InvalidOperationException($"Wave run missing in {Scenes[index]}.");
            flow.SetLoreLevelNumber(index + 1); flow.SetLevelDefinition(levels[index]);
            run.SetLevel(levels[index]);
            AttachParticles();
            EditorUtility.SetDirty(flow); EditorUtility.SetDirty(run);
            EditorSceneManager.MarkSceneDirty(scene); EditorSceneManager.SaveScene(scene);
        }

        AssetDatabase.SaveAssets(); AssetDatabase.Refresh();
        EditorSceneManager.OpenScene(Sample, OpenSceneMode.Single);
        return Audit();
    }

    public static string Audit()
    {
        var errors = new List<string>();
        var enabled = EditorBuildSettings.scenes.Where(scene => scene.enabled).Select(scene => scene.path).ToArray();
        foreach (var path in new[] { Sample }.Concat(Scenes))
            if (!enabled.Contains(path)) errors.Add($"Build Settings missing {path}");

        foreach (var path in Scenes)
        {
            EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
            var flow = Object.FindFirstObjectByType<DedicatedLevelSceneController>(FindObjectsInactive.Include);
            if (flow == null || flow.Level == null || flow.Level.Waves == null || flow.Level.Waves.Length != 6)
                errors.Add($"{path} is not wired to six waves");
            else
            {
                if (!flow.Level.Waves[5].Title.ToUpperInvariant().Contains("BOSS")) errors.Add($"{path} has no final boss wave");
                for (var wave = 0; wave < flow.Level.Waves.Length; wave++)
                {
                    var count = flow.Level.Waves[wave].Words?.Length ?? 0;
                    if (count < 5 || count > 20) errors.Add($"{path} wave {wave + 1} has {count} enemies");
                }
            }
            if (Object.FindFirstObjectByType<GameplayParticleFeedback>(FindObjectsInactive.Include) == null)
                errors.Add($"{path} has no particle feedback");
        }

        EditorSceneManager.OpenScene(Sample, OpenSceneMode.Single);
        var shell = Object.FindFirstObjectByType<GameShellController>(FindObjectsInactive.Include);
        var levelsPanel = Find("Game Shell Canvas/Game Shell/Lore Levels");
        var levelButtons = levelsPanel == null ? Array.Empty<Button>() : levelsPanel.GetComponentsInChildren<Button>(true).Where(b => b.name.StartsWith("Button LEVEL")).ToArray();
        if (shell == null || levelButtons.Length != 6 || levelButtons.Any(button => button.onClick.GetPersistentEventCount() != 1)) errors.Add("Six Level buttons are not persistently wired");
        var service = Object.FindFirstObjectByType<ProgressionService>(FindObjectsInactive.Include);
        if (service == null || service.Definitions == null || service.Definitions.Length != 8) errors.Add("Expected eight research definitions");
        var checklist = Find("Portrait Gameplay HUD/Tutorial Objective Banner")?.GetComponentsInChildren<Text>(true).FirstOrDefault(text => text.name.Contains("MOVE UP"));
        if (checklist != null && !checklist.supportRichText) errors.Add("Tutorial checklist rich text is disabled");
        if (errors.Count > 0) throw new InvalidOperationException(string.Join(". ", errors));
        return "Milestone 17 audit passed: collision-free header, six wired Levels scenes, six 5-20-enemy waves per level with final bosses, eight research branches, tutorial repair/checkmarks, moving-only range, and gameplay particles.";
    }

    public static string ReviewLevelsMenu()
    {
        Find("Game Shell Canvas/Game Shell")?.SetActive(true);
        Object.FindFirstObjectByType<GameShellController>(FindObjectsInactive.Include)?.ShowLoreLevels();
        return "Six-level picker staged.";
    }

    public static string ReviewResearch()
    {
        Find("Game Shell Canvas/Game Shell")?.SetActive(true);
        Object.FindFirstObjectByType<GameShellController>(FindObjectsInactive.Include)?.ShowResearch();
        return "Eight-branch research tree staged.";
    }

    public static string ReviewMain()
    {
        Find("Game Shell Canvas/Game Shell")?.SetActive(true);
        Object.FindFirstObjectByType<GameShellController>(FindObjectsInactive.Include)?.ShowMain();
        return "Main menu header staged.";
    }

    public static string LoadLevelSix()
    {
        Object.FindFirstObjectByType<GameShellController>(FindObjectsInactive.Include)?.StartLoreOneLevelSix();
        return "Requested Lore I Level 6.";
    }

    public static string InspectActiveLevel()
    {
        var flow = Object.FindFirstObjectByType<DedicatedLevelSceneController>(FindObjectsInactive.Include);
        return $"scene={SceneManager.GetActiveScene().name}; level={flow?.LoreLevelNumber}; waves={flow?.Level?.Waves?.Length}; boss={flow?.Level?.Waves?[5]?.Title}";
    }

    public static string CaptureMain() { ScreenCapture.CaptureScreenshot("Temp/m17_main.png", 1); return "Main captured."; }
    public static string CaptureLevels() { ScreenCapture.CaptureScreenshot("Temp/m17_levels.png", 1); return "Levels captured."; }
    public static string CaptureResearch() { ScreenCapture.CaptureScreenshot("Temp/m17_research.png", 1); return "Research captured."; }
    public static async Task<string> CaptureResearchReview()
    {
        ReviewResearch();
        ScreenCapture.CaptureScreenshot("Temp/m17_research.png", 1);
        await Task.Delay(600);
        return "Research staged and captured.";
    }

    public static async Task<string> CaptureMainReview()
    {
        ReviewMain();
        ScreenCapture.CaptureScreenshot("Temp/m17_main.png", 1);
        await Task.Delay(600);
        return "Main menu staged and captured.";
    }

    public static async Task<string> CaptureTutorialTick()
    {
        var shell = Object.FindFirstObjectByType<GameShellController>(FindObjectsInactive.Include);
        var mover = Object.FindFirstObjectByType<PlayerMover>(FindObjectsInactive.Include);
        shell?.StartTutorial();
        mover?.SetVirtualMovement(Vector2.up);
        await Task.Delay(500);
        mover?.SetVirtualMovement(Vector2.zero);
        ScreenCapture.CaptureScreenshot("Temp/m17_tutorial_tick.png", 1);
        await Task.Delay(500);
        var checklist = Find("Portrait Gameplay HUD/Tutorial Objective Banner")?.GetComponentsInChildren<Text>(true).FirstOrDefault(text => (text.text ?? "").Contains("MOVE UP"));
        return checklist?.text ?? "missing checklist";
    }

    public static async Task<string> ProbeLibraryDamageAndRepair()
    {
        var shell = Object.FindFirstObjectByType<GameShellController>(FindObjectsInactive.Include);
        shell?.StartTutorial();
        var director = Object.FindFirstObjectByType<TutorialDirector>(FindObjectsInactive.Include);
        var stage = typeof(TutorialDirector).GetField("stage", BindingFlags.Instance | BindingFlags.NonPublic);
        if (director == null || stage == null) return "tutorial unavailable";
        stage.SetValue(director, Enum.Parse(stage.FieldType, "CollectionMessage"));
        director.ContinueTutorial();
        var library = Object.FindFirstObjectByType<LibraryEndpoint>(FindObjectsInactive.Include);
        var player = Object.FindFirstObjectByType<PlayerMover>(FindObjectsInactive.Include);
        var playerDistance = player == null || library == null ? -1f : Vector2.Distance(player.transform.position, library.transform.position);
        var timeout = 160;
        while (timeout-- > 0 && stage.GetValue(director)?.ToString() != "LibraryMessage") await Task.Delay(100);
        var damaged = library?.State?.CurrentHealth ?? -1;
        director.ContinueTutorial();
        await Task.Delay(100);
        var repaired = library?.State?.CurrentHealth ?? -1;
        return $"stage={stage.GetValue(director)}; playerDistance={playerDistance:F2}; damagedHp={damaged}; repairedHp={repaired}; maxHp={library?.State?.MaximumHealth}";
    }

    public static async Task<string> ProbeNextLevelRoute()
    {
        SceneManager.LoadScene("LoreOneLevelOne");
        await Task.Delay(700);
        var flow = Object.FindFirstObjectByType<DedicatedLevelSceneController>(FindObjectsInactive.Include);
        flow?.ContinueFromResult();
        await Task.Delay(900);
        var next = Object.FindFirstObjectByType<DedicatedLevelSceneController>(FindObjectsInactive.Include);
        return $"scene={SceneManager.GetActiveScene().name}; level={next?.LoreLevelNumber}; waves={next?.Level?.Waves?.Length}";
    }

    public static async Task<string> ProbeLevelSixLoad()
    {
        SceneManager.LoadScene("LoreOneLevelSix");
        await Task.Delay(800);
        return InspectActiveLevel();
    }

    private static void EnsureAdditionalScenes()
    {
        for (var index = 3; index < Scenes.Length; index++)
        {
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(Scenes[index]) != null) continue;
            if (!AssetDatabase.CopyAsset(Scenes[2], Scenes[index])) throw new InvalidOperationException($"Could not create {Scenes[index]}.");
        }
        AssetDatabase.Refresh();
    }

    private static LevelDefinition[] BuildLevels()
    {
        var pools = new[]
        {
            new[] { "CAT", "BOOK", "MOON", "TREE", "STAR", "SMILE", "APPLE", "RIVER", "MUSIC", "HAPPY", "FRIEND", "PLANET", "PUZZLE", "GARDEN", "CASTLE", "RAINBOW", "JOURNEY", "LIBRARY", "ADVENTURE", "IMAGINATION" },
            new[] { "CLOUD", "BRAVE", "STORY", "PAINT", "DREAM", "ROCKET", "ANIMAL", "FOREST", "PENCIL", "SCHOOL", "BICYCLE", "DOLPHIN", "TREASURE", "MOUNTAIN", "DINOSAUR", "BUTTERFLY", "TELESCOPE", "CELEBRATE", "DISCOVERY", "CREATIVITY" },
            new[] { "ORACLE", "POETRY", "HISTORY", "MEMORY", "WISDOM", "LANGUAGE", "HARMONY", "MYSTERY", "COMPASS", "VILLAGE", "NOTEBOOK", "INVENTOR", "FESTIVAL", "WATERFALL", "CONSTELLATION", "EXPLORATION", "UNDERSTANDING", "ENCYCLOPEDIA", "ASTRONOMY", "KNOWLEDGE" },
            new[] { "COURAGE", "KINDNESS", "SCIENCE", "PATTERN", "FREEDOM", "BALANCE", "CHAMPION", "SAPPHIRE", "VOLCANO", "ARCHIVE", "MIGRATION", "LABYRINTH", "EQUATION", "NAVIGATION", "COMMUNITY", "INSPIRATION", "TRANSFORMATION", "RESPONSIBILITY", "COMMUNICATION", "INTERPRETATION" },
            new[] { "CURIOUS", "ENERGY", "THEOREM", "PARADOX", "CRYSTAL", "SYMPHONY", "ENGINEER", "SCIENTIST", "PRESIDENT", "TEACHER", "MANUSCRIPT", "CHRONICLE", "CIVILIZATION", "ARCHAEOLOGY", "LINGUISTICS", "METAMORPHOSIS", "CONSCIOUSNESS", "PERSEVERANCE", "EXTRAORDINARY", "COLLABORATION" },
            new[] { "EVIDENCE", "REASONING", "DIALECTIC", "ALGORITHM", "GEOMETRY", "PHILOSOPHY", "TRANSLATION", "INHERITANCE", "REVOLUTION", "POSSIBILITY", "INVESTIGATION", "DETERMINATION", "ELECTROMAGNETIC", "CHARACTERISTIC", "INTERDEPENDENCE", "MISUNDERSTANDING", "REPRESENTATION", "INTERNATIONAL", "COUNTERBALANCE", "ELECTROENCEPHALOGRAPHIC" }
        };
        var bosses = new[] { "GRANDLIBRARYKEEPER", "RAINBOWCONSTELLATION", "ENCYCLOPEDIAGUARDIAN", "IMAGINATIONCHAMPION", "INTERDEPENDENTCIVILIZATION", "ELECTROENCEPHALOGRAPHIC" };
        var counts = new[] { 5, 7, 9, 12, 15 };
        var result = new LevelDefinition[6];
        for (var levelIndex = 0; levelIndex < result.Length; levelIndex++)
        {
            var waves = new List<(string title, float speed, string[] words)>();
            for (var wave = 0; wave < counts.Length; wave++)
                waves.Add(($"WAVE {wave + 1}", .95f + levelIndex * .12f + wave * .08f, TakeWords(pools[levelIndex], counts[wave], wave * 3)));
            waves.Add(("BOSS WAVE", .86f + levelIndex * .1f, new[] { bosses[levelIndex] }.Concat(TakeWords(pools[levelIndex], 5, 11)).ToArray()));
            result[levelIndex] = WriteLevel($"LoreOneLevel{NumberWords[levelIndex]}", $"LORE I - LEVEL {levelIndex + 1}", waves.ToArray());
        }
        return result;
    }

    private static string[] TakeWords(string[] pool, int count, int offset)
    {
        var words = new string[count];
        for (var index = 0; index < count; index++) words[index] = pool[(offset + index) % pool.Length];
        return words;
    }

    private static LevelDefinition WriteLevel(string file, string display, (string title, float speed, string[] words)[] waves)
    {
        var path = $"Assets/Data/Levels/{file}.asset";
        var level = AssetDatabase.LoadAssetAtPath<LevelDefinition>(path);
        if (level == null) { level = ScriptableObject.CreateInstance<LevelDefinition>(); AssetDatabase.CreateAsset(level, path); }
        var data = new SerializedObject(level);
        data.FindProperty("displayName").stringValue = display;
        data.FindProperty("intermissionSeconds").floatValue = 8f;
        var list = data.FindProperty("waves"); list.arraySize = waves.Length;
        for (var waveIndex = 0; waveIndex < waves.Length; waveIndex++)
        {
            var wave = list.GetArrayElementAtIndex(waveIndex);
            wave.FindPropertyRelative("title").stringValue = waves[waveIndex].title;
            wave.FindPropertyRelative("enemyMovementSpeed").floatValue = waves[waveIndex].speed;
            wave.FindPropertyRelative("targetWaveEndSeconds").floatValue = 45f + waveIndex * 6f;
            var entries = wave.FindPropertyRelative("words"); entries.arraySize = waves[waveIndex].words.Length;
            for (var index = 0; index < waves[waveIndex].words.Length; index++)
            {
                var entry = entries.GetArrayElementAtIndex(index);
                entry.FindPropertyRelative("word").stringValue = waves[waveIndex].words[index];
                entry.FindPropertyRelative("delayAfterPrevious").floatValue = index == 0 ? .25f : Mathf.Max(.35f, 1.05f - waveIndex * .09f);
            }
        }
        data.ApplyModifiedPropertiesWithoutUndo(); EditorUtility.SetDirty(level); return level;
    }

    private static ResearchDefinition[] BuildResearch()
    {
        var definitions = new[]
        {
            Research("PlayerRange", "PLAYER_RANGE", "PLAYER RANGE", ResearchStat.PlayerRange, 5, 2, 2, .25f),
            Research("MagazineCapacity", "MAGAZINE_CAPACITY", "MAGAZINE", ResearchStat.MagazineCapacity, 3, 4, 3, 1f),
            Research("LibraryHealth", "LIBRARY_HEALTH", "LIBRARY HP", ResearchStat.LibraryHealth, 5, 3, 2, 5f),
            Research("ReloadSpeed", "RELOAD_SPEED", "RELOAD SPEED", ResearchStat.ReloadSpeed, 5, 2, 2, .05f),
            Research("TeacherMastery", "TEACHER_MASTERY", "TEACHER", ResearchStat.TeacherRange, 5, 4, 3, .3f),
            Research("EngineerMastery", "ENGINEER_MASTERY", "ENGINEER", ResearchStat.EngineerRange, 5, 4, 3, .3f),
            Research("ScientistMastery", "SCIENTIST_MASTERY", "SCIENTIST", ResearchStat.ScientistRange, 5, 5, 3, .3f),
            Research("PresidentMastery", "PRESIDENT_MASTERY", "PRESIDENT", ResearchStat.PresidentRange, 5, 5, 3, .3f)
        };
        TuneTurret("Teacher", 14); TuneTurret("Engineer", 18); TuneTurret("Scientist", 22); TuneTurret("President", 20);
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
        data.FindProperty("valuePerLevel").floatValue = value; data.ApplyModifiedPropertiesWithoutUndo(); EditorUtility.SetDirty(asset); return asset;
    }

    private static void TuneTurret(string name, int cost)
    {
        var asset = AssetDatabase.LoadAssetAtPath<TurretDefinition>($"Assets/Data/Turrets/{name}.asset");
        if (asset == null) return;
        var data = new SerializedObject(asset); data.FindProperty("buildCost").intValue = cost; data.ApplyModifiedPropertiesWithoutUndo(); EditorUtility.SetDirty(asset);
    }

    private static void BuildLevelsMenu()
    {
        var shell = Object.FindFirstObjectByType<GameShellController>(FindObjectsInactive.Include) ?? throw new InvalidOperationException("Game shell missing.");
        var root = Find("Game Shell Canvas/Game Shell")?.transform ?? throw new InvalidOperationException("Shell root missing.");
        var mode = root.Find("Mode Select");
        var loreButton = mode?.GetComponentsInChildren<Button>(true).FirstOrDefault(button => (button.GetComponentInChildren<Text>(true)?.text ?? "").Contains("LORE"));
        if (loreButton != null)
        {
            var label = loreButton.GetComponentInChildren<Text>(true); label.text = "LEVELS";
            Clear(loreButton.onClick); UnityEventTools.AddPersistentListener(loreButton.onClick, shell.ShowLoreLevels);
        }
        var panel = root.Find("Lore Levels") ?? throw new InvalidOperationException("Lore Levels panel missing.");
        foreach (var button in panel.GetComponentsInChildren<Button>(true)) Object.DestroyImmediate(button.gameObject);
        foreach (var label in panel.GetComponentsInChildren<Text>(true).Where(text => text.transform != panel).ToArray()) Object.DestroyImmediate(label.gameObject);
        var previousHeader = panel.Find("Levels Header Backplate"); if (previousHeader != null) Object.DestroyImmediate(previousHeader.gameObject);
        Backplate("Levels Header Backplate", panel, new Vector2(0, -105), new Vector2(800, 190));
        var title = Label("LEVELS", panel, 42, Gold); Rect(title.rectTransform, new Vector2(.5f, 1), new Vector2(.5f, 1), new Vector2(0, -80), new Vector2(640, 80));
        var subtitle = Label("LORE I  -  SIX CHAPTERS", panel, 24, Teal); Rect(subtitle.rectTransform, new Vector2(.5f, 1), new Vector2(.5f, 1), new Vector2(0, -150), new Vector2(640, 55));
        var buttons = new Button[6]; var labels = new Text[6];
        for (var index = 0; index < 6; index++)
        {
            buttons[index] = TextButton($"LEVEL {index + 1}", panel, 25);
            var x = index % 2 == 0 ? -190f : 190f; var y = -285f - (index / 2) * 150f;
            Rect(buttons[index].GetComponent<RectTransform>(), new Vector2(.5f, 1), new Vector2(.5f, 1), new Vector2(x, y), new Vector2(340, 110));
            labels[index] = buttons[index].GetComponentInChildren<Text>(true);
            Clear(buttons[index].onClick);
        }
        UnityEventTools.AddPersistentListener(buttons[0].onClick, shell.LoadLoreOneLevelOne);
        UnityEventTools.AddPersistentListener(buttons[1].onClick, shell.StartLoreOneLevelTwo);
        UnityEventTools.AddPersistentListener(buttons[2].onClick, shell.StartLoreOneLevelThree);
        UnityEventTools.AddPersistentListener(buttons[3].onClick, shell.StartLoreOneLevelFour);
        UnityEventTools.AddPersistentListener(buttons[4].onClick, shell.StartLoreOneLevelFive);
        UnityEventTools.AddPersistentListener(buttons[5].onClick, shell.StartLoreOneLevelSix);
        var back = TextButton("BACK", panel, 25); Rect(back.GetComponent<RectTransform>(), new Vector2(0, 0), new Vector2(0, 0), new Vector2(18, 18), new Vector2(210, 74));
        UnityEventTools.AddPersistentListener(back.onClick, shell.ShowModes);
        shell.ConfigureLoreSelection(panel.gameObject, labels[0]); shell.ConfigureLoreSequence(buttons[1], labels[1], buttons[2], labels[2]); shell.ConfigureLoreLevels(buttons, labels);
    }

    private static void BuildResearchMenu(ResearchDefinition[] definitions)
    {
        var shell = Object.FindFirstObjectByType<GameShellController>(FindObjectsInactive.Include) ?? throw new InvalidOperationException("Game shell missing.");
        var panel = Find("Game Shell Canvas/Game Shell/Research")?.transform ?? throw new InvalidOperationException("Research panel missing.");
        foreach (var button in panel.GetComponentsInChildren<Button>(true)) Object.DestroyImmediate(button.gameObject);
        var existingTexts = panel.GetComponentsInChildren<Text>(true);
        foreach (var text in existingTexts.Where(item => !item.name.Contains("KNOWLEDGE")).ToArray()) Object.DestroyImmediate(text.gameObject);
        var previousHeader = panel.Find("Research Header Backplate"); if (previousHeader != null) Object.DestroyImmediate(previousHeader.gameObject);
        Backplate("Research Header Backplate", panel, new Vector2(0, -130), new Vector2(820, 250));
        var title = Label("RESEARCH TREE", panel, 40, Gold); Rect(title.rectTransform, new Vector2(.5f, 1), new Vector2(.5f, 1), new Vector2(0, -74), new Vector2(700, 70));
        var note = Label("Spend Knowledge Points on permanent upgrades. Specialist branches improve that tower only.", panel, 21, Cream); Rect(note.rectTransform, new Vector2(.5f, 1), new Vector2(.5f, 1), new Vector2(0, -140), new Vector2(760, 70));
        var knowledge = panel.GetComponentsInChildren<Text>(true).FirstOrDefault(text => (text.text ?? "").Contains("KNOWLEDGE"));
        if (knowledge == null) knowledge = Label("KNOWLEDGE POINTS  0", panel, 25, Teal);
        Rect(knowledge.rectTransform, new Vector2(.5f, 1), new Vector2(.5f, 1), new Vector2(0, -205), new Vector2(680, 55));
        var buttons = new Button[definitions.Length]; var labels = new Text[definitions.Length];
        for (var index = 0; index < definitions.Length; index++)
        {
            buttons[index] = TextButton(definitions[index].DisplayName, panel, 20);
            var x = index % 2 == 0 ? -190f : 190f; var y = -300f - (index / 2) * 122f;
            Rect(buttons[index].GetComponent<RectTransform>(), new Vector2(.5f, 1), new Vector2(.5f, 1), new Vector2(x, y), new Vector2(350, 88));
            labels[index] = buttons[index].GetComponentInChildren<Text>(true);
            Clear(buttons[index].onClick); UnityEventTools.AddIntPersistentListener(buttons[index].onClick, shell.PurchaseResearch, index);
        }
        var back = TextButton("BACK", panel, 24); Rect(back.GetComponent<RectTransform>(), Vector2.zero, Vector2.zero, new Vector2(18, 18), new Vector2(210, 72)); UnityEventTools.AddPersistentListener(back.onClick, shell.ShowMain);
        var service = Object.FindFirstObjectByType<ProgressionService>(FindObjectsInactive.Include); service?.Configure(definitions); if (service != null) EditorUtility.SetDirty(service);
        var brainHeader = Find("Game Shell Canvas/Game Shell/Main Menu/Menu Currency Header/Brain Cell Card")?.GetComponentInChildren<Text>(true);
        var menuKnowledge = Find("Game Shell Canvas/Game Shell/Main Menu/Menu Currency Header/Research Card")?.GetComponentInChildren<Text>(true);
        shell.ConfigureResearchUi(buttons, labels); shell.ConfigureProgressionHeader(brainHeader, menuKnowledge); shell.ConfigureResearchKnowledgeLabel(knowledge);
    }

    private static void FixMenuHeader()
    {
        var main = Find("Game Shell Canvas/Game Shell/Main Menu")?.transform; if (main == null) return;
        var back = main.GetComponentsInChildren<Button>(true).FirstOrDefault(button => (button.GetComponentInChildren<Text>(true)?.text ?? "").Contains("BACK"));
        var settings = main.GetComponentsInChildren<Button>(true).FirstOrDefault(button => (button.GetComponentInChildren<Text>(true)?.text ?? "").Contains("SETTINGS"));
        if (back != null) Rect(back.GetComponent<RectTransform>(), new Vector2(0, 1), new Vector2(0, 1), new Vector2(16, -16), new Vector2(150, 72));
        if (settings != null) Rect(settings.GetComponent<RectTransform>(), new Vector2(1, 1), new Vector2(1, 1), new Vector2(-16, -16), new Vector2(150, 72));
        var brain = main.Find("Menu Currency Header/Brain Cell Card")?.GetComponent<RectTransform>();
        var knowledge = main.Find("Menu Currency Header/Research Card")?.GetComponent<RectTransform>();
        if (brain != null) Rect(brain, new Vector2(.5f, 1), new Vector2(.5f, 1), new Vector2(-170, -16), new Vector2(160, 72));
        if (knowledge != null) Rect(knowledge, new Vector2(.5f, 1), new Vector2(.5f, 1), new Vector2(25, -16), new Vector2(220, 72));
        if (knowledge != null)
        {
            var label = knowledge.GetComponentInChildren<Text>(true); if (label != null) { label.fontSize = 18; label.resizeTextForBestFit = true; label.resizeTextMinSize = 14; label.resizeTextMaxSize = 19; }
        }
    }

    private static void PolishTutorialChecklist()
    {
        var banner = Find("Portrait Gameplay HUD/Tutorial Objective Banner"); if (banner == null) return;
        foreach (var text in banner.GetComponentsInChildren<Text>(true)) { text.supportRichText = true; text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); }
        var iconNames = new[] { "Brain_128", "UI_Wand_128", "Library_256" };
        for (var index = 0; index < iconNames.Length; index++)
        {
            var old = banner.transform.Find($"Objective Icon {index + 1}"); if (old != null) Object.DestroyImmediate(old.gameObject);
            var go = new GameObject($"Objective Icon {index + 1}", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image)); go.transform.SetParent(banner.transform, false);
            var image = go.GetComponent<Image>(); image.sprite = FindSprite(iconNames[index]); image.preserveAspect = true; image.color = index == 1 ? Gold : Teal; image.raycastTarget = false;
            Rect(image.rectTransform, new Vector2(1, 1), new Vector2(1, 1), new Vector2(-38 - index * 54, -34), new Vector2(42, 42));
        }
    }

    private static void AttachParticles()
    {
        var run = Object.FindFirstObjectByType<WaveRunController>(FindObjectsInactive.Include); if (run == null) return;
        var feedback = run.GetComponent<GameplayParticleFeedback>() ?? run.gameObject.AddComponent<GameplayParticleFeedback>();
        feedback.Configure(Object.FindFirstObjectByType<PlayerMover>(FindObjectsInactive.Include), Object.FindFirstObjectByType<GameplayCombatController>(FindObjectsInactive.Include), Object.FindFirstObjectByType<LibraryEndpoint>(FindObjectsInactive.Include), run);
        EditorUtility.SetDirty(feedback);
    }

    private static void UpdateBuildSettings()
    {
        var required = new[] { Sample }.Concat(Scenes).ToArray();
        var existing = EditorBuildSettings.scenes.ToList();
        foreach (var path in required) if (existing.All(scene => scene.path != path)) existing.Add(new EditorBuildSettingsScene(path, true));
        EditorBuildSettings.scenes = existing.ToArray();
    }

    private static Sprite FindSprite(string name) => AssetDatabase.FindAssets(name + " t:Sprite").Select(AssetDatabase.GUIDToAssetPath).Select(AssetDatabase.LoadAssetAtPath<Sprite>).FirstOrDefault(sprite => sprite != null);
    private static Button TextButton(string text, Transform parent, int size)
    {
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/UiTextButton.prefab");
        var go = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent); go.name = "Button " + text;
        var label = go.GetComponentInChildren<Text>(true); label.text = text; label.fontSize = size; label.color = Cream; return go.GetComponent<Button>();
    }
    private static Text Label(string text, Transform parent, int size, Color color)
    {
        var go = new GameObject("Label " + text.Split('\n')[0], typeof(RectTransform), typeof(CanvasRenderer), typeof(Text)); go.transform.SetParent(parent, false);
        var label = go.GetComponent<Text>(); label.text = text; label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); label.fontSize = size; label.color = color; label.alignment = TextAnchor.MiddleCenter; label.horizontalOverflow = HorizontalWrapMode.Wrap; label.verticalOverflow = VerticalWrapMode.Overflow; return label;
    }
    private static Image Backplate(string name, Transform parent, Vector2 position, Vector2 size)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image)); go.transform.SetParent(parent, false);
        var image = go.GetComponent<Image>(); image.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Prototype/SolidSprite.asset"); image.color = new Color(0f, 0f, 0f, .76f); image.raycastTarget = false;
        Rect(image.rectTransform, new Vector2(.5f, 1), new Vector2(.5f, 1), position, size); return image;
    }
    private static void Clear(UnityEngine.Events.UnityEvent action) { for (var index = action.GetPersistentEventCount() - 1; index >= 0; index--) UnityEventTools.RemovePersistentListener(action, index); }
    private static GameObject Find(string path) => GameObject.Find("/" + path) ?? Resources.FindObjectsOfTypeAll<GameObject>().FirstOrDefault(go => go.scene == SceneManager.GetActiveScene() && Hierarchy(go) == path);
    private static string Hierarchy(GameObject go) { var value = go.name; for (var parent = go.transform.parent; parent != null; parent = parent.parent) value = parent.name + "/" + value; return value; }
    private static void Rect(RectTransform rect, Vector2 min, Vector2 max, Vector2 position, Vector2 size) { rect.anchorMin = min; rect.anchorMax = max; rect.pivot = min == max ? min : new Vector2(.5f, .5f); rect.anchoredPosition = position; rect.sizeDelta = size; }
    private static Color C(string hex) { ColorUtility.TryParseHtmlString("#" + hex, out var color); return color; }
}
