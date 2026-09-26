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
        public int loreOneLevelOneStars;
        public float loreOneLevelOneBestSeconds;
        public bool loreOneLevelTwoCompleted;
        public int loreOneLevelTwoStars;
        public float loreOneLevelTwoBestSeconds;
        public bool loreOneLevelThreeCompleted;
        public int loreOneLevelThreeStars;
        public float loreOneLevelThreeBestSeconds;
        public bool loreOneLevelFourCompleted;
        public int loreOneLevelFourStars;
        public float loreOneLevelFourBestSeconds;
        public bool loreOneLevelFiveCompleted;
        public int loreOneLevelFiveStars;
        public float loreOneLevelFiveBestSeconds;
        public bool loreOneLevelSixCompleted;
        public int loreOneLevelSixStars;
        public float loreOneLevelSixBestSeconds;
        public bool endlessModeUnlocked;
        public int brainCells;
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
