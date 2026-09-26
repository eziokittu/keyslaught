using System;
using System.IO;
using System.Linq;
using KeySlaught.Audio;
using KeySlaught.Progression;
using KeySlaught.SceneGameplay;
using KeySlaught.UI;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Object = UnityEngine.Object;

public static class BuildMilestone14Issues6
{
    private const string SampleScene="Assets/Scenes/SampleScene.unity";
    private const string Lore1Scene="Assets/Scenes/LoreOneLevelOne.unity";
    private const string Lore2Scene="Assets/Scenes/LoreOneLevelTwo.unity";
    private const string Lore3Scene="Assets/Scenes/LoreOneLevelThree.unity";
    private const string AudioRoot="Assets/Audio/Generated";
    private const string AudioLibraryPath="Assets/Data/Audio/KeySlaughtAudioLibrary.asset";
    private const string SplashPath="Assets/Art/Splash/KeySlaught_Splash_Cartoon.png";
    private static readonly Color Ink=C("11152B"), Plum=C("492044"), Gold=C("F4C967"), Cream=C("FFF2CF"), Teal=C("38B7B0");

    public static string Build()
    {
        if(EditorApplication.isPlaying) throw new InvalidOperationException("Exit Play Mode before authoring Milestone 14.");
        GenerateOriginalAudio(); AssetDatabase.Refresh(); ConfigureSplash();
        var library=BuildAudioLibrary(); TuneTurrets();
        var levels=BuildLoreLevels();
        BuildSample(library,levels);
        BuildLoreScene(Lore1Scene,library,levels[0],1);
        CopyAndBuildLore(Lore1Scene,Lore2Scene,library,levels[1],2);
        CopyAndBuildLore(Lore1Scene,Lore3Scene,library,levels[2],3);
        UpdateBuildSettings(); AssetDatabase.SaveAssets(); AssetDatabase.Refresh();
        return "Built issues6 milestone: minimalist splash, safe-area UI, compact icon actions, pause settings/polish, persistent replaceable audio, corruption feedback, slower turrets, and three five-wave Lore levels.";
    }

    public static string Audit()
    {
        var audio=AssetDatabase.LoadAssetAtPath<KeySlaughtAudioLibrary>(AudioLibraryPath) ?? throw new InvalidOperationException("Audio library missing.");
        if(audio.music==null || Math.Abs(audio.music.length-120f)>1f) throw new InvalidOperationException("Two-minute music is missing or wrong length.");
        foreach(var path in new[]{SampleScene,Lore1Scene,Lore2Scene,Lore3Scene})
        {
            var scene=EditorSceneManager.OpenScene(path,OpenSceneMode.Single);
            if(Object.FindFirstObjectByType<PersistentAudioDirector>(FindObjectsInactive.Include)==null) throw new InvalidOperationException($"Audio director missing in {path}.");
            if(Object.FindFirstObjectByType<SafeAreaPadding>(FindObjectsInactive.Include)==null) throw new InvalidOperationException($"Safe area missing in {path}.");
            var pause=Object.FindFirstObjectByType<PauseMenuController>(FindObjectsInactive.Include) ?? throw new InvalidOperationException($"Pause controller missing in {path}.");
            var pauseData=new SerializedObject(pause);
            if(pauseData.FindProperty("settingsPanel").objectReferenceValue==null) throw new InvalidOperationException($"Pause settings missing in {path}.");
            var feedback=Object.FindFirstObjectByType<CorruptionFeedback>(FindObjectsInactive.Include);
            if(feedback==null) throw new InvalidOperationException($"Corruption feedback missing in {path}.");
            if(path!=SampleScene)
            {
                var flow=Object.FindFirstObjectByType<DedicatedLevelSceneController>(FindObjectsInactive.Include);
                if(flow==null || flow.Level==null || flow.Level.Waves.Length!=5) throw new InvalidOperationException($"Five-wave level wiring missing in {path}.");
                if(flow.Level.Waves[4].Words[0].Word.Length<20) throw new InvalidOperationException($"Boss word is too short in {path}.");
            }
        }
        var sample=EditorSceneManager.OpenScene(SampleScene,OpenSceneMode.Single);
        var shell=Object.FindFirstObjectByType<GameShellController>(FindObjectsInactive.Include) ?? throw new InvalidOperationException("Shell missing.");
        var shellData=new SerializedObject(shell);
        if(shellData.FindProperty("loreOneLevelTwoButton").objectReferenceValue==null || shellData.FindProperty("loreOneLevelThreeButton").objectReferenceValue==null) throw new InvalidOperationException("Lore sequence buttons are not wired.");
        var splash=Find("Game Shell Canvas/Game Shell/Launch Screen/Cartoon Splash"); if(splash==null) throw new InvalidOperationException("Cartoon splash missing.");
        return "Audit passed: replaceable 120-second audio, safe areas, pause settings, corruption overlay, cartoon splash, and three authored five-wave Lore scenes with 20+ letter bosses.";
    }

