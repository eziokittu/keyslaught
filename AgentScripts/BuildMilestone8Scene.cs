using System;
using System.IO;
using KeySlaught.SceneGameplay;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Object = UnityEngine.Object;

public static class BuildMilestone8Scene
{
    private const string DataRoot = "Assets/Data";
    private const string TurretRoot = DataRoot + "/Turrets";
    private const string AbilityRoot = DataRoot + "/Abilities";
    private const string WaveRoot = DataRoot + "/Waves";

    public static string Build()
    {
        EnsureFolder(DataRoot, "Turrets");
        EnsureFolder(DataRoot, "Abilities");
        EnsureFolder(DataRoot, "Waves");

        var teacher = Asset<TurretDefinition>(TurretRoot + "/Teacher.asset");
        Set(teacher, "kind", (int)TurretKind.Teacher, "coveredLetters", "ABCDEF", "range", 5.2f,
            "secondsPerShot", 0.42f, "buildCost", 6, "rangePerUpgrade", 0.45f, "cadenceMultiplierPerUpgrade", 0.82f);
        var engineer = Asset<TurretDefinition>(TurretRoot + "/Engineer.asset");
        Set(engineer, "kind", (int)TurretKind.Engineer, "coveredLetters", "ABCDEFGHIJKLM", "range", 4.2f,
            "secondsPerShot", 0.7f, "buildCost", 8, "rangePerUpgrade", 0.4f, "cadenceMultiplierPerUpgrade", 0.84f);
        var scientist = Asset<TurretDefinition>(TurretRoot + "/Scientist.asset");
        Set(scientist, "kind", (int)TurretKind.Scientist, "coveredLetters", "ABCDEFGHIJKLMNOPQRSTUVWXYZ", "range", 3.3f,
            "secondsPerShot", 1.05f, "buildCost", 10, "rangePerUpgrade", 0.3f, "cadenceMultiplierPerUpgrade", 0.86f);
        var turretDefinitions = new[] { teacher, engineer, scientist };

        var history = CreateAbility("History", LibraryAbilityKind.History, 8, 5f, 3);
        var social = CreateAbility("SocialMediaInfluence", LibraryAbilityKind.SocialMediaInfluence, 8, 5f, 3);
        var politics = CreateAbility("Politics", LibraryAbilityKind.Politics, 12, 0f, 3);
        var abilityDefinitions = new[] { history, social, politics };

        var book = AssetDatabase.LoadAssetAtPath<EnemyDefinition>("Assets/Data/Enemies/Book.asset");
        var historyEnemy = AssetDatabase.LoadAssetAtPath<EnemyDefinition>("Assets/Data/Enemies/History.asset");
        var boss = Asset<EnemyDefinition>("Assets/Data/Enemies/GrandArchiveBoss.asset");
        Set(boss, "word", "ENCYCLOPEDIAOFETERNALKNOWLEDGE", "movementSpeed", 0.62f);

        var wave1 = CreateWave("Wave01", new[] { book, historyEnemy, book, book, historyEnemy, book }, 1.35f, false);
        var wave2 = CreateWave("Wave02", new[] { historyEnemy, book, historyEnemy, book, historyEnemy, book, book, historyEnemy }, 1.05f, false);
        var wave3 = CreateWave("Wave03Boss", new[] { book, historyEnemy, boss }, 1.5f, true);
        var waves = new[] { wave1, wave2, wave3 };

        var scene = SceneManager.GetActiveScene();
        var root = GameObject.Find("/KeySlaught Gameplay").transform;
        var coordinator = root.Find("Gameplay Coordinator").GetComponent<GameplaySceneCoordinator>();
        var combat = coordinator.GetComponent<GameplayCombatController>();
        var spawner = root.Find("Enemy Spawner").GetComponent<EnemySpawner>();
        var library = root.Find("Divine Library").GetComponent<LibraryEndpoint>();
        var player = root.Find("Player").GetComponent<PlayerMover>();
        var turretParent = root.Find("Placed Turrets");
        var economy = root.Find("Brain Cell Pickups").GetComponent<BrainCellEconomy>();
        spawner.SetAutomaticSpawning(false);

        var systems = ReplaceChild(root, "Bounded Run Systems");
        var abilities = systems.AddComponent<LibraryAbilityController>();
        abilities.Configure(spawner, combat, economy, abilityDefinitions);

        var canvas = GameObject.Find("/Portrait Gameplay HUD").transform;
        var context = canvas.GetComponent<TileContextActionPanel>();
        context.ConfigureRuntimeSystems(coordinator, combat, turretDefinitions, abilities);
        BuildResultOverlay(canvas, out var resultRoot, out var resultLabel, out var restartButton);

        var topHud = canvas.GetComponent<GameplayTopHud>();
        var run = systems.AddComponent<WaveRunController>();
        run.Configure(spawner, library, economy, combat, player, turretParent, abilities, waves,
            context, resultRoot, resultLabel, topHud);
        UnityEventTools.AddPersistentListener(restartButton.onClick, run.RestartRun);

        var serializedHud = new SerializedObject(topHud);
        serializedHud.FindProperty("waveRun").objectReferenceValue = run;
        serializedHud.ApplyModifiedPropertiesWithoutUndo();

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        return "Built bounded three-wave run with boss, data-driven turret combat, obstacle clearing, Library abilities, victory/defeat, and restart reset.";
    }

