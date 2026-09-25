using KeySlaught.SceneGameplay;
using NUnit.Framework;
using UnityEngine;

namespace KeySlaught.Tests.EditMode
{
    public sealed class Milestone10SystemsTests
    {
        [Test]
        public void EnemyPalette_UsesEveryRequestedColorBand()
        {
            var white = EnemyLengthPalette.Evaluate(0);
            var yellow = EnemyLengthPalette.Evaluate(10);
            var orange = EnemyLengthPalette.Evaluate(20);
            var red = EnemyLengthPalette.Evaluate(30);
            var pink = EnemyLengthPalette.Evaluate(40);
            var violet = EnemyLengthPalette.Evaluate(50);
            var purple = EnemyLengthPalette.Evaluate(64);
            Assert.That(white.r, Is.GreaterThan(.9f));
            Assert.That(yellow.r, Is.GreaterThan(yellow.b));
            Assert.That(orange.r, Is.GreaterThan(orange.g));
            Assert.That(red.r, Is.GreaterThan(red.g * 3f));
            Assert.That(pink.r, Is.GreaterThan(pink.g * 2f));
            Assert.That(violet.b, Is.GreaterThan(violet.g));
            Assert.That(purple.b, Is.GreaterThan(purple.r));
            Assert.That(EnemyLengthPalette.UseDarkText(20), Is.True);
            Assert.That(EnemyLengthPalette.UseDarkText(21), Is.False);
            Assert.That(EnemyLengthPalette.Evaluate(3), Is.Not.EqualTo(white), "Lengths 0-10 must gradually shade from white to yellow.");
            Assert.That(EnemyLengthPalette.Evaluate(5), Is.Not.EqualTo(yellow));
        }

        [Test]
        public void LevelWordEntry_TrimsUppercasesAndCapsAt64Characters()
        {
            var entry = new LevelWordEntry();
            var field = typeof(LevelWordEntry).GetField("word", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            field.SetValue(entry, "  " + new string('a', 80) + "  ");
            Assert.That(entry.Word.Length, Is.EqualTo(64));
            Assert.That(entry.Word, Is.EqualTo(new string('A', 64)));
        }

        [Test]
        public void PreviewTurret_IsExplicitlyMarkedNonCombat()
        {
            var go = new GameObject("preview");
            try
            {
                var turret = go.AddComponent<TurretPadController>();
                turret.SetPreview(true);
                Assert.That(turret.IsPreview, Is.True);
                Assert.DoesNotThrow(() => turret.Tick(10f));
            }
            finally { Object.DestroyImmediate(go); }
        }

        [Test]
        public void PresidentTurret_IsASelectableFamily()
        {
            var go = new GameObject("president");
            try
            {
                var turret = go.AddComponent<TurretPadController>();
                Assert.That(turret.Build(TurretKind.President), Is.True);
                Assert.That(turret.Kind, Is.EqualTo(TurretKind.President));
            }
            finally { Object.DestroyImmediate(go); }
        }

        [Test]
        public void PlayerBounds_IdentifyOffMapRewardPositions()
        {
            var go = new GameObject("bounded player");
            try
            {
                var mover = go.AddComponent<PlayerMover>();
                mover.Configure(2f, new Vector2(-2f, -3f), new Vector2(4f, 5f));
                Assert.That(mover.IsInsidePlayableBounds(new Vector2(0f, 0f)), Is.True);
                Assert.That(mover.IsInsidePlayableBounds(new Vector2(-2.01f, 0f)), Is.False);
                Assert.That(mover.IsInsidePlayableBounds(new Vector2(0f, 5.01f)), Is.False);
            }
            finally { Object.DestroyImmediate(go); }
        }
    }
}
