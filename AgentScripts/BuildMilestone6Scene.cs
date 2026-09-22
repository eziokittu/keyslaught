using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using KeySlaught.SceneGameplay;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;
using UnityEngine.UI;
using Object = UnityEngine.Object;

public static class BuildMilestone6Scene
{
    private const string RootName = "KeySlaught Gameplay";
    private const string ArtFolder = "Assets/Art/Monochrome";
    private const string TileFolder = "Assets/Tiles/Monochrome";
    private const string PaletteFolder = "Assets/TilePalettes";
    private const string EnemyPrefabPath = "Assets/Prefabs/EnemyPrototype.prefab";
    private const string TorchPrefabPath = "Assets/Prefabs/MonochromeTorch.prefab";
    private const string BrainPrefabPath = "Assets/Prefabs/BrainCellPickup.prefab";
    private static readonly Color32 Transparent = new(0, 0, 0, 0);
    private static readonly Color32 Black = new(8, 9, 11, 255);
    private static readonly Color32 Charcoal = new(28, 31, 36, 255);
    private static readonly Color32 Mid = new(92, 96, 104, 255);
    private static readonly Color32 Light = new(190, 194, 199, 255);
    private static readonly Color32 White = new(242, 242, 238, 255);

    public static string Build()
    {
        var scene = SceneManager.GetActiveScene();
        if (!scene.IsValid() || scene.path != "Assets/Scenes/SampleScene.unity")
            throw new InvalidOperationException("Open Assets/Scenes/SampleScene.unity first.");

        EnsureFolders();
        GenerateArt();
        AssetDatabase.Refresh();
        ConfigureImports();
        BuildReusablePrefabs();

        var root = GameObject.Find($"/{RootName}") ?? throw new InvalidOperationException("Gameplay root missing.");
        var legacyGrid = GameObject.Find("/Grid"); if (legacyGrid != null) legacyGrid.SetActive(false);
        var legacyLight = GameObject.Find("/Global Light 2D"); if (legacyLight != null) legacyLight.SetActive(false);
        var player = Require<PlayerMover>(root.transform, "Player");
        var coordinator = Require<GameplaySceneCoordinator>(root.transform, "Gameplay Coordinator");
        var combat = coordinator.GetComponent<GameplayCombatController>() ?? throw new InvalidOperationException("Combat controller missing.");
        var library = Require<LibraryEndpoint>(root.transform, "Divine Library");

        var groundTiles = Enumerable.Range(0, 4).Select(i => CreateTileAsset($"Ground_{i}", Sprite($"Ground_{i}_128"))).ToArray();
        var pathTiles = new Dictionary<string, Tile>
        {
            ["H"] = CreateTileAsset("Path_H", Sprite("Path_H_128")), ["V"] = CreateTileAsset("Path_V", Sprite("Path_V_128")),
            ["LT"] = CreateTileAsset("Path_LT", Sprite("Path_LT_128")), ["TR"] = CreateTileAsset("Path_TR", Sprite("Path_TR_128")),
            ["LB"] = CreateTileAsset("Path_LB", Sprite("Path_LB_128")), ["BR"] = CreateTileAsset("Path_BR", Sprite("Path_BR_128"))
        };
        var obstacleTiles = new Dictionary<string, Tile>
        {
            ["Tree"] = CreateTileAsset("Obstacle_Tree", Sprite("Obstacle_Tree_128")),
            ["Boulder"] = CreateTileAsset("Obstacle_Boulder", Sprite("Obstacle_Boulder_128")),
            ["Water0"] = CreateTileAsset("Obstacle_Water_0", Sprite("Obstacle_Water_0_128")),
            ["Water1"] = CreateTileAsset("Obstacle_Water_1", Sprite("Obstacle_Water_1_128")),
            ["Water2"] = CreateTileAsset("Obstacle_Water_2", Sprite("Obstacle_Water_2_128")),
            ["Water3"] = CreateTileAsset("Obstacle_Water_3", Sprite("Obstacle_Water_3_128")),
            ["Mountain0"] = CreateTileAsset("Obstacle_Mountain_0", Sprite("Obstacle_Mountain_0_128")),
            ["Mountain1"] = CreateTileAsset("Obstacle_Mountain_1", Sprite("Obstacle_Mountain_1_128")),
            ["Mountain2"] = CreateTileAsset("Obstacle_Mountain_2", Sprite("Obstacle_Mountain_2_128")),
            ["Mountain3"] = CreateTileAsset("Obstacle_Mountain_3", Sprite("Obstacle_Mountain_3_128")),
            ["OuterRock"] = CreateTileAsset("Outer_Rock", Sprite("Outer_Rock_128")),
            ["Cloud"] = CreateTileAsset("Outer_Cloud", Sprite("Outer_Cloud_128"))
        };
        BuildPalette(groundTiles.Cast<TileBase>().Concat(pathTiles.Values).Concat(obstacleTiles.Values).ToArray());
        var map = BuildMap(root.transform, groundTiles, pathTiles, obstacleTiles);

        var teacher = Sprite("Turret_Teacher_128");
        var engineer = Sprite("Turret_Engineer_128");
        var scientist = Sprite("Turret_Scientist_128");
        var turretRoot = ReplaceChild(root.transform, "Placed Turrets");
        var highlight = BuildHighlight(root.transform);
        ConfigureWorld(root.transform, player, library, teacher, engineer, scientist);
        ConfigurePath(root.transform, map.Path);
        ConfigureEnemies();
        EqualizeEnemySpeeds();
        ConfigureCamera(root.transform, player);
        BuildLighting(root.transform, map.PathCells);
        BuildHud(root.transform, player, coordinator, combat, library, map, highlight, turretRoot.transform, teacher, engineer, scientist);

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        return "Built Milestone 6 issue-correction scene: monochrome art, orthogonal map, ground context, portrait HUD, magazine, dynamic joystick, Cinemachine, terrain, and lighting.";
    }

    public static string ReviewLibraryContext()
    {
        if (!EditorApplication.isPlaying) throw new InvalidOperationException("Enter Play Mode first.");
        var player = GameObject.Find($"/{RootName}/Player") ?? throw new InvalidOperationException("Player missing.");
        player.transform.position = new Vector3(0.5f, 6.5f, 0f);
        return "Moved the live player onto the 2x2 Library footprint for context review.";
    }

    public static string ReviewPauseOverlay()
    {
        if (!EditorApplication.isPlaying) throw new InvalidOperationException("Enter Play Mode first.");
        var pause = Object.FindFirstObjectByType<PauseMenuController>(FindObjectsInactive.Include)
            ?? throw new InvalidOperationException("Pause controller missing.");
        pause.TogglePause();
        return "Opened the live pause overlay and paused Time.timeScale.";
    }

    private static void EnsureFolders()
    {
        Folder("Assets", "Art"); Folder("Assets/Art", "Monochrome");
        Folder("Assets", "Tiles"); Folder("Assets/Tiles", "Monochrome");
        Folder("Assets", "TilePalettes");
        Folder("Assets", "Prefabs");
    }

