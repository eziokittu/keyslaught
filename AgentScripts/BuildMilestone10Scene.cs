using System;
using System.Collections.Generic;
using System.IO;
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

public static class BuildMilestone10Scene
{
    private const string Art = "Assets/Art/Colorful";
    private const string Mono = "Assets/Art/Monochrome";
    private const string Levels = "Assets/Data/Levels";
    private static readonly Color Ink = C("24162F");
    private static readonly Color Plum = C("54224C");
    private static readonly Color Indigo = C("27244F");
    private static readonly Color Gold = C("E6B85C");
    private static readonly Color Parchment = C("FFF0CC");
    private static readonly Color Cyan = C("38E2E8");

    public static string Build()
    {
        // Run BuildMilestone9Scene.Build first when rebuilding from an earlier scene.
        EnsureFolders();
        GenerateColorArt();
        ConfigureImports();
        RewireArtAndTerrain();
        var levels = BuildLevels();
        PolishShellAndHud(levels);
        BuildReusableArenaPrefab();
        ReduceLegacyEnemySpeed();
        var scene = SceneManager.GetActiveScene();
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        return "Built Milestone 10: colorful authored presentation, polished modal UI, user path art, collision, level authoring, tutorial, 30-second skippable intermissions, safe turret previews, 64-step enemy palette, reusable arena prefab, and expressive camera.";
    }

