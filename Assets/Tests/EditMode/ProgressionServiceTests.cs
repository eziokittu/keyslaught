using System.Reflection;
using KeySlaught.Progression;
using KeySlaught.SceneGameplay;
using KeySlaught.UI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;

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
        public void LaterLoreLevels_PersistIndependentlyAndKeepBestResults()
        {
            var go = new GameObject("Lore Sequence Progression");
            var service = go.AddComponent<ProgressionService>(); service.Initialize(new MemoryStore());
            service.CompleteLoreLevel(2, 2, 260f); service.CompleteLoreLevel(2, 3, 190f);
            service.CompleteLoreLevel(3, 1, 340f);
            Assert.That(service.Profile.loreOneLevelTwoCompleted, Is.True);
            Assert.That(service.Profile.loreOneLevelTwoStars, Is.EqualTo(3));
            Assert.That(service.Profile.loreOneLevelTwoBestSeconds, Is.EqualTo(190f));
            Assert.That(service.Profile.loreOneLevelThreeCompleted, Is.True);
            Assert.That(service.Profile.loreOneLevelThreeStars, Is.EqualTo(1));
            Object.DestroyImmediate(go);
        }

        [Test]
        public void ExtendedLoreLevels_FourThroughSixPersistIndependently()
        {
            var go = new GameObject("Extended Lore Progression");
            var service = go.AddComponent<ProgressionService>(); service.Initialize(new MemoryStore());
            service.CompleteLoreLevel(4, 2, 300f);
            service.CompleteLoreLevel(5, 3, 280f);
            service.CompleteLoreLevel(6, 1, 420f);
            Assert.That(service.Profile.loreOneLevelFourCompleted, Is.True);
            Assert.That(service.Profile.loreOneLevelFourStars, Is.EqualTo(2));
            Assert.That(service.Profile.loreOneLevelFiveCompleted, Is.True);
            Assert.That(service.Profile.loreOneLevelFiveStars, Is.EqualTo(3));
            Assert.That(service.Profile.loreOneLevelSixCompleted, Is.True);
            Assert.That(service.Profile.loreOneLevelSixBestSeconds, Is.EqualTo(420f));
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

        [Test]
        public void BrainCells_ArePersistentAndSpendableAcrossServiceInstances()
        {
            var store = new MemoryStore();
            var firstObject = new GameObject("Brain Wallet One");
            var first = firstObject.AddComponent<ProgressionService>(); first.Initialize(store);
            first.CreditBrainCells(14);
            Assert.That(first.TrySpendBrainCells(5), Is.True);
            Assert.That(first.Profile.brainCells, Is.EqualTo(9));

            var secondObject = new GameObject("Brain Wallet Two");
            var second = secondObject.AddComponent<ProgressionService>(); second.Initialize(store);
            Assert.That(second.Profile.brainCells, Is.EqualTo(9));
            Assert.That(second.TrySpendBrainCells(10), Is.False);
            Object.DestroyImmediate(secondObject); Object.DestroyImmediate(firstObject);
        }

        [Test]
        public void Turrets_UnlockSequentiallyFromTutorialAndLoreCompletion()
        {
            var go = new GameObject("Turret Unlock Progression");
            var service = go.AddComponent<ProgressionService>(); service.Initialize(new MemoryStore());
            Assert.That(service.UnlockedTurretCount, Is.Zero);
            service.CompleteTutorial();
            Assert.That(service.IsTurretUnlocked(TurretKind.Teacher), Is.True);
            Assert.That(service.IsTurretUnlocked(TurretKind.Engineer), Is.False);
            service.CompleteLoreLevel(1, 1, 200f);
            Assert.That(service.IsTurretUnlocked(TurretKind.Engineer), Is.True);
            service.CompleteLoreLevel(2, 1, 200f);
            Assert.That(service.IsTurretUnlocked(TurretKind.Scientist), Is.True);
            service.CompleteLoreLevel(3, 1, 200f);
            Assert.That(service.IsTurretUnlocked(TurretKind.President), Is.True);
            Assert.That(service.UnlockedTurretCount, Is.EqualTo(4));
            Object.DestroyImmediate(go);
        }

        [Test]
        public void GameSpeed_ClampsToSupportedMultipliers()
        {
            var previous = PlayerPrefs.GetInt(GameSpeedSettings.PlayerPrefsKey, 1);
            try
            {
                GameSpeedSettings.SetMultiplier(8);
                Assert.That(GameSpeedSettings.Multiplier, Is.EqualTo(3));
                GameSpeedSettings.SetMultiplier(-2);
                Assert.That(GameSpeedSettings.Multiplier, Is.EqualTo(1));
            }
            finally
            {
                PlayerPrefs.SetInt(GameSpeedSettings.PlayerPrefsKey, previous);
                Time.timeScale = 1f;
            }
        }

        [Test]
        public void TutorialGate_RestrictsEachForcedInputStage()
        {
            try
            {
                TutorialInputGate.MovementOnly();
                Assert.That(TutorialInputGate.AllowMovement, Is.True);
                Assert.That(TutorialInputGate.AllowsLetter('C'), Is.False);
                TutorialInputGate.LettersOnly("CAT");
                Assert.That(TutorialInputGate.AllowMovement, Is.False);
                Assert.That(TutorialInputGate.AllowsLetter('C'), Is.True);
                Assert.That(TutorialInputGate.AllowsLetter('Z'), Is.False);
                TutorialInputGate.RefreshOnly();
                Assert.That(TutorialInputGate.AllowRefresh, Is.True);
                Assert.That(TutorialInputGate.AllowsLetter('A'), Is.False);
            }
            finally { TutorialInputGate.Clear(); }
        }

        [Test]
        public void TutorialGate_RequiresTheNextExactLetterAndReportsMistakes()
        {
            string feedback = null; var accepted = true;
            void Capture(string message, bool value) { feedback = message; accepted = value; }
            TutorialInputGate.Feedback += Capture;
            try
            {
                TutorialInputGate.RequireLetter('C');
                Assert.That(TutorialInputGate.TryAllowLetter('A'), Is.False);
                Assert.That(accepted, Is.False); Assert.That(feedback, Does.Contain("PRESS C"));
                Assert.That(TutorialInputGate.TryAllowLetter('C'), Is.True);
                Assert.That(accepted, Is.True); Assert.That(feedback, Does.Contain("CORRECT"));
            }
            finally { TutorialInputGate.Feedback -= Capture; TutorialInputGate.Clear(); }
        }

        [Test]
        public void TutorialMovementChecklist_RequiresAllFourDirections()
        {
            var checklist = new TutorialMovementChecklist(.5f);
            checklist.Advance(new Vector2(.6f, .6f));
            Assert.That(checklist.UpComplete, Is.True); Assert.That(checklist.RightComplete, Is.True);
            Assert.That(checklist.IsComplete, Is.False);
            checklist.Advance(new Vector2(-.6f, -.6f));
            Assert.That(checklist.LeftComplete, Is.True); Assert.That(checklist.DownComplete, Is.True);
            Assert.That(checklist.IsComplete, Is.True);
        }

        [Test]
        public void TutorialFocusGuide_HideToleratesDestroyedComponent()
        {
            var host = new GameObject("Destroyed Tutorial Focus Guide", typeof(RectTransform), typeof(CanvasRenderer), typeof(TutorialFocusGuide));
            var guide = host.GetComponent<TutorialFocusGuide>();
            Object.DestroyImmediate(host);

            Assert.DoesNotThrow(() => guide.Hide());
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