    private static void BuildReusablePrefabs()
    {
        var torch = new GameObject("Monochrome Torch");
        var torchRenderer = torch.AddComponent<SpriteRenderer>(); torchRenderer.sprite = Sprite("Torch_128"); torchRenderer.sharedMaterial = LitMaterial(); torchRenderer.sortingOrder = 14;
        AddLight(torch, null, false, 1f, 3.2f);
        PrefabUtility.SaveAsPrefabAsset(torch, TorchPrefabPath); Object.DestroyImmediate(torch);

        var brain = new GameObject("Brain Cell Pickup"); brain.transform.localScale = Vector3.one * .42f;
        var brainRenderer = brain.AddComponent<SpriteRenderer>(); brainRenderer.sprite = Sprite("Brain_128"); brainRenderer.sharedMaterial = LitMaterial(); brainRenderer.sortingOrder = 18;
        brain.AddComponent<BrainCellPickup>(); AddLight(brain, null, false, .8f, 1.6f);
        PrefabUtility.SaveAsPrefabAsset(brain, BrainPrefabPath); Object.DestroyImmediate(brain);

        foreach (var pair in new[] { ("Teacher", "Turret_Teacher_128"), ("Engineer", "Turret_Engineer_128"), ("Scientist", "Turret_Scientist_128") })
        {
            var turret = new GameObject($"{pair.Item1} Turret");
            var renderer = turret.AddComponent<SpriteRenderer>(); renderer.sprite = Sprite(pair.Item2); renderer.sharedMaterial = LitMaterial(); renderer.sortingOrder = 12;
            PrefabUtility.SaveAsPrefabAsset(turret, $"Assets/Prefabs/Turret{pair.Item1}.prefab"); Object.DestroyImmediate(turret);
        }
    }

    private static void GenerateArt()
    {
        for (var i = 0; i < 4; i++) Save($"Ground_{i}_128", 128, p => DrawGround(p, 128, i));
        Save("Path_H_128", 128, p => DrawPath(p, 128, true, false, true, false));
        Save("Path_V_128", 128, p => DrawPath(p, 128, false, true, false, true));
        Save("Path_LT_128", 128, p => DrawPath(p, 128, true, true, false, false));
        Save("Path_TR_128", 128, p => DrawPath(p, 128, false, true, true, false));
        Save("Path_LB_128", 128, p => DrawPath(p, 128, true, false, false, true));
        Save("Path_BR_128", 128, p => DrawPath(p, 128, false, false, true, true));
        Save("Turret_Teacher_128", 128, p => DrawTurret(p, 128, 0));
        Save("Turret_Engineer_128", 128, p => DrawTurret(p, 128, 1));
        Save("Turret_Scientist_128", 128, p => DrawTurret(p, 128, 2));
        for (var i = 0; i < 4; i++) Save($"Player_Walk_{i}_128", 128, p => DrawPlayer(p, 128, i));
        Save("Library_256", 256, p => DrawLibrary(p, 256));
        Save("Enemy_Card_128", 128, p => DrawEnemyCard(p, 128));
        Save("Brain_128", 128, p => DrawBrain(p, 128));
        Save("Torch_128", 128, p => DrawTorch(p, 128));
        Save("Obstacle_Tree_128", 128, p => DrawObstacle(p, 128, 0));
        Save("Obstacle_Boulder_128", 128, p => DrawObstacle(p, 128, 1));
        for (var i = 0; i < 4; i++) Save($"Obstacle_Water_{i}_128", 128, p => DrawWater(p, 128, i));
        for (var i = 0; i < 4; i++) Save($"Obstacle_Mountain_{i}_128", 128, p => DrawMountain(p, 128, i));
        Save("Outer_Rock_128", 128, p => DrawObstacle(p, 128, 4));
        Save("Outer_Cloud_128", 128, p => DrawObstacle(p, 128, 5));
        Save("Cell_Highlight_128", 128, p => DrawHighlight(p, 128));
        Save("UI_Circle_128", 128, p => DrawCircleUi(p, 128));
        Save("UI_Reload_128", 128, p => DrawReload(p, 128));
        Save("UI_Gun_128", 128, p => DrawGun(p, 128));
        Save("UI_Pause_128", 128, p => DrawPause(p, 128));
        Save("UI_Time_128", 128, p => DrawTime(p, 128));
    }

    private static void ConfigureImports()
    {
        foreach (var path in Directory.GetFiles(ArtFolder, "*.png"))
        {
            var assetPath = path.Replace('\\', '/');
            if (AssetImporter.GetAtPath(assetPath) is not TextureImporter importer) continue;
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = 128f;
            importer.filterMode = FilterMode.Point;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.mipmapEnabled = false;
            importer.alphaIsTransparency = true;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.SaveAndReimport();
        }
    }

    private static MapData BuildMap(Transform root, Tile[] groundTiles, Dictionary<string, Tile> paths, Dictionary<string, Tile> obstacles)
    {
        var old = root.Find("Authored Tilemaps"); if (old != null) Object.DestroyImmediate(old.gameObject);
        var gridObject = new GameObject("Authored Tilemaps"); gridObject.transform.SetParent(root, false); gridObject.AddComponent<Grid>();
        var outer = Tilemap("Outside Rocky Terrain", gridObject.transform, -30);
        var ground = Tilemap("Ground Tilemap", gridObject.transform, -20);
        var path = Tilemap("Enemy Path Tilemap", gridObject.transform, -5);
        var blocked = Tilemap("Blocked Terrain Tilemap", gridObject.transform, -3);
        var clouds = Tilemap("Cloud Cover Tilemap", gridObject.transform, 22);

        for (var x = -11; x <= 10; x++) for (var y = -12; y <= 11; y++)
        {
            var inside = x >= -7 && x <= 6 && y >= -8 && y <= 7;
            if (!inside) outer.SetTile(new Vector3Int(x, y, 0), obstacles["OuterRock"]);
            if (!inside && (x is -8 or 7 || y is -9 or 8) && (Math.Abs(x + y) % 3 != 0))
                clouds.SetTile(new Vector3Int(x, y, 0), obstacles["Cloud"]);
        }
        for (var x = -7; x <= 6; x++) for (var y = -8; y <= 7; y++)
            ground.SetTile(new Vector3Int(x, y, 0), groundTiles[(Math.Abs(x) + Math.Abs(y) * 3) % groundTiles.Length]);

        var route = OrthogonalRoute();
        var pathCells = ExpandRoute(route);
        foreach (var cell in pathCells) path.SetTile(cell, SelectPathTile(cell, pathCells, paths));

        var blockedCells = new Dictionary<Vector3Int, string>
        {
            [new(-5, 5, 0)] = "Tree", [new(5, -5, 0)] = "Tree", [new(-4, -7, 0)] = "Boulder",
            [new(4, 5, 0)] = "Boulder",
            [new(-5, 0, 0)] = "Water0", [new(-4, 0, 0)] = "Water1", [new(-5, 1, 0)] = "Water2", [new(-4, 1, 0)] = "Water3",
            [new(4, 0, 0)] = "Mountain0", [new(5, 0, 0)] = "Mountain1", [new(4, 1, 0)] = "Mountain2", [new(5, 1, 0)] = "Mountain3"
        };
        foreach (var pair in blockedCells) blocked.SetTile(pair.Key, obstacles[pair.Value]);
        return new MapData(ground, path, blocked, route, pathCells);
    }

