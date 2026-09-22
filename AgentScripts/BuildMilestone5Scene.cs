using System;
using System.Collections.Generic;
using KeySlaught.SceneGameplay;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;
using UnityEngine.UI;
using Object = UnityEngine.Object;

public static class BuildMilestone5Scene
{
    private const string GameplayRootName = "KeySlaught Gameplay";
    private const string PixelArtFolder = "Assets/Art/Pixel";
    private const string FontPath = "Assets/Fonts/PressStart2P/PressStart2P-Regular.ttf";
    private const string EnemyPrefabPath = "Assets/Prefabs/EnemyPrototype.prefab";
    private const string TileFolder = "Assets/Tiles";
    private const string PaletteFolder = "Assets/TilePalettes";

    public static string Build()
    {
        var scene = SceneManager.GetActiveScene();
        if (!scene.IsValid() || scene.path != "Assets/Scenes/SampleScene.unity")
        {
            throw new InvalidOperationException("Open Assets/Scenes/SampleScene.unity before building Milestone 5.");
        }

        EnsureFolder("Assets", "Tiles");
        EnsureFolder("Assets", "TilePalettes");
        AssetDatabase.Refresh();
        ConfigureArtImports();

        var root = GameObject.Find($"/{GameplayRootName}");
        if (root == null)
        {
            throw new InvalidOperationException("The authored KeySlaught gameplay root is missing.");
        }

        var player = RequireComponent<PlayerMover>(root.transform, "Player");
        var coordinator = RequireComponent<GameplaySceneCoordinator>(root.transform, "Gameplay Coordinator");
        var combat = coordinator.GetComponent<GameplayCombatController>();
        var library = RequireComponent<LibraryEndpoint>(root.transform, "Divine Library");
        if (combat == null)
        {
            throw new InvalidOperationException("Milestone 4 GameplayCombatController is missing.");
        }

        var groundSprite = LoadSprite($"{PixelArtFolder}/Ground_128.png");
        var pathSprite = LoadSprite($"{PixelArtFolder}/EnemyPath_128.png");
        var enemySprite = LoadSprite($"{PixelArtFolder}/EnemyCard_128.png");
        var teacherSprite = LoadSprite($"{PixelArtFolder}/Turret_Teacher_128.png");
        var engineerSprite = LoadSprite($"{PixelArtFolder}/Turret_Engineer_128.png");
        var scientistSprite = LoadSprite($"{PixelArtFolder}/Turret_Scientist_128.png");
        var playerFrames = new[]
        {
            LoadSprite($"{PixelArtFolder}/Player_Walk_0_128.png"),
            LoadSprite($"{PixelArtFolder}/Player_Walk_1_128.png"),
            LoadSprite($"{PixelArtFolder}/Player_Walk_2_128.png"),
            LoadSprite($"{PixelArtFolder}/Player_Walk_3_128.png")
        };
        var font = AssetDatabase.LoadAssetAtPath<Font>(FontPath);
        if (font == null)
        {
            throw new InvalidOperationException("Press Start 2P font was not imported.");
        }

        var groundTile = CreateOrUpdateTile($"{TileFolder}/Ground.asset", groundSprite, Color.white);
        var pathTile = CreateOrUpdateTile($"{TileFolder}/EnemyPath.asset", pathSprite, Color.white);
        var libraryTile = CreateOrUpdateTile(
            $"{TileFolder}/LibraryContext.asset",
            groundSprite,
            new Color(1f, 0.72f, 0.24f, 0.72f));
        var turretPadTile = CreateOrUpdateTile(
            $"{TileFolder}/TurretPad.asset",
            groundSprite,
            new Color(0.2f, 0.85f, 1f, 0.78f));
        BuildTilePalette(groundTile, pathTile, libraryTile, turretPadTile);

        var map = BuildAuthoredTilemaps(
            root.transform,
            groundTile,
            pathTile,
            libraryTile,
            turretPadTile);
        var pads = BuildTurretPads(
            root.transform,
            map.Context,
            teacherSprite,
            engineerSprite,
            scientistSprite);

        ApplyPixelPresentation(root.transform, player, playerFrames, enemySprite, font);
        BuildDualInputHud(root.transform, player, combat, library, map.Context, libraryTile, turretPadTile, pads, font);

        var arenaRenderer = root.transform.Find("Arena Background")?.GetComponent<SpriteRenderer>();
        if (arenaRenderer != null)
        {
            arenaRenderer.enabled = false;
        }

        var oldPathRenderer = root.transform.Find("Enemy Path")?.GetComponent<LineRenderer>();
        if (oldPathRenderer != null)
        {
            oldPathRenderer.enabled = false;
        }

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        return "Built Milestone 5 dual-input HUD, tilemaps, pixel art, context actions, and turret-pad presentation.";
    }

