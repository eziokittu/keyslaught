using System;
using System.IO;
using System.Linq;
using KeySlaught.Progression;
using KeySlaught.SceneGameplay;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Object = UnityEngine.Object;

public static class BuildMilestone13Changes
{
    private const string SampleScene = "Assets/Scenes/SampleScene.unity";
    private const string LoreScene = "Assets/Scenes/LoreOneLevelOne.unity";
    private const string ButtonArt = "Assets/Art/Colorful/UI_Button_256x96.png";
    private static readonly Color Indigo = C("27244F");
    private static readonly Color Plum = C("54224C");
    private static readonly Color Gold = C("E6B85C");
    private static readonly Color Parchment = C("FFF0CC");

    public static string Build()
    {
        if (EditorApplication.isPlaying) throw new InvalidOperationException("Exit Play Mode before building Milestone 13.");
        GenerateButtonArt();
        ConfigureButtonArt();
        ConfigureTurretVariants();
        UpdateButtonPrefab();

        BuildScene(SampleScene, true);
        BuildScene(LoreScene, false);
        EditorSceneManager.OpenScene(SampleScene, OpenSceneMode.Single);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        return "Built Milestone 13 presentation: interactive tutorial, readable buttons, range circles, fast-forward icon, turret variants, and Lore I level select with stars.";
    }

