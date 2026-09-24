using System;
using KeySlaught.Progression;
using KeySlaught.SceneGameplay;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Object = UnityEngine.Object;

public static class BuildMilestone9Scene
{
    private const string ResearchFolder = "Assets/Data/Research";
    private const string ButtonPrefab = "Assets/Prefabs/UiTextButton.prefab";

    public static string Build()
    {
        EnsureFolder("Assets/Data", "Research");
        var definitions = new[]
        {
            Research("PlayerRange", "PLAYER_RANGE", "PLAYER RANGE", ResearchStat.PlayerRange, 5, 1, 1, .25f),
            Research("MagazineCapacity", "MAGAZINE_CAPACITY", "MAGAZINE CAPACITY", ResearchStat.MagazineCapacity, 2, 2, 2, 1f),
            Research("LibraryHealth", "LIBRARY_HEALTH", "LIBRARY HEALTH", ResearchStat.LibraryHealth, 5, 1, 1, 5f),
            Research("ReloadSpeed", "RELOAD_SPEED", "RELOAD SPEED", ResearchStat.ReloadSpeed, 5, 1, 1, .05f)
        };

        var gameplay = GameObject.Find("/KeySlaught Gameplay") ?? throw new InvalidOperationException("Gameplay root missing.");
        var oldSystems = gameplay.transform.Find("Persistent Progression Systems");
        if (oldSystems != null) Object.DestroyImmediate(oldSystems.gameObject);
        var systems = new GameObject("Persistent Progression Systems"); systems.transform.SetParent(gameplay.transform, false);
        var progression = systems.AddComponent<ProgressionService>(); progression.Configure(definitions);
        var coordinator = gameplay.transform.Find("Gameplay Coordinator").GetComponent<GameplaySceneCoordinator>();
        var combat = coordinator.GetComponent<GameplayCombatController>();
        var library = gameplay.transform.Find("Divine Library").GetComponent<LibraryEndpoint>();
        var run = gameplay.transform.Find("Bounded Run Systems").GetComponent<WaveRunController>();
        var applier = systems.AddComponent<PermanentUpgradeApplier>(); applier.Configure(progression, coordinator, combat, library);
        var shell = systems.AddComponent<GameShellController>();

        var oldCanvas = GameObject.Find("/Game Shell Canvas"); if (oldCanvas != null) Object.DestroyImmediate(oldCanvas);
        var canvasObject = new GameObject("Game Shell Canvas");
        var canvas = canvasObject.AddComponent<Canvas>(); canvas.renderMode = RenderMode.ScreenSpaceOverlay; canvas.sortingOrder = 500;
        var scaler = canvasObject.AddComponent<CanvasScaler>(); scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(900, 1600); scaler.matchWidthOrHeight = .5f;
        canvasObject.AddComponent<GraphicRaycaster>();
        var solid = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Prototype/SolidSprite.asset");
        var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        var shellRoot = Panel("Game Shell", canvasObject.transform, solid, new Color(.015f,.018f,.024f,1)); Full(shellRoot.rectTransform);

        var launch = Screen("Launch Screen", shellRoot.transform, solid);
        Title(launch.transform, font, "K E Y S L A U G H T", "DEFEND THE DIVINE LIBRARY");
        var launchActions = new GameObject("Launch Actions", typeof(RectTransform)); launchActions.transform.SetParent(launch.transform, false);
        Rect(launchActions.GetComponent<RectTransform>(), new Vector2(.5f,.5f), new Vector2(.5f,.5f), new Vector2(0,-240), new Vector2(620,220));
        var continueButton = TextButton("CONTINUE", launchActions.transform, font); Rect(continueButton.GetComponent<RectTransform>(), new Vector2(.5f,1),new Vector2(.5f,1),Vector2.zero,new Vector2(520,76));
        var exitButton = TextButton("EXIT", launchActions.transform, font); Rect(exitButton.GetComponent<RectTransform>(),new Vector2(.5f,1),new Vector2(.5f,1),new Vector2(0,-96),new Vector2(520,76));

        var main = Screen("Main Menu", shellRoot.transform, solid); Title(main.transform,font,"K E Y S L A U G H T","THE DIVINE LIBRARY");
        var startButton = CenterButton(main.transform,font,"START GAME",100);
        var researchButton = CenterButton(main.transform,font,"RESEARCH",0);
        var creditsButton = CenterButton(main.transform,font,"CREDITS",-100);
        var mainBack = CornerButton(main.transform,font,"‹  BACK",true); var settingsButton = CornerButton(main.transform,font,"SETTINGS  ⚙",false);

        var modes = Screen("Mode Select", shellRoot.transform, solid); Header(modes.transform,font,"CHOOSE A MODE","Core gameplay uses the current prototype arena.");
        var tutorialButton = CenterButton(modes.transform,font,"TUTORIAL  •  RECOMMENDED",180); var tutorialText = tutorialButton.GetComponentInChildren<Text>();
        var loreButton = CenterButton(modes.transform,font,"LORE I  •  LEVEL 1",70);
        var endlessButton = CenterButton(modes.transform,font,"ENDLESS  •  LOCKED",-40); var endlessText = endlessButton.GetComponentInChildren<Text>();
        var modesBack = CornerButton(modes.transform,font,"‹  BACK",true);

        var research = Screen("Research", shellRoot.transform, solid); Header(research.transform,font,"RESEARCH","Permanent upgrades persist between runs.");
        var knowledge = Label("KNOWLEDGE POINTS  0",research.transform,font,25,Color.white); Rect(knowledge.rectTransform,new Vector2(.5f,.5f),new Vector2(.5f,.5f),new Vector2(0,250),new Vector2(700,60));
        var researchButtons = new Button[4]; var researchLabels = new Text[4];
        for (var index=0; index<4; index++) { researchButtons[index]=CenterButton(research.transform,font,definitions[index].DisplayName,120-index*92); researchLabels[index]=researchButtons[index].GetComponentInChildren<Text>(); }
        var researchNote=Label("Knowledge is awarded by completing authored lore levels.\nBalance and rewards remain prototype values.",research.transform,font,19,new Color(.72f,.74f,.78f));
        Rect(researchNote.rectTransform,new Vector2(.5f,.5f),new Vector2(.5f,.5f),new Vector2(0,-310),new Vector2(720,100));
        var researchBack=CornerButton(research.transform,font,"‹  BACK",true);

        var credits = Screen("Credits", shellRoot.transform, solid); Header(credits.transform,font,"CREDITS",string.Empty);
        var creditsText=Label("KEYSLAUGHT\n\nDEVELOPED BY\nBODHISATTA BHATTACHARJEE\n\nBuilt with transparent AI-assisted development support.",credits.transform,font,27,Color.white);
        Rect(creditsText.rectTransform,new Vector2(.5f,.5f),new Vector2(.5f,.5f),Vector2.zero,new Vector2(720,500));
        var creditsBack=CornerButton(credits.transform,font,"‹  BACK",true);

        var settings = Screen("Settings", shellRoot.transform, solid); Header(settings.transform,font,"SETTINGS","Local presentation preferences.");
        var musicButton=CenterButton(settings.transform,font,"MUSIC  ON",100); var musicLabel=musicButton.GetComponentInChildren<Text>();
        var sfxButton=CenterButton(settings.transform,font,"SFX  ON",0); var sfxLabel=sfxButton.GetComponentInChildren<Text>();
        var settingsBack=CornerButton(settings.transform,font,"‹  BACK",true);

        shell.Configure(progression,applier,run,shellRoot.gameObject,launch.gameObject,launchActions,main.gameObject,modes.gameObject,
            research.gameObject,credits.gameObject,settings.gameObject,tutorialText,endlessButton,endlessText,knowledge,researchButtons,researchLabels,musicLabel,sfxLabel);
        UnityEventTools.AddPersistentListener(continueButton.onClick,shell.ContinueFromLaunch); UnityEventTools.AddPersistentListener(exitButton.onClick,shell.ExitGame);
        UnityEventTools.AddPersistentListener(startButton.onClick,shell.ShowModes); UnityEventTools.AddPersistentListener(researchButton.onClick,shell.ShowResearch);
        UnityEventTools.AddPersistentListener(creditsButton.onClick,shell.ShowCredits); UnityEventTools.AddPersistentListener(mainBack.onClick,shell.ShowLaunch);
        UnityEventTools.AddPersistentListener(settingsButton.onClick,shell.ShowSettings); UnityEventTools.AddPersistentListener(tutorialButton.onClick,shell.StartTutorial);
        UnityEventTools.AddPersistentListener(loreButton.onClick,shell.StartLoreOneLevelOne); UnityEventTools.AddPersistentListener(endlessButton.onClick,shell.StartEndless);
        UnityEventTools.AddPersistentListener(modesBack.onClick,shell.ShowMain); UnityEventTools.AddPersistentListener(researchBack.onClick,shell.ShowMain);
        UnityEventTools.AddPersistentListener(creditsBack.onClick,shell.ShowMain); UnityEventTools.AddPersistentListener(settingsBack.onClick,shell.ShowMain);
        UnityEventTools.AddPersistentListener(musicButton.onClick,shell.ToggleMusic); UnityEventTools.AddPersistentListener(sfxButton.onClick,shell.ToggleSfx);
        for(var index=0;index<researchButtons.Length;index++) UnityEventTools.AddIntPersistentListener(researchButtons[index].onClick,shell.PurchaseResearch,index);

        WirePauseMenu(shell,font);
        var scene=SceneManager.GetActiveScene(); EditorSceneManager.MarkSceneDirty(scene); EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets(); AssetDatabase.Refresh();
        return "Built persistent research/save foundation and authored launch, main, mode, research, credits, and settings shell.";
    }

