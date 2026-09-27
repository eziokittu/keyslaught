using System;
using KeySlaught.SceneGameplay;
using UnityEngine;

namespace KeySlaught.Progression
{
    public sealed class ProgressionService : MonoBehaviour
    {
        [SerializeField] private ResearchDefinition[] definitions;
        private IProgressionStore store;

        public event Action Changed;
        public ProgressionProfile Profile { get; private set; }
        public ResearchDefinition[] Definitions => definitions;

        public void Configure(ResearchDefinition[] researchDefinitions)
        {
            definitions = researchDefinitions;
        }

        public void Initialize(IProgressionStore overrideStore = null)
        {
            store = overrideStore ?? new PlayerPrefsProgressionStore();
            var json = store.Load();
            Profile = string.IsNullOrWhiteSpace(json)
                ? new ProgressionProfile()
                : JsonUtility.FromJson<ProgressionProfile>(json) ?? new ProgressionProfile();
            Profile.research ??= new System.Collections.Generic.List<ResearchLevelEntry>();
            Changed?.Invoke();
        }

        public bool TryPurchase(string id)
        {
            EnsureInitialized();
            var definition = Array.Find(definitions, item => item != null && item.Id == id);
            if (definition == null) return false;
            var level = Profile.GetLevel(id);
            var cost = definition.CostForLevel(level);
            if (level >= definition.MaxLevel || Profile.knowledgePoints < cost) return false;
            Profile.knowledgePoints -= cost;
            Profile.SetLevel(id, level + 1);
            Save();
            return true;
        }

        public void CreditKnowledge(int amount)
        {
            EnsureInitialized();
            Profile.knowledgePoints += Mathf.Max(0, amount);
            Save();
        }

        public bool IsTurretUnlocked(TurretKind kind)
        {
            EnsureInitialized();
            return kind switch
            {
                TurretKind.Teacher => Profile.tutorialCompleted || Profile.GetLevel("TEACHER_UNLOCK") > 0,
                TurretKind.Engineer => Profile.GetLevel("ENGINEER_UNLOCK") > 0,
                TurretKind.Scientist => Profile.GetLevel("SCIENTIST_UNLOCK") > 0,
                TurretKind.President => Profile.GetLevel("PRESIDENT_UNLOCK") > 0,
                _ => false
            };
        }

        public int UnlockedTurretCount
        {
            get
            {
                EnsureInitialized();
                var count = 0;
                foreach (var kind in new[] { TurretKind.Teacher, TurretKind.Engineer, TurretKind.Scientist, TurretKind.President })
                    if (IsTurretUnlocked(kind)) count++;
                return count;
            }
        }

        public void MarkLaunchSeen()
        {
            EnsureInitialized();
            Profile.firstLaunch = false;
            Save();
        }

        public void CompleteTutorial()
        {
            EnsureInitialized();
            Profile.tutorialCompleted = true;
            Profile.SetLevel("TEACHER_UNLOCK", 1);
            Save();
        }

        public void CompleteTurretTutorial()
        {
            EnsureInitialized();
            Profile.turretTutorialCompleted = true;
            Profile.SetLevel("HISTORY_UNLOCK", 1);
            Save();
        }

        public void CompleteAbilityTutorial()
        {
            EnsureInitialized();
            Profile.abilityTutorialCompleted = true;
            Save();
        }

        public bool IsAbilityUnlocked(LibraryAbilityKind kind)
        {
            EnsureInitialized();
            return kind switch
            {
                LibraryAbilityKind.History => Profile.turretTutorialCompleted || Profile.GetLevel("HISTORY_UNLOCK") > 0,
                LibraryAbilityKind.SocialMediaInfluence => Profile.GetLevel("SOCIAL_UNLOCK") > 0,
                LibraryAbilityKind.Politics => Profile.GetLevel("POLITICS_UNLOCK") > 0,
                _ => false
            };
        }

        public bool IsResearchVisible(ResearchDefinition definition)
        {
            if (definition == null) return false;
            EnsureInitialized();
            if (definition.IsUnlock) return !UnlockSatisfied(definition.Stat);
            return definition.Stat switch
            {
                ResearchStat.TeacherRange or ResearchStat.TeacherAttackSpeed => IsTurretUnlocked(TurretKind.Teacher),
                ResearchStat.EngineerRange or ResearchStat.EngineerAttackSpeed => IsTurretUnlocked(TurretKind.Engineer),
                ResearchStat.ScientistRange or ResearchStat.ScientistAttackSpeed => IsTurretUnlocked(TurretKind.Scientist),
                ResearchStat.PresidentRange or ResearchStat.PresidentAttackSpeed => IsTurretUnlocked(TurretKind.President),
                ResearchStat.HistoryDuration => IsAbilityUnlocked(LibraryAbilityKind.History),
                ResearchStat.SocialInfluenceDuration => IsAbilityUnlocked(LibraryAbilityKind.SocialMediaInfluence),
                ResearchStat.PoliticsTargetCount => IsAbilityUnlocked(LibraryAbilityKind.Politics),
                _ => true
            };
        }

