using System;
using KeySlaught.SceneGameplay;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

public static class BuildMilestone3Scene
{
    private const string GameplayRootName = "KeySlaught Gameplay";
    private const string EnemyPrefabPath = "Assets/Prefabs/EnemyPrototype.prefab";
    private const string BookDefinitionPath = "Assets/Data/Enemies/Book.asset";
    private const string HistoryDefinitionPath = "Assets/Data/Enemies/History.asset";
    private const string LineMaterialPath = "Assets/Art/Prototype/PrototypeLine.mat";
    private const string SolidSpritePath = "Assets/Art/Prototype/SolidSprite.asset";

    public static string Build()
    {
        var scene = SceneManager.GetActiveScene();
        if (!scene.IsValid() || scene.path != "Assets/Scenes/SampleScene.unity")
        {
            throw new InvalidOperationException(
                "Open Assets/Scenes/SampleScene.unity before building Milestone 3.");
        }

        EnsureFolder("Assets", "Prefabs");
        EnsureFolder("Assets", "Data");
        EnsureFolder("Assets/Data", "Enemies");
        EnsureFolder("Assets", "Art");
        EnsureFolder("Assets/Art", "Prototype");

        var squareSprite = GetOrCreateSolidSprite();
        var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (squareSprite == null || font == null)
        {
            throw new InvalidOperationException("Required Unity built-in prototype assets were unavailable.");
        }

        var lineMaterial = GetOrCreateLineMaterial();
        var book = GetOrCreateEnemyDefinition(BookDefinitionPath, "BOOK", 1.45f);
        var history = GetOrCreateEnemyDefinition(HistoryDefinitionPath, "HISTORY", 1.1f);
        var enemyPrefab = CreateEnemyPrefab(squareSprite, font);

        var existingRoot = GameObject.Find($"/{GameplayRootName}");
        if (existingRoot != null)
        {
            Object.DestroyImmediate(existingRoot);
        }

        var root = new GameObject(GameplayRootName);
        var arena = CreateSpriteObject(
            "Arena Background",
            root.transform,
            squareSprite,
            new Color(0.055f, 0.075f, 0.12f, 1f),
            Vector3.zero,
            new Vector3(16f, 18f, 1f),
            -20);
        arena.transform.SetAsFirstSibling();

        CreateBorder(root.transform, lineMaterial);
        var path = CreatePath(root.transform, lineMaterial);
        var library = CreateLibrary(root.transform, squareSprite, font);
        var player = CreatePlayer(root.transform, squareSprite, lineMaterial);
        var spawner = CreateSpawner(root.transform, enemyPrefab, path, book, history);

        var coordinatorObject = new GameObject("Gameplay Coordinator");
        coordinatorObject.transform.SetParent(root.transform);
        var coordinator = coordinatorObject.AddComponent<GameplaySceneCoordinator>();
        SetObjectReference(coordinator, "player", player);
        SetObjectReference(coordinator, "spawner", spawner);
        SetObjectReference(coordinator, "library", library);
        SetFloat(coordinator, "playerAttackRange", 4f);

        CreateWorldText(
            "Prototype Title",
            root.transform,
            font,
            "KEYSLAUGHT  //  PATH PROTOTYPE",
            new Vector3(0f, 8.45f, 0f),
            0.075f,
            new Color(0.55f, 0.9f, 1f, 1f),
            TextAnchor.MiddleCenter,
            30);
        CreateWorldText(
            "Movement Hint",
            root.transform,
            font,
            "WASD / ARROWS  -  MOVE",
            new Vector3(0f, -8.45f, 0f),
            0.075f,
            new Color(0.7f, 0.78f, 0.9f, 1f),
            TextAnchor.MiddleCenter,
            30);

        ConfigureCamera();
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        return $"Built {GameplayRootName} with path, player, library, spawner, and prefab.";
    }

    private static Material GetOrCreateLineMaterial()
    {
        var material = AssetDatabase.LoadAssetAtPath<Material>(LineMaterialPath);
        if (material != null)
        {
            return material;
        }

        var shader = Shader.Find("Sprites/Default");
        if (shader == null)
        {
            throw new InvalidOperationException("Sprites/Default shader was unavailable.");
        }

        material = new Material(shader) { name = "Prototype Line" };
        AssetDatabase.CreateAsset(material, LineMaterialPath);
        return material;
    }