    public static string Audit()
    {
        var sample = EditorSceneManager.OpenScene(SampleScene, OpenSceneMode.Single);
        var shell = Object.FindFirstObjectByType<GameShellController>(FindObjectsInactive.Include)
            ?? throw new InvalidOperationException("Game shell missing.");
        var shellData = new SerializedObject(shell);
        if (shellData.FindProperty("loreLevelsPanel").objectReferenceValue == null) throw new InvalidOperationException("Lore level panel is not wired.");
        var director = Object.FindFirstObjectByType<TutorialDirector>(FindObjectsInactive.Include)
            ?? throw new InvalidOperationException("Tutorial director missing.");
        var tutorialData = new SerializedObject(director);
        if (tutorialData.FindProperty("tutorialJoystickRoot").objectReferenceValue == null) throw new InvalidOperationException("Tutorial joystick missing.");
        if (tutorialData.FindProperty("tutorialInputButtons").arraySize < 27) throw new InvalidOperationException("Tutorial keyboard highlighting is incomplete.");
        var status = GameObject.Find("/Portrait Gameplay HUD/Bottom 20 Percent/Wand Status");
        if (status != null && status.activeSelf) throw new InvalidOperationException("Redundant wand status is still visible.");
        var player = Object.FindFirstObjectByType<PlayerMover>(FindObjectsInactive.Include);
        if (player == null || player.GetComponent<RangeCircleIndicator>() == null) throw new InvalidOperationException("Player range indicator missing.");
        var fastForwardAdapter = Resources.FindObjectsOfTypeAll<IntermissionFastForwardButton>()
            .FirstOrDefault(item => item.gameObject.scene == SceneManager.GetActiveScene());
        var fastForward = fastForwardAdapter == null ? null : fastForwardAdapter.GetComponent<Button>();
        if (fastForward == null || fastForward.GetComponentInChildren<Text>(true)?.text != ">>") throw new InvalidOperationException("Fast-forward icon missing.");
        foreach (var definition in Object.FindObjectsByType<TurretPadController>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            if (definition.GetComponent<RangeCircleIndicator>() == null) throw new InvalidOperationException("Authored turret range indicator missing.");
        var teacher = AssetDatabase.LoadAssetAtPath<TurretDefinition>("Assets/Data/Turrets/Teacher.asset");
        var engineer = AssetDatabase.LoadAssetAtPath<TurretDefinition>("Assets/Data/Turrets/Engineer.asset");
        if (teacher == null || teacher.VariantCount != 4 || engineer == null || engineer.VariantCount != 2)
            throw new InvalidOperationException("Turret variant data is incomplete.");
        EditorSceneManager.MarkSceneDirty(sample);
        return "Audit passed: Lore panel/stars, tutorial controller and 27 inputs, hidden wand hint, pulsing player range, fast-forward icon, and turret variant data.";
    }

    public static string DiagnoseFastForward()
    {
        var adapters = Resources.FindObjectsOfTypeAll<IntermissionFastForwardButton>();
        var buttons = Resources.FindObjectsOfTypeAll<Button>();
        return $"scene={SceneManager.GetActiveScene().name}; adapters={adapters.Length}:" +
            string.Join("|", adapters.Select(item => $"{item.name}/{item.gameObject.scene.name}/{item.GetComponent<Button>() != null}")) +
            $"; buttons={buttons.Length}; matches=" + string.Join("|", buttons.Where(button => button.name.Contains("FORWARD")).Select(button => $"{button.name}/{button.gameObject.scene.name}/{button.GetComponentInChildren<Text>(true)?.text}"));
    }

    public static string ReviewSettings()
    {
        var shell = Object.FindFirstObjectByType<GameShellController>(FindObjectsInactive.Include);
        ActivateShell(shell); shell?.ShowSettings();
        return "Settings shown.";
    }

    public static string ReviewLoreLevels()
    {
        var shell = Object.FindFirstObjectByType<GameShellController>(FindObjectsInactive.Include);
        ActivateShell(shell); shell?.ShowLoreLevels();
        return "Lore levels shown.";
    }

    public static string ReviewTutorial()
    {
        Object.FindFirstObjectByType<GameShellController>(FindObjectsInactive.Include)?.StartTutorial();
        return "Tutorial started.";
    }

    public static string ReviewTutorialMove()
    {
        var player = Object.FindFirstObjectByType<PlayerMover>(FindObjectsInactive.Include);
        player?.ApplyMovement(Vector2.right, 1f);
        return "Tutorial player moved.";
    }

    public static string ReviewTutorialContinue()
    {
        Object.FindFirstObjectByType<TutorialDirector>(FindObjectsInactive.Include)?.ContinueTutorial();
        return "Tutorial continued.";
    }

    public static string ReviewTutorialTypeCat()
    {
        var combat = Object.FindFirstObjectByType<GameplayCombatController>(FindObjectsInactive.Include);
        if (combat == null) return "Combat missing.";
        var c = combat.TryTypeLetter('C').Outcome;
        var a = combat.TryTypeLetter('A').Outcome;
        var t = combat.TryTypeLetter('T').Outcome;
        return $"CAT outcomes: {c}, {a}, {t}; buffer={combat.ErrorBuffer.OccupiedSlotCount}.";
    }

    public static string ReviewTutorialRefresh()
    {
        var combat = Object.FindFirstObjectByType<GameplayCombatController>(FindObjectsInactive.Include);
        return combat != null && combat.TryStartRefresh() ? "Refresh started." : "Refresh did not start.";
    }

    public static string DiagnoseTutorial()
    {
        var director = Object.FindFirstObjectByType<TutorialDirector>(FindObjectsInactive.Include);
        var combat = Object.FindFirstObjectByType<GameplayCombatController>(FindObjectsInactive.Include);
        var stageField = typeof(TutorialDirector).GetField("stage", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
        var stage = director == null ? "missing" : stageField?.GetValue(director)?.ToString();
        return $"stage={stage}; timeScale={Time.timeScale}; buffer={combat?.ErrorBuffer.OccupiedSlotCount}; refreshing={combat?.ErrorBuffer.IsRefreshing}; enemies={Object.FindFirstObjectByType<EnemySpawner>(FindObjectsInactive.Include)?.ActiveEnemies.Count}.";
    }

    public static string DiagnoseUi()
    {
        var canvases = Resources.FindObjectsOfTypeAll<Canvas>().Where(item => item.gameObject.scene == SceneManager.GetActiveScene());
        var shell = Object.FindFirstObjectByType<GameShellController>(FindObjectsInactive.Include);
        var serialized = shell == null ? null : new SerializedObject(shell);
        var root = serialized?.FindProperty("shellRoot").objectReferenceValue as GameObject;
        return string.Join("|", canvases.Select(item => $"{item.name}:self={item.gameObject.activeSelf},hier={item.gameObject.activeInHierarchy},enabled={item.enabled},mode={item.renderMode}")) +
            $"; shellRoot={(root == null ? "null" : root.name + ":" + root.activeSelf + ":" + root.activeInHierarchy)}";
    }

    public static string ReviewEnableCameraCanvasCapture()
    {
        var camera = Camera.main;
        if (camera == null) throw new InvalidOperationException("Main camera missing.");
        foreach (var canvas in Resources.FindObjectsOfTypeAll<Canvas>().Where(item => item.gameObject.scene == SceneManager.GetActiveScene()))
        {
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.worldCamera = camera;
            canvas.planeDistance = 1f;
        }
        return "Live canvases switched to camera capture mode for screenshot review only.";
    }

    private static void ActivateShell(GameShellController shell)
    {
        if (shell == null) return;
        var serialized = new SerializedObject(shell);
        var root = serialized.FindProperty("shellRoot").objectReferenceValue as GameObject;
        if (root != null) root.SetActive(true);
    }

    private static void BuildScene(string path, bool buildMenuAndTutorial)
    {
        var scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
        ApplyCommonGameplayPresentation();
        if (buildMenuAndTutorial)
        {
            BuildTutorialPresentation();
            BuildLoreLevelPanel();
        }
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
    }

    private static void ApplyCommonGameplayPresentation()
    {
        var player = Object.FindFirstObjectByType<PlayerMover>(FindObjectsInactive.Include)
            ?? throw new InvalidOperationException("Player missing.");
        var coordinator = Object.FindFirstObjectByType<GameplaySceneCoordinator>(FindObjectsInactive.Include)
            ?? throw new InvalidOperationException("Gameplay coordinator missing.");
        var range = player.GetComponent<RangeCircleIndicator>();
        if (range == null) range = player.gameObject.AddComponent<RangeCircleIndicator>();
        range.ConfigurePlayer(coordinator);

        var bottom = FindIncludingInactive("Portrait Gameplay HUD/Bottom 20 Percent");
        var status = bottom == null ? null : bottom.transform.Find("Wand Status");
        if (status != null) status.gameObject.SetActive(false);

        var sceneButtons = SceneManager.GetActiveScene().GetRootGameObjects()
            .SelectMany(root => root.GetComponentsInChildren<Button>(true));
        foreach (var button in sceneButtons)
        {
            var label = button.GetComponentInChildren<Text>(true);
            if (label == null || string.IsNullOrWhiteSpace(label.text) || label.text.Trim().Length <= 1) continue;
            if (button.GetComponent<OnScreenRefreshButton>() != null || button.name.Contains("Pause")) continue;
            var rect = button.GetComponent<RectTransform>();
            if (rect != null && rect.sizeDelta.y < 72f) rect.sizeDelta = new Vector2(rect.sizeDelta.x, 76f);
            label.alignment = TextAnchor.MiddleCenter;
        }

        var fastForwardAdapter = Resources.FindObjectsOfTypeAll<IntermissionFastForwardButton>()
            .FirstOrDefault(item => item.gameObject.scene == SceneManager.GetActiveScene());
        var fastForward = fastForwardAdapter == null ? null : fastForwardAdapter.GetComponent<Button>();
        if (fastForward != null)
        {
            var label = fastForward.GetComponentInChildren<Text>(true);
            if (label != null)
            {
                label.text = ">>"; label.fontSize = 32; label.alignment = TextAnchor.MiddleCenter;
                EditorUtility.SetDirty(label); PrefabUtility.RecordPrefabInstancePropertyModifications(label);
            }
            var rect = fastForward.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(1f, 0f);
            rect.anchoredPosition = new Vector2(-72f, -63f);
            rect.sizeDelta = new Vector2(62f, 62f);
            EditorUtility.SetDirty(rect); PrefabUtility.RecordPrefabInstancePropertyModifications(rect);
        }
    }

    private static void BuildTutorialPresentation()
    {
        var canvas = FindIncludingInactive("Portrait Gameplay HUD")?.transform
            ?? throw new InvalidOperationException("Gameplay HUD missing.");
        var overlay = canvas.Find("Tutorial Guidance") ?? throw new InvalidOperationException("Tutorial overlay missing.");
        var card = overlay.Find("Guidance Card") ?? throw new InvalidOperationException("Tutorial card missing.");
        var texts = card.GetComponentsInChildren<Text>(true);
        var title = texts.OrderByDescending(text => text.fontSize).First();
        var next = card.GetComponentsInChildren<Button>(true).First();
        var body = texts.First(text => text != title && !text.transform.IsChildOf(next.transform));
        Rect(card.GetComponent<RectTransform>(), new Vector2(.5f,.5f), new Vector2(.5f,.5f), Vector2.zero, new Vector2(760,690));
        CenterTop(title.rectTransform, 0, -42, 660, 82); title.alignment = TextAnchor.MiddleCenter;
        Rect(body.rectTransform, new Vector2(.5f,.5f), new Vector2(.5f,.5f), new Vector2(0,10), new Vector2(620,310));
        body.alignment = TextAnchor.MiddleCenter; body.fontSize = 27; body.horizontalOverflow = HorizontalWrapMode.Wrap; body.verticalOverflow = VerticalWrapMode.Truncate;
        var nextRect = next.GetComponent<RectTransform>();
        Rect(nextRect, new Vector2(.5f,0), new Vector2(.5f,0), new Vector2(0,45), new Vector2(560,82));

        var previous = canvas.Find("Tutorial Drag Controller");
        if (previous != null) Object.DestroyImmediate(previous.gameObject);
        var solid = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Prototype/SolidSprite.asset");
        var circle = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Monochrome/UI_Circle_128.png") ?? solid;
        var joystickBase = Panel("Tutorial Drag Controller", canvas, circle, new Color(.12f,.55f,.68f,.72f));
        Rect(joystickBase.rectTransform, new Vector2(.5f,1), new Vector2(.5f,1), new Vector2(0,-455), new Vector2(180,180));
        var handle = Panel("Drag Handle", joystickBase.transform, circle, Parchment);
        Rect(handle.rectTransform, new Vector2(.5f,.5f), new Vector2(.5f,.5f), Vector2.zero, new Vector2(72,72));
        var player = Object.FindFirstObjectByType<PlayerMover>(FindObjectsInactive.Include);
        var joystick = joystickBase.gameObject.AddComponent<VirtualJoystick>();
        joystick.Configure(player, joystickBase.rectTransform, joystickBase.rectTransform, handle.rectTransform);
        joystickBase.gameObject.SetActive(false);

        var bottom = canvas.Find("Bottom 20 Percent");
        var inputs = bottom.GetComponentsInChildren<Button>(true)
            .Where(button => button.GetComponent<OnScreenLetterButton>() != null || button.GetComponent<OnScreenRefreshButton>() != null)
            .ToArray();
        var director = Object.FindFirstObjectByType<TutorialDirector>(FindObjectsInactive.Include)
            ?? throw new InvalidOperationException("Tutorial director missing.");
        director.ConfigureInteractiveVisuals(joystickBase.gameObject, inputs);
    }

    private static void BuildLoreLevelPanel()
    {
        var shell = Object.FindFirstObjectByType<GameShellController>(FindObjectsInactive.Include)
            ?? throw new InvalidOperationException("Game shell missing.");
        var root = FindIncludingInactive("Game Shell Canvas/Game Shell")?.transform
            ?? throw new InvalidOperationException("Game shell root missing.");
        var previous = root.Find("Lore Levels");
        if (previous != null) Object.DestroyImmediate(previous.gameObject);
        var solid = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Prototype/SolidSprite.asset");
        var display = AssetDatabase.LoadAssetAtPath<Font>("Assets/Fonts/PressStart2P/PressStart2P-Regular.ttf");
        var bodyFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        var panel = Panel("Lore Levels", root, solid, C("160D24")); Full(panel.rectTransform);
        var heading = Label("LORE I", panel.transform, display, 42, Gold); CenterTop(heading.rectTransform,0,-105,760,80);
        var sub = Label("THREE LEVELS  •  STARS REWARD FAST, FLAWLESS DEFENSE", panel.transform, bodyFont, 24, Parchment); CenterTop(sub.rectTransform,0,-205,780,70);
        var levelOne = TextButton("LEVEL 1  ☆☆☆", panel.transform, bodyFont, 28);
        CenterTop(levelOne.GetComponent<RectTransform>(),0,-345,680,92);
        UnityEventTools.AddPersistentListener(levelOne.onClick, shell.StartLoreOneLevelOne);
        var levelTwo = TextButton("LEVEL 2  •  LOCKED", panel.transform, bodyFont, 27); CenterTop(levelTwo.GetComponent<RectTransform>(),0,-475,680,92); levelTwo.interactable=false;
        var levelThree = TextButton("LEVEL 3  •  LOCKED", panel.transform, bodyFont, 27); CenterTop(levelThree.GetComponent<RectTransform>(),0,-605,680,92); levelThree.interactable=false;
        var loreTwo = TextButton("LORE II  •  LOCKED", panel.transform, bodyFont, 27); CenterTop(loreTwo.GetComponent<RectTransform>(),0,-790,680,92); loreTwo.interactable=false;
        var note = Label("LEVEL 1 IS PLAYABLE. LEVELS 2-3 ARE RESERVED FOR THIS LORE.",panel.transform,bodyFont,22,Parchment);CenterTop(note.rectTransform,0,-920,760,75);
        var back = TextButton("‹  BACK",panel.transform,bodyFont,25);Rect(back.GetComponent<RectTransform>(),new Vector2(0,1),new Vector2(0,1),new Vector2(112,-62),new Vector2(210,72));
        UnityEventTools.AddPersistentListener(back.onClick,shell.ShowModes);
        panel.gameObject.SetActive(false);
        shell.ConfigureLoreSelection(panel.gameObject, levelOne.GetComponentInChildren<Text>(true));

        var modePanel = root.Find("Mode Select") ?? throw new InvalidOperationException("Mode panel missing.");
        var loreButton = modePanel.GetComponentsInChildren<Button>(true)
            .FirstOrDefault(button => button.GetComponentInChildren<Text>(true)?.text.Contains("LORE I") == true)
            ?? throw new InvalidOperationException("Lore I mode button missing.");
        for (var index = loreButton.onClick.GetPersistentEventCount() - 1; index >= 0; index--)
            UnityEventTools.RemovePersistentListener(loreButton.onClick, index);
        UnityEventTools.AddPersistentListener(loreButton.onClick, shell.ShowLoreLevels);
    }

    private static void ConfigureTurretVariants()
    {
        ConfigureVariants("Teacher", new[] { ("A-F","ABCDEF"), ("G-M","GHIJKLM"), ("N-T","NOPQRST"), ("U-Z","UVWXYZ") });
        ConfigureVariants("Engineer", new[] { ("A-M","ABCDEFGHIJKLM"), ("N-Z","NOPQRSTUVWXYZ") });
        ConfigureVariants("Scientist", new[] { ("A-Z","ABCDEFGHIJKLMNOPQRSTUVWXYZ") });
        ConfigureVariants("President", new[] { ("AEIOU + S,T","AEIOUST") });
    }

    private static void ConfigureVariants(string assetName, (string label, string letters)[] values)
    {
        var definition = AssetDatabase.LoadAssetAtPath<TurretDefinition>($"Assets/Data/Turrets/{assetName}.asset")
            ?? throw new InvalidOperationException($"{assetName} turret definition missing.");
        var serialized = new SerializedObject(definition);
        var variants = serialized.FindProperty("variants"); variants.arraySize = values.Length;
        for (var index = 0; index < values.Length; index++)
        {
            var item = variants.GetArrayElementAtIndex(index);
            item.FindPropertyRelative("displayName").stringValue = values[index].label;
            item.FindPropertyRelative("coveredLetters").stringValue = values[index].letters;
        }
        serialized.ApplyModifiedPropertiesWithoutUndo(); EditorUtility.SetDirty(definition);
    }

    private static void GenerateButtonArt()
    {
        const int width=256, height=96;
        var pixels = new Color32[width*height];
        var transparent = new Color32(0,0,0,0);
        for(var i=0;i<pixels.Length;i++)pixels[i]=transparent;
        Box(pixels,width,height,0,0,width,height,C32("3B1838"));
        Box(pixels,width,height,5,5,width-10,height-10,C32("71335F"));
        Box(pixels,width,height,11,11,width-22,height-22,C32("54224C"));
        Box(pixels,width,height,15,15,width-30,5,C32("E6B85C"));
        Box(pixels,width,height,15,height-20,width-30,5,C32("2A1028"));
        var texture = new Texture2D(width,height,TextureFormat.RGBA32,false);texture.SetPixels32(pixels);texture.Apply();
        File.WriteAllBytes(ButtonArt,texture.EncodeToPNG());Object.DestroyImmediate(texture);AssetDatabase.Refresh();
    }

    private static void ConfigureButtonArt()
    {
        if (AssetImporter.GetAtPath(ButtonArt) is not TextureImporter importer) return;
        importer.textureType=TextureImporterType.Sprite;importer.spriteImportMode=SpriteImportMode.Single;importer.spritePixelsPerUnit=128;
        importer.filterMode=FilterMode.Point;importer.textureCompression=TextureImporterCompression.Uncompressed;importer.mipmapEnabled=false;importer.alphaIsTransparency=true;importer.SaveAndReimport();
    }

    private static void UpdateButtonPrefab()
    {
        const string path="Assets/Prefabs/UiTextButton.prefab";
        var root=PrefabUtility.LoadPrefabContents(path);
        try
        {
            var image=root.GetComponent<Image>();image.sprite=AssetDatabase.LoadAssetAtPath<Sprite>(ButtonArt);image.type=Image.Type.Simple;image.color=Color.white;
            var rect=root.GetComponent<RectTransform>();if(rect.sizeDelta.y<76)rect.sizeDelta=new Vector2(rect.sizeDelta.x,76);
            PrefabUtility.SaveAsPrefabAsset(root,path);
        }
        finally{PrefabUtility.UnloadPrefabContents(root);}
    }

    private static GameObject FindIncludingInactive(string path)
    {
        var names=path.Split('/');
        var root=SceneManager.GetActiveScene().GetRootGameObjects().FirstOrDefault(go=>go.name==names[0]);
        if(root==null)return null;var current=root.transform;
        for(var i=1;i<names.Length;i++){current=current.Find(names[i]);if(current==null)return null;}
        return current.gameObject;
    }
    private static Image Panel(string name,Transform parent,Sprite sprite,Color color){var go=new GameObject(name,typeof(RectTransform),typeof(CanvasRenderer),typeof(Image));go.transform.SetParent(parent,false);var image=go.GetComponent<Image>();image.sprite=sprite;image.color=color;return image;}
    private static Text Label(string value,Transform parent,Font font,int size,Color color){var go=new GameObject("Label "+value,typeof(RectTransform),typeof(CanvasRenderer),typeof(Text));go.transform.SetParent(parent,false);var text=go.GetComponent<Text>();text.text=value;text.font=font;text.fontSize=size;text.color=color;text.alignment=TextAnchor.MiddleCenter;text.horizontalOverflow=HorizontalWrapMode.Wrap;text.verticalOverflow=VerticalWrapMode.Overflow;return text;}
    private static Button TextButton(string value,Transform parent,Font font,int size){var prefab=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/UiTextButton.prefab");var go=(GameObject)PrefabUtility.InstantiatePrefab(prefab,parent);go.name="Button "+value;var text=go.GetComponentInChildren<Text>(true);text.text=value;text.font=font;text.fontSize=size;text.color=Parchment;text.alignment=TextAnchor.MiddleCenter;return go.GetComponent<Button>();}
    private static void Full(RectTransform rect){rect.anchorMin=Vector2.zero;rect.anchorMax=Vector2.one;rect.offsetMin=Vector2.zero;rect.offsetMax=Vector2.zero;}
    private static void Rect(RectTransform rect,Vector2 min,Vector2 max,Vector2 position,Vector2 size){rect.anchorMin=min;rect.anchorMax=max;rect.pivot=min==max?min:new Vector2(.5f,.5f);rect.anchoredPosition=position;rect.sizeDelta=size;}
    private static void CenterTop(RectTransform rect,float x,float y,float width,float height){rect.anchorMin=rect.anchorMax=rect.pivot=new Vector2(.5f,1);rect.anchoredPosition=new Vector2(x,y);rect.sizeDelta=new Vector2(width,height);}
    private static void Box(Color32[] pixels,int width,int height,int x,int y,int boxWidth,int boxHeight,Color32 color){for(var yy=Mathf.Max(0,y);yy<Mathf.Min(height,y+boxHeight);yy++)for(var xx=Mathf.Max(0,x);xx<Mathf.Min(width,x+boxWidth);xx++)pixels[yy*width+xx]=color;}
    private static Color C(string hex){ColorUtility.TryParseHtmlString("#"+hex,out var color);return color;}
    private static Color32 C32(string hex)=>(Color32)C(hex);
}