    public static string Audit()
    {
        var systems=GameObject.Find("/KeySlaught Gameplay/Persistent Progression Systems") ?? throw new InvalidOperationException("Progression systems missing.");
        var service=systems.GetComponent<ProgressionService>() ?? throw new InvalidOperationException("ProgressionService missing.");
        var shell=systems.GetComponent<GameShellController>() ?? throw new InvalidOperationException("GameShellController missing.");
        if(service.Definitions==null||service.Definitions.Length!=4) throw new InvalidOperationException("Expected four research definitions.");
        var canvas=GameObject.Find("/Game Shell Canvas") ?? throw new InvalidOperationException("Game shell canvas missing.");
        foreach(var name in new[]{"Launch Screen","Main Menu","Mode Select","Research","Credits","Settings"})
            if(canvas.transform.Find("Game Shell/"+name)==null) throw new InvalidOperationException(name+" panel missing.");
        return "Audit passed: 4 research definitions, persistent service/applier, and 6 authored shell panels with mode gating.";
    }

    private static ResearchDefinition Research(string file,string id,string title,ResearchStat stat,int max,int cost,int growth,float value)
    {
        var path=$"{ResearchFolder}/{file}.asset"; var asset=AssetDatabase.LoadAssetAtPath<ResearchDefinition>(path);
        if(asset==null){asset=ScriptableObject.CreateInstance<ResearchDefinition>();AssetDatabase.CreateAsset(asset,path);}
        var s=new SerializedObject(asset); s.FindProperty("id").stringValue=id;s.FindProperty("displayName").stringValue=title;s.FindProperty("stat").enumValueIndex=(int)stat;
        s.FindProperty("maxLevel").intValue=max;s.FindProperty("baseCost").intValue=cost;s.FindProperty("costIncreasePerLevel").intValue=growth;s.FindProperty("valuePerLevel").floatValue=value;
        s.ApplyModifiedPropertiesWithoutUndo();EditorUtility.SetDirty(asset);return asset;
    }