        private bool UnlockSatisfied(ResearchStat stat) => stat switch
        {
            ResearchStat.TeacherUnlock => IsTurretUnlocked(TurretKind.Teacher),
            ResearchStat.EngineerUnlock => IsTurretUnlocked(TurretKind.Engineer),
            ResearchStat.ScientistUnlock => IsTurretUnlocked(TurretKind.Scientist),
            ResearchStat.PresidentUnlock => IsTurretUnlocked(TurretKind.President),
            ResearchStat.HistoryUnlock => IsAbilityUnlocked(LibraryAbilityKind.History),
            ResearchStat.SocialInfluenceUnlock => IsAbilityUnlocked(LibraryAbilityKind.SocialMediaInfluence),
            ResearchStat.PoliticsUnlock => IsAbilityUnlocked(LibraryAbilityKind.Politics),
            _ => true
        };

        public void CompleteLoreOneLevelOne()
        {
            CompleteLoreOneLevelOne(1, 0f);
        }

        public void CompleteLoreOneLevelOne(int stars, float elapsedSeconds)
        {
            EnsureInitialized();
            Profile.loreOneLevelOneCompleted = true;
            Profile.endlessModeUnlocked = true;
            Profile.loreOneLevelOneStars = Mathf.Max(Profile.loreOneLevelOneStars, Mathf.Clamp(stars, 1, 3));
            if (elapsedSeconds > 0f && (Profile.loreOneLevelOneBestSeconds <= 0f || elapsedSeconds < Profile.loreOneLevelOneBestSeconds))
                Profile.loreOneLevelOneBestSeconds = elapsedSeconds;
            Save();
        }

        public void CompleteLoreLevel(int level, int stars, float elapsedSeconds)
        {
            if (level <= 1) { CompleteLoreOneLevelOne(stars, elapsedSeconds); return; }
            EnsureInitialized(); stars = Mathf.Clamp(stars, 1, 3);
            if (level == 2)
            {
                Profile.loreOneLevelTwoCompleted = true;
                Profile.loreOneLevelTwoStars = Mathf.Max(Profile.loreOneLevelTwoStars, stars);
                if (elapsedSeconds > 0f && (Profile.loreOneLevelTwoBestSeconds <= 0f || elapsedSeconds < Profile.loreOneLevelTwoBestSeconds)) Profile.loreOneLevelTwoBestSeconds = elapsedSeconds;
            }
            else if (level == 3)
            {
                Profile.loreOneLevelThreeCompleted = true;
                Profile.loreOneLevelThreeStars = Mathf.Max(Profile.loreOneLevelThreeStars, stars);
                if (elapsedSeconds > 0f && (Profile.loreOneLevelThreeBestSeconds <= 0f || elapsedSeconds < Profile.loreOneLevelThreeBestSeconds)) Profile.loreOneLevelThreeBestSeconds = elapsedSeconds;
            }
            else if (level == 4)
            {
                Profile.loreOneLevelFourCompleted = true;
                Profile.loreOneLevelFourStars = Mathf.Max(Profile.loreOneLevelFourStars, stars);
                if (elapsedSeconds > 0f && (Profile.loreOneLevelFourBestSeconds <= 0f || elapsedSeconds < Profile.loreOneLevelFourBestSeconds)) Profile.loreOneLevelFourBestSeconds = elapsedSeconds;
            }
            else if (level == 5)
            {
                Profile.loreOneLevelFiveCompleted = true;
                Profile.loreOneLevelFiveStars = Mathf.Max(Profile.loreOneLevelFiveStars, stars);
                if (elapsedSeconds > 0f && (Profile.loreOneLevelFiveBestSeconds <= 0f || elapsedSeconds < Profile.loreOneLevelFiveBestSeconds)) Profile.loreOneLevelFiveBestSeconds = elapsedSeconds;
            }
            else
            {
                Profile.loreOneLevelSixCompleted = true;
                Profile.loreOneLevelSixStars = Mathf.Max(Profile.loreOneLevelSixStars, stars);
                if (elapsedSeconds > 0f && (Profile.loreOneLevelSixBestSeconds <= 0f || elapsedSeconds < Profile.loreOneLevelSixBestSeconds)) Profile.loreOneLevelSixBestSeconds = elapsedSeconds;
            }
            Save();
        }

        public static int CalculateLoreStars(int libraryHits, float elapsedSeconds)
        {
            if (libraryHits <= 0 && elapsedSeconds <= 180f) return 3;
            if (libraryHits <= 1 && elapsedSeconds <= 300f) return 2;
            return 1;
        }

        public float BonusFor(ResearchStat stat)
        {
            EnsureInitialized();
            var total = 0f;
            if (definitions == null) return total;
            foreach (var definition in definitions)
                if (definition != null && definition.Stat == stat)
                    total += Profile.GetLevel(definition.Id) * definition.ValuePerLevel;
            return total;
        }

        public void ResetProgress()
        {
            EnsureInitialized();
            store.Delete();
            Profile = new ProgressionProfile();
            Changed?.Invoke();
        }

        private void Awake() => Initialize();
        private void EnsureInitialized() { if (Profile == null) Initialize(); }
        private void Save() { store.Save(JsonUtility.ToJson(Profile)); Changed?.Invoke(); }
    }
}