    public static string ReviewLoreMenu()
    {
        var shell=Object.FindFirstObjectByType<GameShellController>(FindObjectsInactive.Include);Find("Game Shell Canvas/Game Shell")?.SetActive(true);shell?.ShowLoreLevels();return "Lore menu shown.";
    }

    public static string ReviewPauseSettings()
    {
        Find("Game Shell Canvas/Game Shell")?.SetActive(false);var tutorial=Find("Portrait Gameplay HUD/Tutorial Guidance");if(tutorial!=null)tutorial.SetActive(false);
        var pause=Object.FindFirstObjectByType<PauseMenuController>(FindObjectsInactive.Include);if(pause!=null){if(!pause.IsPaused)pause.TogglePause();pause.ToggleSettings();}return "Pause settings shown.";
    }

    public static string ReviewCorruption()
    {
        Find("Game Shell Canvas/Game Shell")?.SetActive(false);var pause=Object.FindFirstObjectByType<PauseMenuController>(FindObjectsInactive.Include);if(pause!=null&&pause.IsPaused)pause.Continue();
        Object.FindFirstObjectByType<GameplayCombatController>(FindObjectsInactive.Include)?.CorruptFor(8f);return "Corruption feedback shown.";
    }

    public static string ReviewTurretChoices()
    {
        Find("Game Shell Canvas/Game Shell")?.SetActive(false);var pause=Object.FindFirstObjectByType<PauseMenuController>(FindObjectsInactive.Include);if(pause!=null&&pause.IsPaused)pause.Continue();
        var context=Object.FindFirstObjectByType<TileContextActionPanel>(FindObjectsInactive.Include);context?.SelectAction(1);return "Turret family choices shown.";
    }

    public static string ReviewLoadLevelTwo(){SceneManager.LoadScene("LoreOneLevelTwo");return "Requested Lore I Level 2.";}
    public static string ReviewRouteStatus()
    {
        var flow=Object.FindFirstObjectByType<DedicatedLevelSceneController>(FindObjectsInactive.Include);var audio=Object.FindObjectsByType<PersistentAudioDirector>(FindObjectsInactive.Include,FindObjectsSortMode.None);
        var progression=Object.FindFirstObjectByType<ProgressionService>(FindObjectsInactive.Include);return $"scene={SceneManager.GetActiveScene().name}; audio={audio.Length}; progression={(progression!=null)}; level={flow?.LoreLevelNumber}; waves={flow?.Level?.Waves?.Length}.";
    }

