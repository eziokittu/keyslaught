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

public static class BuildMilestone11Scene
{
    private const string Art = "Assets/Art/Colorful";
    private static readonly Color Ink = C("160D24");
    private static readonly Color Indigo = C("27244F");
    private static readonly Color Plum = C("54224C");
    private static readonly Color Gold = C("E6B85C");
    private static readonly Color Parchment = C("FFF0CC");

    public static string Build()
    {
        if (EditorApplication.isPlaying) throw new InvalidOperationException("Exit Play Mode before building Milestone 11.");
        GenerateUiAndPresidentArt();
        ConfigureImports();
        var president = BuildPresidentAssets();
        ApplyGeneratedArtToPrefabs(president.sprite);
        var confirmation = BuildConfirmationPrefab();
        WireConfirmation(confirmation, president.sprite);
        AlignGameplayHud();
        RebuildTutorialPresentation();
        RenameConfirmationSetting();
        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        EditorSceneManager.SaveOpenScenes();
        AssetDatabase.SaveAssets();
        BuildLoreOneScene();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        return "Built Milestone 11: dimmed action confirmation prefab, four turret families, generated UI art wiring, centered HUD, progressive tutorial, and dedicated Lore I Level 1 scene foundation.";
    }

    public static string Audit()
    {
        var context = Object.FindFirstObjectByType<TileContextActionPanel>(FindObjectsInactive.Include)
            ?? throw new InvalidOperationException("Tile context controller missing.");
        var contextData = new SerializedObject(context);
        if (contextData.FindProperty("confirmationPanel").objectReferenceValue == null) throw new InvalidOperationException("Confirmation panel is not wired.");
        if (contextData.FindProperty("presidentSprite").objectReferenceValue == null) throw new InvalidOperationException("President sprite is not wired.");
        if (AssetDatabase.LoadAssetAtPath<TurretDefinition>("Assets/Data/Turrets/President.asset") == null) throw new InvalidOperationException("President definition missing.");
        if (AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/InteractionConfirmationPanel.prefab") == null) throw new InvalidOperationException("Confirmation prefab missing.");
        if (AssetDatabase.LoadAssetAtPath<SceneAsset>("Assets/Scenes/LoreOneLevelOne.unity") == null) throw new InvalidOperationException("Lore scene missing.");
        var bottom = GameObject.Find("/Portrait Gameplay HUD/Bottom 20 Percent")?.transform
            ?? throw new InvalidOperationException("Bottom HUD missing.");
        var target = bottom.GetComponentsInChildren<Text>(true).FirstOrDefault(t => t.name == "Target Word");
        if (target == null || target.alignment != TextAnchor.MiddleCenter) throw new InvalidOperationException("Target label is not centered.");
        var objective = GameObject.Find("/Portrait Gameplay HUD")?.transform.Find("Tutorial Objective Banner")?.gameObject;
        if (objective == null) throw new InvalidOperationException("Tutorial objective banner missing.");
        return "Audit passed: confirmation prefab and wiring, President family, centered HUD, tutorial objective banner, generated art wiring, and dedicated lore scene.";
    }

    private static void GenerateUiAndPresidentArt()
    {
        Save("Enemy_Card_128", 128, pixels =>
        {
            Fill(pixels,new Color32(0,0,0,0)); Box(pixels,128,7,7,114,114,C32("FFFFFF"));
            Box(pixels,128,13,13,102,102,C32("F4F1E8")); Box(pixels,128,25,25,78,78,C32("FFFFFF"));
            for(var i=0;i<4;i++){Circle(pixels,128,18+i*30,18,4,C32("D8D3C8"));Circle(pixels,128,18+i*30,110,4,C32("D8D3C8"));}
        });
        Save("Turret_President_128", 128, pixels =>
        {
            Fill(pixels, new Color32(0,0,0,0)); Circle(pixels,128,64,34,39,C32("3A3565"));
            Box(pixels,128,28,24,72,31,C32("6D4A83")); Box(pixels,128,43,61,42,35,C32("FFF0CC"));
            Circle(pixels,128,64,91,22,C32("D85B7E")); Box(pixels,128,48,91,32,9,C32("E6B85C"));
            Circle(pixels,128,46,105,7,C32("38E2E8")); Circle(pixels,128,64,110,7,C32("38E2E8")); Circle(pixels,128,82,105,7,C32("38E2E8"));
        });
        Save("UI_Button_256x96", 256, pixels =>
        {
            Fill(pixels, new Color32(0,0,0,0));
            Box(pixels,256,8,80,240,96,C32("FFF0CC")); Box(pixels,256,16,88,224,80,C32("FFFFFF"));
        });
    }

    private static void ConfigureImports()
    {
        AssetDatabase.Refresh();
        foreach (var path in new[] { $"{Art}/Enemy_Card_128.png", $"{Art}/Turret_President_128.png", $"{Art}/UI_Button_256x96.png" })
        {
            if (AssetImporter.GetAtPath(path) is not TextureImporter importer) continue;
            importer.textureType = TextureImporterType.Sprite; importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = 128; importer.filterMode = FilterMode.Point;
            importer.textureCompression = TextureImporterCompression.Uncompressed; importer.mipmapEnabled = false;
            importer.alphaIsTransparency = true; importer.SaveAndReimport();
        }
    }

    private static (TurretDefinition definition, Sprite sprite) BuildPresidentAssets()
    {
        var sprite = AssetDatabase.LoadAssetAtPath<Sprite>($"{Art}/Turret_President_128.png");
        var definitionPath = "Assets/Data/Turrets/President.asset";
        var definition = AssetDatabase.LoadAssetAtPath<TurretDefinition>(definitionPath);
        if (definition == null) { definition = ScriptableObject.CreateInstance<TurretDefinition>(); AssetDatabase.CreateAsset(definition, definitionPath); }
        var data = new SerializedObject(definition);
        data.FindProperty("kind").enumValueIndex = (int)TurretKind.President;
        data.FindProperty("coveredLetters").stringValue = "AEIOUST";
        data.FindProperty("range").floatValue = 3.6f;
        data.FindProperty("secondsPerShot").floatValue = .72f;
        data.FindProperty("buildCost").intValue = 9;
        data.ApplyModifiedPropertiesWithoutUndo(); EditorUtility.SetDirty(definition);

        var source = PrefabUtility.LoadPrefabContents("Assets/Prefabs/TurretScientist.prefab");
        try
        {
            source.name = "President Turret";
            source.GetComponent<SpriteRenderer>().sprite = sprite;
            PrefabUtility.SaveAsPrefabAsset(source, "Assets/Prefabs/TurretPresident.prefab");
        }
        finally { PrefabUtility.UnloadPrefabContents(source); }
        return (definition, sprite);
    }

    private static void ApplyGeneratedArtToPrefabs(Sprite president)
    {
        var map = new[]
        {
            ("Assets/Prefabs/TurretTeacher.prefab", "Turret_Teacher_128"),
            ("Assets/Prefabs/TurretEngineer.prefab", "Turret_Engineer_128"),
            ("Assets/Prefabs/TurretScientist.prefab", "Turret_Scientist_128"),
            ("Assets/Prefabs/TurretPresident.prefab", "Turret_President_128"),
            ("Assets/Prefabs/EnemyPrototype.prefab", "Enemy_Card_128"),
            ("Assets/Prefabs/BrainCellPickup.prefab", "Brain_128")
        };
        foreach (var item in map)
        {
            var root = PrefabUtility.LoadPrefabContents(item.Item1);
            try { root.GetComponent<SpriteRenderer>().sprite = AssetDatabase.LoadAssetAtPath<Sprite>($"{Art}/{item.Item2}.png"); PrefabUtility.SaveAsPrefabAsset(root,item.Item1); }
            finally { PrefabUtility.UnloadPrefabContents(root); }
        }
        var button = PrefabUtility.LoadPrefabContents("Assets/Prefabs/UiTextButton.prefab");
        try
        {
            var image = button.GetComponent<Image>(); image.sprite = AssetDatabase.LoadAssetAtPath<Sprite>($"{Art}/UI_Button_256x96.png"); image.type = Image.Type.Simple;
            PrefabUtility.SaveAsPrefabAsset(button,"Assets/Prefabs/UiTextButton.prefab");
        }
        finally { PrefabUtility.UnloadPrefabContents(button); }
    }

    private static GameObject BuildConfirmationPrefab()
    {
        var old = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/InteractionConfirmationPanel.prefab");
        if (old != null) AssetDatabase.DeleteAsset("Assets/Prefabs/InteractionConfirmationPanel.prefab");
        var solid = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Prototype/SolidSprite.asset");
        var body = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        var display = AssetDatabase.LoadAssetAtPath<Font>("Assets/Fonts/PressStart2P/PressStart2P-Regular.ttf");
        var root = Panel("Interaction Confirmation", null, solid, new Color(.015f,.008f,.025f,.88f)); Full(root.rectTransform);
        var card = Panel("Confirmation Card",root.transform,solid,Indigo); Rect(card.rectTransform,new Vector2(.5f,.5f),new Vector2(.5f,.5f),Vector2.zero,new Vector2(760,570));
        var title = Label("KEEP THIS ACTION?",card.transform,display,29,Gold); Rect(title.rectTransform,new Vector2(.5f,1),new Vector2(.5f,1),new Vector2(0,-64),new Vector2(660,92));
        var description = Label("Confirm this action.",card.transform,body,27,Parchment); Rect(description.rectTransform,new Vector2(.5f,.5f),new Vector2(.5f,.5f),new Vector2(0,56),new Vector2(620,145));
        var cost = Label("REQUIRED  0 BRAIN CELLS",card.transform,body,25,Gold); Rect(cost.rectTransform,new Vector2(.5f,.5f),new Vector2(.5f,.5f),new Vector2(0,-55),new Vector2(620,58));
        var keep = Button("KEEP",card.transform,body,solid); Rect(keep.GetComponent<RectTransform>(),new Vector2(.5f,0),new Vector2(.5f,0),new Vector2(-170,54),new Vector2(280,76));
        var cancel = Button("CANCEL",card.transform,body,solid); Rect(cancel.GetComponent<RectTransform>(),new Vector2(.5f,0),new Vector2(.5f,0),new Vector2(170,54),new Vector2(280,76));
        var controller = root.gameObject.AddComponent<InteractionConfirmationPanel>(); controller.Configure(root.gameObject,title,description,cost);
        UnityEventTools.AddPersistentListener(keep.onClick,controller.Keep); UnityEventTools.AddPersistentListener(cancel.onClick,controller.Cancel);
        root.gameObject.SetActive(false);
        var prefab = PrefabUtility.SaveAsPrefabAsset(root.gameObject,"Assets/Prefabs/InteractionConfirmationPanel.prefab");
        Object.DestroyImmediate(root.gameObject); return prefab;
    }

    private static void WireConfirmation(GameObject prefab, Sprite president)
    {
        var canvas = GameObject.Find("/Portrait Gameplay HUD")?.transform ?? throw new InvalidOperationException("Gameplay HUD missing.");
        var previous = canvas.Find("Interaction Confirmation"); if (previous != null) Object.DestroyImmediate(previous.gameObject);
        var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab,canvas); instance.name = "Interaction Confirmation"; instance.transform.SetAsLastSibling(); instance.SetActive(false);
        var panel = instance.GetComponent<InteractionConfirmationPanel>();
        var context = Object.FindFirstObjectByType<TileContextActionPanel>(FindObjectsInactive.Include);
        context.ConfigureConfirmation(panel,president);
        var so = new SerializedObject(context);
        var definitions = so.FindProperty("turretDefinitions");
        var all = AssetDatabase.FindAssets("t:TurretDefinition",new[]{"Assets/Data/Turrets"}).Select(g=>AssetDatabase.LoadAssetAtPath<TurretDefinition>(AssetDatabase.GUIDToAssetPath(g))).Where(x=>x!=null).OrderBy(x=>(int)x.Kind).ToArray();
        definitions.arraySize = all.Length;
        for(var i=0;i<all.Length;i++) definitions.GetArrayElementAtIndex(i).objectReferenceValue=all[i];
        so.ApplyModifiedPropertiesWithoutUndo(); EditorUtility.SetDirty(context);
    }

