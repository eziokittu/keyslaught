using System;
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
            Save();
        }

        public void CompleteLoreOneLevelOne()
        {
            EnsureInitialized();
            Profile.loreOneLevelOneCompleted = true;
            Profile.endlessModeUnlocked = true;
            Save();
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