    private static Sprite GetOrCreateSolidSprite()
    {
        foreach (var asset in AssetDatabase.LoadAllAssetsAtPath(SolidSpritePath))
        {
            if (asset is Sprite existingSprite)
            {
                return existingSprite;
            }
        }

        var texture = new Texture2D(1, 1, TextureFormat.RGBA32, false)
        {
            name = "Solid White Texture",
            filterMode = FilterMode.Point,
            wrapMode = TextureWrapMode.Clamp
        };
        texture.SetPixel(0, 0, Color.white);
        texture.Apply();
        AssetDatabase.CreateAsset(texture, SolidSpritePath);

        var sprite = Sprite.Create(
            texture,
            new Rect(0f, 0f, 1f, 1f),
            new Vector2(0.5f, 0.5f),
            1f);
        sprite.name = "Solid Square";
        AssetDatabase.AddObjectToAsset(sprite, texture);
        AssetDatabase.SaveAssets();
        return sprite;
    }

    private static EnemyDefinition GetOrCreateEnemyDefinition(string path, string word, float speed)
    {
        var definition = AssetDatabase.LoadAssetAtPath<EnemyDefinition>(path);
        if (definition == null)
        {
            definition = ScriptableObject.CreateInstance<EnemyDefinition>();
            AssetDatabase.CreateAsset(definition, path);
        }

        var serialized = new SerializedObject(definition);
        serialized.FindProperty("word").stringValue = word;
        serialized.FindProperty("movementSpeed").floatValue = speed;
        serialized.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(definition);
        return definition;
    }

    private static EnemyAgent CreateEnemyPrefab(Sprite sprite, Font font)
    {
        var source = new GameObject("EnemyPrototype");
        var renderer = source.AddComponent<SpriteRenderer>();
        renderer.sprite = sprite;
        renderer.color = new Color(0.95f, 0.28f, 0.34f, 1f);
        renderer.sortingOrder = 10;
        source.transform.localScale = new Vector3(0.75f, 0.75f, 1f);

        var label = CreateWorldText(
            "Word Label",
            source.transform,
            font,
            "BOOK",
            new Vector3(0f, 0.9f, 0f),
            0.16f,
            Color.white,
            TextAnchor.LowerCenter,
            20);

        var agent = source.AddComponent<EnemyAgent>();
        SetObjectReference(agent, "wordLabel", label);

        var prefab = PrefabUtility.SaveAsPrefabAsset(source, EnemyPrefabPath);
        Object.DestroyImmediate(source);
        return prefab.GetComponent<EnemyAgent>();
    }

    private static WaypointPath CreatePath(Transform parent, Material material)
    {
        var pathObject = new GameObject("Enemy Path");
        pathObject.transform.SetParent(parent);
        var renderer = pathObject.AddComponent<LineRenderer>();
        renderer.material = material;
        renderer.startColor = new Color(0.42f, 0.28f, 0.2f, 1f);
        renderer.endColor = new Color(0.64f, 0.43f, 0.24f, 1f);
        renderer.startWidth = 0.85f;
        renderer.endWidth = 0.65f;
        renderer.numCornerVertices = 6;
        renderer.numCapVertices = 6;
        renderer.sortingOrder = -5;
        renderer.useWorldSpace = true;

        var positions = new[]
        {
            new Vector3(-6.5f, -7.5f, 0f),
            new Vector3(-6.5f, -2.5f, 0f),
            new Vector3(-3.5f, 0.5f, 0f),
            new Vector3(2.5f, 0.5f, 0f),
            new Vector3(2.5f, 4.5f, 0f),
            new Vector3(0f, 6.65f, 0f)
        };
        var waypoints = new Transform[positions.Length];
        for (var index = 0; index < positions.Length; index++)
        {
            var waypoint = new GameObject($"Waypoint {index + 1:00}");
            waypoint.transform.SetParent(pathObject.transform);
            waypoint.transform.position = positions[index];
            waypoints[index] = waypoint.transform;
        }

        var path = pathObject.AddComponent<WaypointPath>();
        path.Configure(waypoints, renderer);
        return path;
    }