    private static void AlignGameplayHud()
    {
        var bottom = GameObject.Find("/Portrait Gameplay HUD/Bottom 20 Percent")?.transform ?? throw new InvalidOperationException("Bottom HUD missing.");
        var texts = bottom.GetComponentsInChildren<Text>(true);
        var target = texts.OrderByDescending(t=>t.fontSize).First(); target.name="Target Word"; target.alignment=TextAnchor.MiddleCenter; target.fontSize=36;
        StretchTop(target.rectTransform,42,-12,46);
        var divider = bottom.Find("Target Divider")?.GetComponent<RectTransform>(); if(divider!=null) StretchTop(divider,52,-60,3);
        var wand = bottom.Find("Wand Icon")?.GetComponent<RectTransform>(); if(wand!=null) CenterTop(wand,-250,-76,58,58);
        var magazine = texts.FirstOrDefault(t=>t.text.Contains("[") && t.text.Contains("]"));
        if(magazine!=null){magazine.name="Wand Magazine";magazine.alignment=TextAnchor.MiddleCenter;magazine.fontSize=31;CenterTop(magazine.rectTransform,-40,-76,380,60);}
        var status = texts.FirstOrDefault(t=>t.text.Contains("TYPE A-Z") || t.text.Contains("RELOAD"));
        if(status!=null){status.name="Wand Status";status.alignment=TextAnchor.MiddleCenter;status.fontSize=18;CenterTop(status.rectTransform,236,-76,310,54);}
        var rows = new[]{"QWERTYUIOP","ASDFGHJKL","ZXCVBNM"};
        for(var row=0;row<rows.Length;row++)
        {
            var width = row==0?68f:row==1?70f:74f; var gap=7f; var y=-143-row*61f;
            for(var col=0;col<rows[row].Length;col++)
            {
                var button=bottom.GetComponentsInChildren<Button>(true).FirstOrDefault(b=>b.GetComponentInChildren<Text>(true)?.text==rows[row][col].ToString());
                if(button==null)continue; var total=rows[row].Length*width+(rows[row].Length-1)*gap;
                var x=-total*.5f+width*.5f+col*(width+gap); CenterTop(button.GetComponent<RectTransform>(),x,y,width,51);
                var label=button.GetComponentInChildren<Text>(true);label.fontSize=31;label.alignment=TextAnchor.MiddleCenter;
            }
        }
        var reload = bottom.GetComponentsInChildren<Button>(true).FirstOrDefault(b=>b.name.Contains("Reload")); if(reload!=null)CenterTop(reload.GetComponent<RectTransform>(),378,-242,66,66);
        foreach(var text in GameObject.Find("/Portrait Gameplay HUD").GetComponentsInChildren<Text>(true))
        {
            if(text.fontSize is >= 14 and < 22) text.fontSize+=3;
            text.alignment=TextAnchor.MiddleCenter;
        }
        var context = GameObject.Find("/Portrait Gameplay HUD/Top 20 Percent/Tile Context Actions");
        if(context!=null) foreach(var text in context.GetComponentsInChildren<Text>(true)){text.fontSize=Mathf.Max(text.fontSize,20);text.resizeTextForBestFit=true;text.resizeTextMinSize=14;text.alignment=TextAnchor.MiddleCenter;}
    }

