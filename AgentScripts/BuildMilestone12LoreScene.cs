using System;
using System.Collections.Generic;
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

public static class BuildMilestone12LoreScene
{
    private const string LoreScene = "Assets/Scenes/LoreOneLevelOne.unity";
    private const string MenuScene = "Assets/Scenes/SampleScene.unity";
    private const string ArenaPrefab = "Assets/Prefabs/ReusableLevelArena.prefab";
    private const string TileFolder = "Assets/Tiles/Monochrome";

    public static string Build()
    {
        if (EditorApplication.isPlaying) throw new InvalidOperationException("Exit Play Mode before authoring Lore I Level 1.");
        var scene = EditorSceneManager.OpenScene(LoreScene, OpenSceneMode.Single);
        var root = Object.FindFirstObjectByType<GameplaySceneCoordinator>(FindObjectsInactive.Include)?.transform.parent
            ?? throw new InvalidOperationException("Gameplay root missing in Lore scene.");

        RemoveLegacyArena(root);
        var arenaPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(ArenaPrefab)
            ?? throw new InvalidOperationException("ReusableLevelArena prefab missing.");
        var arenaObject = (GameObject)PrefabUtility.InstantiatePrefab(arenaPrefab, root);
        arenaObject.name = "Lore I Authored Arena";
        var arena = arenaObject.GetComponent<ReusableLevelArena>();
        PaintLoreArena(arena);
        var path = ConfigureLorePath(root, arena);
        RewireRuntime(root, arena, path);
        BuildDedicatedFlow(root);

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();
        EditorSceneManager.OpenScene(MenuScene, OpenSceneMode.Single);
        return "Built Lore I Level 1: unique prefab-backed arena, ten-point route, dedicated scene flow, progression completion, and pause/result return to Main Menu.";
    }

    public static string Audit()
    {
        var scene = EditorSceneManager.OpenScene(LoreScene, OpenSceneMode.Single);
        var arena = Object.FindFirstObjectByType<ReusableLevelArena>(FindObjectsInactive.Include)
            ?? throw new InvalidOperationException("Lore arena component missing.");
        if (PrefabUtility.GetCorrespondingObjectFromSource(arena.gameObject) == null)
            throw new InvalidOperationException("Lore arena is not a connected prefab instance.");
        if (CountTiles(arena.Ground) < 200) throw new InvalidOperationException("Lore ground is not painted.");
        if (CountTiles(arena.EnemyPath) < 25) throw new InvalidOperationException("Lore path is incomplete.");
        if (CountTiles(arena.BlockedTerrain) < 10) throw new InvalidOperationException("Lore terrain features are incomplete.");
        var path = Object.FindFirstObjectByType<WaypointPath>(FindObjectsInactive.Include)
            ?? throw new InvalidOperationException("Waypoint path missing.");
        if (path.WaypointCount != 10) throw new InvalidOperationException($"Expected 10 Lore waypoints, found {path.WaypointCount}.");
        var flow = Object.FindFirstObjectByType<DedicatedLevelSceneController>(FindObjectsInactive.Include)
            ?? throw new InvalidOperationException("Dedicated level flow missing.");
        if (flow.Level == null || flow.Level.DisplayName != "LORE I - LEVEL 1") throw new InvalidOperationException("Lore level data is not assigned.");
        var shell = Object.FindFirstObjectByType<GameShellController>(FindObjectsInactive.Include);
        if (shell == null || shell.enabled) throw new InvalidOperationException("Menu shell must remain authored but disabled in the dedicated scene.");
        var pause = Object.FindFirstObjectByType<PauseMenuController>(FindObjectsInactive.Include);
        var pauseData = new SerializedObject(pause);
        if (pauseData.FindProperty("dedicatedLevel").objectReferenceValue != flow) throw new InvalidOperationException("Pause return is not wired to dedicated flow.");
        var result = GameObject.Find("/Portrait Gameplay HUD")?.transform.Find("Run Result Overlay");
        if (result == null || result.Find("Button MAIN MENU") == null) throw new InvalidOperationException("Result Main Menu action missing.");
        if (!EditorBuildSettings.scenes.Any(s => s.enabled && s.path == LoreScene)) throw new InvalidOperationException("Lore scene missing from Build Settings.");
        EditorSceneManager.OpenScene(MenuScene, OpenSceneMode.Single);
        return "Audit passed: connected reusable arena, unique painted maps, 10-point Lore route, assigned level, progression flow, and both return-to-menu paths.";
    }

    private static void RemoveLegacyArena(Transform root)
    {
        foreach (var name in new[] { "Authored Tilemaps", "Placed Turrets", "Lore I Authored Arena" })
        {
            var child = root.Find(name);
            if (child != null) Object.DestroyImmediate(child.gameObject);
        }
    }