    private static LibraryEndpoint CreateLibrary(Transform parent, Sprite sprite, Font font)
    {
        var libraryObject = CreateSpriteObject(
            "Divine Library",
            parent,
            sprite,
            new Color(0.95f, 0.72f, 0.24f, 1f),
            new Vector3(0f, 7.15f, 0f),
            new Vector3(3.2f, 1.5f, 1f),
            5);
        var label = CreateWorldText(
            "Health Label",
            libraryObject.transform,
            font,
            "LIBRARY  30/30",
            new Vector3(0f, 0f, 0f),
            0.12f,
            new Color(0.12f, 0.08f, 0.03f, 1f),
            TextAnchor.MiddleCenter,
            20);
        label.transform.localScale = new Vector3(0.32f, 0.66f, 1f);

        var endpoint = libraryObject.AddComponent<LibraryEndpoint>();
        SetInt(endpoint, "maximumHealth", 30);
        SetObjectReference(endpoint, "healthLabel", label);
        return endpoint;
    }

    private static PlayerMover CreatePlayer(Transform parent, Sprite sprite, Material lineMaterial)
    {
        var playerObject = CreateSpriteObject(
            "Player",
            parent,
            sprite,
            new Color(0.15f, 0.82f, 0.94f, 1f),
            new Vector3(0f, -4.5f, 0f),
            new Vector3(0.95f, 0.95f, 1f),
            15);
        playerObject.transform.rotation = Quaternion.Euler(0f, 0f, 45f);
        var mover = playerObject.AddComponent<PlayerMover>();
        var inputAsset = AssetDatabase.LoadAssetAtPath<InputActionAsset>("Assets/InputSystem_Actions.inputactions");
        if (inputAsset == null)
        {
            throw new InvalidOperationException("Assets/InputSystem_Actions.inputactions was unavailable.");
        }

        SetObjectReference(mover, "inputActions", inputAsset);
        SetString(mover, "moveActionName", "Player/Move");
        SetFloat(mover, "movementSpeed", 5f);
        SetVector2(mover, "minimumBounds", new Vector2(-7.3f, -8f));
        SetVector2(mover, "maximumBounds", new Vector2(7.3f, 8f));

        var ringObject = new GameObject("Attack Range Preview");
        ringObject.transform.SetParent(playerObject.transform, false);
        ringObject.transform.rotation = Quaternion.Euler(0f, 0f, -45f);
        var ring = ringObject.AddComponent<LineRenderer>();
        ring.material = lineMaterial;
        ring.useWorldSpace = false;
        ring.loop = true;
        ring.positionCount = 64;
        ring.startWidth = 0.035f;
        ring.endWidth = 0.035f;
        ring.startColor = new Color(0.15f, 0.82f, 0.94f, 0.3f);
        ring.endColor = ring.startColor;
        ring.sortingOrder = 3;
        for (var index = 0; index < ring.positionCount; index++)
        {
            var angle = Mathf.PI * 2f * index / ring.positionCount;
            ring.SetPosition(index, new Vector3(Mathf.Cos(angle) * 4f, Mathf.Sin(angle) * 4f, 0f));
        }

        return mover;
    }

    private static EnemySpawner CreateSpawner(
        Transform parent,
        EnemyAgent enemyPrefab,
        WaypointPath path,
        EnemyDefinition book,
        EnemyDefinition history)
    {
        var spawnerObject = new GameObject("Enemy Spawner");
        spawnerObject.transform.SetParent(parent);
        spawnerObject.transform.position = path.StartPosition;
        var activeRoot = new GameObject("Active Enemies");
        activeRoot.transform.SetParent(parent);

        var spawner = spawnerObject.AddComponent<EnemySpawner>();
        SetObjectReference(spawner, "enemyPrefab", enemyPrefab);
        SetObjectReference(spawner, "path", path);
        SetObjectReferenceArray(spawner, "definitions", new Object[] { book, history });
        SetObjectReference(spawner, "spawnRoot", activeRoot.transform);
        SetBool(spawner, "spawnOnStart", true);
        SetFloat(spawner, "spawnInterval", 4f);
        return spawner;
    }