    private static void RebuildTutorialPresentation()
    {
        var canvas=GameObject.Find("/Portrait Gameplay HUD").transform;
        var overlay=canvas.Find("Tutorial Guidance"); if(overlay==null)throw new InvalidOperationException("Tutorial guidance missing.");
        var card=overlay.Find("Guidance Card");
        var title=card.GetComponentsInChildren<Text>(true).OrderByDescending(t=>t.fontSize).First();
        var body=card.GetComponentsInChildren<Text>(true).First(t=>t!=title && !t.transform.IsChildOf(card.GetComponentsInChildren<Button>(true).First().transform));
        var next=card.GetComponentsInChildren<Button>(true).First();
        var old=canvas.Find("Tutorial Objective Banner");if(old!=null)Object.DestroyImmediate(old.gameObject);
        var solid=AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Prototype/SolidSprite.asset");
        var objective=Panel("Tutorial Objective Banner",canvas,solid,new Color(.08f,.035f,.12f,.96f));
        Rect(objective.rectTransform,new Vector2(.5f,1),new Vector2(.5f,1),new Vector2(0,-350),new Vector2(760,82));
        var objectiveText=Label("TRY IT",objective.transform,Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"),25,Gold);Full(objectiveText.rectTransform);
        objective.gameObject.SetActive(false); objective.transform.SetSiblingIndex(Mathf.Max(0,canvas.childCount-3));
        var director=Object.FindFirstObjectByType<TutorialDirector>(FindObjectsInactive.Include);
        var player=Object.FindFirstObjectByType<PlayerMover>(FindObjectsInactive.Include);var run=Object.FindFirstObjectByType<WaveRunController>(FindObjectsInactive.Include);
        director.Configure(overlay.gameObject,title,body,next,player,run,objective.gameObject,objectiveText);
    }

    private static void RenameConfirmationSetting()
    {
        var settings=GameObject.Find("/Game Shell Canvas/Game Shell/Settings")?.transform;if(settings==null)return;
        var button=settings.GetComponentsInChildren<Button>(true).FirstOrDefault(b=>b.GetComponentInChildren<Text>(true)?.text.Contains("OPTION MENUS")==true);
        if(button==null)return;button.name="Button ACTION CONFIRMATIONS  ON";var label=button.GetComponentInChildren<Text>(true);label.text="ACTION CONFIRMATIONS  ON";label.fontSize=23;
    }

    private static void BuildLoreOneScene()
    {
        const string sample="Assets/Scenes/SampleScene.unity";const string lorePath="Assets/Scenes/LoreOneLevelOne.unity";
        EditorSceneManager.SaveOpenScenes();
        if(AssetDatabase.LoadAssetAtPath<SceneAsset>(lorePath)!=null)AssetDatabase.DeleteAsset(lorePath);
        if(!AssetDatabase.CopyAsset(sample,lorePath))throw new InvalidOperationException("Could not create Lore I scene.");
        AssetDatabase.Refresh();
        var loreScene=EditorSceneManager.OpenScene(lorePath,OpenSceneMode.Single);
        var run=Object.FindFirstObjectByType<WaveRunController>(FindObjectsInactive.Include);var lore=AssetDatabase.LoadAssetAtPath<LevelDefinition>("Assets/Data/Levels/LoreOneLevelOne.asset");
        var so=new SerializedObject(run);so.FindProperty("activeLevel").objectReferenceValue=lore;so.ApplyModifiedPropertiesWithoutUndo();
        var root=GameObject.Find("/KeySlaught Gameplay");root.name="Lore I Level 1 Gameplay";
        var shellController=Object.FindFirstObjectByType<GameShellController>(FindObjectsInactive.Include);if(shellController!=null)shellController.enabled=false;
        var shell=GameObject.Find("/Game Shell Canvas");if(shell!=null)shell.SetActive(false);
        var marker=new GameObject("AUTHORED LEVEL - LORE I LEVEL 1");marker.transform.SetParent(root.transform,false);
        EditorSceneManager.MarkSceneDirty(loreScene);EditorSceneManager.SaveScene(loreScene);
        var scenes=EditorBuildSettings.scenes.Select(s=>s.path).ToList();if(!scenes.Contains(lorePath))scenes.Add(lorePath);
        EditorBuildSettings.scenes=scenes.Select(p=>new EditorBuildSettingsScene(p,true)).ToArray();
        EditorSceneManager.OpenScene(sample,OpenSceneMode.Single);
    }

    private static Image Panel(string name,Transform parent,Sprite sprite,Color color){var go=new GameObject(name,typeof(RectTransform),typeof(CanvasRenderer),typeof(Image));if(parent!=null)go.transform.SetParent(parent,false);var image=go.GetComponent<Image>();image.sprite=sprite;image.color=color;return image;}
    private static Text Label(string value,Transform parent,Font font,int size,Color color){var go=new GameObject("Label "+value,typeof(RectTransform),typeof(CanvasRenderer),typeof(Text));go.transform.SetParent(parent,false);var t=go.GetComponent<Text>();t.text=value;t.font=font;t.fontSize=size;t.color=color;t.alignment=TextAnchor.MiddleCenter;t.horizontalOverflow=HorizontalWrapMode.Wrap;t.verticalOverflow=VerticalWrapMode.Overflow;return t;}
    private static Button Button(string value,Transform parent,Font font,Sprite solid){var prefab=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/UiTextButton.prefab");var go=(GameObject)PrefabUtility.InstantiatePrefab(prefab,parent);go.name="Button "+value;var text=go.GetComponentInChildren<Text>(true);text.text=value;text.font=font;text.fontSize=26;text.color=Parchment;var image=go.GetComponent<Image>();image.sprite=AssetDatabase.LoadAssetAtPath<Sprite>($"{Art}/UI_Button_256x96.png");var button=go.GetComponent<Button>();var colors=button.colors;colors.normalColor=Plum;colors.highlightedColor=C("733866");colors.pressedColor=C("32142F");button.colors=colors;return button;}
    private static void Full(RectTransform r){r.anchorMin=Vector2.zero;r.anchorMax=Vector2.one;r.offsetMin=Vector2.zero;r.offsetMax=Vector2.zero;}
    private static void Rect(RectTransform r,Vector2 min,Vector2 max,Vector2 pos,Vector2 size){r.anchorMin=min;r.anchorMax=max;r.pivot=min==max?min:new Vector2(.5f,.5f);r.anchoredPosition=pos;r.sizeDelta=size;}
    private static void CenterTop(RectTransform r,float x,float y,float w,float h){r.anchorMin=r.anchorMax=r.pivot=new Vector2(.5f,1);r.anchoredPosition=new Vector2(x,y);r.sizeDelta=new Vector2(w,h);}
    private static void StretchTop(RectTransform r,float margin,float y,float h){r.anchorMin=new Vector2(0,1);r.anchorMax=new Vector2(1,1);r.pivot=new Vector2(.5f,1);r.anchoredPosition=new Vector2(0,y);r.sizeDelta=new Vector2(-margin*2,h);}
    private static Color C(string hex){ColorUtility.TryParseHtmlString("#"+hex,out var color);return color;}
    private static Color32 C32(string hex)=>(Color32)C(hex);
    private static void Save(string name,int size,Action<Color32[]> draw){var pixels=new Color32[size*size];draw(pixels);var texture=new Texture2D(size,size,TextureFormat.RGBA32,false);texture.SetPixels32(pixels);texture.Apply();File.WriteAllBytes($"{Art}/{name}.png",texture.EncodeToPNG());Object.DestroyImmediate(texture);}
    private static void Fill(Color32[] p,Color32 c){for(var i=0;i<p.Length;i++)p[i]=c;}
    private static void Box(Color32[] p,int s,int x,int y,int w,int h,Color32 c){for(var yy=Mathf.Max(0,y);yy<Mathf.Min(s,y+h);yy++)for(var xx=Mathf.Max(0,x);xx<Mathf.Min(s,x+w);xx++)p[yy*s+xx]=c;}
    private static void Circle(Color32[] p,int s,int cx,int cy,int r,Color32 c){var rr=r*r;for(var y=Mathf.Max(0,cy-r);y<Mathf.Min(s,cy+r+1);y++)for(var x=Mathf.Max(0,cx-r);x<Mathf.Min(s,cx+r+1);x++)if((x-cx)*(x-cx)+(y-cy)*(y-cy)<=rr)p[y*s+x]=c;}
}