    public static string Audit()
    {
        var systems = GameObject.Find("/KeySlaught Gameplay/Bounded Run Systems")
            ?? throw new InvalidOperationException("Bounded Run Systems root is missing.");
        var run = systems.GetComponent<WaveRunController>()
            ?? throw new InvalidOperationException("WaveRunController is missing.");
        var abilities = systems.GetComponent<LibraryAbilityController>()
            ?? throw new InvalidOperationException("LibraryAbilityController is missing.");
        var runData = new SerializedObject(run);
        if (runData.FindProperty("waves").arraySize != 3) throw new InvalidOperationException("Expected exactly three authored waves.");
        if (runData.FindProperty("topHud").objectReferenceValue == null || runData.FindProperty("contextActions").objectReferenceValue == null)
            throw new InvalidOperationException("Run UI references are incomplete.");
        var abilityData = new SerializedObject(abilities);
        if (abilityData.FindProperty("definitions").arraySize != 3) throw new InvalidOperationException("Expected three Library abilities.");
        var boss = AssetDatabase.LoadAssetAtPath<EnemyDefinition>("Assets/Data/Enemies/GrandArchiveBoss.asset");
        if (boss == null || boss.Word.Length < 30 || boss.MovementSpeed >= 1f) throw new InvalidOperationException("Boss class data is invalid.");
        if (AssetDatabase.FindAssets("t:TurretDefinition", new[] { TurretRoot }).Length != 3)
            throw new InvalidOperationException("Expected three turret definitions.");
        var canvas = GameObject.Find("/Portrait Gameplay HUD");
        var result = canvas == null ? null : canvas.transform.Find("Run Result Overlay");
        if (result == null || result.gameObject.activeSelf) throw new InvalidOperationException("Inactive run-result overlay is missing.");
        return $"Audit passed: 3 waves, boss {boss.Word.Length} letters at speed {boss.MovementSpeed:0.00}, 3 turret definitions, 3 abilities, serialized HUD/context/restart wiring.";
    }

    private static AbilityDefinition CreateAbility(string name, LibraryAbilityKind kind, int cost, float duration, int targets)
    {
        var asset = Asset<AbilityDefinition>($"{AbilityRoot}/{name}.asset");
        Set(asset, "kind", (int)kind, "cost", cost, "duration", duration, "targetCount", targets);
        return asset;
    }

