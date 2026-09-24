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
            var yellow = EnemyLengthPalette.Evaluate(20);
            var orange = EnemyLengthPalette.Evaluate(30);
            var red = EnemyLengthPalette.Evaluate(40);
            var pink = EnemyLengthPalette.Evaluate(50);
            var purple = EnemyLengthPalette.Evaluate(64);
            Assert.That(white.r, Is.GreaterThan(.9f));
            Assert.That(yellow.r, Is.GreaterThan(yellow.b));
            Assert.That(orange.r, Is.GreaterThan(orange.g));
            Assert.That(red.r, Is.GreaterThan(red.g * 3f));
            Assert.That(pink.r, Is.GreaterThan(pink.g * 2f));
            Assert.That(purple.b, Is.GreaterThan(purple.r));
            Assert.That(EnemyLengthPalette.UseDarkText(30), Is.True);
            Assert.That(EnemyLengthPalette.UseDarkText(31), Is.False);
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
    }
}