    private static void PaintLoreArena(ReusableLevelArena arena)
    {
        var ground = Enumerable.Range(0,4).Select(i=>Tile($"Ground_{i}")).ToArray();
        var paths = new Dictionary<string,Tile>
        {
            ["H"]=Tile("Path_H"), ["V"]=Tile("Path_V"), ["LT"]=Tile("Path_LT"),
            ["TR"]=Tile("Path_TR"), ["LB"]=Tile("Path_LB"), ["BR"]=Tile("Path_BR")
        };
        var features = new Dictionary<string,Tile>
        {
            ["Tree"]=Tile("Obstacle_Tree"), ["Boulder"]=Tile("Obstacle_Boulder"),
            ["Water0"]=Tile("Obstacle_Water_0"), ["Water1"]=Tile("Obstacle_Water_1"),
            ["Water2"]=Tile("Obstacle_Water_2"), ["Water3"]=Tile("Obstacle_Water_3"),
            ["Mountain0"]=Tile("Obstacle_Mountain_0"), ["Mountain1"]=Tile("Obstacle_Mountain_1"),
            ["Mountain2"]=Tile("Obstacle_Mountain_2"), ["Mountain3"]=Tile("Obstacle_Mountain_3"),
            ["OuterRock"]=Tile("Outer_Rock"), ["Cloud"]=Tile("Outer_Cloud")
        };
        arena.Ground.ClearAllTiles(); arena.EnemyPath.ClearAllTiles(); arena.BlockedTerrain.ClearAllTiles();
        arena.OutsideTerrain.ClearAllTiles(); arena.CloudCover.ClearAllTiles();
        for(var x=-11;x<=10;x++)for(var y=-12;y<=11;y++)
        {
            var inside=x>=-7&&x<=6&&y>=-8&&y<=7;
            if(!inside)arena.OutsideTerrain.SetTile(new Vector3Int(x,y,0),features["OuterRock"]);
            if(!inside&&(x is -8 or 7||y is -9 or 8)&&Math.Abs(x*2-y)%4!=0)
                arena.CloudCover.SetTile(new Vector3Int(x,y,0),features["Cloud"]);
        }
        for(var x=-7;x<=6;x++)for(var y=-8;y<=7;y++)
            arena.Ground.SetTile(new Vector3Int(x,y,0),ground[Math.Abs(x*5+y*3+12)%ground.Length]);

        var route=Route();var cells=Expand(route);
        foreach(var cell in cells)arena.EnemyPath.SetTile(cell,PathTile(cell,cells,paths));
        var blocked=new Dictionary<Vector3Int,string>
        {
            [new(-5,-4,0)]="Mountain0", [new(-4,-4,0)]="Mountain1", [new(-5,-3,0)]="Mountain2", [new(-4,-3,0)]="Mountain3",
            [new(4,-6,0)]="Water0", [new(5,-6,0)]="Water1", [new(4,-5,0)]="Water2", [new(5,-5,0)]="Water3",
            [new(3,4,0)]="Tree", [new(4,4,0)]="Tree", [new(-1,-6,0)]="Tree", [new(1,-1,0)]="Tree",
            [new(-5,3,0)]="Boulder", [new(4,-1,0)]="Boulder", [new(0,4,0)]="Boulder"
        };
        foreach(var pair in blocked)if(!cells.Contains(pair.Key))arena.BlockedTerrain.SetTile(pair.Key,features[pair.Value]);
    }

    private static WaypointPath ConfigureLorePath(Transform root, ReusableLevelArena arena)
    {
        var path=root.Find("Enemy Path")?.GetComponent<WaypointPath>()
            ?? throw new InvalidOperationException("Enemy Path missing.");
        foreach(Transform child in path.transform.Cast<Transform>().ToArray())Object.DestroyImmediate(child.gameObject);
        foreach(Transform child in arena.WaypointRoot.Cast<Transform>().ToArray())Object.DestroyImmediate(child.gameObject);
        var route=Route();var points=new Transform[route.Length];
        for(var i=0;i<route.Length;i++)
        {
            var point=new GameObject($"Lore Waypoint {i+1:00}").transform;
            point.SetParent(arena.WaypointRoot,false);point.position=arena.EnemyPath.GetCellCenterWorld(route[i]);points[i]=point;
        }
        path.Configure(points,null);
        var line=path.GetComponent<LineRenderer>();if(line!=null)line.enabled=false;
        return path;
    }

    private static void RewireRuntime(Transform root, ReusableLevelArena arena, WaypointPath path)
    {
        var player=root.Find("Player")?.GetComponent<PlayerMover>() ?? throw new InvalidOperationException("Player missing.");
        player.transform.position=new Vector3(-.5f,-6.5f,0);player.ConfigureBlockedTerrain(arena.BlockedTerrain);
        var context=Object.FindFirstObjectByType<TileContextActionPanel>(FindObjectsInactive.Include);
        var contextData=new SerializedObject(context);
        contextData.FindProperty("groundTilemap").objectReferenceValue=arena.Ground;
        contextData.FindProperty("pathTilemap").objectReferenceValue=arena.EnemyPath;
        contextData.FindProperty("blockedTilemap").objectReferenceValue=arena.BlockedTerrain;
        contextData.FindProperty("turretRoot").objectReferenceValue=arena.TurretRoot;
        contextData.ApplyModifiedPropertiesWithoutUndo();EditorUtility.SetDirty(context);
        var run=Object.FindFirstObjectByType<WaveRunController>(FindObjectsInactive.Include);
        var runData=new SerializedObject(run);runData.FindProperty("turretRoot").objectReferenceValue=arena.TurretRoot;
        runData.FindProperty("playerStartPosition").vector3Value=player.transform.position;runData.ApplyModifiedPropertiesWithoutUndo();EditorUtility.SetDirty(run);
        var spawner=Object.FindFirstObjectByType<EnemySpawner>(FindObjectsInactive.Include);
        var spawnData=new SerializedObject(spawner);spawnData.FindProperty("path").objectReferenceValue=path;spawnData.ApplyModifiedPropertiesWithoutUndo();EditorUtility.SetDirty(spawner);
    }

