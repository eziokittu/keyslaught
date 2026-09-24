using UnityEngine;

namespace KeySlaught.SceneGameplay
{
    public enum LibraryAbilityKind { History, SocialMediaInfluence, Politics }

    [CreateAssetMenu(menuName = "KeySlaught/Ability Definition", fileName = "AbilityDefinition")]
    public sealed class AbilityDefinition : ScriptableObject
    {
        [SerializeField] private LibraryAbilityKind kind;
        [SerializeField, Min(0)] private int cost = 8;
        [SerializeField, Min(0f)] private float duration = 5f;
        [SerializeField, Min(1)] private int targetCount = 3;

        public LibraryAbilityKind Kind => kind;
        public int Cost => cost;
        public float Duration => duration;
        public int TargetCount => targetCount;
    }
}