    private static void WirePauseMenu(GameShellController shell,Font font)
    {
        var pause=Object.FindFirstObjectByType<PauseMenuController>(FindObjectsInactive.Include); if(pause==null)return;
        pause.ConfigureShell(shell); var confirmation=pause.transform.Find("Menu Confirmation"); if(confirmation==null)return;
        var old=confirmation.Find("Button CONFIRM");if(old!=null)Object.DestroyImmediate(old.gameObject);
        var confirm=TextButton("CONFIRM",confirmation,font);Rect(confirm.GetComponent<RectTransform>(),new Vector2(.5f,0),new Vector2(.5f,0),new Vector2(0,118),new Vector2(430,72));
        UnityEventTools.AddPersistentListener(confirm.onClick,pause.ConfirmBackToMenu);
    }

    private static Image Screen(string name,Transform parent,Sprite solid){var p=Panel(name,parent,solid,new Color(.018f,.021f,.029f,1));Full(p.rectTransform);return p;}
    private static void Title(Transform parent,Font font,string title,string subtitle){var t=Label(title,parent,font,54,Color.white);t.fontStyle=FontStyle.Bold;Rect(t.rectTransform,new Vector2(.5f,.5f),new Vector2(.5f,.5f),new Vector2(0,250),new Vector2(820,100));var s=Label(subtitle,parent,font,21,new Color(.7f,.73f,.78f));Rect(s.rectTransform,new Vector2(.5f,.5f),new Vector2(.5f,.5f),new Vector2(0,180),new Vector2(760,50));}
    private static void Header(Transform parent,Font font,string title,string subtitle){var t=Label(title,parent,font,44,Color.white);t.fontStyle=FontStyle.Bold;Rect(t.rectTransform,new Vector2(.5f,1),new Vector2(.5f,1),new Vector2(0,-140),new Vector2(760,80));var s=Label(subtitle,parent,font,20,new Color(.7f,.73f,.78f));Rect(s.rectTransform,new Vector2(.5f,1),new Vector2(.5f,1),new Vector2(0,-210),new Vector2(760,70));}
    private static Button CenterButton(Transform parent,Font font,string text,float y){var b=TextButton(text,parent,font);Rect(b.GetComponent<RectTransform>(),new Vector2(.5f,.5f),new Vector2(.5f,.5f),new Vector2(0,y),new Vector2(590,76));return b;}
    private static Button CornerButton(Transform parent,Font font,string text,bool left){var b=TextButton(text,parent,font);Rect(b.GetComponent<RectTransform>(),new Vector2(left?0:1,1),new Vector2(left?0:1,1),new Vector2(left?24:-24,-24),new Vector2(190,64));return b;}
    private static Button TextButton(string text,Transform parent,Font font){var prefab=AssetDatabase.LoadAssetAtPath<GameObject>(ButtonPrefab);var go=(GameObject)PrefabUtility.InstantiatePrefab(prefab,parent);go.name="Button "+text;var label=go.GetComponentInChildren<Text>(true);label.text=text;label.font=font;label.fontSize=23;return go.GetComponent<Button>();}
    private static Image Panel(string name,Transform parent,Sprite sprite,Color color){var go=new GameObject(name,typeof(RectTransform),typeof(CanvasRenderer),typeof(Image));go.transform.SetParent(parent,false);var i=go.GetComponent<Image>();i.sprite=sprite;i.color=color;return i;}
    private static Text Label(string text,Transform parent,Font font,int size,Color color){var go=new GameObject("Label "+text,typeof(RectTransform),typeof(CanvasRenderer),typeof(Text));go.transform.SetParent(parent,false);var t=go.GetComponent<Text>();t.text=text;t.font=font;t.fontSize=size;t.color=color;t.alignment=TextAnchor.MiddleCenter;t.horizontalOverflow=HorizontalWrapMode.Wrap;t.verticalOverflow=VerticalWrapMode.Overflow;return t;}
    private static void Full(RectTransform r){r.anchorMin=Vector2.zero;r.anchorMax=Vector2.one;r.offsetMin=Vector2.zero;r.offsetMax=Vector2.zero;}
    private static void Rect(RectTransform r,Vector2 min,Vector2 max,Vector2 pos,Vector2 size){r.anchorMin=min;r.anchorMax=max;r.pivot=min==max?min:new Vector2(.5f,.5f);r.anchoredPosition=pos;r.sizeDelta=size;}
    private static void EnsureFolder(string parent,string name){var path=parent+"/"+name;if(!AssetDatabase.IsValidFolder(path))AssetDatabase.CreateFolder(parent,name);}
}
