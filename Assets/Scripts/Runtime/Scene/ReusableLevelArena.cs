using UnityEngine;
using UnityEngine.Tilemaps;

namespace KeySlaught.SceneGameplay
{
    public sealed class ReusableLevelArena : MonoBehaviour
    {
        [SerializeField] private Tilemap ground;
        [SerializeField] private Tilemap enemyPath;
        [SerializeField] private Tilemap blockedTerrain;
        [SerializeField] private Tilemap outsideTerrain;
        [SerializeField] private Tilemap cloudCover;
        [SerializeField] private Transform waypointRoot;
        [SerializeField] private Transform turretRoot;

        public Tilemap Ground => ground;
        public Tilemap EnemyPath => enemyPath;
        public Tilemap BlockedTerrain => blockedTerrain;
        public Tilemap OutsideTerrain => outsideTerrain;
        public Tilemap CloudCover => cloudCover;
        public Transform WaypointRoot => waypointRoot;
        public Transform TurretRoot => turretRoot;

        public void Configure(Tilemap groundMap, Tilemap pathMap, Tilemap blockedMap,
            Tilemap outsideMap, Tilemap clouds, Transform waypoints, Transform turrets)
        {
            ground = groundMap; enemyPath = pathMap; blockedTerrain = blockedMap;
            outsideTerrain = outsideMap; cloudCover = clouds; waypointRoot = waypoints; turretRoot = turrets;
        }
    }
}
