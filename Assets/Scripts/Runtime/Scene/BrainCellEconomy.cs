using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace KeySlaught.SceneGameplay
{
    public sealed class BrainCellEconomy : MonoBehaviour
    {
        [SerializeField] private GameplayCombatController combat;
        [SerializeField] private PlayerMover player;
        [SerializeField] private Sprite pickupSprite;
        [SerializeField] private BrainCellPickup pickupPrefab;
        [SerializeField] private Text currencyLabel;
        [SerializeField, Min(0.1f)] private float collectRadius = 0.55f;
        [SerializeField, Min(0.1f)] private float clumpRadius = 0.65f;
        private readonly List<BrainCellPickup> activePickups = new();

        public int Balance { get; private set; }

        public void Configure(GameplayCombatController controller, PlayerMover mover, Sprite sprite, Text label)
        {
            Configure(controller, mover, sprite, label, null);
        }

        public void Configure(GameplayCombatController controller, PlayerMover mover, Sprite sprite, Text label, BrainCellPickup prefab)
        {
            if (isActiveAndEnabled && combat != null) combat.TargetDefeated -= OnTargetDefeated;
            combat = controller;
            player = mover;
            pickupSprite = sprite;
            pickupPrefab = prefab;
            currencyLabel = label;
            if (isActiveAndEnabled && combat != null) combat.TargetDefeated += OnTargetDefeated;
            RefreshLabel();
        }

        public bool TrySpend(int amount)
        {
            if (amount < 0 || Balance < amount) return false;
            Balance -= amount;
            RefreshLabel();
            return true;
        }

        public void Credit(int amount)
        {
            Balance += Mathf.Max(0, amount);
            RefreshLabel();
        }

        public static int CalculateReward(KeySlaught.Gameplay.EnemyWordState word)
        {
            return word == null ? 0 : word.OriginalWord.Length;
        }

        public void CollectAll()
        {
            for (var index = activePickups.Count - 1; index >= 0; index--)
                Collect(activePickups[index]);
        }

        private void OnEnable() { if (combat != null) combat.TargetDefeated += OnTargetDefeated; }
        private void OnDisable() { if (combat != null) combat.TargetDefeated -= OnTargetDefeated; }

        private void Update()
        {
            if (player == null) return;
            for (var index = activePickups.Count - 1; index >= 0; index--)
            {
                var pickup = activePickups[index];
                if (pickup == null) { activePickups.RemoveAt(index); continue; }
                if (Vector2.Distance(player.transform.position, pickup.transform.position) <= collectRadius)
                    Collect(pickup);
            }
        }

        private void OnTargetDefeated(EnemyAgent enemy)
        {
            if (enemy == null || enemy.WordState == null) return;
            var value = CalculateReward(enemy.WordState);
            foreach (var existing in activePickups)
            {
                if (existing != null && Vector2.Distance(existing.transform.position, enemy.transform.position) <= clumpRadius)
                {
                    existing.Add(value);
                    return;
                }
            }

            BrainCellPickup pickup;
            if (pickupPrefab != null)
            {
                pickup = Instantiate(pickupPrefab, enemy.transform.position, Quaternion.identity, transform);
            }
            else
            {
                var pickupObject = new GameObject($"Brain Cells +{value}");
                pickupObject.transform.SetParent(transform, false);
                pickupObject.transform.position = enemy.transform.position;
                pickupObject.transform.localScale = Vector3.one * 0.42f;
                var renderer = pickupObject.AddComponent<SpriteRenderer>();
                renderer.sprite = pickupSprite;
                renderer.sortingOrder = 18;
                pickup = pickupObject.AddComponent<BrainCellPickup>();
                TryAddPointLight(pickupObject);
            }
            pickup.Initialize(value);
            activePickups.Add(pickup);
        }

        private void Collect(BrainCellPickup pickup)
        {
            if (pickup == null) return;
            Balance += pickup.Value;
            activePickups.Remove(pickup);
            Destroy(pickup.gameObject);
            RefreshLabel();
        }

        private void RefreshLabel()
        {
            if (currencyLabel != null) currencyLabel.text = $"BRAIN CELLS  {Balance}";
        }

        private static void TryAddPointLight(GameObject target)
        {
            var type = Type.GetType("UnityEngine.Rendering.Universal.Light2D, Unity.RenderPipelines.Universal.Runtime");
            if (type == null) return;
            var light = target.AddComponent(type);
            type.GetProperty("intensity")?.SetValue(light, 0.8f);
            type.GetProperty("pointLightOuterRadius")?.SetValue(light, 1.6f);
            type.GetProperty("color")?.SetValue(light, Color.white);
        }
    }

    public sealed class BrainCellPickup : MonoBehaviour
    {
        public int Value { get; private set; }
        public void Initialize(int value) => Value = Mathf.Max(0, value);
        public void Add(int value)
        {
            Value += Mathf.Max(0, value);
            name = $"Brain Cells +{Value}";
            transform.localScale = Vector3.one * Mathf.Min(0.7f, 0.42f + Value * 0.015f);
        }
    }
}
