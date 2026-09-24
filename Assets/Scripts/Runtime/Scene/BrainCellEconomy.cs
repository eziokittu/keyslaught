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
        [SerializeField, Min(0f)] private float scatterRadius = 0.48f;
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

        public void ResetState()
        {
            for (var index = activePickups.Count - 1; index >= 0; index--)
                if (activePickups[index] != null) Destroy(activePickups[index].gameObject);
            activePickups.Clear();
            Balance = 0;
            RefreshLabel();
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
                var scatter = UnityEngine.Random.insideUnitCircle * scatterRadius;
                pickup = Instantiate(pickupPrefab, enemy.transform.position + (Vector3)scatter, Quaternion.identity, transform);
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
            var pickupRenderer = pickup.GetComponent<SpriteRenderer>();
            if (pickupRenderer != null) pickupRenderer.color = new Color(.18f, .92f, 1f);
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

}
