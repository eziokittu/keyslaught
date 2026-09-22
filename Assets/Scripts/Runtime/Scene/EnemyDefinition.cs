using UnityEngine;

namespace KeySlaught.SceneGameplay
{
    [CreateAssetMenu(menuName = "KeySlaught/Enemy Definition", fileName = "EnemyDefinition")]
    public sealed class EnemyDefinition : ScriptableObject
    {
        [SerializeField] private string word = "BOOK";
        [SerializeField, Min(0.01f)] private float movementSpeed = 1.5f;

        public string Word => word;

        public float MovementSpeed => movementSpeed;

        private void OnValidate()
        {
            word = string.IsNullOrWhiteSpace(word) ? "BOOK" : word.ToUpperInvariant();
            movementSpeed = Mathf.Max(0.01f, movementSpeed);
        }
    }
}
