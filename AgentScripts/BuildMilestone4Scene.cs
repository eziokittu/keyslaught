using System;
using KeySlaught.SceneGameplay;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

public static class BuildMilestone4Scene
{
    private const string GameplayRootName = "KeySlaught Gameplay";
    private const string LineMaterialPath = "Assets/Art/Prototype/PrototypeLine.mat";

    public static string Build()
    {
        var scene = SceneManager.GetActiveScene();
        if (!scene.IsValid() || scene.path != "Assets/Scenes/SampleScene.unity")
        {
            throw new InvalidOperationException(
                "Open Assets/Scenes/SampleScene.unity before building Milestone 4.");
        }

        var root = GameObject.Find($"/{GameplayRootName}");
        if (root == null)
        {
            throw new InvalidOperationException(
                "The Milestone 3 gameplay root is missing. Run BuildMilestone3Scene first.");
        }

        var coordinator = RequireComponent<GameplaySceneCoordinator>(root.transform, "Gameplay Coordinator");
        var lineMaterial = AssetDatabase.LoadAssetAtPath<Material>(LineMaterialPath);
        var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (lineMaterial == null || font == null)
        {
            throw new InvalidOperationException("Required prototype presentation assets were unavailable.");
        }

        var existingHud = root.transform.Find("Combat HUD");
        if (existingHud != null)
        {
            Object.DestroyImmediate(existingHud.gameObject);
        }

        var hud = new GameObject("Combat HUD");
        hud.transform.SetParent(root.transform, false);
        var bufferLabel = CreateWorldText(
            "Error Buffer Label",
            hud.transform,
            font,
            "ERROR BUFFER  [ ] [ ] [ ] [ ]",
            new Vector3(0f, -7.65f, 0f),
            0.078f,
            new Color(0.95f, 0.8f, 0.36f, 1f),
            40);
        var statusLabel = CreateWorldText(
            "Combat Status Label",
            hud.transform,
            font,
            "TYPE A-Z TO FIRE  //  SPACE REFRESH (EARLY ON)",
            new Vector3(0f, -8.15f, 0f),
            0.058f,
            new Color(0.68f, 0.82f, 0.95f, 1f),
            40);

        var coordinatorObject = coordinator.gameObject;
        var shotLine = coordinatorObject.GetComponent<LineRenderer>();
        if (shotLine == null)
        {
            shotLine = coordinatorObject.AddComponent<LineRenderer>();
        }

        shotLine.material = lineMaterial;
        shotLine.useWorldSpace = true;
        shotLine.positionCount = 2;
        shotLine.startWidth = 0.12f;
        shotLine.endWidth = 0.04f;
        shotLine.startColor = new Color(0.35f, 0.95f, 1f, 1f);
        shotLine.endColor = new Color(1f, 0.95f, 0.5f, 1f);
        shotLine.sortingOrder = 30;
        shotLine.enabled = false;

        var combat = coordinatorObject.GetComponent<GameplayCombatController>();
        if (combat == null)
        {
            combat = coordinatorObject.AddComponent<GameplayCombatController>();
        }

        SetObjectReference(combat, "sceneCoordinator", coordinator);
        SetObjectReference(combat, "bufferLabel", bufferLabel);
        SetObjectReference(combat, "statusLabel", statusLabel);
        SetObjectReference(combat, "shotLine", shotLine);
        SetInt(combat, "bufferCapacity", 4);
        SetFloat(combat, "secondsPerOccupiedSlot", 0.5f);
        SetBool(combat, "allowRefreshBeforeFull", true);
        SetFloat(combat, "corruptionDuration", 2.5f);
        SetFloat(combat, "corruptionContactDistance", 0.8f);
        SetFloat(combat, "shotVisibleSeconds", 0.1f);

        var input = coordinatorObject.GetComponent<KeyboardCombatInput>();
        if (input == null)
        {
            input = coordinatorObject.AddComponent<KeyboardCombatInput>();
        }

        SetObjectReference(input, "combatController", combat);

        var title = root.transform.Find("Prototype Title")?.GetComponent<TextMesh>();
        if (title != null)
        {
            title.text = "KEYSLAUGHT  //  TYPED COMBAT";
        }

        var movementHint = root.transform.Find("Movement Hint")?.GetComponent<TextMesh>();
        if (movementHint != null)
        {
            movementHint.text = "WASD / ARROWS MOVE  //  A-Z FIRE  //  SPACE REFRESH";
            movementHint.transform.localPosition = new Vector3(0f, -8.62f, 0f);
            movementHint.characterSize = 0.052f;
        }

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        return "Built Milestone 4 typed combat, Refresh HUD, direct-shot line, and corruption presentation.";
    }

    private static T RequireComponent<T>(Transform root, string childName) where T : Component
    {
        var child = root.Find(childName);
        if (child == null)
        {
            throw new InvalidOperationException($"Required scene object '{childName}' is missing.");
        }

        var component = child.GetComponent<T>();
        if (component == null)
        {
            throw new InvalidOperationException(
                $"Required component {typeof(T).Name} is missing from '{childName}'.");
        }

        return component;
    }

    private static TextMesh CreateWorldText(
        string name,
        Transform parent,
        Font font,
        string text,
        Vector3 position,
        float characterSize,
        Color color,
        int sortingOrder)
    {
        var textObject = new GameObject(name);
        textObject.transform.SetParent(parent, false);
        textObject.transform.localPosition = position;
        var textMesh = textObject.AddComponent<TextMesh>();
        textMesh.font = font;
        textMesh.fontSize = 64;
        textMesh.characterSize = characterSize;
        textMesh.anchor = TextAnchor.MiddleCenter;
        textMesh.alignment = TextAlignment.Center;
        textMesh.color = color;
        textMesh.text = text;
        var renderer = textObject.GetComponent<MeshRenderer>();
        renderer.sharedMaterial = font.material;
        renderer.sortingOrder = sortingOrder;
        return textMesh;
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
}
