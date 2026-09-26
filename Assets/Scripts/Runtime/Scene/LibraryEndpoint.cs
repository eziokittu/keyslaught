using System;
using KeySlaught.Gameplay;
using UnityEngine;
using KeySlaught.Audio;
using UnityEngine.UI;

namespace KeySlaught.SceneGameplay
{
    public sealed class LibraryEndpoint : MonoBehaviour
    {
        [SerializeField, Min(1)] private int maximumHealth = 30;
        [SerializeField] private TextMesh healthLabel;
        [SerializeField] private Text healthUiLabel;

        public LibraryState State { get; private set; }
        public event Action<int> EnemyDamageReceived;

        public void Initialize(int health)
        {
            maximumHealth = Mathf.Max(1, health);
            State = new LibraryState(maximumHealth);
            RefreshLabel();
        }

        public int ReceiveEnemy(EnemyAgent enemy)
        {
            if (State == null)
            {
                Initialize(maximumHealth);
            }

            var damage = State.ApplyEnemyArrival(enemy.WordState);
            RefreshLabel();
            EnemyDamageReceived?.Invoke(damage);
            if (damage > 0) PersistentAudioDirector.Play(KeySlaughtSound.LibraryHit);
            return damage;
        }

        public int Repair(int amount)
        {
            if (State == null)
            {
                Initialize(maximumHealth);
            }

            var repaired = State.Repair(amount);
            RefreshLabel();
            return repaired;
        }

        public void ResetState()
        {
            Initialize(maximumHealth);
        }

        public void SetMaximumHealth(int health)
        {
            maximumHealth = Mathf.Max(1, health);
            Initialize(maximumHealth);
        }

        public void ConfigureUi(Text label)
        {
            healthUiLabel = label;
            RefreshLabel();
        }

        private void Awake()
        {
            Initialize(maximumHealth);
        }

        private void RefreshLabel()
        {
            if (healthLabel != null)
            {
                healthLabel.text = State == null
                    ? "HP"
                    : $"HP  {State.CurrentHealth}/{State.MaximumHealth}";
            }
            if (healthUiLabel != null)
                healthUiLabel.text = State == null ? "LIBRARY HP" : $"LIBRARY HP  {State.CurrentHealth}/{State.MaximumHealth}";
        }
    }
}