    private static void CreateBorder(Transform parent, Material material)
    {
        var borderObject = new GameObject("Arena Border");
        borderObject.transform.SetParent(parent);
        var border = borderObject.AddComponent<LineRenderer>();
        border.material = material;
        border.useWorldSpace = true;
        border.loop = true;
        border.positionCount = 4;
        border.startWidth = 0.12f;
        border.endWidth = 0.12f;
        border.startColor = new Color(0.25f, 0.48f, 0.65f, 1f);
        border.endColor = border.startColor;
        border.sortingOrder = -10;
        border.SetPositions(new[]
        {
            new Vector3(-8f, -9f, 0f),
            new Vector3(-8f, 9f, 0f),
            new Vector3(8f, 9f, 0f),
            new Vector3(8f, -9f, 0f)
        });
    }

    private static GameObject CreateSpriteObject(
        string name,
        Transform parent,
        Sprite sprite,
        Color color,
        Vector3 position,
        Vector3 scale,
        int sortingOrder)
    {
        var gameObject = new GameObject(name);
        gameObject.transform.SetParent(parent);
        gameObject.transform.position = position;
        gameObject.transform.localScale = scale;
        var renderer = gameObject.AddComponent<SpriteRenderer>();
        renderer.sprite = sprite;
        renderer.color = color;
        renderer.sortingOrder = sortingOrder;
        return gameObject;
    }

    private static TextMesh CreateWorldText(
        string name,
        Transform parent,
        Font font,
        string text,
        Vector3 localPosition,
        float characterSize,
        Color color,
        TextAnchor anchor,
        int sortingOrder)
    {
        var textObject = new GameObject(name);
        textObject.transform.SetParent(parent, false);
        textObject.transform.localPosition = localPosition;
        var textMesh = textObject.AddComponent<TextMesh>();
        textMesh.font = font;
        textMesh.fontSize = 64;
        textMesh.characterSize = characterSize;
        textMesh.anchor = anchor;
        textMesh.alignment = TextAlignment.Center;
        textMesh.color = color;
        textMesh.text = text;
        textObject.GetComponent<MeshRenderer>().sharedMaterial = font.material;
        textObject.GetComponent<MeshRenderer>().sortingOrder = sortingOrder;
        return textMesh;
    }

    private static void ConfigureCamera()
    {
        var camera = Camera.main ?? Object.FindFirstObjectByType<Camera>();
        if (camera == null)
        {
            throw new InvalidOperationException("The active scene has no Camera.");
        }

        camera.orthographic = true;
        camera.orthographicSize = 10.5f;
        camera.transform.position = new Vector3(0f, 0f, -10f);
        camera.backgroundColor = new Color(0.025f, 0.035f, 0.065f, 1f);
        camera.clearFlags = CameraClearFlags.SolidColor;
    }

    private static void EnsureFolder(string parent, string name)
    {
        var path = $"{parent}/{name}";
        if (!AssetDatabase.IsValidFolder(path))
        {
            AssetDatabase.CreateFolder(parent, name);
        }
    }

    private static SerializedObject Serialized(Component component)
    {
        return new SerializedObject(component);
    }

    private static void SetObjectReference(Component component, string propertyName, Object value)
    {
        var serialized = Serialized(component);
        serialized.FindProperty(propertyName).objectReferenceValue = value;
        serialized.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void SetObjectReferenceArray(Component component, string propertyName, Object[] values)
    {
        var serialized = Serialized(component);
        var property = serialized.FindProperty(propertyName);
        property.arraySize = values.Length;
        for (var index = 0; index < values.Length; index++)
        {
            property.GetArrayElementAtIndex(index).objectReferenceValue = values[index];
        }

        serialized.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void SetFloat(Component component, string propertyName, float value)
    {
        var serialized = Serialized(component);
        serialized.FindProperty(propertyName).floatValue = value;
        serialized.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void SetInt(Component component, string propertyName, int value)
    {
        var serialized = Serialized(component);
        serialized.FindProperty(propertyName).intValue = value;
        serialized.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void SetBool(Component component, string propertyName, bool value)
    {
        var serialized = Serialized(component);
        serialized.FindProperty(propertyName).boolValue = value;
        serialized.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void SetString(Component component, string propertyName, string value)
    {
        var serialized = Serialized(component);
        serialized.FindProperty(propertyName).stringValue = value;
        serialized.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void SetVector2(Component component, string propertyName, Vector2 value)
    {
        var serialized = Serialized(component);
        serialized.FindProperty(propertyName).vector2Value = value;
        serialized.ApplyModifiedPropertiesWithoutUndo();
    }
}
