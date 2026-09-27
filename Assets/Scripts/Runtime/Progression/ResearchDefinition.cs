using UnityEngine;

namespace KeySlaught.Progression
{
    public enum ResearchStat
    {
        PlayerRange,
        MagazineCapacity,
        LibraryHealth,
        ReloadSpeed,
        TeacherRange,
        EngineerRange,
        ScientistRange,
        PresidentRange,
        TeacherAttackSpeed,
        EngineerAttackSpeed,
        ScientistAttackSpeed,
        PresidentAttackSpeed,
        HistoryDuration,
        SocialInfluenceDuration,
        PoliticsTargetCount,
        TeacherUnlock,
        EngineerUnlock,
        ScientistUnlock,
        PresidentUnlock,
        HistoryUnlock,
        SocialInfluenceUnlock,
        PoliticsUnlock
    }

    [CreateAssetMenu(menuName = "KeySlaught/Research Definition", fileName = "ResearchDefinition")]
    public sealed class ResearchDefinition : ScriptableObject
    {
        [SerializeField] private string id = "PLAYER_RANGE";
        [SerializeField] private string displayName = "Player Range";
        [SerializeField] private ResearchStat stat;
        [SerializeField, Min(1)] private int maxLevel = 5;
        [SerializeField, Min(0)] private int baseCost = 1;
        [SerializeField, Min(0)] private int costIncreasePerLevel = 1;
        [SerializeField] private float valuePerLevel = 0.25f;

        public string Id => id;
        public string DisplayName => displayName;
        public ResearchStat Stat => stat;
        public int MaxLevel => maxLevel;
        public float ValuePerLevel => valuePerLevel;
        public int CostForLevel(int currentLevel) => baseCost + Mathf.Max(0, currentLevel) * costIncreasePerLevel;
        public bool IsUnlock => stat is ResearchStat.TeacherUnlock or ResearchStat.EngineerUnlock or
            ResearchStat.ScientistUnlock or ResearchStat.PresidentUnlock or ResearchStat.HistoryUnlock or
            ResearchStat.SocialInfluenceUnlock or ResearchStat.PoliticsUnlock;
    }
}