    private static void ConfigureArtImports()
    {
        var spritePaths = new[]
        {
            $"{PixelArtFolder}/Ground_128.png",
            $"{PixelArtFolder}/EnemyPath_128.png",
            $"{PixelArtFolder}/EnemyCard_128.png",
            $"{PixelArtFolder}/Turret_Teacher_128.png",
            $"{PixelArtFolder}/Turret_Engineer_128.png",
            $"{PixelArtFolder}/Turret_Scientist_128.png",
            $"{PixelArtFolder}/Player_Walk_0_128.png",
            $"{PixelArtFolder}/Player_Walk_1_128.png",
            $"{PixelArtFolder}/Player_Walk_2_128.png",
            $"{PixelArtFolder}/Player_Walk_3_128.png"
        };

        foreach (var path in spritePaths)
        {
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null)
            {
                throw new InvalidOperationException($"Texture import failed for {path}.");
            }

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

    private static Tile CreateOrUpdateTile(string path, Sprite sprite, Color color)
    {
        var tile = AssetDatabase.LoadAssetAtPath<Tile>(path);
        if (tile == null)
        {
            tile = ScriptableObject.CreateInstance<Tile>();
            AssetDatabase.CreateAsset(tile, path);
        }

        tile.sprite = sprite;
        tile.color = color;
        tile.colliderType = Tile.ColliderType.None;
        EditorUtility.SetDirty(tile);
        return tile;
    }

    private static void BuildTilePalette(params TileBase[] tiles)
    {
        var source = new GameObject("KeySlaught Tile Palette");
        source.AddComponent<Grid>();
        var tilemapObject = new GameObject("Layer1");
        tilemapObject.transform.SetParent(source.transform, false);
        var tilemap = tilemapObject.AddComponent<Tilemap>();
        tilemapObject.AddComponent<TilemapRenderer>();
        for (var index = 0; index < tiles.Length; index++)
        {
            tilemap.SetTile(new Vector3Int(index, 0, 0), tiles[index]);
        }

        PrefabUtility.SaveAsPrefabAsset(source, $"{PaletteFolder}/KeySlaughtPalette.prefab");
        Object.DestroyImmediate(source);
    }

    private static MapLayers BuildAuthoredTilemaps(
        Transform root,
        TileBase groundTile,
        TileBase pathTile,
        TileBase libraryTile,
        TileBase turretPadTile)
    {
        var existing = root.Find("Authored Tilemaps");
        if (existing != null)
        {
            Object.DestroyImmediate(existing.gameObject);
        }

        var gridObject = new GameObject("Authored Tilemaps");
        gridObject.transform.SetParent(root, false);
        gridObject.AddComponent<Grid>();

        var ground = CreateTilemap("Ground Tilemap", gridObject.transform, -20);
        for (var x = -8; x <= 7; x++)
        {
            for (var y = -9; y <= 8; y++)
            {
                ground.SetTile(new Vector3Int(x, y, 0), groundTile);
            }
        }

        var path = CreateTilemap("Enemy Path Tilemap", gridObject.transform, -5);
        var waypoints = new[]
        {
            new Vector2Int(-7, -8),
            new Vector2Int(-7, -3),
            new Vector2Int(-4, 1),
            new Vector2Int(3, 1),
            new Vector2Int(3, 5),
            new Vector2Int(0, 7)
        };
        for (var index = 0; index < waypoints.Length - 1; index++)
        {
            foreach (var cell in RasterLine(waypoints[index], waypoints[index + 1]))
            {
                path.SetTile(cell, pathTile);
            }
        }

        var context = CreateTilemap("Context Tilemap", gridObject.transform, -3);
        context.SetTile(new Vector3Int(0, 7, 0), libraryTile);
        context.SetTile(new Vector3Int(-1, 7, 0), libraryTile);
        context.SetTile(new Vector3Int(1, 7, 0), libraryTile);
        context.SetTile(new Vector3Int(-3, -4, 0), turretPadTile);
        context.SetTile(new Vector3Int(4, -2, 0), turretPadTile);
        context.SetTile(new Vector3Int(5, 4, 0), turretPadTile);
        return new MapLayers(ground, path, context);
    }

    private static IEnumerable<Vector3Int> RasterLine(Vector2Int start, Vector2Int end)
    {
        var x = start.x;
        var y = start.y;
        var dx = Mathf.Abs(end.x - start.x);
        var dy = Mathf.Abs(end.y - start.y);
        var sx = start.x < end.x ? 1 : -1;
        var sy = start.y < end.y ? 1 : -1;
        var error = dx - dy;
        while (true)
        {
            yield return new Vector3Int(x, y, 0);
            if (x == end.x && y == end.y)
            {
                yield break;
            }

            var doubled = error * 2;
            if (doubled > -dy)
            {
                error -= dy;
                x += sx;
            }

            if (doubled < dx)
            {
                error += dx;
                y += sy;
            }
        }
    }

    private static Tilemap CreateTilemap(string name, Transform parent, int sortingOrder)
    {
        var gameObject = new GameObject(name);
        gameObject.transform.SetParent(parent, false);
        var tilemap = gameObject.AddComponent<Tilemap>();
        var renderer = gameObject.AddComponent<TilemapRenderer>();
        renderer.sortingOrder = sortingOrder;
        return tilemap;
    }

    private static TurretPadController[] BuildTurretPads(
        Transform root,
        Tilemap contextTilemap,
        Sprite teacher,
        Sprite engineer,
        Sprite scientist)
    {
        var existing = root.Find("Turret Pads");
        if (existing != null)
        {
            Object.DestroyImmediate(existing.gameObject);
        }

        var padRoot = new GameObject("Turret Pads");
        padRoot.transform.SetParent(root, false);
        var cells = new[]
        {
            new Vector3Int(-3, -4, 0),
            new Vector3Int(4, -2, 0),
            new Vector3Int(5, 4, 0)
        };
        var pads = new TurretPadController[cells.Length];
        for (var index = 0; index < cells.Length; index++)
        {
            var padObject = new GameObject($"Turret Pad {index + 1:00}");
            padObject.transform.SetParent(padRoot.transform, false);
            padObject.transform.position = contextTilemap.GetCellCenterWorld(cells[index]);
            var renderer = padObject.AddComponent<SpriteRenderer>();
            renderer.sortingOrder = 12;
            var pad = padObject.AddComponent<TurretPadController>();
            pad.Configure(cells[index], renderer, teacher, engineer, scientist);
            EditorUtility.SetDirty(pad);
            pads[index] = pad;
        }

        return pads;
    }

    private static void ApplyPixelPresentation(
        Transform root,
        PlayerMover player,
        Sprite[] playerFrames,
        Sprite enemySprite,
        Font font)
    {
        var playerRenderer = player.GetComponent<SpriteRenderer>();
        playerRenderer.sprite = playerFrames[0];
        playerRenderer.color = Color.white;
        playerRenderer.sortingOrder = 15;
        player.transform.rotation = Quaternion.identity;
        player.transform.localScale = new Vector3(1.1f, 1.1f, 1f);
        var animator = player.GetComponent<PlayerPixelAnimator>();
        if (animator == null)
        {
            animator = player.gameObject.AddComponent<PlayerPixelAnimator>();
        }
        animator.Configure(player, playerRenderer, playerFrames);
        EditorUtility.SetDirty(animator);

        var prefabRoot = PrefabUtility.LoadPrefabContents(EnemyPrefabPath);
        try
        {
            var renderer = prefabRoot.GetComponent<SpriteRenderer>();
            renderer.sprite = enemySprite;
            renderer.color = Color.white;
            prefabRoot.transform.localScale = Vector3.one;
            var label = prefabRoot.GetComponentInChildren<TextMesh>(true);
            label.font = font;
            label.color = new Color(0.12f, 0.04f, 0.08f, 1f);
            label.characterSize = 0.085f;
            label.fontSize = 48;
            label.anchor = TextAnchor.MiddleCenter;
            label.transform.localPosition = new Vector3(0f, 0.1f, -0.1f);
            label.GetComponent<MeshRenderer>().sharedMaterial = font.material;
            PrefabUtility.SaveAsPrefabAsset(prefabRoot, EnemyPrefabPath);
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(prefabRoot);
        }

        foreach (var label in root.GetComponentsInChildren<TextMesh>(true))
        {
            label.font = font;
            label.GetComponent<MeshRenderer>().sharedMaterial = font.material;
        }
    }

    private static void BuildDualInputHud(
        Transform root,
        PlayerMover player,
        GameplayCombatController combat,
        LibraryEndpoint library,
        Tilemap contextTilemap,
        TileBase libraryTile,
        TileBase turretPadTile,
        TurretPadController[] pads,
        Font font)
    {
        var existing = GameObject.Find("/Dual Input HUD");
        if (existing != null)
        {
            Object.DestroyImmediate(existing);
        }

        var canvasObject = new GameObject("Dual Input HUD");
        var canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;
        var scaler = canvasObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(900f, 1000f);
        scaler.matchWidthOrHeight = 0.5f;
        canvasObject.AddComponent<GraphicRaycaster>();

        EnsureEventSystem();
        var solidSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Prototype/SolidSprite.asset");
        if (solidSprite == null)
        {
            throw new InvalidOperationException("Prototype solid sprite is unavailable for the UI.");
        }

        BuildAlphabetKeyboard(canvas.transform, combat, font, solidSprite);
        BuildJoystick(canvas.transform, player, font, solidSprite);
        var panel = BuildContextPanel(canvas.transform, font, solidSprite, out var title, out var status, out var buttons, out var labels);
        var controller = canvasObject.AddComponent<TileContextActionPanel>();
        controller.Configure(
            player,
            contextTilemap,
            libraryTile,
            turretPadTile,
            library,
            pads,
            panel,
            title,
            status,
            buttons,
            labels);
        EditorUtility.SetDirty(controller);
        for (var index = 0; index < buttons.Length; index++)
        {
            var action = buttons[index].gameObject.AddComponent<ContextActionButton>();
            action.Configure(controller, index + 1);
            EditorUtility.SetDirty(action);
        }
    }

    private static void BuildAlphabetKeyboard(Transform canvas, GameplayCombatController combat, Font font, Sprite sprite)
    {
        var panel = CreatePanel("Alphabet Keyboard", canvas, sprite, new Color(0.02f, 0.04f, 0.09f, 0.88f));
        SetRect(panel.GetComponent<RectTransform>(), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(40f, 12f), new Vector2(540f, 112f));
        for (var index = 0; index < 26; index++)
        {
            var row = index / 13;
            var column = index % 13;
            var letter = (char)('A' + index);
            var button = CreateButton(letter.ToString(), panel.transform, font, sprite, 12, new Color(0.08f, 0.18f, 0.28f, 0.96f));
            SetRect(
                button.GetComponent<RectTransform>(),
                new Vector2(0f, 1f),
                new Vector2(0f, 1f),
                new Vector2(0f, 1f),
                new Vector2(8f + column * 40f, -8f - row * 48f),
                new Vector2(34f, 40f));
            var letterButton = button.gameObject.AddComponent<OnScreenLetterButton>();
            letterButton.Configure(combat, letter);
            EditorUtility.SetDirty(letterButton);
        }

        var refresh = CreateButton("REFRESH", canvas, font, sprite, 11, new Color(0.75f, 0.38f, 0.12f, 0.96f));
        SetRect(refresh.GetComponent<RectTransform>(), new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-18f, 18f), new Vector2(145f, 52f));
        var refreshButton = refresh.gameObject.AddComponent<OnScreenRefreshButton>();
        refreshButton.Configure(combat);
        EditorUtility.SetDirty(refreshButton);
    }