    private static WaveDefinition CreateWave(string name, EnemyDefinition[] enemies, float interval, bool boss)
    {
        var asset = Asset<WaveDefinition>($"{WaveRoot}/{name}.asset");
        var serialized = new SerializedObject(asset);
        var list = serialized.FindProperty("enemies");
        list.arraySize = enemies.Length;
        for (var index = 0; index < enemies.Length; index++) list.GetArrayElementAtIndex(index).objectReferenceValue = enemies[index];
        serialized.FindProperty("spawnInterval").floatValue = interval;
        serialized.FindProperty("bossWave").boolValue = boss;
        serialized.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(asset);
        return asset;
    }

    private static void BuildResultOverlay(Transform canvas, out GameObject root, out Text result, out Button restart)
    {
        var old = canvas.Find("Run Result Overlay");
        if (old != null) Object.DestroyImmediate(old.gameObject);
        var solid = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Prototype/SolidSprite.asset");
        var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        root = new GameObject("Run Result Overlay", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        root.transform.SetParent(canvas, false);
        var rootRect = root.GetComponent<RectTransform>();
        Rect(rootRect, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        var image = root.GetComponent<Image>(); image.sprite = solid; image.color = new Color(0.015f, 0.018f, 0.025f, 0.94f);
        result = Label("LIBRARY DEFENDED", root.transform, font, 50);
        Rect(result.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, 110), new Vector2(760, 120));
        restart = Button("RESTART RUN", root.transform, font, solid);
        Rect(restart.GetComponent<RectTransform>(), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, -30), new Vector2(520, 76));
        root.SetActive(false);
    }

    private static Text Label(string value, Transform parent, Font font, int size)
    {
        var go = new GameObject("Result Label", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
        go.transform.SetParent(parent, false);
        var text = go.GetComponent<Text>(); text.font = font; text.fontSize = size; text.fontStyle = FontStyle.Bold;
        text.alignment = TextAnchor.MiddleCenter; text.color = new Color(0.95f, 0.95f, 0.93f); text.text = value;
        return text;
    }

    private static Button Button(string value, Transform parent, Font font, Sprite solid)
    {
        var go = new GameObject("Button " + value, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        go.transform.SetParent(parent, false);
        var image = go.GetComponent<Image>(); image.sprite = solid; image.color = new Color(0.72f, 0.73f, 0.76f);
        var label = Label(value, go.transform, font, 26); label.color = new Color(0.04f, 0.045f, 0.055f);
        Rect(label.rectTransform, Vector2.zero, Vector2.one, new Vector2(4, 3), new Vector2(-8, -6));
        return go.GetComponent<Button>();
    }

    private static void Rect(RectTransform rect, Vector2 min, Vector2 max, Vector2 position, Vector2 size)
    {
        rect.anchorMin = min; rect.anchorMax = max; rect.pivot = min == max ? min : new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position; rect.sizeDelta = size;
    }

    private static GameObject ReplaceChild(Transform parent, string name)
    {
        var old = parent.Find(name); if (old != null) Object.DestroyImmediate(old.gameObject);
        var go = new GameObject(name); go.transform.SetParent(parent, false); return go;
    }

    private static T Asset<T>(string path) where T : ScriptableObject
    {
        var asset = AssetDatabase.LoadAssetAtPath<T>(path);
        if (asset != null) return asset;
        asset = ScriptableObject.CreateInstance<T>(); AssetDatabase.CreateAsset(asset, path); return asset;
    }

    private static void Set(Object target, params object[] values)
    {
        var serialized = new SerializedObject(target);
        for (var index = 0; index < values.Length; index += 2)
        {
            var property = serialized.FindProperty((string)values[index]);
            var value = values[index + 1];
            if (value is string text) property.stringValue = text;
            else if (value is int integer && property.propertyType == SerializedPropertyType.Enum) property.enumValueIndex = integer;
            else if (value is int integerValue) property.intValue = integerValue;
            else if (value is float number) property.floatValue = number;
        }
        serialized.ApplyModifiedPropertiesWithoutUndo(); EditorUtility.SetDirty(target);
    }

    private static void EnsureFolder(string parent, string name)
    {
        var path = parent + "/" + name;
        if (!AssetDatabase.IsValidFolder(path)) AssetDatabase.CreateFolder(parent, name);
    }
}
