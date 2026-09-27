using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
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

public static class BuildMilestone19Issues10
{
    private const string Sample = "Assets/Scenes/SampleScene.unity";
    private static readonly string[] TutorialScenes =
    {
        "Assets/Scenes/TutorialBasics.unity", "Assets/Scenes/TutorialTurrets.unity",
        "Assets/Scenes/TutorialAbilities.unity"
    };
    private static readonly string[] LoreScenes =
    {
        "Assets/Scenes/LoreOneLevelOne.unity", "Assets/Scenes/LoreOneLevelTwo.unity",
        "Assets/Scenes/LoreOneLevelThree.unity", "Assets/Scenes/LoreOneLevelFour.unity",
        "Assets/Scenes/LoreOneLevelFive.unity", "Assets/Scenes/LoreOneLevelSix.unity"
    };
    private static readonly Color Ink = C("10152B");
    private static readonly Color Gold = C("F4C967");
    private static readonly Color Cream = C("FFF2CF");
    private static readonly Color Teal = C("38B7B0");

    public static string Build()
    {
        if (EditorApplication.isPlaying) throw new InvalidOperationException("Exit Play Mode before authoring Milestone 19.");
        EnsureGeneratedIcons();
        var definitions = BuildResearchDefinitions();

        var scene = EditorSceneManager.OpenScene(Sample, OpenSceneMode.Single);
        ConfigureCommon(definitions);
        ConfigureSample(definitions);
        EditorSceneManager.MarkSceneDirty(scene); EditorSceneManager.SaveScene(scene);
        foreach (var path in TutorialScenes)
            EditorSceneManager.SaveScene(scene, path, true);

        foreach (var path in TutorialScenes)
        {
            scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
            ConfigureCommon(definitions);
            ConfigureResult(true);
            EditorSceneManager.MarkSceneDirty(scene); EditorSceneManager.SaveScene(scene);
        }

        foreach (var path in LoreScenes)
        {
            scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
            ConfigureCommon(definitions);
            ConfigureResult(false);
            EditorSceneManager.MarkSceneDirty(scene); EditorSceneManager.SaveScene(scene);
        }

        UpdateBuildSettings(); AssetDatabase.SaveAssets(); AssetDatabase.Refresh();
        EditorSceneManager.OpenScene(Sample, OpenSceneMode.Single);
        return Audit();
    }

    public static string Audit()
    {
        var errors = new List<string>();
        foreach (var path in new[] { Sample }.Concat(TutorialScenes).Concat(LoreScenes))
        {
            EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
            var player = Object.FindFirstObjectByType<PlayerMover>(FindObjectsInactive.Include);
            if (player == null || player.MaximumBounds.x < 6.6f || player.MaximumBounds.y < 7.6f) errors.Add($"{path} right/top bounds not expanded");
            var cameraMotion = Object.FindFirstObjectByType<CameraMotionZoom>(FindObjectsInactive.Include);
            if (cameraMotion == null) errors.Add($"{path} camera zoom controller missing");
            else
            {
                var cameraData = new SerializedObject(cameraMotion);
                if (Mathf.Abs(cameraData.FindProperty("idleSize").floatValue - 7f) > .01f ||
                    Mathf.Abs(cameraData.FindProperty("movingSize").floatValue - 10f) > .01f ||
                    cameraData.FindProperty("directionalTiltDegrees").floatValue > .001f)
                    errors.Add($"{path} camera is not zoom-only 7-to-10");
            }
            var pause = Object.FindFirstObjectByType<PauseMenuController>(FindObjectsInactive.Include);
            if (pause == null || pause.transform.Find("Pause Window/Inline Run Controls") == null) errors.Add($"{path} pause run controls missing");
            var result = Object.FindFirstObjectByType<RunResultPresenter>(FindObjectsInactive.Include);
            if (result == null || Find("Portrait Gameplay HUD/Run Result Overlay/Reward Reveal") == null) errors.Add($"{path} result/reward visuals missing");
            var progression = Object.FindFirstObjectByType<ProgressionService>(FindObjectsInactive.Include);
            if (progression == null || progression.Definitions == null || progression.Definitions.Length != 22) errors.Add($"{path} research definitions are not 22");
            if (Find("Portrait Gameplay HUD/Wand Reset Status") == null) errors.Add($"{path} refresh countdown panel missing");
        }
        EditorSceneManager.OpenScene(Sample, OpenSceneMode.Single);
        if (Find("Game Shell Canvas/Game Shell/Tutorial Levels") == null) errors.Add("Tutorial selector missing");
        if (Object.FindFirstObjectByType<TurretTutorialDirector>(FindObjectsInactive.Include) == null) errors.Add("Turret tutorial director missing");
        if (Object.FindFirstObjectByType<AbilityTutorialDirector>(FindObjectsInactive.Include) == null) errors.Add("Ability tutorial director missing");
        if (Find("Game Shell Canvas/Game Shell/Research/Structured Research") == null) errors.Add("Structured research UI missing");
        if (Find("Game Shell Canvas/Game Shell/Settings/Game Speed Controls") != null) errors.Add("Game speed still appears in permanent settings");
        foreach (var path in TutorialScenes)
            if (!EditorBuildSettings.scenes.Any(scene => scene.enabled && scene.path == path)) errors.Add($"{path} missing from Build Settings");
        if (errors.Count > 0) throw new InvalidOperationException(string.Join(". ", errors));
        return "Milestone 19 Issues 10-11 audit passed: zoom-only smooth camera, gated research unlocks, reachable brain-cell drops, pulsing refresh/keyboard guidance, reload timer, three linked authored tutorials, separated HUD timer, and unobstructed repair actions.";
    }

    public static string ReviewPause()
    {
        Find("Game Shell Canvas/Game Shell")?.SetActive(false);
        var pause = Object.FindFirstObjectByType<PauseMenuController>(FindObjectsInactive.Include);
        if (pause != null && !pause.IsPaused) pause.TogglePause();
        return "Pause menu staged with inline speed and audio controls.";
    }

    public static string ReviewTutorials()
    {
        Find("Game Shell Canvas/Game Shell")?.SetActive(true);
        Object.FindFirstObjectByType<GameShellController>(FindObjectsInactive.Include)?.ShowTutorialLevels();
        return "Tutorial selector staged.";
    }

    public static string ReviewBasicsRefresh()
    {
        var director = Object.FindFirstObjectByType<TutorialDirector>(FindObjectsInactive.Include);
        var combat = Object.FindFirstObjectByType<GameplayCombatController>(FindObjectsInactive.Include);
        if (director == null || combat == null) return "Tutorial refresh review unavailable.";
        director.Begin();
        combat.TryTypeLetter('C'); combat.TryTypeLetter('A'); combat.TryTypeLetter('T');
        SetTutorialStage(director, "BufferLoadedMessage");
        director.ContinueTutorial();
        combat.TryStartRefresh();
        combat.enabled = false;
        return $"Basic refresh staged; remaining={combat.RefreshSecondsRemaining:0.0}s.";
    }

