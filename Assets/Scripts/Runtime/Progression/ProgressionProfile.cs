using System;
using System.Collections.Generic;

namespace KeySlaught.Progression
{
    [Serializable]
    public sealed class ResearchLevelEntry
    {
        public string id;
        public int level;
    }

    [Serializable]
    public sealed class ProgressionProfile
    {
        public bool firstLaunch = true;
        public bool tutorialCompleted;
        public bool loreOneLevelOneCompleted;
        public bool endlessModeUnlocked;
        public int knowledgePoints;
        public List<ResearchLevelEntry> research = new();

        public int GetLevel(string id)
        {
            var entry = research.Find(item => item != null && item.id == id);
            return entry == null ? 0 : entry.level;
        }

        public void SetLevel(string id, int level)
        {
            var entry = research.Find(item => item != null && item.id == id);
            if (entry == null)
            {
                entry = new ResearchLevelEntry { id = id };
                research.Add(entry);
            }
            entry.level = Math.Max(0, level);
        }
    }
}