    public static string Audit()
    {
        var root = GameObject.Find("/KeySlaught Gameplay") ?? throw new InvalidOperationException("Gameplay root missing.");
        var player = root.transform.Find("Player")?.GetComponent<PlayerMover>() ?? throw new InvalidOperationException("Player missing.");
        var playerData=new SerializedObject(player);if(playerData.FindProperty("blockedTerrain").objectReferenceValue==null)throw new InvalidOperationException("Player blocked terrain is not wired.");
        var highlight = root.transform.Find("Selected Ground Highlight")?.GetComponent<SpriteRenderer>() ?? throw new InvalidOperationException("Highlight missing.");
        if (highlight.sortingOrder >= player.GetComponent<SpriteRenderer>().sortingOrder) throw new InvalidOperationException("Highlight must render below player.");
        if (AssetDatabase.LoadAssetAtPath<LevelDefinition>($"{Levels}/Tutorial.asset") == null) throw new InvalidOperationException("Tutorial level missing.");
        var tutorial=AssetDatabase.LoadAssetAtPath<LevelDefinition>($"{Levels}/Tutorial.asset");if(tutorial.Waves.Length<2||tutorial.Waves.Any(w=>w==null||w.Words.Length<3))throw new InvalidOperationException("Tutorial wave content is incomplete.");
        if (AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/ReusableLevelArena.prefab") == null) throw new InvalidOperationException("Reusable arena prefab missing.");
        var pause = Object.FindFirstObjectByType<PauseMenuController>(FindObjectsInactive.Include) ?? throw new InvalidOperationException("Pause menu missing.");
        var close = pause.transform.Find("Controls Panel/Button CLOSE") ?? throw new InvalidOperationException("Controls close button missing.");
        var pathTile = AssetDatabase.LoadAssetAtPath<Tile>("Assets/Tiles/Monochrome/Path_H.asset");
        if (pathTile == null || pathTile.sprite == null || !pathTile.sprite.name.EndsWith("_2", StringComparison.Ordinal)) throw new InvalidOperationException("User path sprites are not wired.");
        return "Audit passed: colorful scene, user _2 paths, authored levels, controls modal, player-over-highlight ordering, and reusable arena prefab.";
    }

    private static void EnsureFolders()
    {
        Ensure("Assets/Art", "Colorful"); Ensure(Art, "GeneratedSources"); Ensure("Assets/Data", "Levels");
    }

    private static void GenerateColorArt()
    {
        Save("Ground_0_128",128,p=>Ground(p,128,0)); Save("Ground_1_128",128,p=>Ground(p,128,1));
        Save("Ground_2_128",128,p=>Ground(p,128,2)); Save("Ground_3_128",128,p=>Ground(p,128,3));
        Save("Player_Walk_0_128",128,p=>Player(p,128,0)); Save("Player_Walk_1_128",128,p=>Player(p,128,1));
        Save("Player_Walk_2_128",128,p=>Player(p,128,2)); Save("Player_Walk_3_128",128,p=>Player(p,128,3));
        Save("Library_256",256,p=>Library(p,256)); Save("Enemy_Card_128",128,p=>EnemyCard(p,128));
        Save("Brain_128",128,p=>Brain(p,128)); Save("Obstacle_Tree_128",128,p=>Tree(p,128));
        Save("Obstacle_Boulder_128",128,p=>Boulder(p,128));
        for(var i=0;i<4;i++){var v=i;Save($"Obstacle_Water_{i}_128",128,p=>Water(p,128,v));Save($"Obstacle_Mountain_{i}_128",128,p=>Mountain(p,128,v));}
        Save("Outer_Rock_128",128,p=>Boulder(p,128)); Save("Outer_Cloud_128",128,p=>Cloud(p,128));
        Save("Turret_Teacher_128",128,p=>Turret(p,128,0)); Save("Turret_Engineer_128",128,p=>Turret(p,128,1)); Save("Turret_Scientist_128",128,p=>Turret(p,128,2));
        Save("Cell_Highlight_128",128,p=>Highlight(p,128)); Save("UI_Pause_128",128,p=>Pause(p,128));
        Save("UI_Time_128",128,p=>TimeIcon(p,128)); Save("UI_Reload_128",128,p=>Reload(p,128)); Save("UI_Wand_128",128,p=>Wand(p,128));
    }

    private static void ConfigureImports()
    {
        AssetDatabase.Refresh();
        foreach(var path in Directory.GetFiles(Art,"*.png",SearchOption.TopDirectoryOnly).Concat(Directory.GetFiles(Mono,"Path_*_128_2.png")))
        {
            var assetPath=path.Replace('\\','/'); if(AssetImporter.GetAtPath(assetPath) is not TextureImporter importer) continue;
            importer.textureType=TextureImporterType.Sprite; importer.spriteImportMode=SpriteImportMode.Single; importer.spritePixelsPerUnit=128;
            importer.filterMode=FilterMode.Point; importer.textureCompression=TextureImporterCompression.Uncompressed; importer.mipmapEnabled=false; importer.alphaIsTransparency=true;
            importer.SaveAndReimport();
        }
    }

    private static void RewireArtAndTerrain()
    {
        for(var i=0;i<4;i++) SetTileSprite($"Ground_{i}", Sprite($"Ground_{i}_128"));
        foreach(var code in new[]{"H","V","LT","TR","LB","BR"}) SetTileSprite($"Path_{code}", AssetDatabase.LoadAssetAtPath<Sprite>($"{Mono}/Path_{code}_128_2.png"));
        SetTileSprite("Obstacle_Tree",Sprite("Obstacle_Tree_128")); SetTileSprite("Obstacle_Boulder",Sprite("Obstacle_Boulder_128"));
        for(var i=0;i<4;i++){SetTileSprite($"Obstacle_Water_{i}",Sprite($"Obstacle_Water_{i}_128"));SetTileSprite($"Obstacle_Mountain_{i}",Sprite($"Obstacle_Mountain_{i}_128"));}
        SetTileSprite("Outer_Rock",Sprite("Outer_Rock_128")); SetTileSprite("Outer_Cloud",Sprite("Outer_Cloud_128"));

        var root=GameObject.Find("/KeySlaught Gameplay").transform;
        var player=root.Find("Player").GetComponent<PlayerMover>(); var playerRenderer=player.GetComponent<SpriteRenderer>();
        var frames=Enumerable.Range(0,4).Select(i=>Sprite($"Player_Walk_{i}_128")).ToArray(); playerRenderer.sprite=frames[0]; playerRenderer.color=Color.white; playerRenderer.sortingOrder=14;
        player.GetComponent<PlayerPixelAnimator>().Configure(player,playerRenderer,frames);
        var library=root.Find("Divine Library").GetComponent<SpriteRenderer>(); library.sprite=Sprite("Library_256"); library.color=Color.white;
        var blocked=root.Find("Authored Tilemaps/Blocked Terrain Tilemap").GetComponent<Tilemap>(); player.ConfigureBlockedTerrain(blocked);
        var highlight=root.Find("Selected Ground Highlight").GetComponent<SpriteRenderer>(); highlight.sprite=Sprite("Cell_Highlight_128"); highlight.sortingOrder=8;
        var cameraMotion=root.Find("Cinemachine Player Camera")?.GetComponent<CameraMotionZoom>(); if(cameraMotion!=null) EditorUtility.SetDirty(cameraMotion);

        UpdatePrefab("Assets/Prefabs/EnemyPrototype.prefab",go=>{var r=go.GetComponent<SpriteRenderer>();r.sprite=Sprite("Enemy_Card_128");r.color=Color.white;go.transform.localScale=Vector3.one*1.08f;var t=go.GetComponentInChildren<TextMesh>();t.characterSize=.065f;t.fontSize=58;t.transform.localScale=Vector3.one*.72f;});
        UpdatePrefab("Assets/Prefabs/BrainCellPickup.prefab",go=>{GameObjectUtility.RemoveMonoBehavioursWithMissingScript(go);var r=go.GetComponent<SpriteRenderer>();r.sprite=Sprite("Brain_128");r.color=C("38E2E8");if(go.GetComponent<BrainCellPickup>()==null)go.AddComponent<BrainCellPickup>();});
        foreach(var pair in new[]{("TurretTeacher","Turret_Teacher_128"),("TurretEngineer","Turret_Engineer_128"),("TurretScientist","Turret_Scientist_128")})
            UpdatePrefab($"Assets/Prefabs/{pair.Item1}.prefab",go=>go.GetComponent<SpriteRenderer>().sprite=Sprite(pair.Item2));
    }

    private static LevelDefinition[] BuildLevels()
    {
        var tutorial=Level("Tutorial","TUTORIAL",new[]{
            new[]{("CAT",2.8f),("SUN",4.0f),("BOOK",4.5f)}, new[]{("MAP",2.4f),("TREE",3.8f),("IDEA",4.2f)}});
        var lore=Level("LoreOneLevelOne","LORE I - LEVEL 1",new[]{
            new[]{("HISTORY",1.8f),("BOOK",2.2f),("ARCHIVE",2.4f)}, new[]{("KNOWLEDGE",2.0f),("MEMORY",2.0f),("LIBRARY",2.3f)}, new[]{("PRESERVE",1.8f),("CULTURE",1.8f),("WISDOM",2.0f)}});
        var endless=Level("EndlessPrototype","ENDLESS",new[]{new[]{("BOOK",1.5f),("HISTORY",1.5f),("IDEA",1.5f),("MEMORY",1.5f)}});
        return new[]{tutorial,lore,endless};
    }

    private static LevelDefinition Level(string file,string title,(string,float)[][] waves)
    {
        var path=$"{Levels}/{file}.asset"; var level=AssetDatabase.LoadAssetAtPath<LevelDefinition>(path);
        if(level==null){level=ScriptableObject.CreateInstance<LevelDefinition>();AssetDatabase.CreateAsset(level,path);}
        var so=new SerializedObject(level);so.FindProperty("displayName").stringValue=title;so.FindProperty("intermissionSeconds").floatValue=30f;
        var waveArray=so.FindProperty("waves");waveArray.arraySize=waves.Length;
        for(var w=0;w<waves.Length;w++)
        {
            var wave=waveArray.GetArrayElementAtIndex(w);wave.FindPropertyRelative("title").stringValue=$"WAVE {w+1}";
            wave.FindPropertyRelative("averageTypingWordsPerMinute").floatValue=file=="Tutorial"?16f:24f;
            wave.FindPropertyRelative("targetWaveEndSeconds").floatValue=30f;wave.FindPropertyRelative("enemyMovementSpeed").floatValue=file=="Tutorial"?.54f:.8625f;
            var words=wave.FindPropertyRelative("words");words.arraySize=waves[w].Length;
            for(var i=0;i<waves[w].Length;i++){var word=words.GetArrayElementAtIndex(i);word.FindPropertyRelative("word").stringValue=waves[w][i].Item1;word.FindPropertyRelative("delayAfterPrevious").floatValue=waves[w][i].Item2;}
        }
        so.ApplyModifiedPropertiesWithoutUndo();EditorUtility.SetDirty(level);return level;
    }

    private static void PolishShellAndHud(LevelDefinition[] levels)
    {
        var solid=AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Prototype/SolidSprite.asset");
        var displayFont=AssetDatabase.LoadAssetAtPath<Font>("Assets/Fonts/PressStart2P/PressStart2P-Regular.ttf");
        var bodyFont=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        foreach(var text in Object.FindObjectsByType<Text>(FindObjectsInactive.Include,FindObjectsSortMode.None))
        {
            text.color=text.fontSize>=30?Parchment:new Color(.94f,.88f,.76f); text.font=text.fontSize>=30?displayFont:bodyFont;
            text.alignment=TextAnchor.MiddleCenter; text.resizeTextForBestFit=text.fontSize>20; text.resizeTextMinSize=12; text.resizeTextMaxSize=text.fontSize;
            if(text.text.Contains("K E Y S L A U G H T")){text.text="KEYSLAUGHT";text.fontSize=43;text.resizeTextForBestFit=false;text.horizontalOverflow=HorizontalWrapMode.Overflow;}
        }
        foreach(var button in Object.FindObjectsByType<Button>(FindObjectsInactive.Include,FindObjectsSortMode.None))
        {
            var image=button.GetComponent<Image>(); if(image!=null) image.color=Plum;
            var colors=button.colors;colors.normalColor=Plum;colors.highlightedColor=C("733866");colors.pressedColor=C("32142F");colors.selectedColor=C("67305E");colors.disabledColor=new Color(.23f,.20f,.30f,.72f);button.colors=colors;
            var buttonSerialized=new SerializedObject(button);buttonSerialized.FindProperty("m_Colors.m_NormalColor").colorValue=Plum;buttonSerialized.FindProperty("m_Colors.m_HighlightedColor").colorValue=C("733866");buttonSerialized.FindProperty("m_Colors.m_PressedColor").colorValue=C("32142F");buttonSerialized.FindProperty("m_Colors.m_SelectedColor").colorValue=C("67305E");buttonSerialized.ApplyModifiedPropertiesWithoutUndo();
            if(image!=null){var imageSerialized=new SerializedObject(image);imageSerialized.FindProperty("m_Color").colorValue=Color.white;imageSerialized.ApplyModifiedPropertiesWithoutUndo();}
            var buttonText=button.GetComponentInChildren<Text>(true);if(buttonText!=null)buttonText.color=Parchment;
            var icon=(button.name.Contains("Pause")||button.name.Contains("Reload"))
                ? button.GetComponentsInChildren<Image>(true).FirstOrDefault(i=>i.gameObject!=button.gameObject)
                : null;
            if(icon!=null){var r=icon.rectTransform;r.anchorMin=r.anchorMax=r.pivot=new Vector2(.5f,.5f);r.anchoredPosition=Vector2.zero;r.sizeDelta=button.name.Contains("Pause")?new Vector2(30,30):new Vector2(42,42);icon.preserveAspect=true;icon.color=Parchment;}
            if(image!=null)EditorUtility.SetDirty(image);EditorUtility.SetDirty(button);if(buttonText!=null)EditorUtility.SetDirty(buttonText);
        }
        var shell=GameObject.Find("/Game Shell Canvas/Game Shell");
        if(shell!=null)
        {
            shell.GetComponent<Image>().color=Ink;
            foreach(Transform panel in shell.transform){var image=panel.GetComponent<Image>();if(image!=null)image.color=panel.name=="Launch Screen"?C("1D294F"):Ink;}
            var settings=shell.transform.Find("Settings"); var old=settings?.Find("Button OPTION MENUS  PAUSE");if(old!=null)Object.DestroyImmediate(old.gameObject);
            if(settings!=null)
            {
                var b=TextButton("OPTION MENUS  PAUSE",settings,bodyFont,solid,23);Rect(b.GetComponent<RectTransform>(),new Vector2(.5f,.5f),new Vector2(.5f,.5f),new Vector2(0,-100),new Vector2(590,76));
                var controller=GameObject.Find("/KeySlaught Gameplay/Persistent Progression Systems").GetComponent<GameShellController>();
                UnityEventTools.AddPersistentListener(b.onClick,controller.ToggleSelectionPause);
                var tutorial=BuildTutorialOverlay(bodyFont,displayFont,solid,out var tutorialDirector);
                controller.ConfigureLevelContent(levels[0],levels[1],levels[2],tutorialDirector,b.GetComponentInChildren<Text>());
            }
        }
        PolishPause(bodyFont,displayFont,solid);
        BuildFastForward(bodyFont,solid);
    }

    private static GameObject BuildTutorialOverlay(Font body,Font display,Sprite solid,out TutorialDirector director)
    {
        var canvas=GameObject.Find("/Portrait Gameplay HUD").transform;var existing=canvas.Find("Tutorial Guidance");if(existing!=null)Object.DestroyImmediate(existing.gameObject);
        var overlay=Panel("Tutorial Guidance",canvas,solid,new Color(.04f,.025f,.09f,.93f));Full(overlay.rectTransform);
        var card=Panel("Guidance Card",overlay.transform,solid,Indigo);Rect(card.rectTransform,new Vector2(.5f,.5f),new Vector2(.5f,.5f),Vector2.zero,new Vector2(760,720));
        var title=Label("MOVE YOUR LIBRARIAN",card.transform,display,30,Gold);Rect(title.rectTransform,new Vector2(0,1),new Vector2(1,1),new Vector2(48,-58),new Vector2(-96,110));
        var bodyText=Label(string.Empty,card.transform,body,27,Parchment);bodyText.alignment=TextAnchor.MiddleLeft;Rect(bodyText.rectTransform,new Vector2(0,0),new Vector2(1,1),new Vector2(58,140),new Vector2(-116,-290));
        var next=TextButton("CONTINUE",card.transform,body,solid,25);Rect(next.GetComponent<RectTransform>(),new Vector2(.5f,0),new Vector2(.5f,0),new Vector2(0,48),new Vector2(520,76));
        director=GameObject.Find("/KeySlaught Gameplay/Persistent Progression Systems").GetComponent<TutorialDirector>();if(director==null)director=GameObject.Find("/KeySlaught Gameplay/Persistent Progression Systems").AddComponent<TutorialDirector>();
        var run=Object.FindFirstObjectByType<WaveRunController>(FindObjectsInactive.Include);var player=Object.FindFirstObjectByType<PlayerMover>(FindObjectsInactive.Include);
        director.Configure(overlay.gameObject,title,bodyText,next,player,run);UnityEventTools.AddPersistentListener(next.onClick,director.ContinueTutorial);overlay.gameObject.SetActive(false);return overlay.gameObject;
    }

    private static void PolishPause(Font body,Font display,Sprite solid)
    {
        var pause=Object.FindFirstObjectByType<PauseMenuController>(FindObjectsInactive.Include);if(pause==null)return;
        var overlay=pause.transform;var card=overlay.Find("Pause Window");if(card!=null)card.GetComponent<Image>().color=Indigo;
        var oldDim=overlay.Find("Modal Dimmer");if(oldDim!=null)Object.DestroyImmediate(oldDim.gameObject);
        var dim=Panel("Modal Dimmer",overlay,solid,new Color(.02f,.01f,.05f,.88f));Full(dim.rectTransform);dim.transform.SetSiblingIndex(Mathf.Max(1,overlay.childCount-3));dim.gameObject.SetActive(false);pause.ConfigureModalDimmer(dim.gameObject);
        if(card!=null)
        {
            var details=card.GetComponentsInChildren<Text>(true).FirstOrDefault(t=>t.text.Contains("ENDLESS MODE"));
            if(details!=null){details.text="THE DIVINE LIBRARY\n\nHold the path. Preserve knowledge.\n\nWave 1\nBrain cells are collected from defeated words.";details.font=body;details.fontSize=22;details.resizeTextForBestFit=false;details.alignment=TextAnchor.UpperLeft;var r=details.rectTransform;r.anchorMin=r.anchorMax=new Vector2(.5f,1);r.pivot=new Vector2(.5f,1);r.anchoredPosition=new Vector2(0,-155);r.sizeDelta=new Vector2(560,270);}
        }
        var controls=overlay.Find("Controls Panel");if(controls!=null)
        {
            controls.GetComponent<Image>().color=Indigo;var old=controls.Find("Button CLOSE");if(old!=null)Object.DestroyImmediate(old.gameObject);
            var close=TextButton("CLOSE",controls,body,solid,23);Rect(close.GetComponent<RectTransform>(),new Vector2(.5f,0),new Vector2(.5f,0),new Vector2(0,34),new Vector2(400,68));UnityEventTools.AddPersistentListener(close.onClick,pause.HideControls);
            var headingOld=controls.Find("Label CONTROLS");if(headingOld!=null)Object.DestroyImmediate(headingOld.gameObject);
            var heading=Label("CONTROLS",controls,display,28,Gold);Rect(heading.rectTransform,new Vector2(.5f,1),new Vector2(.5f,1),new Vector2(0,-38),new Vector2(600,60));
            var text=controls.GetComponentsInChildren<Text>(true).FirstOrDefault(t=>t.text.Contains("ARROW KEYS"));if(text!=null){text.text="MOVE\nArrow keys, controller, or drag in the arena.\n\nTYPE\nA-Z loads letters. Space reloads the wand.\n\nBUILD\n1-9 chooses the visible tile actions.";text.alignment=TextAnchor.UpperLeft;text.font=body;text.fontSize=22;text.resizeTextForBestFit=false;var r=text.rectTransform;r.anchorMin=Vector2.zero;r.anchorMax=Vector2.one;r.offsetMin=new Vector2(64,128);r.offsetMax=new Vector2(-64,-112);}
            controls.gameObject.SetActive(false);
        }
        var confirm=overlay.Find("Menu Confirmation");if(confirm!=null){confirm.GetComponent<Image>().color=C("3B214B");confirm.gameObject.SetActive(false);}
    }

    private static void BuildFastForward(Font font,Sprite solid)
    {
        var stats=GameObject.Find("/Portrait Gameplay HUD/Top 20 Percent/Aligned Stats Bar")?.transform;if(stats==null)return;
        var old=stats.Find("Button FAST FORWARD");if(old!=null)Object.DestroyImmediate(old.gameObject);
        var b=TextButton("FAST FORWARD",stats,font,solid,18);Rect(b.GetComponent<RectTransform>(),new Vector2(1,0),new Vector2(1,0),new Vector2(-105,-62),new Vector2(210,52));
        var run=Object.FindFirstObjectByType<WaveRunController>(FindObjectsInactive.Include);UnityEventTools.AddPersistentListener(b.onClick,run.SkipIntermission);b.gameObject.AddComponent<IntermissionFastForwardButton>().Configure(run,b);b.gameObject.SetActive(false);
        var hud=GameObject.Find("/Portrait Gameplay HUD").GetComponent<GameplayTopHud>();
        var coordinator=Object.FindFirstObjectByType<GameplaySceneCoordinator>();var labels=stats.GetComponentsInChildren<Text>(true);var wave=labels.FirstOrDefault(t=>t.gameObject.name.StartsWith("Label Wave"));var time=labels.FirstOrDefault(t=>t.text.Contains(":"));
        var target=GameObject.Find("/Portrait Gameplay HUD/Bottom 20 Percent")?.GetComponentsInChildren<Text>(true).FirstOrDefault(t=>t.fontSize>=30);hud.Configure(coordinator,wave,time,target,run);
    }

    private static void BuildReusableArenaPrefab()
    {
        var go=new GameObject("Reusable Level Arena");var grid=go.AddComponent<Grid>();
        Tilemap AddMap(string name,int order){var child=new GameObject(name);child.transform.SetParent(go.transform,false);var map=child.AddComponent<Tilemap>();child.AddComponent<TilemapRenderer>().sortingOrder=order;return map;}
        var outside=AddMap("Outside Terrain Tilemap",-30);var ground=AddMap("Ground Tilemap",-20);var path=AddMap("Enemy Path Tilemap",-5);var blocked=AddMap("Blocked Terrain Tilemap",-3);var clouds=AddMap("Cloud Cover Tilemap",22);
        var waypoints=new GameObject("Enemy Path Waypoints");waypoints.transform.SetParent(go.transform,false);var turrets=new GameObject("Placed Turrets");turrets.transform.SetParent(go.transform,false);
        go.AddComponent<ReusableLevelArena>().Configure(ground,path,blocked,outside,clouds,waypoints.transform,turrets.transform);
        PrefabUtility.SaveAsPrefabAsset(go,"Assets/Prefabs/ReusableLevelArena.prefab");Object.DestroyImmediate(go);
    }

    private static void ReduceLegacyEnemySpeed()
    {
        foreach(var guid in AssetDatabase.FindAssets("t:EnemyDefinition",new[]{"Assets/Data/Enemies"}))
        {var asset=AssetDatabase.LoadAssetAtPath<EnemyDefinition>(AssetDatabase.GUIDToAssetPath(guid));var so=new SerializedObject(asset);so.FindProperty("movementSpeed").floatValue=.8625f;so.ApplyModifiedPropertiesWithoutUndo();EditorUtility.SetDirty(asset);}
    }

    private static void UpdatePrefab(string path,Action<GameObject> update){var go=PrefabUtility.LoadPrefabContents(path);try{update(go);PrefabUtility.SaveAsPrefabAsset(go,path);}finally{PrefabUtility.UnloadPrefabContents(go);}}
    private static void SetTileSprite(string name,Sprite sprite){var tile=AssetDatabase.LoadAssetAtPath<Tile>($"Assets/Tiles/Monochrome/{name}.asset");if(tile==null||sprite==null)return;tile.sprite=sprite;tile.colliderType=Tile.ColliderType.None;EditorUtility.SetDirty(tile);}
    private static Sprite Sprite(string name)=>AssetDatabase.LoadAssetAtPath<Sprite>($"{Art}/{name}.png")??throw new InvalidOperationException("Missing "+name);
    private static void Ensure(string parent,string name){var p=parent+"/"+name;if(!AssetDatabase.IsValidFolder(p))AssetDatabase.CreateFolder(parent,name);}
    private static Color C(string hex){ColorUtility.TryParseHtmlString("#"+hex,out var c);return c;}

    private static void Save(string name,int size,Action<Color32[]> draw){var pixels=new Color32[size*size];draw(pixels);var texture=new Texture2D(size,size,TextureFormat.RGBA32,false);texture.SetPixels32(pixels);texture.Apply();File.WriteAllBytes($"{Art}/{name}.png",texture.EncodeToPNG());Object.DestroyImmediate(texture);}
    private static void Fill(Color32[] p,Color32 c){for(var i=0;i<p.Length;i++)p[i]=c;}
    private static void Box(Color32[] p,int s,int x,int y,int w,int h,Color32 c){for(var yy=Mathf.Max(0,y);yy<Mathf.Min(s,y+h);yy++)for(var xx=Mathf.Max(0,x);xx<Mathf.Min(s,x+w);xx++)p[yy*s+xx]=c;}
    private static void Circle(Color32[] p,int s,int cx,int cy,int r,Color32 c){var rr=r*r;for(var y=Mathf.Max(0,cy-r);y<Mathf.Min(s,cy+r+1);y++)for(var x=Mathf.Max(0,cx-r);x<Mathf.Min(s,cx+r+1);x++)if((x-cx)*(x-cx)+(y-cy)*(y-cy)<=rr)p[y*s+x]=c;}
    private static void Line(Color32[] p,int s,int x0,int y0,int x1,int y1,Color32 c,int width){var dx=Math.Abs(x1-x0);var sx=x0<x1?1:-1;var dy=-Math.Abs(y1-y0);var sy=y0<y1?1:-1;var err=dx+dy;while(true){Circle(p,s,x0,y0,Mathf.Max(1,width/2),c);if(x0==x1&&y0==y1)break;var e2=2*err;if(e2>=dy){err+=dy;x0+=sx;}if(e2<=dx){err+=dx;y0+=sy;}}}
    private static void Ground(Color32[] p,int s,int v){Fill(p,C("355E43"));for(var y=10;y<s;y+=24)for(var x=8;x<s;x+=26)Circle(p,s,x+(v*7+y)%13,y,2,C("78A85C"));}
    private static void Player(Color32[] p,int s,int f){Fill(p,new Color32(0,0,0,0));Circle(p,s,64,80,30,C("332B65"));Box(p,s,38,20+(f%2)*2,52,56,C("4A326E"));Circle(p,s,64,82,16,C("F4C7A1"));Box(p,s,48,76,32,9,C("EEF5FF"));Box(p,s,54,35,20,28,C("D05A72"));Circle(p,s,97,47,6,C("45E4EC"));Line(p,s,82,35,96,48,C("E6B85C"),4);}
    private static void Library(Color32[] p,int s){Fill(p,new Color32(0,0,0,0));Box(p,s,28,28,200,118,C("343061"));Box(p,s,48,52,160,100,C("F4D88A"));Box(p,s,68,70,120,82,C("FFF0CC"));Box(p,s,108,28,40,75,C("7B3F71"));for(var x=38;x<220;x+=36){Box(p,s,x,150,22,58,C("5A4A83"));Circle(p,s,x+11,210,18,C("E6B85C"));}Circle(p,s,128,202,30,C("45E4EC"));}
    private static void EnemyCard(Color32[] p,int s){Fill(p,new Color32(0,0,0,0));Box(p,s,8,8,112,112,C("32142F"));Box(p,s,14,14,100,100,C("F05278"));Box(p,s,29,29,70,70,C("5B224C"));for(var i=0;i<4;i++){Circle(p,s,18+i*30,18,4,C("E6B85C"));Circle(p,s,18+i*30,110,4,C("E6B85C"));}}
    private static void Brain(Color32[] p,int s){Fill(p,new Color32(0,0,0,0));Circle(p,s,48,65,29,C("38E2E8"));Circle(p,s,80,65,29,C("58F3FF"));Line(p,s,64,40,64,92,C("145B86"),5);for(var y=48;y<88;y+=13){Line(p,s,34,y,55,y+8,C("D3FFFF"),4);Line(p,s,94,y,73,y+8,C("D3FFFF"),4);}}
    private static void Tree(Color32[] p,int s){Fill(p,new Color32(0,0,0,0));Box(p,s,57,14,16,48,C("75442F"));Circle(p,s,64,76,42,C("1D6949"));Circle(p,s,42,72,25,C("34945B"));Circle(p,s,86,72,25,C("55B56C"));}
    private static void Boulder(Color32[] p,int s){Fill(p,new Color32(0,0,0,0));Circle(p,s,64,48,43,C("514B72"));Circle(p,s,48,67,34,C("706B92"));Circle(p,s,83,66,30,C("3A385D"));Line(p,s,43,76,63,58,C("A99DCB"),4);}
    private static void Water(Color32[] p,int s,int v){Fill(p,C("167C94"));for(var y=16+v*3;y<s;y+=22){for(var x=-10;x<s;x+=38)Line(p,s,x,y,x+20,y,C("71E6E2"),4);}}
    private static void Mountain(Color32[] p,int s,int v){Fill(p,new Color32(0,0,0,0));for(var x=-12;x<s;x+=46){var peak=82+((x+v*17)%28);for(var y=12;y<peak;y++){var half=(peak-y)/2;Box(p,s,x-half,y,half*2,1,C("6C4A9A"));}Line(p,s,x,peak,x-22,18,C("BFA7F5"),5);}}
    private static void Cloud(Color32[] p,int s){Fill(p,new Color32(0,0,0,0));Circle(p,s,44,62,30,C("F6D9E9"));Circle(p,s,72,72,39,C("FFF0F1"));Circle(p,s,98,60,27,C("DCCAF5"));Box(p,s,28,42,84,30,C("F5E8F2"));}
    private static void Turret(Color32[] p,int s,int t){Fill(p,new Color32(0,0,0,0));Circle(p,s,64,35,38,C("3A3565"));Box(p,s,31,25,66,30,C("6D4A83"));if(t==0){Box(p,s,38,64,52,38,C("FFF0CC"));Line(p,s,64,65,64,103,C("E6B85C"),4);}else if(t==1){Line(p,s,35,65,99,94,C("E6B85C"),15);Circle(p,s,94,94,15,C("38E2E8"));}else{Circle(p,s,64,82,24,C("A977E8"));Circle(p,s,64,82,12,C("F0DDFF"));}}
    private static void Highlight(Color32[] p,int s){Fill(p,new Color32(0,0,0,0));var c=(Color32)C("F2C45E");Box(p,s,4,4,s-8,4,c);Box(p,s,4,s-8,s-8,4,c);Box(p,s,4,4,4,s-8,c);Box(p,s,s-8,4,4,s-8,c);}
    private static void Pause(Color32[] p,int s){Fill(p,new Color32(0,0,0,0));Box(p,s,34,24,20,80,C("FFF0CC"));Box(p,s,74,24,20,80,C("FFF0CC"));}
    private static void TimeIcon(Color32[] p,int s){Fill(p,new Color32(0,0,0,0));Circle(p,s,64,62,44,C("E6B85C"));Circle(p,s,64,62,34,C("27244F"));Line(p,s,64,62,64,86,C("FFF0CC"),6);Line(p,s,64,62,83,50,C("FFF0CC"),6);}
    private static void Reload(Color32[] p,int s){Fill(p,new Color32(0,0,0,0));for(var d=35;d<330;d+=4){var a=d*Mathf.Deg2Rad;Circle(p,s,64+(int)(Mathf.Cos(a)*38),64+(int)(Mathf.Sin(a)*38),4,C("FFF0CC"));}Line(p,s,98,37,112,35,C("E6B85C"),7);Line(p,s,98,37,104,53,C("E6B85C"),7);}
    private static void Wand(Color32[] p,int s){Fill(p,new Color32(0,0,0,0));Line(p,s,28,24,92,92,C("E6B85C"),11);Line(p,s,31,27,90,90,C("FFF0CC"),5);Circle(p,s,97,97,16,C("38E2E8"));}

    private static Image Panel(string name,Transform parent,Sprite sprite,Color color){var go=new GameObject(name,typeof(RectTransform),typeof(CanvasRenderer),typeof(Image));go.transform.SetParent(parent,false);var image=go.GetComponent<Image>();image.sprite=sprite;image.color=color;return image;}
    private static Text Label(string value,Transform parent,Font font,int size,Color color){var go=new GameObject("Label "+value,typeof(RectTransform),typeof(CanvasRenderer),typeof(Text));go.transform.SetParent(parent,false);var t=go.GetComponent<Text>();t.text=value;t.font=font;t.fontSize=size;t.color=color;t.alignment=TextAnchor.MiddleCenter;t.horizontalOverflow=HorizontalWrapMode.Wrap;t.verticalOverflow=VerticalWrapMode.Overflow;return t;}
    private static Button TextButton(string value,Transform parent,Font font,Sprite solid,int size){var prefab=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/UiTextButton.prefab");var go=(GameObject)PrefabUtility.InstantiatePrefab(prefab,parent);go.name="Button "+value;var t=go.GetComponentInChildren<Text>(true);t.text=value;t.font=font;t.fontSize=size;t.color=Parchment;var image=go.GetComponent<Image>();image.color=Color.white;var button=go.GetComponent<Button>();var colors=button.colors;colors.normalColor=Plum;colors.highlightedColor=C("733866");colors.pressedColor=C("32142F");colors.selectedColor=C("67305E");button.colors=colors;return button;}
    private static void Full(RectTransform r){r.anchorMin=Vector2.zero;r.anchorMax=Vector2.one;r.offsetMin=Vector2.zero;r.offsetMax=Vector2.zero;}
    private static void Rect(RectTransform r,Vector2 min,Vector2 max,Vector2 pos,Vector2 size){r.anchorMin=min;r.anchorMax=max;r.pivot=min==max?min:new Vector2(.5f,.5f);r.anchoredPosition=pos;r.sizeDelta=size;}
}