    private static void BuildSample(KeySlaughtAudioLibrary library,LevelDefinition[] levels)
    {
        var scene=EditorSceneManager.OpenScene(SampleScene,OpenSceneMode.Single);
        CommonScene(library); BuildLaunchSplash(); ConfigureLoreMenu(levels); EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);
    }

    private static void CopyAndBuildLore(string source,string target,KeySlaughtAudioLibrary library,LevelDefinition level,int number)
    {
        if(AssetDatabase.LoadAssetAtPath<SceneAsset>(target)==null && !AssetDatabase.CopyAsset(source,target)) throw new InvalidOperationException($"Could not copy {target}.");
        BuildLoreScene(target,library,level,number);
    }

    private static void BuildLoreScene(string path,KeySlaughtAudioLibrary library,LevelDefinition level,int number)
    {
        var scene=EditorSceneManager.OpenScene(path,OpenSceneMode.Single); CommonScene(library);
        var flow=Object.FindFirstObjectByType<DedicatedLevelSceneController>(FindObjectsInactive.Include) ?? throw new InvalidOperationException("Dedicated level flow missing.");
        flow.SetLevelDefinition(level);flow.SetLoreLevelNumber(number);EditorUtility.SetDirty(flow);
        var run=Object.FindFirstObjectByType<WaveRunController>(FindObjectsInactive.Include);run.SetLevel(level);
        EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);
    }

    private static void CommonScene(KeySlaughtAudioLibrary library)
    {
        EnsureAudio(library); ConfigureSafeArea(); ConfigureTopHud(); ConfigureContextActions(); ConfigurePause(); ConfigureCorruption(); WireButtonAudio();
    }

    private static void EnsureAudio(KeySlaughtAudioLibrary library)
    {
        var existing=Object.FindFirstObjectByType<PersistentAudioDirector>(FindObjectsInactive.Include);
        if(existing!=null&&existing.gameObject.name!="Persistent Audio Director"){Object.DestroyImmediate(existing);existing=null;}
        if(existing==null){var host=new GameObject("Persistent Audio Director");existing=host.AddComponent<PersistentAudioDirector>();}
        existing.Configure(library);EditorUtility.SetDirty(existing);
    }

    private static void ConfigureSafeArea()
    {
        var canvas=Find("Portrait Gameplay HUD");if(canvas==null)return;
        var safe=canvas.GetComponent<SafeAreaPadding>()??canvas.AddComponent<SafeAreaPadding>();safe.Configure(22,24);
        var shell=Find("Game Shell Canvas");if(shell!=null){var shellSafe=shell.GetComponent<SafeAreaPadding>()??shell.AddComponent<SafeAreaPadding>();shellSafe.Configure(22,24);}
    }

    private static void ConfigureTopHud()
    {
        var stats=Find("Portrait Gameplay HUD/Top 20 Percent/Aligned Stats Bar")?.transform;if(stats==null)return;
        var labels=stats.GetComponentsInChildren<Text>(true);var wave=labels.FirstOrDefault(t=>t.gameObject.name.StartsWith("Label Wave"));
        var currency=labels.FirstOrDefault(t=>t.text.Contains("BRAIN CELLS")||t.gameObject.name.Contains("BRAIN"));var time=labels.FirstOrDefault(t=>t.text.Contains(":"));
        if(wave!=null)Rect(wave.rectTransform,new Vector2(0,1),new Vector2(0,1),new Vector2(26,-14),new Vector2(260,56));
        if(currency!=null){currency.text="0";currency.color=Cream;currency.fontSize=27;currency.resizeTextForBestFit=false;currency.alignment=TextAnchor.MiddleLeft;Rect(currency.rectTransform,new Vector2(.5f,1),new Vector2(.5f,1),new Vector2(-88,-14),new Vector2(120,56));}
        var brain=ChildImage(stats,"Brain Cell Icon",FindSprite("Brain_128"),Cream);Rect(brain.rectTransform,new Vector2(.5f,1),new Vector2(.5f,1),new Vector2(-134,-20),new Vector2(38,38));
        var oldTime=stats.Find("Time Icon");if(oldTime!=null)Object.DestroyImmediate(oldTime.gameObject);
        var clock=ChildImage(stats,"Elegant Time Icon",FindSprite("UI_Time_128"),Gold);Rect(clock.rectTransform,new Vector2(.5f,1),new Vector2(.5f,1),new Vector2(44,-20),new Vector2(38,38));
        if(time!=null)Rect(time.rectTransform,new Vector2(.5f,1),new Vector2(.5f,1),new Vector2(88,-14),new Vector2(120,56));
    }

    private static void ConfigureContextActions()
    {
        var panel=Find("Portrait Gameplay HUD/Top 20 Percent/Transparent Context Actions")?.GetComponent<RectTransform>();if(panel==null)return;
        panel.anchoredPosition=new Vector2(0,14);panel.sizeDelta=new Vector2(-44,230);
        var buttons=panel.GetComponentsInChildren<Button>(true).Where(b=>b.GetComponent<ContextActionButton>()!=null).OrderBy(b=>b.name).ToArray();
        var teacher=FindSprite("Turret_Teacher_128");var engineer=FindSprite("Turret_Engineer_128");var scientist=FindSprite("Turret_Scientist_128");var president=FindSprite("Turret_President_128");var brain=FindSprite("Brain_128");
        for(var i=0;i<buttons.Length;i++)
        {
            var r=buttons[i].GetComponent<RectTransform>();var row=i/2;var col=i%2;Rect(r,new Vector2(0,1),new Vector2(0,1),new Vector2(20+col*418,-48-row*66),new Vector2(394,58));
            var label=buttons[i].GetComponentInChildren<Text>(true);if(label!=null){label.fontSize=20;label.alignment=TextAnchor.MiddleCenter;label.rectTransform.offsetMin=new Vector2(58,4);label.rectTransform.offsetMax=new Vector2(-44,-4);}
            var family=ChildImage(buttons[i].transform,"Family Icon",null,Cream);family.raycastTarget=false;Rect(family.rectTransform,new Vector2(0,.5f),new Vector2(0,.5f),new Vector2(14,-22),new Vector2(44,44));
            var cost=ChildImage(buttons[i].transform,"Cost Icon",brain,Teal);cost.raycastTarget=false;Rect(cost.rectTransform,new Vector2(1,.5f),new Vector2(1,.5f),new Vector2(-43,-14),new Vector2(28,28));
            var presenter=buttons[i].GetComponent<ContextActionIconPresenter>()??buttons[i].gameObject.AddComponent<ContextActionIconPresenter>();presenter.Configure(label,family,cost,teacher,engineer,scientist,president,brain);
        }
    }

    private static void ConfigurePause()
    {
        var pause=Object.FindFirstObjectByType<PauseMenuController>(FindObjectsInactive.Include);if(pause==null)return;var overlay=pause.transform;
        var card=overlay.Find("Pause Window") as RectTransform;if(card!=null)
        {
            card.sizeDelta=new Vector2(720,1040);var title=card.GetComponentsInChildren<Text>(true).FirstOrDefault(t=>t.text=="PAUSED");
            if(title!=null){title.fontSize=48;Rect(title.rectTransform,new Vector2(.5f,1),new Vector2(.5f,1),new Vector2(0,-76),new Vector2(600,90));if(title.GetComponent<UiBreathingAnimator>()==null)title.gameObject.AddComponent<UiBreathingAnimator>().Configure(.018f,.45f);}
            var details=card.GetComponentsInChildren<Text>(true).FirstOrDefault(t=>t.text.Contains("DIVINE LIBRARY"));if(details!=null){details.text="◆  THE DIVINE LIBRARY  ◆\n\nDefend the path • collect knowledge\nBuild specialists • preserve every word";details.fontSize=27;details.alignment=TextAnchor.MiddleCenter;Rect(details.rectTransform,new Vector2(.5f,1),new Vector2(.5f,1),new Vector2(0,-205),new Vector2(600,230));}
            var continueButton=card.GetComponentsInChildren<Button>(true).FirstOrDefault(b=>b.name.Contains("CONTINUE"));var controlsButton=card.GetComponentsInChildren<Button>(true).FirstOrDefault(b=>b.name.Contains("CONTROLS"));var menuButton=card.GetComponentsInChildren<Button>(true).FirstOrDefault(b=>b.name.Contains("BACK TO MENU"));
            if(continueButton!=null)CenterBottom(continueButton.GetComponent<RectTransform>(),250);if(controlsButton!=null)CenterBottom(controlsButton.GetComponent<RectTransform>(),155);if(menuButton!=null)CenterBottom(menuButton.GetComponent<RectTransform>(),-35);
            var old=card.Find("Button SETTINGS");if(old!=null)Object.DestroyImmediate(old.gameObject);var settingsButton=TextButton("SETTINGS",card,26);CenterBottom(settingsButton.GetComponent<RectTransform>(),60);UnityEventTools.AddPersistentListener(settingsButton.onClick,pause.ToggleSettings);settingsButton.gameObject.AddComponent<UiBreathingAnimator>().Configure(.012f,.38f,1.1f);
        }
        var previous=overlay.Find("Audio Settings Panel");if(previous!=null)Object.DestroyImmediate(previous.gameObject);
        var settingsPanel=Panel("Audio Settings Panel",overlay,Ink);Rect(settingsPanel.rectTransform,new Vector2(.5f,.5f),new Vector2(.5f,.5f),Vector2.zero,new Vector2(700,760));
        var settingsTitle=Label("SETTINGS",settingsPanel.transform,34,Gold,Vector2.zero,new Vector2(600,80));CenterTop(settingsTitle.rectTransform,-58);
        var music=TextButton("MUSIC  ON",settingsPanel.transform,25);CenterTop(music.GetComponent<RectTransform>(),-175);
        var sfx=TextButton("SFX  ON",settingsPanel.transform,25);CenterTop(sfx.GetComponent<RectTransform>(),-365);
        var musicSlider=SliderControl("Music Volume",settingsPanel.transform,-265);var sfxSlider=SliderControl("Sfx Volume",settingsPanel.transform,-455);
        var close=TextButton("CLOSE",settingsPanel.transform,24);CenterBottom(close.GetComponent<RectTransform>(),45);
        var controller=settingsPanel.gameObject.AddComponent<AudioSettingsPanel>();controller.Configure(music.GetComponentInChildren<Text>(),sfx.GetComponentInChildren<Text>(),musicSlider,sfxSlider);
        UnityEventTools.AddPersistentListener(music.onClick,controller.ToggleMusic);UnityEventTools.AddPersistentListener(sfx.onClick,controller.ToggleSfx);UnityEventTools.AddPersistentListener(musicSlider.onValueChanged,controller.SetMusicVolume);UnityEventTools.AddPersistentListener(sfxSlider.onValueChanged,controller.SetSfxVolume);UnityEventTools.AddPersistentListener(close.onClick,pause.HideSettings);
        pause.ConfigureSettings(settingsPanel.gameObject);settingsPanel.gameObject.SetActive(false);
    }

    private static void ConfigureCorruption()
    {
        var bottom=Find("Portrait Gameplay HUD/Bottom 20 Percent")?.transform;if(bottom==null)return;var old=bottom.Find("Corruption Overlay");if(old!=null)Object.DestroyImmediate(old.gameObject);
        var group=bottom.GetComponent<CanvasGroup>()??bottom.gameObject.AddComponent<CanvasGroup>();var overlay=Panel("Corruption Overlay",bottom,new Color(.12f,.02f,.14f,.92f));Full(overlay.rectTransform);
        var label=Label("WAND CORRUPTED\n2.5s",overlay.transform,32,Cream,Vector2.zero,new Vector2(650,140));label.fontStyle=FontStyle.Bold;label.gameObject.AddComponent<UiBreathingAnimator>().Configure(.022f,.8f);
        var feedback=bottom.GetComponent<CorruptionFeedback>()??bottom.gameObject.AddComponent<CorruptionFeedback>();feedback.Configure(Object.FindFirstObjectByType<GameplayCombatController>(FindObjectsInactive.Include),Object.FindFirstObjectByType<PlayerMover>(FindObjectsInactive.Include),group,overlay.gameObject,label);overlay.gameObject.SetActive(false);
    }

    private static void BuildLaunchSplash()
    {
        var launch=Find("Game Shell Canvas/Game Shell/Launch Screen")?.transform;if(launch==null)return;var old=launch.Find("Cartoon Splash");if(old!=null)Object.DestroyImmediate(old.gameObject);
        var sprite=AssetDatabase.LoadAssetAtPath<Sprite>(SplashPath)??throw new InvalidOperationException("Approved cartoon splash did not import as a sprite.");
        var splash=ChildImage(launch,"Cartoon Splash",sprite,Color.white);Full(splash.rectTransform);splash.preserveAspect=false;splash.raycastTarget=false;splash.transform.SetAsFirstSibling();
        foreach(var text in launch.GetComponentsInChildren<Text>(true))if(text.text.Replace(" ","").ToUpperInvariant().Contains("KEYSLAUGHT"))text.gameObject.SetActive(false);
        var shade=Panel("Splash Readability Shade",launch,new Color(.02f,.03f,.09f,.12f));Full(shade.rectTransform);shade.raycastTarget=false;shade.transform.SetSiblingIndex(1);
    }

    private static void ConfigureLoreMenu(LevelDefinition[] levels)
    {
        var shell=Object.FindFirstObjectByType<GameShellController>(FindObjectsInactive.Include);if(shell==null)return;var root=Find("Game Shell Canvas/Game Shell/Lore Levels")?.transform;if(root==null)return;
        var level2=root.GetComponentsInChildren<Button>(true).FirstOrDefault(b=>b.name.Contains("LEVEL 2"));var level3=root.GetComponentsInChildren<Button>(true).FirstOrDefault(b=>b.name.Contains("LEVEL 3"));
        if(level2!=null){level2.interactable=true;Clear(level2.onClick);UnityEventTools.AddPersistentListener(level2.onClick,shell.StartLoreOneLevelTwo);}
        if(level3!=null){level3.interactable=true;Clear(level3.onClick);UnityEventTools.AddPersistentListener(level3.onClick,shell.StartLoreOneLevelThree);}
        shell.ConfigureLoreSequence(level2,level2?.GetComponentInChildren<Text>(true),level3,level3?.GetComponentInChildren<Text>(true));
        var note=root.GetComponentsInChildren<Text>(true).FirstOrDefault(t=>t.text.Contains("RESERVED"));if(note!=null)note.text="COMPLETE EACH LEVEL TO UNLOCK THE NEXT CHAPTER.";
    }

    private static void WireButtonAudio()
    {
        foreach(var button in SceneManager.GetActiveScene().GetRootGameObjects().SelectMany(r=>r.GetComponentsInChildren<Button>(true)))
        {
            var relay=button.GetComponent<AudioClickRelay>()??button.gameObject.AddComponent<AudioClickRelay>();
            var exists=Enumerable.Range(0,button.onClick.GetPersistentEventCount()).Any(i=>button.onClick.GetPersistentTarget(i)==relay);
            if(!exists)UnityEventTools.AddPersistentListener(button.onClick,relay.PlayClick);
        }
    }

    private static LevelDefinition[] BuildLoreLevels()
    {
        EnsureFolder("Assets/Data","Levels");
        return new[]{
            Level("LoreOneLevelOne","LORE I - LEVEL 1",new[]{
                Wave("WAVE 1",.54f,"BOOK","CAT","IDEA"),Wave("WAVE 2",.57f,"HISTORY","LOGIC","POETRY","MAP"),Wave("WAVE 3",.60f,"ARCHIVE","ORACLE","LIBRARY","SCROLL"),Wave("WAVE 4",.63f,"KNOWLEDGE","WISDOM","LANGUAGE","MEMORY","TRUTH"),Wave("BOSS WAVE",.48f,"COUNTERREVOLUTIONARIES")}),
            Level("LoreOneLevelTwo","LORE I - LEVEL 2",new[]{
                Wave("WAVE 1",.60f,"SYMBOL","REASON","LEGEND","VERSE"),Wave("WAVE 2",.64f,"PHILOSOPHY","RHETORIC","DIALECTIC","MYTH"),Wave("WAVE 3",.67f,"MANUSCRIPT","CHRONICLE","TESTAMENT","CIPHER"),Wave("WAVE 4",.70f,"CIVILIZATION","IMAGINATION","TRANSLATION","DISCOVERY","EVIDENCE"),Wave("BOSS WAVE",.54f,"INCOMPREHENSIBILITIES")}),
            Level("LoreOneLevelThree","LORE I - LEVEL 3",new[]{
                Wave("WAVE 1",.66f,"THEOREM","PARADOX","AXIOM","PROOF"),Wave("WAVE 2",.70f,"CONSTELLATION","ALGORITHM","EQUATION","NEBULA"),Wave("WAVE 3",.74f,"ARCHAEOLOGY","LINGUISTICS","ASTRONOMY","GEOMETRY"),Wave("WAVE 4",.78f,"INTERPRETATION","CONSCIOUSNESS","METAMORPHOSIS","INHERITANCE","REVOLUTION"),Wave("BOSS WAVE",.59f,"ELECTROENCEPHALOGRAPHIC")})};
    }

    private static (string,float,string[]) Wave(string title,float speed,params string[] words)=>(title,speed,words);
    private static LevelDefinition Level(string file,string display,(string title,float speed,string[] words)[] waves)
    {
        var path=$"Assets/Data/Levels/{file}.asset";var level=AssetDatabase.LoadAssetAtPath<LevelDefinition>(path);if(level==null){level=ScriptableObject.CreateInstance<LevelDefinition>();AssetDatabase.CreateAsset(level,path);}
        var data=new SerializedObject(level);data.FindProperty("displayName").stringValue=display;data.FindProperty("intermissionSeconds").floatValue=12f;var list=data.FindProperty("waves");list.arraySize=waves.Length;
        for(var i=0;i<waves.Length;i++){var wave=list.GetArrayElementAtIndex(i);wave.FindPropertyRelative("title").stringValue=waves[i].title;wave.FindPropertyRelative("enemyMovementSpeed").floatValue=waves[i].speed;wave.FindPropertyRelative("targetWaveEndSeconds").floatValue=42+i*8;var words=wave.FindPropertyRelative("words");words.arraySize=waves[i].words.Length;for(var j=0;j<waves[i].words.Length;j++){var entry=words.GetArrayElementAtIndex(j);entry.FindPropertyRelative("word").stringValue=waves[i].words[j];entry.FindPropertyRelative("delayAfterPrevious").floatValue=j==0?.4f:1.5f;}}
        data.ApplyModifiedPropertiesWithoutUndo();EditorUtility.SetDirty(level);return level;
    }

    private static void TuneTurrets()
    {
        foreach(var item in new[]{("Teacher",1.35f),("Engineer",1.85f),("Scientist",2.35f),("President",2.85f)})
        {var path=AssetDatabase.FindAssets($"{item.Item1} t:TurretDefinition").Select(AssetDatabase.GUIDToAssetPath).FirstOrDefault();var def=AssetDatabase.LoadAssetAtPath<TurretDefinition>(path);if(def==null)continue;var data=new SerializedObject(def);data.FindProperty("secondsPerShot").floatValue=item.Item2;data.ApplyModifiedPropertiesWithoutUndo();EditorUtility.SetDirty(def);}
    }

    private static KeySlaughtAudioLibrary BuildAudioLibrary()
    {
        EnsureFolder("Assets/Data","Audio");var lib=AssetDatabase.LoadAssetAtPath<KeySlaughtAudioLibrary>(AudioLibraryPath);if(lib==null){lib=ScriptableObject.CreateInstance<KeySlaughtAudioLibrary>();AssetDatabase.CreateAsset(lib,AudioLibraryPath);}
        var data=new SerializedObject(lib);SetClip(data,"music","KeySlaught_EarlyJazz_120s.wav");
        foreach(var item in new[]{("enemyHit","EnemyHit_Guitar.wav"),("libraryHit","LibraryHit_Tabla.wav"),("playerCorrupted","PlayerCorrupted_Sax.wav"),("turretPlaced","TurretPlaced_Tabla.wav"),("turretFired","TurretFired_Guitar.wav"),("turretSold","TurretSold_Sax.wav"),("turretUpgraded","TurretUpgraded_Guitar.wav"),("uiClick","UiClick_Tabla.wav"),("gameWin","GameWin_Sax.wav"),("gameLost","GameLost_Guitar.wav"),("roundStart","RoundStart_Tabla.wav")})SetClip(data,item.Item1,item.Item2);
        data.ApplyModifiedPropertiesWithoutUndo();EditorUtility.SetDirty(lib);return lib;
    }
    private static void SetClip(SerializedObject data,string property,string file)=>data.FindProperty(property).objectReferenceValue=AssetDatabase.LoadAssetAtPath<AudioClip>($"{AudioRoot}/{file}");

    private static void GenerateOriginalAudio()
    {
        Directory.CreateDirectory(AudioRoot);WriteMusic($"{AudioRoot}/KeySlaught_EarlyJazz_120s.wav");
        var cues=new[]{("EnemyHit_Guitar.wav",.20,220.0,0), ("LibraryHit_Tabla.wav",.52,92.0,1),("PlayerCorrupted_Sax.wav",.75,174.0,2),("TurretPlaced_Tabla.wav",.34,128.0,1),("TurretFired_Guitar.wav",.16,330.0,0),("TurretSold_Sax.wav",.38,196.0,2),("TurretUpgraded_Guitar.wav",.62,262.0,0),("UiClick_Tabla.wav",.12,180.0,1),("GameWin_Sax.wav",1.6,220.0,2),("GameLost_Guitar.wav",1.35,147.0,0),("RoundStart_Tabla.wav",.72,110.0,1)};
        foreach(var c in cues){var path=$"{AudioRoot}/{c.Item1}";if(!File.Exists(path))WriteCue(path,c.Item2,c.Item3,c.Item4);}
    }
    private static void WriteMusic(string path)
    {
        if(File.Exists(path))return;const int rate=22050;const double length=120;var chords=new[]{new[]{220.0,261.63,329.63},new[]{196.0,246.94,293.66},new[]{174.61,220.0,261.63},new[]{164.81,207.65,246.94}};
        WriteWave(path,rate,(int)(rate*length),i=>{var t=i/(double)rate;var bar=(int)(t/2.4);var chord=chords[bar%chords.Length];var beat=t%2.4;var env=.55+.45*Math.Sin(Math.PI*Math.Min(1,beat/.18));double guitar=0;foreach(var f in chord)guitar+=Math.Sin(2*Math.PI*f*t)+.22*Math.Sin(4*Math.PI*f*t);guitar*=.038*env;var bass=.075*Math.Sin(2*Math.PI*(chord[0]/2)*t);var step=(int)(t/.3);var melodyFreq=new[]{329.63,392.0,440.0,392.0,349.23,329.63,293.66,261.63}[step%8];var swing=(step%2==1?.06:0);var phase=Math.Max(0,t-(step*.3+swing));var sax=phase<.22?.07*Math.Sin(2*Math.PI*melodyFreq*t)*(1-phase/.22):0;var hit=t%1.2;var tabla=hit<.06?.08*Math.Sin(2*Math.PI*(115-70*hit/.06)*t)*(1-hit/.06):0;return guitar+bass+sax+tabla;});
    }
    private static void WriteCue(string path,double seconds,double frequency,int voice)
    {
        const int rate=22050;WriteWave(path,rate,(int)(rate*seconds),i=>{var t=i/(double)rate;var p=t/seconds;var env=Math.Sin(Math.PI*Math.Min(1,p))*Math.Pow(1-p,.55);if(voice==1)return .34*env*(Math.Sin(2*Math.PI*(frequency*(1-0.35*p))*t)+.35*Math.Sin(2*Math.PI*frequency*2.1*t));if(voice==2)return .26*env*(Math.Sin(2*Math.PI*frequency*t)+.28*Math.Sin(2*Math.PI*frequency*2*t)+.12*Math.Sin(2*Math.PI*frequency*3*t));return .26*env*(Math.Sin(2*Math.PI*frequency*t)+.4*Math.Sin(2*Math.PI*frequency*2*t));});
    }
    private static void WriteWave(string path,int rate,int count,Func<int,double> sample)
    {
        using var stream=File.Create(path);using var writer=new BinaryWriter(stream);writer.Write(System.Text.Encoding.ASCII.GetBytes("RIFF"));writer.Write(36+count*2);writer.Write(System.Text.Encoding.ASCII.GetBytes("WAVEfmt "));writer.Write(16);writer.Write((short)1);writer.Write((short)1);writer.Write(rate);writer.Write(rate*2);writer.Write((short)2);writer.Write((short)16);writer.Write(System.Text.Encoding.ASCII.GetBytes("data"));writer.Write(count*2);for(var i=0;i<count;i++)writer.Write((short)(Math.Max(-1,Math.Min(1,sample(i)))*short.MaxValue));
    }

    private static void ConfigureSplash(){if(AssetImporter.GetAtPath(SplashPath) is TextureImporter importer){importer.textureType=TextureImporterType.Sprite;importer.spriteImportMode=SpriteImportMode.Single;importer.mipmapEnabled=false;importer.alphaIsTransparency=true;importer.SaveAndReimport();}}
    private static void UpdateBuildSettings(){var required=new[]{SampleScene,Lore1Scene,Lore2Scene,Lore3Scene};var existing=EditorBuildSettings.scenes.ToList();foreach(var path in required)if(existing.All(s=>s.path!=path))existing.Add(new EditorBuildSettingsScene(path,true));EditorBuildSettings.scenes=existing.ToArray();}
    private static void Clear(UnityEngine.Events.UnityEvent action){for(var i=action.GetPersistentEventCount()-1;i>=0;i--)UnityEventTools.RemovePersistentListener(action,i);}
    private static void EnsureFolder(string parent,string name){var path=$"{parent}/{name}";if(!AssetDatabase.IsValidFolder(path))AssetDatabase.CreateFolder(parent,name);}
    private static GameObject Find(string path)=>GameObject.Find("/"+path)??Resources.FindObjectsOfTypeAll<GameObject>().FirstOrDefault(g=>g.scene==SceneManager.GetActiveScene()&&Hierarchy(g)==path);
    private static string Hierarchy(GameObject go){var value=go.name;for(var p=go.transform.parent;p!=null;p=p.parent)value=p.name+"/"+value;return value;}
    private static Sprite FindSprite(params string[] names){foreach(var name in names){var path=AssetDatabase.FindAssets($"{name} t:Sprite").Select(AssetDatabase.GUIDToAssetPath).FirstOrDefault();var sprite=AssetDatabase.LoadAssetAtPath<Sprite>(path);if(sprite!=null)return sprite;}return null;}
    private static Image ChildImage(Transform parent,string name,Sprite sprite,Color color){var old=parent.Find(name);if(old!=null)Object.DestroyImmediate(old.gameObject);var go=new GameObject(name,typeof(RectTransform),typeof(CanvasRenderer),typeof(Image));go.transform.SetParent(parent,false);var image=go.GetComponent<Image>();image.sprite=sprite;image.color=color;image.preserveAspect=true;return image;}
    private static Image Panel(string name,Transform parent,Color color){var image=ChildImage(parent,name,AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Prototype/SolidSprite.asset"),color);image.type=Image.Type.Sliced;return image;}
    private static Text Label(string text,Transform parent,int size,Color color,Vector2 pos,Vector2 dimensions){var go=new GameObject("Label "+text.Split('\n')[0],typeof(RectTransform),typeof(CanvasRenderer),typeof(Text));go.transform.SetParent(parent,false);var label=go.GetComponent<Text>();label.text=text;label.font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");label.fontSize=size;label.color=color;label.alignment=TextAnchor.MiddleCenter;label.resizeTextForBestFit=true;label.resizeTextMinSize=14;label.resizeTextMaxSize=size;Rect(label.rectTransform,new Vector2(.5f,.5f),new Vector2(.5f,.5f),pos,dimensions);return label;}
    private static Button TextButton(string text,Transform parent,int size){var prefab=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/UiTextButton.prefab");var go=(GameObject)PrefabUtility.InstantiatePrefab(prefab,parent);go.name="Button "+text;var label=go.GetComponentInChildren<Text>(true);label.text=text;label.fontSize=size;label.color=Cream;return go.GetComponent<Button>();}
    private static Slider SliderControl(string name,Transform parent,float y){var go=new GameObject(name,typeof(RectTransform),typeof(Slider));go.transform.SetParent(parent,false);Rect(go.GetComponent<RectTransform>(),new Vector2(.5f,1),new Vector2(.5f,1),new Vector2(0,y),new Vector2(520,42));var bg=Panel("Background",go.transform,new Color(.12f,.08f,.18f,1));Full(bg.rectTransform);var fillArea=new GameObject("Fill Area",typeof(RectTransform));fillArea.transform.SetParent(go.transform,false);Full(fillArea.GetComponent<RectTransform>());var fill=Panel("Fill",fillArea.transform,Teal);Full(fill.rectTransform);var handleArea=new GameObject("Handle Slide Area",typeof(RectTransform));handleArea.transform.SetParent(go.transform,false);Full(handleArea.GetComponent<RectTransform>());var handle=Panel("Handle",handleArea.transform,Gold);Rect(handle.rectTransform,new Vector2(.5f,.5f),new Vector2(.5f,.5f),Vector2.zero,new Vector2(32,54));var slider=go.GetComponent<Slider>();slider.fillRect=fill.rectTransform;slider.handleRect=handle.rectTransform;slider.targetGraphic=handle;slider.minValue=0;slider.maxValue=1;slider.value=1;return slider;}
    private static void CenterTop(RectTransform r,float y){Rect(r,new Vector2(.5f,1),new Vector2(.5f,1),new Vector2(0,y),new Vector2(520,76));}
    private static void CenterBottom(RectTransform r,float y){Rect(r,new Vector2(.5f,0),new Vector2(.5f,0),new Vector2(0,y),new Vector2(520,76));}
    private static void Full(RectTransform r){r.anchorMin=Vector2.zero;r.anchorMax=Vector2.one;r.offsetMin=Vector2.zero;r.offsetMax=Vector2.zero;}
    private static void Rect(RectTransform r,Vector2 min,Vector2 max,Vector2 pos,Vector2 size){r.anchorMin=min;r.anchorMax=max;r.pivot=min==max?new Vector2(min.x,min.y):new Vector2(.5f,.5f);r.anchoredPosition=pos;r.sizeDelta=size;}
    private static Color C(string hex){ColorUtility.TryParseHtmlString("#"+hex,out var color);return color;}
}