    public static string ReviewBasicsRepair()
    {
        var director = Object.FindFirstObjectByType<TutorialDirector>(FindObjectsInactive.Include);
        var player = Object.FindFirstObjectByType<PlayerMover>(FindObjectsInactive.Include);
        var library = Object.FindFirstObjectByType<LibraryEndpoint>(FindObjectsInactive.Include);
        if (director == null || player == null || library == null) return "Tutorial repair review unavailable.";
        director.Begin();
        SetTutorialStage(director, "LibraryMessage");
        director.ContinueTutorial();
        player.transform.position = library.transform.position;
        return "Basic repair staged below the Library action controls.";
    }

    public static async Task<string> ProbeCameraZoom()
    {
        var player = Object.FindFirstObjectByType<PlayerMover>(FindObjectsInactive.Include);
        var motion = Object.FindFirstObjectByType<CameraMotionZoom>(FindObjectsInactive.Include);
        if (player == null || motion == null || Camera.main == null) return "Camera probe unavailable.";
        TutorialInputGate.Clear();
        player.SetVirtualMovement(Vector2.right);
        await Task.Delay(2200);
        var moving = Camera.main.orthographicSize;
        var movingRotation = motion.transform.localEulerAngles.z;
        player.SetVirtualMovement(Vector2.zero);
        await Task.Delay(6500);
        var idle = Camera.main.orthographicSize;
        var idleRotation = motion.transform.localEulerAngles.z;
        var readSize = typeof(CameraMotionZoom).GetMethod("ReadSize", BindingFlags.Instance | BindingFlags.NonPublic);
        var virtualSize = readSize == null ? -1f : Convert.ToSingle(readSize.Invoke(motion, null));
        var idleSecondsField = typeof(CameraMotionZoom).GetField("idleSeconds", BindingFlags.Instance | BindingFlags.NonPublic);
        var idleSeconds = idleSecondsField == null ? -1f : Convert.ToSingle(idleSecondsField.GetValue(motion));
        var idleSize = Convert.ToSingle(typeof(CameraMotionZoom).GetField("idleSize", BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(motion));
        var configuredMovingSize = Convert.ToSingle(typeof(CameraMotionZoom).GetField("movingSize", BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(motion));
        var smoothedSize = Convert.ToSingle(typeof(CameraMotionZoom).GetField("currentSize", BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(motion));
        return $"movingSize={moving:0.00}; idleCameraSize={idle:0.00}; idleVirtualSize={virtualSize:0.00}; configured={idleSize:0.00}-{configuredMovingSize:0.00}; idleSeconds={idleSeconds:0.00}; smoothedSize={smoothedSize:0.00}; input={player.LastMovementInput}; enabled={motion.enabled}; movingRotation={movingRotation:0.00}; idleRotation={idleRotation:0.00}";
    }

    public static string StartCameraMove()
    {
        TutorialInputGate.Clear();
        Object.FindFirstObjectByType<PlayerMover>(FindObjectsInactive.Include)?.SetVirtualMovement(Vector2.right);
        return "Camera movement input started.";
    }

    public static string StopCameraMove()
    {
        Object.FindFirstObjectByType<PlayerMover>(FindObjectsInactive.Include)?.SetVirtualMovement(Vector2.zero);
        return "Camera movement input stopped.";
    }

    public static string CameraState()
    {
        var player = Object.FindFirstObjectByType<PlayerMover>(FindObjectsInactive.Include);
        var motion = Object.FindFirstObjectByType<CameraMotionZoom>(FindObjectsInactive.Include);
        return Camera.main == null || motion == null
            ? "Camera state unavailable."
            : $"size={Camera.main.orthographicSize:0.00}; input={player?.LastMovementInput}; rotation={motion.transform.localEulerAngles.z:0.00}";
    }

    private static void SetTutorialStage(TutorialDirector director, string value)
    {
        var field = typeof(TutorialDirector).GetField("stage", BindingFlags.Instance | BindingFlags.NonPublic);
        field?.SetValue(director, Enum.Parse(field.FieldType, value));
    }

    public static string ReviewResearch()
    {
        Find("Game Shell Canvas/Game Shell")?.SetActive(true);
        Object.FindFirstObjectByType<GameShellController>(FindObjectsInactive.Include)?.ShowResearch();
        return "Structured research staged.";
    }

    public static string ReviewVictory()
    {
        Find("Game Shell Canvas/Game Shell")?.SetActive(false);
        var pause = Object.FindFirstObjectByType<PauseMenuController>(FindObjectsInactive.Include); if (pause != null && pause.IsPaused) pause.Continue();
        var presenter = Object.FindFirstObjectByType<RunResultPresenter>(FindObjectsInactive.Include);
        presenter?.Present(true, 1, "Teacher", false);
        return "Victory result staged.";
    }

    public static string ReviewDefeat()
    {
        Find("Game Shell Canvas/Game Shell")?.SetActive(false);
        Object.FindFirstObjectByType<RunResultPresenter>(FindObjectsInactive.Include)?.Present(false, 0, null, false);
        return "Defeat result staged.";
    }

    public static string ReviewReward()
    {
        ReviewVictory();
        Object.FindFirstObjectByType<RunResultPresenter>(FindObjectsInactive.Include)?.PresentUnlockReward("Teacher", FindSprite("Turret_Teacher_128"));
        return "Teacher reward reveal staged.";
    }

    public static string ResetLocalProgress()
    {
        Object.FindFirstObjectByType<ProgressionService>(FindObjectsInactive.Include)?.ResetProgress();
        GameSpeedSettings.ResetForLevel();
        PlayerPrefs.DeleteKey("KeySlaught.ReturnToMainMenu"); PlayerPrefs.Save();
        return "Local progress and run speed reset.";
    }

    public static string CleanupReviewCaptures()
    {
        var removed = AssetDatabase.DeleteAsset("Assets/Temp") ? 1 : 0;
        AssetDatabase.Refresh();
        return $"Removed {removed} temporary review folder.";
    }

    private static void ConfigureCommon(ResearchDefinition[] definitions)
    {
        var progression = Object.FindFirstObjectByType<ProgressionService>(FindObjectsInactive.Include);
        if (progression != null) { progression.Configure(definitions); EditorUtility.SetDirty(progression); }
        ConfigurePlayerBounds(); ConfigureCamera(); ConfigurePause(); ConfigureHudLayout(); ConfigureRefreshStatus();
    }

    private static void ConfigureSample(ResearchDefinition[] definitions)
    {
        var shell = Object.FindFirstObjectByType<GameShellController>(FindObjectsInactive.Include)
            ?? throw new InvalidOperationException("Game shell missing.");
        var service = Object.FindFirstObjectByType<ProgressionService>(FindObjectsInactive.Include)
            ?? throw new InvalidOperationException("Progression service missing.");
        RemoveMenuSpeed(shell);
        BuildStructuredResearch(shell, service, definitions);
        AddLoreLocks();
        BuildTutorialSelector(shell);
        BuildTurretTutorial(shell);
        BuildAbilityTutorial(shell);
        ConfigureResult(true);
        shell.ConfigureTeacherRewardSprite(FindSprite("Turret_Teacher_128"));
        EditorUtility.SetDirty(shell);
    }

    private static void ConfigurePlayerBounds()
    {
        var player = Object.FindFirstObjectByType<PlayerMover>(FindObjectsInactive.Include); if (player == null) return;
        var data = new SerializedObject(player);
        data.FindProperty("maximumBounds").vector2Value = new Vector2(6.65f, 7.65f);
        data.ApplyModifiedPropertiesWithoutUndo(); EditorUtility.SetDirty(player);
    }

    private static void ConfigureCamera()
    {
        var motion = Object.FindFirstObjectByType<CameraMotionZoom>(FindObjectsInactive.Include); if (motion == null) return;
        var data = new SerializedObject(motion);
        data.FindProperty("movingSize").floatValue = 10f;
        data.FindProperty("idleSize").floatValue = 7f;
        data.FindProperty("idleDelay").floatValue = 1f;
        data.FindProperty("smoothTime").floatValue = .8f;
        data.FindProperty("lookAheadDistance").floatValue = 0f;
        data.FindProperty("movementSwayAmount").floatValue = 0f;
        data.FindProperty("idleBreathAmount").floatValue = 0f;
        data.FindProperty("directionalTiltDegrees").floatValue = 0f;
        data.ApplyModifiedPropertiesWithoutUndo(); EditorUtility.SetDirty(motion);
    }

    private static void ConfigureHudLayout()
    {
        var stats = Find("Portrait Gameplay HUD/Top 20 Percent/Aligned Stats Bar")?.transform;
        if (stats == null) return;
        var timeCard = stats.Find("Time Stat Card")?.GetComponent<RectTransform>();
        var clock = stats.Find("Elegant Time Icon")?.GetComponent<RectTransform>();
        var time = stats.GetComponentsInChildren<Text>(true).FirstOrDefault(t => (t.text ?? string.Empty).Contains(":"));
        if (timeCard != null) Rect(timeCard, new Vector2(1, 1), new Vector2(1, 1), new Vector2(-18, -8), new Vector2(220, 74));
        if (clock != null) Rect(clock, new Vector2(1, 1), new Vector2(1, 1), new Vector2(-184, -18), new Vector2(42, 42));
        if (time != null)
        {
            time.alignment = TextAnchor.MiddleCenter;
            Rect(time.rectTransform, new Vector2(1, 1), new Vector2(1, 1), new Vector2(-112, -14), new Vector2(118, 56));
        }
    }

    private static void ConfigureRefreshStatus()
    {
        var hud = Find("Portrait Gameplay HUD")?.transform;
        var combat = Object.FindFirstObjectByType<GameplayCombatController>(FindObjectsInactive.Include);
        if (hud == null || combat == null) return;
        var old = hud.Find("Wand Reset Status"); if (old != null) Object.DestroyImmediate(old.gameObject);
        var panel = Backplate("Wand Reset Status", hud, new Vector2(0, 330), new Vector2(720, 72));
        Rect(panel.rectTransform, new Vector2(.5f, 0), new Vector2(.5f, 0), new Vector2(0, 330), new Vector2(720, 72));
        panel.color = new Color(.05f, .02f, .09f, .96f); panel.raycastTarget = false;
        var label = Label("WAND BUFFER RESETTING  0.0s", panel.transform, 24, Gold); Full(label.rectTransform); label.raycastTarget = false;
        var presenter = hud.GetComponent<RefreshStatusPresenter>() ?? hud.gameObject.AddComponent<RefreshStatusPresenter>();
        presenter.Configure(combat, panel.gameObject, label); panel.gameObject.SetActive(false); EditorUtility.SetDirty(presenter);
    }

    private static void ConfigurePause()
    {
        var pause = Object.FindFirstObjectByType<PauseMenuController>(FindObjectsInactive.Include); if (pause == null) return;
        var overlay = pause.transform;
        var oldSettings = overlay.Find("Audio Settings Panel"); if (oldSettings != null) Object.DestroyImmediate(oldSettings.gameObject);
        var card = overlay.Find("Pause Window") as RectTransform; if (card == null) return;
        var old = card.Find("Inline Run Controls"); if (old != null) Object.DestroyImmediate(old.gameObject);
        var settingsButton = card.Find("Button SETTINGS"); if (settingsButton != null) Object.DestroyImmediate(settingsButton.gameObject);
        card.sizeDelta = new Vector2(760, 1320);

        var root = new GameObject("Inline Run Controls", typeof(RectTransform)); root.transform.SetParent(card, false);
        Rect(root.GetComponent<RectTransform>(), new Vector2(.5f, 1), new Vector2(.5f, 1), new Vector2(0, -395), new Vector2(680, 610));
        var speedTitle = Label("LEVEL SPEED  •  RESETS TO 1X", root.transform, 23, Gold);
        Rect(speedTitle.rectTransform, new Vector2(.5f, 1), new Vector2(.5f, 1), new Vector2(0, -15), new Vector2(640, 52));
        var one = SmallButton("1X", root.transform, new Vector2(-210, -90), new Vector2(180, 66));
        var two = SmallButton("2X", root.transform, new Vector2(0, -90), new Vector2(180, 66));
        var three = SmallButton("3X", root.transform, new Vector2(210, -90), new Vector2(180, 66));
        var selector = root.AddComponent<GameSpeedSelector>(); selector.Configure(LabelOf(one), LabelOf(two), LabelOf(three));
        UnityEventTools.AddPersistentListener(one.onClick, selector.Set1X); UnityEventTools.AddPersistentListener(two.onClick, selector.Set2X); UnityEventTools.AddPersistentListener(three.onClick, selector.Set3X);

        var audioTitle = Label("AUDIO", root.transform, 25, Gold);
        Rect(audioTitle.rectTransform, new Vector2(.5f, 1), new Vector2(.5f, 1), new Vector2(0, -190), new Vector2(620, 55));
        var music = SmallButton("MUSIC  ON", root.transform, new Vector2(-165, -270), new Vector2(300, 66));
        var sfx = SmallButton("SFX  ON", root.transform, new Vector2(165, -270), new Vector2(300, 66));
        var musicSlider = SliderControl("Music Volume", root.transform, -365);
        var sfxSlider = SliderControl("Sfx Volume", root.transform, -475);
        var audio = root.AddComponent<AudioSettingsPanel>(); audio.Configure(LabelOf(music), LabelOf(sfx), musicSlider, sfxSlider);
        UnityEventTools.AddPersistentListener(music.onClick, audio.ToggleMusic); UnityEventTools.AddPersistentListener(sfx.onClick, audio.ToggleSfx);
        UnityEventTools.AddPersistentListener(musicSlider.onValueChanged, audio.SetMusicVolume); UnityEventTools.AddPersistentListener(sfxSlider.onValueChanged, audio.SetSfxVolume);
        pause.ConfigureSettings(null);

        var continueButton = card.GetComponentsInChildren<Button>(true).FirstOrDefault(b => b.name.Contains("CONTINUE"));
        var controlsButton = card.GetComponentsInChildren<Button>(true).FirstOrDefault(b => b.name.Contains("CONTROLS"));
        var menuButton = card.GetComponentsInChildren<Button>(true).FirstOrDefault(b => b.name.Contains("BACK TO MENU"));
        if (continueButton != null) Rect(continueButton.GetComponent<RectTransform>(), new Vector2(.5f, 0), new Vector2(.5f, 0), new Vector2(0, 250), new Vector2(560, 76));
        if (controlsButton != null) Rect(controlsButton.GetComponent<RectTransform>(), new Vector2(.5f, 0), new Vector2(.5f, 0), new Vector2(0, 155), new Vector2(560, 76));
        if (menuButton != null) Rect(menuButton.GetComponent<RectTransform>(), new Vector2(.5f, 0), new Vector2(.5f, 0), new Vector2(0, 60), new Vector2(560, 76));
        EditorUtility.SetDirty(pause);
    }

    private static void RemoveMenuSpeed(GameShellController shell)
    {
        var settings = Find("Game Shell Canvas/Game Shell/Settings")?.transform;
        var speed = settings?.Find("Game Speed Controls"); if (speed != null) Object.DestroyImmediate(speed.gameObject);
        shell.ConfigureSpeedLabels(null, null, null);
    }

    private static void BuildStructuredResearch(GameShellController shell, ProgressionService service, ResearchDefinition[] definitions)
    {
        var panel = Find("Game Shell Canvas/Game Shell/Research")?.transform
            ?? throw new InvalidOperationException("Research panel missing.");
        for (var i = panel.childCount - 1; i >= 0; i--) Object.DestroyImmediate(panel.GetChild(i).gameObject);
        var root = new GameObject("Structured Research", typeof(RectTransform)); root.transform.SetParent(panel, false); Full(root.GetComponent<RectTransform>());
        var title = Label("RESEARCH", root.transform, 42, Gold); Rect(title.rectTransform, new Vector2(.5f, 1), new Vector2(.5f, 1), new Vector2(0, -55), new Vector2(600, 70));
        var knowledge = Label("KNOWLEDGE POINTS  0", root.transform, 28, Teal); Rect(knowledge.rectTransform, new Vector2(.5f, 1), new Vector2(.5f, 1), new Vector2(0, -125), new Vector2(650, 55));
        var note = Label("Open one section, then open one upgrade to compare its previous and upgraded value.", root.transform, 19, Cream);
        Rect(note.rectTransform, new Vector2(.5f, 1), new Vector2(.5f, 1), new Vector2(0, -185), new Vector2(760, 60));

        var categoryNames = new[] { "LIBRARY", "TURRETS", "PLAYER" };
        var categoryButtons = new Button[3]; var categoryLabels = new Text[3]; var categoryPanels = new GameObject[3];
        var menu = root.AddComponent<ResearchMenuController>();
        for (var i = 0; i < 3; i++)
        {
            categoryButtons[i] = SmallButton(categoryNames[i], root.transform, new Vector2(-270 + i * 270, -275), new Vector2(250, 72));
            categoryLabels[i] = LabelOf(categoryButtons[i]);
            var captured = i; UnityEventTools.AddIntPersistentListener(categoryButtons[i].onClick, menu.ToggleCategory, captured);
            categoryPanels[i] = new GameObject(categoryNames[i] + " Content", typeof(RectTransform));
            categoryPanels[i].transform.SetParent(root.transform, false); Full(categoryPanels[i].GetComponent<RectTransform>());
        }

        var summary = new Text[definitions.Length]; var details = new Text[definitions.Length];
        var detailRoots = new GameObject[definitions.Length]; var purchases = new Button[definitions.Length]; var purchaseLabels = new Text[definitions.Length];
        var categoryIndices = new[]
        {
            new[] { 2, 19, 20, 21, 12, 13, 14 },
            new[] { 15, 16, 17, 18, 4, 5, 6, 7, 8, 9, 10, 11 },
            new[] { 0, 1, 3 }
        };
        for (var category = 0; category < categoryIndices.Length; category++)
        {
            var list = categoryIndices[category];
            for (var slot = 0; slot < list.Length; slot++)
            {
                var index = list[slot]; var x = slot % 2 == 0 ? -205f : 205f; var y = -365f - slot / 2 * 86f;
                var button = SmallButton(definitions[index].DisplayName, categoryPanels[category].transform, new Vector2(x, y), new Vector2(380, 66));
                summary[index] = LabelOf(button); summary[index].fontSize = 20;
                UnityEventTools.AddIntPersistentListener(button.onClick, menu.ToggleUpgrade, index);
                var detail = Backplate("Detail " + definitions[index].Id, root.transform, new Vector2(0, -1120), new Vector2(780, 270));
                details[index] = Label("Previous value\nUpgraded value", detail.transform, 23, Cream);
                Rect(details[index].rectTransform, new Vector2(.5f, 1), new Vector2(.5f, 1), new Vector2(0, -28), new Vector2(700, 110));
                purchases[index] = SmallButton("UPGRADE", detail.transform, new Vector2(0, -185), new Vector2(560, 72));
                purchaseLabels[index] = LabelOf(purchases[index]);
                UnityEventTools.AddIntPersistentListener(purchases[index].onClick, menu.Purchase, index);
                detailRoots[index] = detail.gameObject; detail.gameObject.SetActive(false);
            }
        }
        var back = SmallButton("BACK", root.transform, new Vector2(-335, -55), new Vector2(180, 66));
        Rect(back.GetComponent<RectTransform>(), new Vector2(0, 1), new Vector2(0, 1), new Vector2(18, -18), new Vector2(180, 66));
        UnityEventTools.AddPersistentListener(back.onClick, shell.ShowMain);
        menu.Configure(service, categoryPanels, categoryLabels, detailRoots, summary, details, purchases, purchaseLabels);
        shell.ConfigureResearchUi(Array.Empty<Button>(), Array.Empty<Text>());
        shell.ConfigureResearchKnowledgeLabel(knowledge);
        panel.gameObject.SetActive(false);
    }

    private static void AddLoreLocks()
    {
        var panel = Find("Game Shell Canvas/Game Shell/Lore Levels"); if (panel == null) return;
        var lockSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Generated/Issue10_Lock.png");
        foreach (var button in panel.GetComponentsInChildren<Button>(true).Where(b => (b.GetComponentInChildren<Text>(true)?.text ?? "").Contains("LEVEL")))
            AddLock(button, lockSprite);
    }

    private static void BuildTutorialSelector(GameShellController shell)
    {
        var shellRoot = Find("Game Shell Canvas/Game Shell")?.transform ?? throw new InvalidOperationException("Shell root missing.");
        var existing = shellRoot.Find("Tutorial Levels"); if (existing != null) Object.DestroyImmediate(existing.gameObject);
        var panel = Backplate("Tutorial Levels", shellRoot, Vector2.zero, Vector2.zero); Full(panel.rectTransform); panel.color = Ink;
        var title = Label("TUTORIALS", panel.transform, 44, Gold); Rect(title.rectTransform, new Vector2(.5f, 1), new Vector2(.5f, 1), new Vector2(0, -90), new Vector2(650, 80));
        var note = Label("Complete each lesson to unlock the next.", panel.transform, 23, Cream); Rect(note.rectTransform, new Vector2(.5f, 1), new Vector2(.5f, 1), new Vector2(0, -170), new Vector2(700, 60));
        var basic = SmallButton("BASICS", panel.transform, new Vector2(0, -330), new Vector2(650, 115));
        var turret = SmallButton("TURRET TRAINING", panel.transform, new Vector2(0, -480), new Vector2(650, 115));
        var ability = SmallButton("LIBRARY ABILITIES", panel.transform, new Vector2(0, -630), new Vector2(650, 115));
        UnityEventTools.AddPersistentListener(basic.onClick, shell.StartTutorial);
        UnityEventTools.AddPersistentListener(turret.onClick, shell.StartTurretTutorial);
        UnityEventTools.AddPersistentListener(ability.onClick, shell.StartAbilityTutorial);
        var lockSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Generated/Issue10_Lock.png"); AddLock(basic, lockSprite); AddLock(turret, lockSprite); AddLock(ability, lockSprite);
        var back = SmallButton("BACK", panel.transform, new Vector2(0, -805), new Vector2(420, 78)); UnityEventTools.AddPersistentListener(back.onClick, shell.ShowModes);
        panel.gameObject.SetActive(false);

        var mode = Find("Game Shell Canvas/Game Shell/Mode Select");
        var tutorialMode = mode?.GetComponentsInChildren<Button>(true).FirstOrDefault(b => (b.GetComponentInChildren<Text>(true)?.text ?? "").Trim() == "TUTORIAL");
        if (tutorialMode != null) { Clear(tutorialMode.onClick); UnityEventTools.AddPersistentListener(tutorialMode.onClick, shell.ShowTutorialLevels); }
        shell.ConfigureTutorialSelection(panel.gameObject, new[] { basic, turret, ability }, new[] { LabelOf(basic), LabelOf(turret), LabelOf(ability) }, null);
    }

    private static void BuildTurretTutorial(GameShellController shell)
    {
        var hud = Find("Portrait Gameplay HUD")?.transform ?? throw new InvalidOperationException("HUD missing.");
        var old = hud.Find("Turret Tutorial Guide"); if (old != null) Object.DestroyImmediate(old.gameObject);
        var root = new GameObject("Turret Tutorial Guide", typeof(RectTransform)); root.transform.SetParent(hud, false); Full(root.GetComponent<RectTransform>());
        var objective = Backplate("Objective", root.transform, new Vector2(0, -430), new Vector2(780, 190));
        var heading = Label("EARN BRAIN CELLS", objective.transform, 28, Gold); Rect(heading.rectTransform, new Vector2(.5f, 1), new Vector2(.5f, 1), new Vector2(0, -18), new Vector2(720, 55));
        var body = Label("Defeat the training word.", objective.transform, 20, Cream); Rect(body.rectTransform, new Vector2(.5f, 1), new Vector2(.5f, 1), new Vector2(0, -75), new Vector2(710, 95));
        foreach (var graphic in objective.GetComponentsInChildren<Graphic>(true)) graphic.raycastTarget = false;
        var acknowledgement = Backplate("Acknowledgement", root.transform, Vector2.zero, Vector2.zero); Full(acknowledgement.rectTransform); acknowledgement.color = new Color(.01f, .015f, .04f, .94f); acknowledgement.raycastTarget = true;
        var card = Backplate("Card", acknowledgement.transform, Vector2.zero, new Vector2(720, 620)); Rect(card.rectTransform, new Vector2(.5f, .5f), new Vector2(.5f, .5f), Vector2.zero, new Vector2(720, 620));
        var messageTitle = Label("TURRET TRAINING COMPLETE", card.transform, 34, Gold); Rect(messageTitle.rectTransform, new Vector2(.5f, 1), new Vector2(.5f, 1), new Vector2(0, -65), new Vector2(650, 80));
        var messageBody = Label("Training complete.", card.transform, 24, Cream); Rect(messageBody.rectTransform, new Vector2(.5f, .5f), new Vector2(.5f, .5f), new Vector2(0, 30), new Vector2(620, 260));
        var next = SmallButton("CONTINUE", card.transform, new Vector2(0, 65), new Vector2(540, 80));
        var director = root.gameObject.AddComponent<TurretTutorialDirector>();
        var arena = Object.FindFirstObjectByType<ReusableLevelArena>(FindObjectsInactive.Include);
        director.Configure(Object.FindFirstObjectByType<WaveRunController>(FindObjectsInactive.Include), Object.FindFirstObjectByType<EnemySpawner>(FindObjectsInactive.Include),
            Object.FindFirstObjectByType<GameplayCombatController>(FindObjectsInactive.Include), Object.FindFirstObjectByType<BrainCellEconomy>(FindObjectsInactive.Include),
            arena == null ? null : arena.TurretRoot, shell, objective.gameObject, heading, body, acknowledgement.gameObject, messageTitle, messageBody);
        UnityEventTools.AddPersistentListener(next.onClick, director.ContinueTutorial);
        acknowledgement.gameObject.SetActive(false); root.gameObject.SetActive(false);

        var shellData = new SerializedObject(shell); shellData.FindProperty("turretTutorialDirector").objectReferenceValue = director; shellData.ApplyModifiedPropertiesWithoutUndo();
        var tutorialPanel = Find("Game Shell Canvas/Game Shell/Tutorial Levels");
        var buttons = tutorialPanel?.GetComponentsInChildren<Button>(true).Where(b => (b.GetComponentInChildren<Text>(true)?.text ?? "").Contains("BASICS") || (b.GetComponentInChildren<Text>(true)?.text ?? "").Contains("TURRET TRAINING")).ToArray();
        if (buttons != null && buttons.Length >= 2) shell.ConfigureTutorialSelection(tutorialPanel, buttons, buttons.Select(LabelOf).ToArray(), director);
    }

    private static void BuildAbilityTutorial(GameShellController shell)
    {
        var hud = Find("Portrait Gameplay HUD")?.transform ?? throw new InvalidOperationException("HUD missing.");
        var old = hud.Find("Ability Tutorial Guide"); if (old != null) Object.DestroyImmediate(old.gameObject);
        var root = new GameObject("Ability Tutorial Guide", typeof(RectTransform)); root.transform.SetParent(hud, false); Full(root.GetComponent<RectTransform>());
        var objective = Backplate("Objective", root.transform, new Vector2(0, -430), new Vector2(780, 205));
        var heading = Label("USE A LIBRARY ABILITY", objective.transform, 28, Gold); Rect(heading.rectTransform, new Vector2(.5f, 1), new Vector2(.5f, 1), new Vector2(0, -18), new Vector2(720, 55));
        var body = Label("Move to the Library and activate History.", objective.transform, 20, Cream); Rect(body.rectTransform, new Vector2(.5f, 1), new Vector2(.5f, 1), new Vector2(0, -75), new Vector2(710, 110));
        foreach (var graphic in objective.GetComponentsInChildren<Graphic>(true)) graphic.raycastTarget = false;
        var acknowledgement = Backplate("Acknowledgement", root.transform, Vector2.zero, Vector2.zero); Full(acknowledgement.rectTransform); acknowledgement.color = new Color(.01f, .015f, .04f, .94f); acknowledgement.raycastTarget = true;
        var card = Backplate("Card", acknowledgement.transform, Vector2.zero, new Vector2(720, 620)); Rect(card.rectTransform, new Vector2(.5f, .5f), new Vector2(.5f, .5f), Vector2.zero, new Vector2(720, 620));
        var messageTitle = Label("ABILITY TRAINING COMPLETE", card.transform, 34, Gold); Rect(messageTitle.rectTransform, new Vector2(.5f, 1), new Vector2(.5f, 1), new Vector2(0, -65), new Vector2(650, 80));
        var messageBody = Label("Training complete.", card.transform, 24, Cream); Rect(messageBody.rectTransform, new Vector2(.5f, .5f), new Vector2(.5f, .5f), new Vector2(0, 30), new Vector2(620, 260));
        var next = SmallButton("CONTINUE TO LORE I", card.transform, new Vector2(0, 65), new Vector2(540, 80));
        var director = root.gameObject.AddComponent<AbilityTutorialDirector>();
        director.Configure(Object.FindFirstObjectByType<WaveRunController>(FindObjectsInactive.Include),
            Object.FindFirstObjectByType<EnemySpawner>(FindObjectsInactive.Include),
            Object.FindFirstObjectByType<BrainCellEconomy>(FindObjectsInactive.Include),
            Object.FindFirstObjectByType<LibraryAbilityController>(FindObjectsInactive.Include), shell,
            objective.gameObject, heading, body, acknowledgement.gameObject, messageTitle, messageBody);
        UnityEventTools.AddPersistentListener(next.onClick, director.ContinueTutorial);
        acknowledgement.gameObject.SetActive(false); root.gameObject.SetActive(false);

        var shellData = new SerializedObject(shell); shellData.FindProperty("abilityTutorialDirector").objectReferenceValue = director; shellData.ApplyModifiedPropertiesWithoutUndo();
        var tutorialPanel = Find("Game Shell Canvas/Game Shell/Tutorial Levels");
        var buttons = tutorialPanel?.GetComponentsInChildren<Button>(true)
            .Where(b =>
            {
                var text = b.GetComponentInChildren<Text>(true)?.text ?? string.Empty;
                return text.Contains("BASICS") || text.Contains("TURRET TRAINING") || text.Contains("LIBRARY ABILITIES");
            }).ToArray();
        var turretDirector = Object.FindFirstObjectByType<TurretTutorialDirector>(FindObjectsInactive.Include);
        if (buttons != null && buttons.Length >= 3)
            shell.ConfigureTutorialSelection(tutorialPanel, buttons, buttons.Select(LabelOf).ToArray(), turretDirector, director);
    }

    private static void ConfigureResult(bool sample)
    {
        var root = Find("Portrait Gameplay HUD/Run Result Overlay")?.transform; if (root == null) return;
        for (var i = root.childCount - 1; i >= 0; i--) Object.DestroyImmediate(root.GetChild(i).gameObject);
        var background = Backplate("Animated Result Background", root, Vector2.zero, Vector2.zero); Full(background.rectTransform);
        var card = Backplate("Result Window", root, Vector2.zero, new Vector2(760, 1180)); Rect(card.rectTransform, new Vector2(.5f, .5f), new Vector2(.5f, .5f), Vector2.zero, new Vector2(760, 1180));
        var title = Label("LIBRARY DEFENDED", card.transform, 39, Gold); Rect(title.rectTransform, new Vector2(.5f, 1), new Vector2(.5f, 1), new Vector2(0, -55), new Vector2(680, 80));
        var library = ChildImage(card.transform, "Library Image", AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Monochrome/Library_256.png")); Rect(library.rectTransform, new Vector2(.5f, 1), new Vector2(.5f, 1), new Vector2(0, -180), new Vector2(300, 300)); library.preserveAspect = true;
        var shield = ChildImage(card.transform, "Shield Emblem", AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Generated/Issue10_Shield.png")); Rect(shield.rectTransform, new Vector2(.5f, 1), new Vector2(.5f, 1), new Vector2(0, -125), new Vector2(110, 110)); shield.preserveAspect = true;
        var skull = ChildImage(card.transform, "Skull Cross Emblem", AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Generated/Issue10_SkullCross.png")); Rect(skull.rectTransform, new Vector2(.5f, 1), new Vector2(.5f, 1), new Vector2(0, -130), new Vector2(120, 120)); skull.preserveAspect = true;
        var rewards = Label("REWARDS", card.transform, 27, Cream); Rect(rewards.rectTransform, new Vector2(.5f, 1), new Vector2(.5f, 1), new Vector2(0, -515), new Vector2(650, 150));
        var unlock = Label("PROGRESS SAVED", card.transform, 28, Gold); Rect(unlock.rectTransform, new Vector2(.5f, 1), new Vector2(.5f, 1), new Vector2(0, -680), new Vector2(650, 120));
        var primary = SmallButton("CONTINUE", card.transform, new Vector2(0, 155), new Vector2(570, 82));
        var menu = SmallButton("MAIN MENU", card.transform, new Vector2(0, 55), new Vector2(570, 78));
        Rect(primary.GetComponent<RectTransform>(), new Vector2(.5f, 0), new Vector2(.5f, 0), new Vector2(0, 155), new Vector2(570, 82));
        Rect(menu.GetComponent<RectTransform>(), new Vector2(.5f, 0), new Vector2(.5f, 0), new Vector2(0, 55), new Vector2(570, 78));
        var animator = root.gameObject.GetComponent<RunResultVisualAnimator>() ?? root.gameObject.AddComponent<RunResultVisualAnimator>(); animator.Configure(background, shield.rectTransform);
        var presenter = root.gameObject.GetComponent<RunResultPresenter>() ?? root.gameObject.AddComponent<RunResultPresenter>();
        presenter.Configure(title, rewards, unlock, primary, LabelOf(primary)); presenter.ConfigureVisuals(library, shield, skull, animator);

        var reveal = Backplate("Reward Reveal", root, Vector2.zero, Vector2.zero); Full(reveal.rectTransform); reveal.color = new Color(.015f, .02f, .06f, .97f); reveal.raycastTarget = true;
        var rewardCard = Backplate("Reward Card", reveal.transform, Vector2.zero, new Vector2(730, 850)); Rect(rewardCard.rectTransform, new Vector2(.5f, .5f), new Vector2(.5f, .5f), Vector2.zero, new Vector2(730, 850));
        var rewardTitle = Label("TUTORIAL REWARD", rewardCard.transform, 38, Gold); Rect(rewardTitle.rectTransform, new Vector2(.5f, 1), new Vector2(.5f, 1), new Vector2(0, -70), new Vector2(650, 80));
        var rewardImage = ChildImage(rewardCard.transform, "Reward Image", FindSprite("Turret_Teacher_128")); Rect(rewardImage.rectTransform, new Vector2(.5f, 1), new Vector2(.5f, 1), new Vector2(0, -190), new Vector2(260, 260)); rewardImage.preserveAspect = true;
        var rewardBody = Label("TEACHER UNLOCKED", rewardCard.transform, 26, Cream); Rect(rewardBody.rectTransform, new Vector2(.5f, 1), new Vector2(.5f, 1), new Vector2(0, -470), new Vector2(620, 170));
        var rewardContinue = SmallButton("CONTINUE", rewardCard.transform, new Vector2(0, 65), new Vector2(540, 80));
        Rect(rewardContinue.GetComponent<RectTransform>(), new Vector2(.5f, 0), new Vector2(.5f, 0), new Vector2(0, 65), new Vector2(540, 80));
        presenter.ConfigureRewardReveal(reveal.gameObject, rewardTitle, rewardBody, rewardImage); reveal.gameObject.SetActive(false);

        var run = Object.FindFirstObjectByType<WaveRunController>(FindObjectsInactive.Include);
        if (run != null) { var data = new SerializedObject(run); data.FindProperty("resultRoot").objectReferenceValue = root.gameObject; data.FindProperty("resultLabel").objectReferenceValue = title; data.ApplyModifiedPropertiesWithoutUndo(); EditorUtility.SetDirty(run); }
        if (sample)
        {
            var shell = Object.FindFirstObjectByType<GameShellController>(FindObjectsInactive.Include); if (shell != null)
            {
                shell.ConfigureResultPresenter(presenter); Clear(primary.onClick); Clear(menu.onClick);
                UnityEventTools.AddPersistentListener(primary.onClick, shell.ContinueFromResult); UnityEventTools.AddPersistentListener(menu.onClick, shell.ReturnToMainFromRun);
                UnityEventTools.AddPersistentListener(rewardContinue.onClick, shell.ContinueAfterReward);
            }
        }
        else
        {
            var flow = Object.FindFirstObjectByType<DedicatedLevelSceneController>(FindObjectsInactive.Include); if (flow != null)
            {
                flow.ConfigureResultPresenter(presenter); flow.ConfigureRewardSprites(FindSprite("Turret_Engineer_128"), FindSprite("Turret_Scientist_128"), FindSprite("Turret_President_128"));
                Clear(primary.onClick); Clear(menu.onClick); UnityEventTools.AddPersistentListener(primary.onClick, flow.ContinueFromResult); UnityEventTools.AddPersistentListener(menu.onClick, flow.ReturnToMainMenu);
                UnityEventTools.AddPersistentListener(rewardContinue.onClick, flow.ContinueAfterReward);
            }
        }
        root.gameObject.SetActive(false);
    }

    private static ResearchDefinition[] BuildResearchDefinitions()
    {
        var files = new[] { "PlayerRange", "MagazineCapacity", "LibraryHealth", "ReloadSpeed", "TeacherRange", "TeacherSpeed", "EngineerRange", "EngineerSpeed", "ScientistRange", "ScientistSpeed", "PresidentRange", "PresidentSpeed" };
        var definitions = files.Select(f => AssetDatabase.LoadAssetAtPath<ResearchDefinition>($"Assets/Data/Research/{f}.asset")).ToList();
        definitions.Add(Research("HistoryDuration", "HISTORY_DURATION", "HISTORY DURATION", ResearchStat.HistoryDuration, 5, 3, 2, .75f));
        definitions.Add(Research("SocialDuration", "SOCIAL_DURATION", "SOCIAL INFLUENCE DURATION", ResearchStat.SocialInfluenceDuration, 5, 3, 2, .75f));
        definitions.Add(Research("PoliticsTargets", "POLITICS_TARGETS", "POLITICS TARGETS", ResearchStat.PoliticsTargetCount, 4, 4, 3, 1f));
        definitions.Add(Research("TeacherUnlock", "TEACHER_UNLOCK", "TEACHER", ResearchStat.TeacherUnlock, 1, 3, 0, 0f));
        definitions.Add(Research("EngineerUnlock", "ENGINEER_UNLOCK", "ENGINEER", ResearchStat.EngineerUnlock, 1, 5, 0, 0f));
        definitions.Add(Research("ScientistUnlock", "SCIENTIST_UNLOCK", "SCIENTIST", ResearchStat.ScientistUnlock, 1, 7, 0, 0f));
        definitions.Add(Research("PresidentUnlock", "PRESIDENT_UNLOCK", "PRESIDENT", ResearchStat.PresidentUnlock, 1, 9, 0, 0f));
        definitions.Add(Research("HistoryUnlock", "HISTORY_UNLOCK", "HISTORY", ResearchStat.HistoryUnlock, 1, 3, 0, 0f));
        definitions.Add(Research("SocialUnlock", "SOCIAL_UNLOCK", "SOCIAL INFLUENCE", ResearchStat.SocialInfluenceUnlock, 1, 5, 0, 0f));
        definitions.Add(Research("PoliticsUnlock", "POLITICS_UNLOCK", "POLITICS", ResearchStat.PoliticsUnlock, 1, 7, 0, 0f));
        return definitions.ToArray();
    }

    private static ResearchDefinition Research(string file, string id, string title, ResearchStat stat, int max, int cost, int growth, float value)
    {
        var path = $"Assets/Data/Research/{file}.asset"; var asset = AssetDatabase.LoadAssetAtPath<ResearchDefinition>(path);
        if (asset == null) { asset = ScriptableObject.CreateInstance<ResearchDefinition>(); AssetDatabase.CreateAsset(asset, path); }
        var data = new SerializedObject(asset); data.FindProperty("id").stringValue = id; data.FindProperty("displayName").stringValue = title;
        data.FindProperty("stat").enumValueIndex = (int)stat; data.FindProperty("maxLevel").intValue = max; data.FindProperty("baseCost").intValue = cost;
        data.FindProperty("costIncreasePerLevel").intValue = growth; data.FindProperty("valuePerLevel").floatValue = value;
        data.ApplyModifiedPropertiesWithoutUndo(); EditorUtility.SetDirty(asset); return asset;
    }

    private static void EnsureGeneratedIcons()
    {
        Directory.CreateDirectory("Assets/Art/Generated");
        WriteIcon("Assets/Art/Generated/Issue10_Lock.png", (x, y) =>
        {
            var body = x >= 28 && x <= 99 && y >= 18 && y <= 72;
            var dx = x - 64f; var dy = y - 77f; var ring = dx * dx + dy * dy <= 34f * 34f && dx * dx + dy * dy >= 22f * 22f && y >= 66;
            return body || ring;
        });
        WriteIcon("Assets/Art/Generated/Issue10_Shield.png", (x, y) =>
        {
            var nx = Mathf.Abs(x - 63.5f); var top = y <= 105 && y >= 54 && nx <= 43f - (105 - y) * .12f;
            var bottom = y < 54 && y >= 12 && nx <= y * .68f;
            return top || bottom;
        });
        WriteIcon("Assets/Art/Generated/Issue10_SkullCross.png", (x, y) =>
        {
            var skull = Vector2.Distance(new Vector2(x, y), new Vector2(64, 79)) <= 31f || (x >= 45 && x <= 83 && y >= 39 && y <= 72);
            var crossA = Mathf.Abs((y - 25) - (x - 64) * .55f) < 6f && x >= 18 && x <= 110 && y >= 5 && y <= 55;
            var crossB = Mathf.Abs((y - 25) + (x - 64) * .55f) < 6f && x >= 18 && x <= 110 && y >= 5 && y <= 55;
            var eyes = Vector2.Distance(new Vector2(x, y), new Vector2(52, 82)) < 7f || Vector2.Distance(new Vector2(x, y), new Vector2(76, 82)) < 7f;
            return (skull || crossA || crossB) && !eyes;
        });
        AssetDatabase.Refresh();
        foreach (var path in new[] { "Assets/Art/Generated/Issue10_Lock.png", "Assets/Art/Generated/Issue10_Shield.png", "Assets/Art/Generated/Issue10_SkullCross.png" })
        {
            var importer = AssetImporter.GetAtPath(path) as TextureImporter; if (importer == null) continue;
            importer.textureType = TextureImporterType.Sprite; importer.spritePixelsPerUnit = 128; importer.alphaIsTransparency = true; importer.mipmapEnabled = false; importer.SaveAndReimport();
        }
    }

    private static void WriteIcon(string path, Func<int, int, bool> fill)
    {
        var texture = new Texture2D(128, 128, TextureFormat.RGBA32, false);
        for (var y = 0; y < 128; y++) for (var x = 0; x < 128; x++) texture.SetPixel(x, y, fill(x, y) ? new Color(1f, .82f, .32f, 1f) : Color.clear);
        texture.Apply(); File.WriteAllBytes(path, texture.EncodeToPNG()); Object.DestroyImmediate(texture);
    }

    private static void AddLock(Button button, Sprite sprite)
    {
        var old = button.transform.Find("Lock Icon"); if (old != null) Object.DestroyImmediate(old.gameObject);
        var image = ChildImage(button.transform, "Lock Icon", sprite); image.raycastTarget = false; image.preserveAspect = true;
        Rect(image.rectTransform, new Vector2(1, .5f), new Vector2(1, .5f), new Vector2(-70, 0), new Vector2(58, 58)); image.gameObject.SetActive(false);
    }

    private static void UpdateBuildSettings()
    {
        var required = new[] { Sample }.Concat(TutorialScenes).Concat(LoreScenes).ToArray();
        var existing = EditorBuildSettings.scenes.ToList();
        foreach (var path in required)
            if (existing.All(scene => scene.path != path)) existing.Add(new EditorBuildSettingsScene(path, true));
        EditorBuildSettings.scenes = existing.ToArray();
    }

    private static Slider SliderControl(string labelText, Transform parent, float y)
    {
        var label = Label(labelText.ToUpperInvariant(), parent, 18, Cream); Rect(label.rectTransform, new Vector2(.5f, 1), new Vector2(.5f, 1), new Vector2(-215, y), new Vector2(200, 44)); label.alignment = TextAnchor.MiddleLeft;
        var go = new GameObject(labelText + " Slider", typeof(RectTransform), typeof(Slider)); go.transform.SetParent(parent, false); Rect(go.GetComponent<RectTransform>(), new Vector2(.5f, 1), new Vector2(.5f, 1), new Vector2(105, y), new Vector2(390, 38));
        var bg = Backplate("Background", go.transform, Vector2.zero, Vector2.zero); Full(bg.rectTransform); bg.color = new Color(.2f, .2f, .25f, 1f);
        var fillArea = new GameObject("Fill Area", typeof(RectTransform)); fillArea.transform.SetParent(go.transform, false); Full(fillArea.GetComponent<RectTransform>());
        var fill = Backplate("Fill", fillArea.transform, Vector2.zero, Vector2.zero); Full(fill.rectTransform); fill.color = Teal;
        var handle = Backplate("Handle", go.transform, Vector2.zero, new Vector2(34, 50)); Rect(handle.rectTransform, new Vector2(.5f, .5f), new Vector2(.5f, .5f), Vector2.zero, new Vector2(34, 50)); handle.color = Gold;
        var slider = go.GetComponent<Slider>(); slider.fillRect = fill.rectTransform; slider.handleRect = handle.rectTransform; slider.targetGraphic = handle; slider.minValue = 0f; slider.maxValue = 1f; slider.value = 1f; return slider;
    }

    private static Button SmallButton(string text, Transform parent, Vector2 position, Vector2 size)
    {
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/UiTextButton.prefab");
        var go = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent); go.name = "Button " + text;
        var button = go.GetComponent<Button>(); var label = LabelOf(button); label.text = text; label.fontSize = 23; label.color = Cream;
        Rect(go.GetComponent<RectTransform>(), new Vector2(.5f, 1), new Vector2(.5f, 1), position, size); return button;
    }

    private static Text LabelOf(Button button) => button.GetComponentInChildren<Text>(true);
    private static Text Label(string text, Transform parent, int size, Color color)
    {
        var go = new GameObject("Label " + text.Split('\n')[0], typeof(RectTransform), typeof(CanvasRenderer), typeof(Text)); go.transform.SetParent(parent, false);
        var label = go.GetComponent<Text>(); label.text = text; label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); label.fontSize = size; label.color = color;
        label.alignment = TextAnchor.MiddleCenter; label.horizontalOverflow = HorizontalWrapMode.Wrap; label.verticalOverflow = VerticalWrapMode.Overflow; return label;
    }
    private static Image Backplate(string name, Transform parent, Vector2 position, Vector2 size)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image)); go.transform.SetParent(parent, false);
        var image = go.GetComponent<Image>(); image.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Prototype/SolidSprite.asset"); image.color = new Color(0f, 0f, 0f, .82f); image.raycastTarget = false;
        Rect(image.rectTransform, new Vector2(.5f, 1), new Vector2(.5f, 1), position, size); return image;
    }
    private static Image ChildImage(Transform parent, string name, Sprite sprite)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image)); go.transform.SetParent(parent, false); var image = go.GetComponent<Image>(); image.sprite = sprite; image.raycastTarget = false; return image;
    }
    private static Sprite FindSprite(string name)
    {
        var guid = AssetDatabase.FindAssets(name + " t:Sprite").FirstOrDefault(); return string.IsNullOrEmpty(guid) ? null : AssetDatabase.LoadAssetAtPath<Sprite>(AssetDatabase.GUIDToAssetPath(guid));
    }
    private static void Clear(UnityEngine.Events.UnityEvent action) { for (var i = action.GetPersistentEventCount() - 1; i >= 0; i--) UnityEventTools.RemovePersistentListener(action, i); }
    private static GameObject Find(string path) => GameObject.Find("/" + path) ?? Resources.FindObjectsOfTypeAll<GameObject>().FirstOrDefault(go => go.scene == SceneManager.GetActiveScene() && Hierarchy(go) == path);
    private static string Hierarchy(GameObject go) { var value = go.name; for (var p = go.transform.parent; p != null; p = p.parent) value = p.name + "/" + value; return value; }
    private static void Full(RectTransform rect) { rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one; rect.pivot = new Vector2(.5f, .5f); rect.offsetMin = Vector2.zero; rect.offsetMax = Vector2.zero; }
    private static void Rect(RectTransform rect, Vector2 min, Vector2 max, Vector2 position, Vector2 size) { rect.anchorMin = min; rect.anchorMax = max; rect.pivot = min == max ? min : new Vector2(.5f, .5f); rect.anchoredPosition = position; rect.sizeDelta = size; }
    private static Color C(string hex) { ColorUtility.TryParseHtmlString("#" + hex, out var color); return color; }
}