    private static void BuildDedicatedFlow(Transform root)
    {
        var systems=root.Find("Persistent Progression Systems") ?? throw new InvalidOperationException("Progression systems missing.");
        var run=Object.FindFirstObjectByType<WaveRunController>(FindObjectsInactive.Include);
        var progression=systems.GetComponent<ProgressionService>();var upgrades=systems.GetComponent<PermanentUpgradeApplier>();
        var level=AssetDatabase.LoadAssetAtPath<LevelDefinition>("Assets/Data/Levels/LoreOneLevelOne.asset");
        var flow=systems.GetComponent<DedicatedLevelSceneController>() ?? systems.gameObject.AddComponent<DedicatedLevelSceneController>();
        flow.Configure(run,progression,upgrades,level,"SampleScene");
        var shell=systems.GetComponent<GameShellController>();if(shell!=null)shell.enabled=false;
        var shellCanvas=GameObject.Find("/Game Shell Canvas");if(shellCanvas!=null)shellCanvas.SetActive(false);
        var pause=Object.FindFirstObjectByType<PauseMenuController>(FindObjectsInactive.Include);pause.ConfigureDedicatedLevel(flow);

        var result=GameObject.Find("/Portrait Gameplay HUD")?.transform.Find("Run Result Overlay")
            ?? throw new InvalidOperationException("Run result overlay missing.");
        var previous=result.Find("Button MAIN MENU");if(previous!=null)Object.DestroyImmediate(previous.gameObject);
        var restart=result.GetComponentsInChildren<Button>(true).FirstOrDefault(b=>b.name.Contains("RESTART"));
        if(restart!=null){var rr=restart.GetComponent<RectTransform>();rr.anchoredPosition=new Vector2(0,-20);}
        var prefab=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/UiTextButton.prefab");
        var menu=(GameObject)PrefabUtility.InstantiatePrefab(prefab,result);menu.name="Button MAIN MENU";
        var label=menu.GetComponentInChildren<Text>(true);label.text="MAIN MENU";label.fontSize=25;
        var rect=menu.GetComponent<RectTransform>();rect.anchorMin=rect.anchorMax=rect.pivot=new Vector2(.5f,.5f);rect.anchoredPosition=new Vector2(0,-118);rect.sizeDelta=new Vector2(520,76);
        UnityEventTools.AddPersistentListener(menu.GetComponent<Button>().onClick,flow.ReturnToMainMenu);
    }

    private static Vector3Int[] Route()=>new[]
    {
        new Vector3Int(-10,5,0),new Vector3Int(-6,5,0),new Vector3Int(-6,2,0),
        new Vector3Int(-2,2,0),new Vector3Int(-2,-3,0),new Vector3Int(3,-3,0),
        new Vector3Int(3,2,0),new Vector3Int(5,2,0),new Vector3Int(5,6,0),new Vector3Int(1,6,0)
    };

    private static HashSet<Vector3Int> Expand(Vector3Int[] route)
    {
        var cells=new HashSet<Vector3Int>();
        for(var i=0;i<route.Length-1;i++)
        {
            var current=route[i];var end=route[i+1];var step=new Vector3Int(Math.Sign(end.x-current.x),Math.Sign(end.y-current.y),0);
            if(step.x!=0&&step.y!=0)throw new InvalidOperationException("Lore route contains a diagonal segment.");
            cells.Add(current);while(current!=end){current+=step;cells.Add(current);}
        }
        return cells;
    }

    private static Tile PathTile(Vector3Int cell,HashSet<Vector3Int> cells,Dictionary<string,Tile> tiles)
    {
        var l=cells.Contains(cell+Vector3Int.left);var r=cells.Contains(cell+Vector3Int.right);
        var t=cells.Contains(cell+Vector3Int.up);var b=cells.Contains(cell+Vector3Int.down);
        if(l&&t)return tiles["LT"];if(t&&r)return tiles["TR"];if(l&&b)return tiles["LB"];if(b&&r)return tiles["BR"];
        return (t||b)&&!(l||r)?tiles["V"]:tiles["H"];
    }

    private static Tile Tile(string name)=>AssetDatabase.LoadAssetAtPath<Tile>($"{TileFolder}/{name}.asset")
        ?? throw new InvalidOperationException($"Missing tile {name}.");

    private static int CountTiles(Tilemap map)
    {
        var count=0;
        foreach(var position in map.cellBounds.allPositionsWithin)if(map.HasTile(position))count++;
        return count;
    }
}