    private static Vector3Int[] OrthogonalRoute() => new[]
    {
        new Vector3Int(-10, -6, 0), new Vector3Int(-6, -6, 0), new Vector3Int(-6, -2, 0),
        new Vector3Int(-2, -2, 0), new Vector3Int(-2, 3, 0), new Vector3Int(2, 3, 0),
        new Vector3Int(2, 6, 0), new Vector3Int(0, 6, 0)
    };

    private static HashSet<Vector3Int> ExpandRoute(Vector3Int[] route)
    {
        var cells = new HashSet<Vector3Int>();
        for (var i = 0; i < route.Length - 1; i++)
        {
            var current = route[i]; var end = route[i + 1];
            var step = new Vector3Int(Math.Sign(end.x - current.x), Math.Sign(end.y - current.y), 0);
            if (step.x != 0 && step.y != 0) throw new InvalidOperationException("Route contains a diagonal segment.");
            cells.Add(current);
            while (current != end) { current += step; cells.Add(current); }
        }
        return cells;
    }

    private static Tile SelectPathTile(Vector3Int cell, HashSet<Vector3Int> cells, Dictionary<string, Tile> tiles)
    {
        var l = cells.Contains(cell + Vector3Int.left); var r = cells.Contains(cell + Vector3Int.right);
        var t = cells.Contains(cell + Vector3Int.up); var b = cells.Contains(cell + Vector3Int.down);
        if (l && t) return tiles["LT"]; if (t && r) return tiles["TR"];
        if (l && b) return tiles["LB"]; if (b && r) return tiles["BR"];
        return (t || b) && !(l || r) ? tiles["V"] : tiles["H"];
    }

    private static void ConfigureWorld(Transform root, PlayerMover player, LibraryEndpoint library, Sprite teacher, Sprite engineer, Sprite scientist)
    {
        player.Configure(2.5f, new Vector2(-6.65f, -7.65f), new Vector2(5.65f, 6.65f));
        var playerRenderer = player.GetComponent<SpriteRenderer>();
        playerRenderer.sharedMaterial = LitMaterial();
        var frames = Enumerable.Range(0, 4).Select(i => Sprite($"Player_Walk_{i}_128")).ToArray();
        playerRenderer.sprite = frames[0]; playerRenderer.color = Color.white; playerRenderer.sortingOrder = 15;
        player.transform.localScale = Vector3.one;
        var animator = player.GetComponent<PlayerPixelAnimator>() ?? player.gameObject.AddComponent<PlayerPixelAnimator>();
        animator.Configure(player, playerRenderer, frames);

        var libraryRenderer = library.GetComponent<SpriteRenderer>();
        libraryRenderer.sprite = Sprite("Library_256"); libraryRenderer.sharedMaterial = LitMaterial(); libraryRenderer.color = Color.white; libraryRenderer.sortingOrder = 9;
        library.transform.position = new Vector3(0.5f, 6.5f, 0f); library.transform.localScale = Vector3.one;
        var health = library.GetComponentInChildren<TextMesh>(true);
        if (health != null)
        {
            health.transform.localPosition = new Vector3(0f, 1.25f, -0.2f); health.characterSize = 0.11f;
            health.fontSize = 52; health.anchor = TextAnchor.MiddleCenter; health.color = White;
        }

        var oldPads = root.Find("Turret Pads"); if (oldPads != null) Object.DestroyImmediate(oldPads.gameObject);
        var arena = root.Find("Arena Background")?.GetComponent<SpriteRenderer>(); if (arena != null) arena.enabled = false;
        foreach (var label in root.GetComponentsInChildren<TextMesh>(true))
        {
            label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            label.GetComponent<MeshRenderer>().sharedMaterial = label.font.material;
        }
        foreach (var obsoleteName in new[] { "Prototype Title", "Movement Hint", "Combat HUD", "Arena Border" })
        {
            var obsolete = root.Find(obsoleteName); if (obsolete != null) obsolete.gameObject.SetActive(false);
        }
        var rangePreview = player.transform.Find("Attack Range Preview"); if (rangePreview != null) rangePreview.gameObject.SetActive(false);
    }

    private static void ConfigurePath(Transform root, Tilemap pathMap)
    {
        var path = Require<WaypointPath>(root, "Enemy Path");
        foreach (Transform child in path.transform.Cast<Transform>().ToArray()) Object.DestroyImmediate(child.gameObject);
        var route = OrthogonalRoute(); var points = new Transform[route.Length];
        for (var i = 0; i < route.Length; i++)
        {
            var point = new GameObject($"Waypoint {i + 1:00}").transform; point.SetParent(path.transform, false);
            point.position = pathMap.GetCellCenterWorld(route[i]); points[i] = point;
        }
        path.Configure(points, null);
        var line = path.GetComponent<LineRenderer>(); if (line != null) line.enabled = false;
    }

