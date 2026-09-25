using System.Reflection;
using KeySlaught.Progression;
using KeySlaught.SceneGameplay;
using NUnit.Framework;
using UnityEngine;

namespace KeySlaught.Tests.EditMode
{
    public sealed class ProgressionServiceTests
    {
        [Test]
        public void PurchaseResearch_SpendsKnowledgeAndPersistsLevel()
        {
            var definition = Definition("PLAYER_RANGE", ResearchStat.PlayerRange, 3, 1, 1, 0.25f);
            var store = new MemoryStore();
            var firstObject = new GameObject("Progression 1");
            var first = firstObject.AddComponent<ProgressionService>();
            first.Configure(new[] { definition }); first.Initialize(store); first.CreditKnowledge(3);

            Assert.That(first.TryPurchase("PLAYER_RANGE"), Is.True);
            Assert.That(first.Profile.knowledgePoints, Is.EqualTo(2));
            Assert.That(first.Profile.GetLevel("PLAYER_RANGE"), Is.EqualTo(1));

            var secondObject = new GameObject("Progression 2");
            var second = secondObject.AddComponent<ProgressionService>();
            second.Configure(new[] { definition }); second.Initialize(store);
            Assert.That(second.Profile.GetLevel("PLAYER_RANGE"), Is.EqualTo(1));
            Assert.That(second.BonusFor(ResearchStat.PlayerRange), Is.EqualTo(.25f).Within(.001f));
            Object.DestroyImmediate(secondObject); Object.DestroyImmediate(firstObject); Object.DestroyImmediate(definition);
        }

        [Test]
        public void CompletingFirstLoreLevel_UnlocksEndlessMode()
        {
            var go = new GameObject("Unlock Progression");
            var service = go.AddComponent<ProgressionService>(); service.Initialize(new MemoryStore());

            Assert.That(service.Profile.endlessModeUnlocked, Is.False);
            service.CompleteLoreOneLevelOne();
            Assert.That(service.Profile.loreOneLevelOneCompleted, Is.True);
            Assert.That(service.Profile.endlessModeUnlocked, Is.True);
            Object.DestroyImmediate(go);
        }

        [Test]
        public void LoreStars_UseBothLibraryHitsAndCompletionTime()
        {
            Assert.That(ProgressionService.CalculateLoreStars(0, 179f), Is.EqualTo(3));
            Assert.That(ProgressionService.CalculateLoreStars(1, 240f), Is.EqualTo(2));
            Assert.That(ProgressionService.CalculateLoreStars(0, 301f), Is.EqualTo(1));
            Assert.That(ProgressionService.CalculateLoreStars(2, 120f), Is.EqualTo(1));
        }

        [Test]
        public void LoreCompletion_KeepsBestStarsAndFastestTime()
        {
            var go = new GameObject("Rated Progression");
            var service = go.AddComponent<ProgressionService>(); service.Initialize(new MemoryStore());
            service.CompleteLoreOneLevelOne(2, 250f);
            service.CompleteLoreOneLevelOne(1, 280f);
            service.CompleteLoreOneLevelOne(3, 170f);
            Assert.That(service.Profile.loreOneLevelOneStars, Is.EqualTo(3));
            Assert.That(service.Profile.loreOneLevelOneBestSeconds, Is.EqualTo(170f));
            Object.DestroyImmediate(go);
        }

        [Test]
        public void ResearchPurchase_RejectsInsufficientPointsAndMaximumLevel()
        {
            var definition = Definition("MAG", ResearchStat.MagazineCapacity, 1, 2, 1, 1f);
            var go = new GameObject("Research Limits");
            var service = go.AddComponent<ProgressionService>(); service.Configure(new[] { definition }); service.Initialize(new MemoryStore());
            Assert.That(service.TryPurchase("MAG"), Is.False);
            service.CreditKnowledge(2);
            Assert.That(service.TryPurchase("MAG"), Is.True);
            service.CreditKnowledge(20);
            Assert.That(service.TryPurchase("MAG"), Is.False);
            Object.DestroyImmediate(go); Object.DestroyImmediate(definition);
        }

        [Test]
        public void DedicatedLoreCompletion_PersistsUnlockAndRewardsOnlyOncePerRun()
        {
            var progressionObject = new GameObject("Dedicated Progression");
            var service = progressionObject.AddComponent<ProgressionService>();
            service.Initialize(new MemoryStore());
            var flowObject = new GameObject("Dedicated Flow");
            var flow = flowObject.AddComponent<DedicatedLevelSceneController>();
            flow.Configure(null, service, null, null);

            flow.RecordCompletion(true);
            flow.RecordCompletion(true);

            Assert.That(service.Profile.loreOneLevelOneCompleted, Is.True);
            Assert.That(service.Profile.endlessModeUnlocked, Is.True);
            Assert.That(service.Profile.knowledgePoints, Is.EqualTo(1));
            Object.DestroyImmediate(flowObject); Object.DestroyImmediate(progressionObject);
        }

        private static ResearchDefinition Definition(string id, ResearchStat stat, int max, int cost, int growth, float value)
        {
            var definition = ScriptableObject.CreateInstance<ResearchDefinition>();
            Set(definition, "id", id); Set(definition, "stat", stat); Set(definition, "maxLevel", max);
            Set(definition, "baseCost", cost); Set(definition, "costIncreasePerLevel", growth); Set(definition, "valuePerLevel", value);
            return definition;
        }

        private static void Set(object target, string field, object value) =>
            target.GetType().GetField(field, BindingFlags.Instance | BindingFlags.NonPublic).SetValue(target, value);

        private sealed class MemoryStore : IProgressionStore
        {
            private string value;
            public string Load() => value;
            public void Save(string json) => value = json;
            public void Delete() => value = null;
        }
    }
}
