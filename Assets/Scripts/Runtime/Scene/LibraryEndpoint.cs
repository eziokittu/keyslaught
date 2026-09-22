using KeySlaught.Gameplay;
using UnityEngine;

namespace KeySlaught.SceneGameplay
{
    public sealed class LibraryEndpoint : MonoBehaviour
    {
        [SerializeField, Min(1)] private int maximumHealth = 30;
        [SerializeField] private TextMesh healthLabel;

        public LibraryState State { get; private set; }

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

        private void Awake()
        {
            Initialize(maximumHealth);
        }

        private void RefreshLabel()
        {
            if (healthLabel != null)
            {
                healthLabel.text = State == null
                    ? "LIBRARY"
                    : $"LIBRARY  {State.CurrentHealth}/{State.MaximumHealth}";
            }
        }
    }
}