    private static void ConfigureEnemies()
    {
        var prefab = PrefabUtility.LoadPrefabContents(EnemyPrefabPath);
        try
        {
            var renderer = prefab.GetComponent<SpriteRenderer>(); renderer.sprite = Sprite("Enemy_Card_128"); renderer.sharedMaterial = LitMaterial(); renderer.color = Color.white;
            prefab.transform.localScale = new Vector3(0.85f, 0.85f, 1f);
            var label = prefab.GetComponentInChildren<TextMesh>(true);
            label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); label.fontStyle = FontStyle.Bold;
            label.color = Black; label.characterSize = 0.18f; label.fontSize = 64; label.anchor = TextAnchor.MiddleCenter;
            label.transform.localPosition = new Vector3(0f, 0.03f, -0.1f); label.GetComponent<MeshRenderer>().sharedMaterial = label.font.material;
            PrefabUtility.SaveAsPrefabAsset(prefab, EnemyPrefabPath);
        }
        finally { PrefabUtility.UnloadPrefabContents(prefab); }
    }

    private static void EqualizeEnemySpeeds()
    {
        foreach (var guid in AssetDatabase.FindAssets("t:EnemyDefinition", new[] { "Assets/Data/Enemies" }))
        {
            var asset = AssetDatabase.LoadAssetAtPath<EnemyDefinition>(AssetDatabase.GUIDToAssetPath(guid));
            var serialized = new SerializedObject(asset); serialized.FindProperty("movementSpeed").floatValue = 1.15f;
            serialized.ApplyModifiedPropertiesWithoutUndo(); EditorUtility.SetDirty(asset);
        }
    }

    private static SpriteRenderer BuildHighlight(Transform root)
    {
        var old = root.Find("Selected Ground Highlight"); if (old != null) Object.DestroyImmediate(old.gameObject);
        var go = new GameObject("Selected Ground Highlight"); go.transform.SetParent(root, false);
        var renderer = go.AddComponent<SpriteRenderer>(); renderer.sprite = Sprite("Cell_Highlight_128"); renderer.sortingOrder = 20;
        go.SetActive(false); return renderer;
    }

    private static void ConfigureCamera(Transform root, PlayerMover player)
    {
        var camera = Camera.main ?? Object.FindFirstObjectByType<Camera>();
        camera.orthographic = true; camera.orthographicSize = 8.5f; camera.backgroundColor = Black;
        var brainType = Type.GetType("Unity.Cinemachine.CinemachineBrain, Unity.Cinemachine");
        var cameraType = Type.GetType("Unity.Cinemachine.CinemachineCamera, Unity.Cinemachine");
        var composerType = Type.GetType("Unity.Cinemachine.CinemachinePositionComposer, Unity.Cinemachine");
        if (brainType == null || cameraType == null) throw new InvalidOperationException("Cinemachine 3.1.7 was not imported.");
        if (camera.GetComponent(brainType) == null) camera.gameObject.AddComponent(brainType);
        var old = root.Find("Cinemachine Player Camera"); if (old != null) Object.DestroyImmediate(old.gameObject);
        var virtualObject = new GameObject("Cinemachine Player Camera"); virtualObject.transform.SetParent(root, false);
        var virtualCamera = virtualObject.AddComponent(cameraType);
        cameraType.GetProperty("Follow")?.SetValue(virtualCamera, player.transform);
        if (composerType != null) virtualObject.AddComponent(composerType);
        var zoom = virtualObject.AddComponent<CameraMotionZoom>(); zoom.Configure(player, camera, virtualCamera);
    }

    private static void BuildLighting(Transform root, HashSet<Vector3Int> pathCells)
    {
        var lights = ReplaceChild(root, "Authored Lighting");
        AddLight(new GameObject("Playable Area Base Light"), lights.transform, true, 0.08f, 18f);
        var torchCells = new[] { new Vector3Int(-5,-5,0), new Vector3Int(-5,-1,0), new Vector3Int(-1,-1,0), new Vector3Int(-1,2,0), new Vector3Int(1,4,0) };
        foreach (var cell in torchCells)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(TorchPrefabPath);
            var torch = (GameObject)PrefabUtility.InstantiatePrefab(prefab, lights.transform); torch.name = $"Torch {cell.x},{cell.y}";
            torch.transform.position = new Vector3(cell.x + 0.5f, cell.y + 0.5f, 0f);
        }
    }

    private static void AddLight(GameObject go, Transform parent, bool global, float intensity, float radius)
    {
        if (parent != null) go.transform.SetParent(parent, false);
        var type = Type.GetType("UnityEngine.Rendering.Universal.Light2D, Unity.RenderPipelines.Universal.Runtime");
        if (type == null) return;
        var component = go.GetComponent(type) ?? go.AddComponent(type);
        var serialized = new SerializedObject(component);
        serialized.FindProperty("m_LightType").enumValueIndex = global ? 4 : 0;
        serialized.FindProperty("m_Intensity").floatValue = intensity;
        var outer = serialized.FindProperty("m_PointLightOuterRadius"); if (outer != null) outer.floatValue = radius;
        var inner = serialized.FindProperty("m_PointLightInnerRadius"); if (inner != null) inner.floatValue = radius * 0.2f;
        serialized.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void BuildHud(Transform root, PlayerMover player, GameplaySceneCoordinator coordinator, GameplayCombatController combat,
        LibraryEndpoint library, MapData map, SpriteRenderer highlight, Transform turretRoot, Sprite teacher, Sprite engineer, Sprite scientist)
    {
        var existing = GameObject.Find("/Dual Input HUD"); if (existing != null) Object.DestroyImmediate(existing);
        existing = GameObject.Find("/Portrait Gameplay HUD"); if (existing != null) Object.DestroyImmediate(existing);
        var canvasGo = new GameObject("Portrait Gameplay HUD");
        var canvas = canvasGo.AddComponent<Canvas>(); canvas.renderMode = RenderMode.ScreenSpaceOverlay; canvas.sortingOrder = 100;
        var scaler = canvasGo.AddComponent<CanvasScaler>(); scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(900, 1600); scaler.matchWidthOrHeight = 0.5f;
        canvasGo.AddComponent<GraphicRaycaster>(); EnsureEventSystem();
        var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        var solid = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Prototype/SolidSprite.asset");

        var top = Panel("Top 30 Percent", canvas.transform, solid, new Color(0.025f,0.027f,0.032f,1f));
        Rect(top.rectTransform, new Vector2(0,0.7f), Vector2.one, Vector2.zero, Vector2.zero);
        var wave = Label("Wave 1", top.transform, font, 30, White); Rect(wave.rectTransform, new Vector2(0,1), new Vector2(0,1), new Vector2(28,-30), new Vector2(210,58));
        var timeIcon = Image("Time Icon", top.transform, Sprite("UI_Time_128"), Color.white); Rect(timeIcon.rectTransform, new Vector2(.5f,1), new Vector2(.5f,1), new Vector2(-80,-30), new Vector2(42,42));
        var time = Label("00:00", top.transform, font, 28, White); Rect(time.rectTransform, new Vector2(.5f,1), new Vector2(.5f,1), new Vector2(-25,-30), new Vector2(150,58));
        var pauseButton = IconButton("Pause", top.transform, Sprite("UI_Pause_128"), solid); Rect(pauseButton.GetComponent<RectTransform>(), new Vector2(1,1), new Vector2(1,1), new Vector2(-28,-26), new Vector2(62,62));
        var target = Label("NO TARGET IN RANGE", top.transform, font, 40, White); target.fontStyle = FontStyle.Bold;
        Rect(target.rectTransform, new Vector2(0,1), new Vector2(1,1), new Vector2(35,-145), new Vector2(-70,62));
        var divider = Panel("Divider", top.transform, solid, new Color(1,1,1,.38f)); Rect(divider.rectTransform, new Vector2(0,1), new Vector2(1,1), new Vector2(40,-190), new Vector2(-80,3));

        var contextPanel = Panel("Context Actions", top.transform, solid, new Color(.07f,.075f,.085f,1f));
        contextPanel.rectTransform.anchorMin = new Vector2(0,0); contextPanel.rectTransform.anchorMax = new Vector2(1,0);
        contextPanel.rectTransform.pivot = new Vector2(.5f,0); contextPanel.rectTransform.anchoredPosition = new Vector2(0,20);
        contextPanel.rectTransform.sizeDelta = new Vector2(-52,250);
        var contextTitle = Label("CONTEXT", contextPanel.transform, font, 24, Light); contextTitle.fontStyle = FontStyle.Bold;
        Rect(contextTitle.rectTransform, new Vector2(0,1), new Vector2(1,1), new Vector2(18,-12), new Vector2(-36,46));
        var buttons = new Button[9]; var labels = new Text[9];
        for (var i = 0; i < 9; i++)
        {
            buttons[i] = TextButton($"Action {i+1}", contextPanel.transform, font, solid, 18);
            var row = i / 3; var col = i % 3;
            Rect(buttons[i].GetComponent<RectTransform>(), new Vector2(0,1), new Vector2(0,1), new Vector2(18 + col * 278, -65 - row * 62), new Vector2(256,50));
            labels[i] = buttons[i].GetComponentInChildren<Text>();
        }
        var contextStatus = Label(string.Empty, contextPanel.transform, font, 18, Light);
        Rect(contextStatus.rectTransform, new Vector2(0,0), new Vector2(1,0), new Vector2(18,10), new Vector2(-36,42));

        var controller = canvasGo.AddComponent<TileContextActionPanel>();
        controller.Configure(player, map.Ground, map.Path, map.Blocked, library, new Vector3Int(0,6,0), new Vector3Int(1,7,0),
            highlight, turretRoot, teacher, engineer, scientist, contextPanel.gameObject, contextTitle, contextStatus, buttons, labels);
        for (var i = 0; i < buttons.Length; i++) { var action = buttons[i].gameObject.AddComponent<ContextActionButton>(); action.Configure(controller, i + 1); }

        var playSurface = Panel("Dynamic Joystick Surface", canvas.transform, solid, new Color(0,0,0,0.001f));
        Rect(playSurface.rectTransform, new Vector2(0,.3f), new Vector2(1,.7f), Vector2.zero, Vector2.zero);
        var joystickBase = Image("Dynamic Circular Joystick", playSurface.transform, Sprite("UI_Circle_128"), new Color(1,1,1,.36f));
        joystickBase.raycastTarget = false; Rect(joystickBase.rectTransform, new Vector2(.5f,.5f), new Vector2(.5f,.5f), Vector2.zero, new Vector2(176,176));
        var handle = Image("Joystick Handle", joystickBase.transform, Sprite("UI_Circle_128"), new Color(.18f,.18f,.2f,.72f));
        handle.raycastTarget = false; Rect(handle.rectTransform, new Vector2(.5f,.5f), new Vector2(.5f,.5f), Vector2.zero, new Vector2(72,72));
        var joystick = playSurface.gameObject.AddComponent<VirtualJoystick>(); joystick.Configure(player, playSurface.rectTransform, joystickBase.rectTransform, handle.rectTransform);

        var bottom = Panel("Bottom 30 Percent", canvas.transform, solid, new Color(.025f,.027f,.032f,1f));
        Rect(bottom.rectTransform, Vector2.zero, new Vector2(1,.3f), Vector2.zero, Vector2.zero);
        var gun = Image("Gun Icon", bottom.transform, Sprite("UI_Gun_128"), Color.white); Rect(gun.rectTransform, new Vector2(0,1), new Vector2(0,1), new Vector2(32,-22), new Vector2(72,72));
        var magazine = Label("▰  [ ●  ●  ●  ● ]", bottom.transform, font, 32, White); magazine.fontStyle = FontStyle.Bold;
        Rect(magazine.rectTransform, new Vector2(0,1), new Vector2(1,1), new Vector2(112,-22), new Vector2(-245,72));
        var reload = IconButton("Reload", bottom.transform, Sprite("UI_Reload_128"), solid); Rect(reload.GetComponent<RectTransform>(), new Vector2(1,1), new Vector2(1,1), new Vector2(-28,-20), new Vector2(112,78));
        var refreshAdapter = reload.gameObject.AddComponent<OnScreenRefreshButton>(); refreshAdapter.Configure(combat);
        var status = Label("TYPE A-Z TO LOAD", bottom.transform, font, 16, Light); Rect(status.rectTransform, new Vector2(0,1), new Vector2(1,1), new Vector2(32,-95), new Vector2(-64,34));
        BuildQwerty(bottom.transform, combat, font, solid);
        combat.ConfigureUi(magazine, status);

        var topHud = canvasGo.AddComponent<GameplayTopHud>(); topHud.Configure(coordinator, wave, time, target);
        var currency = Label("BRAIN CELLS  0", top.transform, font, 20, White); Rect(currency.rectTransform, new Vector2(0,1), new Vector2(0,1), new Vector2(28,-72), new Vector2(260,40));
        var economyRoot = ReplaceChild(root, "Brain Cell Pickups");
        var brainPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(BrainPrefabPath).GetComponent<BrainCellPickup>();
        var economy = economyRoot.AddComponent<BrainCellEconomy>(); economy.Configure(combat, player, Sprite("Brain_128"), currency, brainPrefab);
        controller.ConfigureEconomy(economy);
        BuildPause(canvas.transform, font, solid, pauseButton, out _);
    }

    private static void BuildQwerty(Transform parent, GameplayCombatController combat, Font font, Sprite solid)
    {
        var rows = new[] { "QWERTYUIOP", "ASDFGHJKL", "ZXCVBNM" };
        var widths = new[] { 80f, 84f, 88f }; var offsets = new[] { 25f, 65f, 110f };
        for (var row = 0; row < rows.Length; row++) for (var col = 0; col < rows[row].Length; col++)
        {
            var letter = rows[row][col]; var button = TextButton(letter.ToString(), parent, font, solid, 24);
            Rect(button.GetComponent<RectTransform>(), new Vector2(0,1), new Vector2(0,1),
                new Vector2(offsets[row] + col * (widths[row] + 6), -145 - row * 92), new Vector2(widths[row],78));
            var adapter = button.gameObject.AddComponent<OnScreenLetterButton>(); adapter.Configure(combat, letter);
        }
    }

    private static void BuildPause(Transform canvas, Font font, Sprite solid, Button pauseButton, out PauseMenuController controller)
    {
        var overlay = Panel("Pause Overlay", canvas, solid, new Color(0,0,0,.82f)); Rect(overlay.rectTransform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        var card = Panel("Pause Window", overlay.transform, solid, new Color(.08f,.085f,.095f,.99f));
        Rect(card.rectTransform, new Vector2(.5f,.5f), new Vector2(.5f,.5f), Vector2.zero, new Vector2(700,920));
        var title = Label("PAUSED", card.transform, font, 52, White); title.fontStyle = FontStyle.Bold; Rect(title.rectTransform, new Vector2(0,1),new Vector2(1,1),new Vector2(35,-40),new Vector2(-70,80));
        var details = Label("ENDLESS MODE\n\nTHE DIVINE LIBRARY\nHold the path. Preserve knowledge.\n\nWave 1\nElapsed time continues below the HUD.\nBrain cells are collected from defeated words.", card.transform, font, 25, Light);
        Rect(details.rectTransform, new Vector2(0,1),new Vector2(1,1),new Vector2(55,-150),new Vector2(-110,360));
        var continueButton = TextButton("CONTINUE", card.transform, font, solid, 26); Rect(continueButton.GetComponent<RectTransform>(),new Vector2(.5f,0),new Vector2(.5f,0),new Vector2(0,210),new Vector2(520,72));
        var controlsButton = TextButton("CONTROLS", card.transform, font, solid, 26); Rect(controlsButton.GetComponent<RectTransform>(),new Vector2(.5f,0),new Vector2(.5f,0),new Vector2(0,120),new Vector2(520,72));
        var menuButton = TextButton("BACK TO MENU", card.transform, font, solid, 26); Rect(menuButton.GetComponent<RectTransform>(),new Vector2(.5f,0),new Vector2(.5f,0),new Vector2(0,30),new Vector2(520,72));
        var controls = Panel("Controls Panel", overlay.transform, solid, new Color(.05f,.055f,.065f,1)); Rect(controls.rectTransform,new Vector2(.5f,.5f),new Vector2(.5f,.5f),Vector2.zero,new Vector2(720,600));
        var controlsText = Label("CONTROLS\n\nARROW KEYS — MOVE\nA–Z — LOAD LETTERS\nSPACE — RELOAD\n1–9 — CONTEXT ACTIONS\n\nTOUCH\nDrag anywhere in the game view to move.\nTap the QWERTY keyboard to load letters.",controls.transform,font,24,White);
        Rect(controlsText.rectTransform,Vector2.zero,Vector2.one,new Vector2(45,45),new Vector2(-90,-90));
        var confirmation = Panel("Menu Confirmation", overlay.transform, solid, new Color(.04f,.045f,.052f,1)); Rect(confirmation.rectTransform,new Vector2(.5f,.5f),new Vector2(.5f,.5f),Vector2.zero,new Vector2(650,340));
        var confirmationText = Label("LEAVE THIS RUN?\nCurrent progress will be lost.",confirmation.transform,font,28,White); Rect(confirmationText.rectTransform,new Vector2(0,1),new Vector2(1,1),new Vector2(35,-45),new Vector2(-70,120));
        var cancel = TextButton("CANCEL",confirmation.transform,font,solid,24); Rect(cancel.GetComponent<RectTransform>(),new Vector2(.5f,0),new Vector2(.5f,0),new Vector2(0,35),new Vector2(430,72));
        controller = overlay.gameObject.AddComponent<PauseMenuController>(); controller.Configure(overlay.gameObject, confirmation.gameObject, controls.gameObject);
        UnityEventTools.AddPersistentListener(pauseButton.onClick, controller.TogglePause);
        UnityEventTools.AddPersistentListener(continueButton.onClick, controller.Continue);
        UnityEventTools.AddPersistentListener(controlsButton.onClick, controller.ToggleControls);
        UnityEventTools.AddPersistentListener(menuButton.onClick, controller.ShowMenuConfirmation);
        UnityEventTools.AddPersistentListener(cancel.onClick, controller.HideMenuConfirmation);
    }

    private static void DrawGround(Color32[] p, int s, int variant)
    {
        Fill(p, Charcoal); for (var y = 0; y < s; y += 32) for (var x = 0; x < s; x += 32)
        {
            var inset = 3 + ((x / 32 + y / 32 + variant) % 2) * 2; Box(p,s,x+inset,y+inset,32-inset*2,32-inset*2,new Color32(38,42,47,255));
            if ((x/32 + y/32 + variant) % 3 == 0) Box(p,s,x+14,y+14,4,4,Mid);
        }
        Box(p,s,0,63,s,2,new Color32(22,24,28,255)); Box(p,s,63,0,2,s,new Color32(22,24,28,255));
    }

    private static void DrawPath(Color32[] p, int s, bool left, bool top, bool right, bool bottom)
    {
        Fill(p, Transparent); var c=s/2; var bw=72; var iw=52;
        if(left){Box(p,s,0,c-bw/2,c,bw,Black);Box(p,s,0,c-iw/2,c,iw,Light);} if(right){Box(p,s,c,c-bw/2,c,bw,Black);Box(p,s,c,c-iw/2,c,iw,Light);}
        if(bottom){Box(p,s,c-bw/2,0,bw,c,Black);Box(p,s,c-iw/2,0,iw,c,Light);} if(top){Box(p,s,c-bw/2,c,bw,c,Black);Box(p,s,c-iw/2,c,iw,c,Light);}
        Circle(p,s,c,c,bw/2,Black); Circle(p,s,c,c,iw/2,Light); Circle(p,s,c,c,5,Mid);
    }

    private static void DrawTurret(Color32[] p,int s,int type)
    {
        Fill(p,Transparent); Circle(p,s,64,35,39,Black); Circle(p,s,64,35,31,Mid); Box(p,s,36,28,56,16,Light); Box(p,s,44,43,40,42,Charcoal);
        if(type==0){Box(p,s,48,57,32,24,White);Box(p,s,62,57,4,24,Black);Box(p,s,52,87,24,6,Light);}
        else if(type==1){Circle(p,s,64,75,23,Light);Circle(p,s,64,75,10,Black);for(int i=0;i<8;i++){var a=i*Mathf.PI/4;Box(p,s,60+(int)(Mathf.Cos(a)*25),71+(int)(Mathf.Sin(a)*25),8,8,White);}}
        else {Line(p,s,51,94,58,64,White,7);Line(p,s,77,94,70,64,White,7);Box(p,s,50,52,28,12,Light);Circle(p,s,64,80,20,Charcoal);Circle(p,s,64,80,7,White);}
    }

    private static void DrawPlayer(Color32[] p,int s,int frame)
    {
        Fill(p,Transparent); var bob=frame%2*3; Circle(p,s,64,83+bob,28,Black); Circle(p,s,64,82+bob,20,Charcoal);
        Box(p,s,52,77+bob,24,8,Black); Box(p,s,57,79+bob,4,4,White); Box(p,s,68,79+bob,4,4,White);
        Box(p,s,45,34+bob,38,40,Charcoal); Box(p,s,38+(frame%2)*4,23,18,15,Black); Box(p,s,73-(frame%2)*4,23,18,15,Black);
    }

    private static void DrawLibrary(Color32[] p,int s)
    {
        Fill(p,Transparent); for(var tier=0;tier<3;tier++){var inset=24+tier*28;var y=28+tier*48;Box(p,s,inset,y,s-inset*2,42,Black);Box(p,s,inset+7,y+7,s-(inset+7)*2,28,tier%2==0?Mid:Charcoal);}
        for(var x=42;x<214;x+=28) Box(p,s,x,35,9,118,Light); Line(p,s,128,238,42,154,Black,13); Line(p,s,128,238,214,154,Black,13); Line(p,s,128,225,70,166,White,8); Line(p,s,128,225,186,166,White,8);
        Box(p,s,111,28,34,64,Black); Box(p,s,118,35,20,57,White);
    }

    private static void DrawEnemyCard(Color32[] p,int s){Fill(p,Transparent);Box(p,s,18,18,92,92,Black);Box(p,s,25,25,78,78,White);Box(p,s,31,31,66,66,Light);for(int i=0;i<4;i++){Circle(p,s,18+i*30,18,5,White);Circle(p,s,18+i*30,110,5,White);}}
    private static void DrawBrain(Color32[] p,int s){Fill(p,Transparent);Circle(p,s,49,65,28,Light);Circle(p,s,77,65,28,Light);Line(p,s,64,40,64,90,Black,5);for(int y=48;y<86;y+=14){Line(p,s,38,y,55,y+7,Mid,4);Line(p,s,90,y,73,y+7,Mid,4);}}
    private static void DrawTorch(Color32[] p,int s){Fill(p,Transparent);Box(p,s,57,18,14,57,Mid);Box(p,s,48,70,32,12,Black);Circle(p,s,64,91,22,White);Circle(p,s,64,84,14,Light);}

    private static void DrawObstacle(Color32[] p,int s,int type)
    {
        Fill(p,Transparent);
        if(type==0){Box(p,s,58,16,12,52,Mid);Circle(p,s,64,79,37,Charcoal);Circle(p,s,45,76,21,Mid);Circle(p,s,82,82,22,Light);}
        else if(type==1){Circle(p,s,50,48,31,Mid);Circle(p,s,78,55,34,Charcoal);Line(p,s,30,40,60,74,Light,5);}
        else if(type==2){Fill(p,new Color32(18,21,25,255));for(int y=20;y<118;y+=24){Line(p,s,6,y,46,y+8,Light,4);Line(p,s,70,y+10,122,y+2,Mid,4);}}
        else if(type==3){Fill(p,Transparent);Line(p,s,15,22,63,109,Black,18);Line(p,s,63,109,113,22,Black,18);Line(p,s,31,32,63,91,Mid,12);Line(p,s,63,91,97,32,Light,12);}
        else if(type==4){Fill(p,new Color32(11,12,15,255));for(int y=12;y<128;y+=30)for(int x=(y/30%2)*15;x<128;x+=30)Circle(p,s,x,y,16,new Color32(31,34,40,255));}
        else {Circle(p,s,35,54,28,new Color32(220,222,225,170));Circle(p,s,67,70,39,new Color32(235,235,235,180));Circle(p,s,100,53,25,new Color32(205,208,214,165));Box(p,s,18,38,94,34,new Color32(225,227,230,175));}
    }

    private static void DrawWater(Color32[] p,int s,int variant)
    {
        Fill(p,new Color32(16,19,23,255));
        for(int y=12+variant*3;y<124;y+=22)
        {
            var shift=(variant%2==0?1:-1)*(y/22%2)*12;
            Line(p,s,4+shift,y,45+shift,y+6,variant%2==0?Light:Mid,4);
            Line(p,s,68-shift,y+10,124-shift,y+3,variant%2==0?Mid:Light,4);
        }
        if(variant>=2) Box(p,s,0,variant==2?116:0,s,12,Charcoal);
    }

    private static void DrawMountain(Color32[] p,int s,int variant)
    {
        Fill(p,Transparent); var peakX=variant%2==0?52:76; var peakY=112-(variant/2)*12;
        Line(p,s,5,15,peakX,peakY,Black,20); Line(p,s,peakX,peakY,123,15,Black,20);
        Line(p,s,18,20,peakX,peakY-16,Mid,12); Line(p,s,peakX,peakY-16,110,20,Light,12);
        Line(p,s,peakX,peakY-16,peakX-13,peakY-34,White,7); Line(p,s,peakX,peakY-16,peakX+14,peakY-35,White,7);
    }

    private static void DrawHighlight(Color32[] p,int s){Fill(p,Transparent);Box(p,s,2,2,s-4,5,White);Box(p,s,2,s-7,s-4,5,White);Box(p,s,2,2,5,s-4,White);Box(p,s,s-7,2,5,s-4,White);}
    private static void DrawCircleUi(Color32[] p,int s){Fill(p,Transparent);Circle(p,s,64,64,61,new Color32(225,225,225,190));Circle(p,s,64,64,52,new Color32(45,47,53,180));}
    private static void DrawPause(Color32[] p,int s){Fill(p,Transparent);Box(p,s,34,22,20,84,White);Box(p,s,74,22,20,84,White);}
    private static void DrawTime(Color32[] p,int s){Fill(p,Transparent);Circle(p,s,64,62,43,White);Circle(p,s,64,62,35,Charcoal);Line(p,s,64,62,64,84,White,6);Line(p,s,64,62,82,52,White,6);Box(p,s,51,108,26,8,White);}
    private static void DrawGun(Color32[] p,int s){Fill(p,Transparent);Box(p,s,18,65,76,20,White);Box(p,s,86,70,24,10,Light);Box(p,s,48,34,22,34,Mid);Line(p,s,68,65,88,45,White,8);}
    private static void DrawReload(Color32[] p,int s){Fill(p,Transparent);for(int i=0;i<28;i++){var a=(35+i*8)*Mathf.Deg2Rad;Circle(p,s,64+(int)(Mathf.Cos(a)*38),64+(int)(Mathf.Sin(a)*38),5,White);}Line(p,s,91,91,108,89,White,7);Line(p,s,91,91,94,108,White,7);Line(p,s,37,37,20,39,White,7);Line(p,s,37,37,34,20,White,7);}

    private static void Save(string name,int size,Action<Color32[]> draw)
    {
        var pixels=new Color32[size*size];draw(pixels);var texture=new Texture2D(size,size,TextureFormat.RGBA32,false);texture.SetPixels32(pixels);texture.Apply(false,false);
        File.WriteAllBytes($"{ArtFolder}/{name}.png",texture.EncodeToPNG());Object.DestroyImmediate(texture);
    }
    private static void Fill(Color32[] p,Color32 c){for(int i=0;i<p.Length;i++)p[i]=c;}
    private static void Box(Color32[] p,int s,int x,int y,int w,int h,Color32 c){for(int yy=Mathf.Max(0,y);yy<Mathf.Min(s,y+h);yy++)for(int xx=Mathf.Max(0,x);xx<Mathf.Min(s,x+w);xx++)p[yy*s+xx]=c;}
    private static void Circle(Color32[] p,int s,int cx,int cy,int r,Color32 c){var rr=r*r;for(int y=Mathf.Max(0,cy-r);y<Mathf.Min(s,cy+r+1);y++)for(int x=Mathf.Max(0,cx-r);x<Mathf.Min(s,cx+r+1);x++)if((x-cx)*(x-cx)+(y-cy)*(y-cy)<=rr)p[y*s+x]=c;}
    private static void Line(Color32[] p,int s,int x0,int y0,int x1,int y1,Color32 c,int width){var dx=Math.Abs(x1-x0);var sx=x0<x1?1:-1;var dy=-Math.Abs(y1-y0);var sy=y0<y1?1:-1;var err=dx+dy;while(true){Circle(p,s,x0,y0,Mathf.Max(1,width/2),c);if(x0==x1&&y0==y1)break;var e2=2*err;if(e2>=dy){err+=dy;x0+=sx;}if(e2<=dx){err+=dx;y0+=sy;}}}

    private static Tile CreateTileAsset(string name,Sprite sprite){var path=$"{TileFolder}/{name}.asset";var tile=AssetDatabase.LoadAssetAtPath<Tile>(path);if(tile==null){tile=ScriptableObject.CreateInstance<Tile>();AssetDatabase.CreateAsset(tile,path);}tile.sprite=sprite;tile.color=Color.white;tile.colliderType=Tile.ColliderType.None;EditorUtility.SetDirty(tile);return tile;}
    private static Sprite Sprite(string name)=>AssetDatabase.LoadAssetAtPath<Sprite>($"{ArtFolder}/{name}.png")??throw new InvalidOperationException($"Missing sprite {name}");
    private static Tilemap Tilemap(string name,Transform parent,int order){var go=new GameObject(name);go.transform.SetParent(parent,false);var map=go.AddComponent<Tilemap>();var renderer=go.AddComponent<TilemapRenderer>();renderer.sharedMaterial=LitMaterial();renderer.sortingOrder=order;return map;}
    private static Material LitMaterial(){const string path=ArtFolder+"/MonochromeSpriteLit.mat";var material=AssetDatabase.LoadAssetAtPath<Material>(path);if(material!=null)return material;var shader=Shader.Find("Universal Render Pipeline/2D/Sprite-Lit-Default");if(shader==null)throw new InvalidOperationException("URP 2D lit sprite shader is unavailable.");material=new Material(shader){name="Monochrome Sprite Lit"};AssetDatabase.CreateAsset(material,path);return material;}
    private static void BuildPalette(TileBase[] tiles){var source=new GameObject("KeySlaught Monochrome Palette");source.AddComponent<Grid>();var layer=new GameObject("Layer1");layer.transform.SetParent(source.transform,false);var map=layer.AddComponent<Tilemap>();layer.AddComponent<TilemapRenderer>();for(int i=0;i<tiles.Length;i++)map.SetTile(new Vector3Int(i%8,-i/8,0),tiles[i]);PrefabUtility.SaveAsPrefabAsset(source,$"{PaletteFolder}/KeySlaughtPalette.prefab");Object.DestroyImmediate(source);}
    private static GameObject ReplaceChild(Transform parent,string name){var old=parent.Find(name);if(old!=null)Object.DestroyImmediate(old.gameObject);var go=new GameObject(name);go.transform.SetParent(parent,false);return go;}
    private static T Require<T>(Transform root,string child)where T:Component{var found=root.Find(child)?.GetComponent<T>();return found??throw new InvalidOperationException($"Missing {typeof(T).Name} at {child}");}
    private static void Folder(string parent,string name){var path=$"{parent}/{name}";if(!AssetDatabase.IsValidFolder(path))AssetDatabase.CreateFolder(parent,name);}
    private static void EnsureEventSystem(){var eventSystem=Object.FindFirstObjectByType<EventSystem>();if(eventSystem==null){var go=new GameObject("EventSystem");go.AddComponent<EventSystem>();go.AddComponent<InputSystemUIInputModule>();}}

    private static Image Panel(string name,Transform parent,Sprite sprite,Color color){var go=new GameObject(name,typeof(RectTransform),typeof(CanvasRenderer),typeof(Image));go.transform.SetParent(parent,false);var image=go.GetComponent<Image>();image.sprite=sprite;image.color=color;return image;}
    private static Image Image(string name,Transform parent,Sprite sprite,Color color){var image=Panel(name,parent,sprite,color);image.preserveAspect=true;return image;}
    private static Text Label(string text,Transform parent,Font font,int size,Color color){var go=new GameObject(string.IsNullOrEmpty(text)?"Label":"Label "+text,typeof(RectTransform),typeof(CanvasRenderer),typeof(Text));go.transform.SetParent(parent,false);var label=go.GetComponent<Text>();label.font=font;label.fontSize=size;label.color=color;label.text=text;label.alignment=TextAnchor.MiddleCenter;label.horizontalOverflow=HorizontalWrapMode.Wrap;label.verticalOverflow=VerticalWrapMode.Overflow;return label;}
    private static Button TextButton(string text,Transform parent,Font font,Sprite sprite,int fontSize){var panel=Panel("Button "+text,parent,sprite,new Color(.72f,.73f,.76f,1));var button=panel.gameObject.AddComponent<Button>();button.targetGraphic=panel;var colors=button.colors;colors.normalColor=Color.white;colors.highlightedColor=new Color(1f,1f,1f,1);colors.pressedColor=new Color(.56f,.58f,.62f,1);colors.selectedColor=new Color(.88f,.89f,.92f,1);colors.fadeDuration=.08f;button.colors=colors;var label=Label(text,panel.transform,font,fontSize,Black);label.fontStyle=FontStyle.Bold;label.raycastTarget=false;Rect(label.rectTransform,Vector2.zero,Vector2.one,new Vector2(4,3),new Vector2(-8,-6));return button;}
    private static Button IconButton(string name,Transform parent,Sprite icon,Sprite solid){var panel=Panel(name,parent,solid,new Color(.14f,.15f,.17f,1));var button=panel.gameObject.AddComponent<Button>();button.targetGraphic=panel;var colors=button.colors;colors.highlightedColor=Color.white;colors.pressedColor=new Color(.45f,.46f,.49f,1);colors.fadeDuration=.08f;button.colors=colors;var image=Image(name+" Icon",panel.transform,icon,Color.white);image.raycastTarget=false;Rect(image.rectTransform,Vector2.zero,Vector2.one,new Vector2(10,10),new Vector2(-20,-20));return button;}
    private static void Rect(RectTransform rect,Vector2 min,Vector2 max,Vector2 position,Vector2 size){rect.anchorMin=min;rect.anchorMax=max;rect.pivot=min==max?min:new Vector2(.5f,.5f);rect.anchoredPosition=position;rect.sizeDelta=size;}

    private readonly struct MapData
    {
        public MapData(Tilemap ground,Tilemap path,Tilemap blocked,Vector3Int[] route,HashSet<Vector3Int> cells){Ground=ground;Path=path;Blocked=blocked;Route=route;PathCells=cells;}
        public Tilemap Ground{get;} public Tilemap Path{get;} public Tilemap Blocked{get;} public Vector3Int[] Route{get;} public HashSet<Vector3Int> PathCells{get;}
    }
}