    private static void BuildJoystick(Transform canvas, PlayerMover player, Font font, Sprite sprite)
    {
        var label = CreateText("MOVE", canvas, font, 11, Color.white);
        SetRect(label.rectTransform, new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(20f, 170f), new Vector2(150f, 25f));
        var basePanel = CreatePanel("Virtual Joystick", canvas, sprite, new Color(0.08f, 0.22f, 0.32f, 0.72f));
        SetRect(basePanel.GetComponent<RectTransform>(), new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(20f, 18f), new Vector2(145f, 145f));
        var handle = CreatePanel("Handle", basePanel.transform, sprite, new Color(0.2f, 0.85f, 1f, 0.9f));
        SetRect(handle.GetComponent<RectTransform>(), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(55f, 55f));
        var joystick = basePanel.AddComponent<VirtualJoystick>();
        joystick.Configure(player, basePanel.GetComponent<RectTransform>(), handle.GetComponent<RectTransform>());
        EditorUtility.SetDirty(joystick);
    }

    private static GameObject BuildContextPanel(
        Transform canvas,
        Font font,
        Sprite sprite,
        out Text title,
        out Text status,
        out Button[] buttons,
        out Text[] labels)
    {
        var panel = CreatePanel("Context Actions", canvas, sprite, new Color(0.025f, 0.06f, 0.12f, 0.94f));
        SetRect(panel.GetComponent<RectTransform>(), new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-12f, 95f), new Vector2(265f, 390f));
        title = CreateText("CONTEXT", panel.transform, font, 14, new Color(0.4f, 0.92f, 1f, 1f));
        SetRect(title.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -14f), new Vector2(245f, 42f));
        buttons = new Button[9];
        labels = new Text[9];
        for (var index = 0; index < 9; index++)
        {
            buttons[index] = CreateButton(string.Empty, panel.transform, font, sprite, 10, new Color(0.1f, 0.2f, 0.3f, 0.96f));
            buttons[index].gameObject.name = $"Context Action {index + 1}";
            buttons[index].GetComponentInChildren<Text>().gameObject.name = $"Context Action {index + 1} Label";
            SetRect(buttons[index].GetComponent<RectTransform>(), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -62f - index * 34f), new Vector2(235f, 29f));
            labels[index] = buttons[index].GetComponentInChildren<Text>();
        }
        status = CreateText(string.Empty, panel.transform, font, 9, new Color(1f, 0.72f, 0.25f, 1f));
        status.gameObject.name = "Context Status";
        status.alignment = TextAnchor.MiddleCenter;
        SetRect(status.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 10f), new Vector2(240f, 48f));
        return panel;
    }

    private static GameObject CreatePanel(string name, Transform parent, Sprite sprite, Color color)
    {
        var gameObject = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        gameObject.transform.SetParent(parent, false);
        var image = gameObject.GetComponent<Image>();
        image.sprite = sprite;
        image.color = color;
        return gameObject;
    }

    private static Button CreateButton(string text, Transform parent, Font font, Sprite sprite, int fontSize, Color color)
    {
        var gameObject = CreatePanel($"Button {text}", parent, sprite, color);
        var button = gameObject.AddComponent<Button>();
        button.targetGraphic = gameObject.GetComponent<Image>();
        var label = CreateText(text, gameObject.transform, font, fontSize, Color.white);
        label.raycastTarget = false;
        label.alignment = TextAnchor.MiddleCenter;
        SetRect(label.rectTransform, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
        label.rectTransform.offsetMin = new Vector2(3f, 2f);
        label.rectTransform.offsetMax = new Vector2(-3f, -2f);
        return button;
    }

    private static Text CreateText(string text, Transform parent, Font font, int fontSize, Color color)
    {
        var gameObject = new GameObject($"Label {text}", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
        gameObject.transform.SetParent(parent, false);
        var label = gameObject.GetComponent<Text>();
        label.font = font;
        label.fontSize = fontSize;
        label.color = color;
        label.text = text;
        label.alignment = TextAnchor.MiddleCenter;
        label.horizontalOverflow = HorizontalWrapMode.Wrap;
        label.verticalOverflow = VerticalWrapMode.Overflow;
        return label;
    }

    private static void SetRect(
        RectTransform rect,
        Vector2 anchorMin,
        Vector2 anchorMax,
        Vector2 pivot,
        Vector2 anchoredPosition,
        Vector2 size)
    {
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.pivot = pivot;
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = size;
    }

    private static void EnsureEventSystem()
    {
        var eventSystem = Object.FindFirstObjectByType<EventSystem>();
        if (eventSystem != null)
        {
            return;
        }

        var eventObject = new GameObject("EventSystem");
        eventObject.AddComponent<EventSystem>();
        eventObject.AddComponent<InputSystemUIInputModule>();
    }

    private static Sprite LoadSprite(string path)
    {
        var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
        if (sprite == null)
        {
            throw new InvalidOperationException($"Sprite is unavailable at {path}.");
        }

        return sprite;
    }

    private static T RequireComponent<T>(Transform root, string childName) where T : Component
    {
        var child = root.Find(childName);
        var component = child == null ? null : child.GetComponent<T>();
        if (component == null)
        {
            throw new InvalidOperationException($"Required {typeof(T).Name} is missing from {childName}.");
        }

        return component;
    }

    private static void EnsureFolder(string parent, string name)
    {
        var path = $"{parent}/{name}";
        if (!AssetDatabase.IsValidFolder(path))
        {
            AssetDatabase.CreateFolder(parent, name);
        }
    }

    private readonly struct MapLayers
    {
        public MapLayers(Tilemap ground, Tilemap path, Tilemap context)
        {
            Ground = ground;
            Path = path;
            Context = context;
        }

        public Tilemap Ground { get; }

        public Tilemap Path { get; }

        public Tilemap Context { get; }
    }
}
