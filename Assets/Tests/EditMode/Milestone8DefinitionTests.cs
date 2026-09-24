using System.Reflection;
using KeySlaught.SceneGameplay;
using NUnit.Framework;
using UnityEngine;

namespace KeySlaught.Tests.EditMode
{
    public sealed class Milestone8DefinitionTests
    {
        [Test]
        public void TurretDefinition_UsesCoverageAndUpgradeCurvesFromData()
        {
            var definition = ScriptableObject.CreateInstance<TurretDefinition>();
            Set(definition, "coveredLetters", "ABCDEF");
            Set(definition, "range", 4f);
            Set(definition, "rangePerUpgrade", 0.5f);
            Set(definition, "secondsPerShot", 1f);
            Set(definition, "cadenceMultiplierPerUpgrade", 0.8f);

            Assert.That(definition.Covers('a'), Is.True);
            Assert.That(definition.Covers('Z'), Is.False);
            Assert.That(definition.RangeAtLevel(3), Is.EqualTo(5f).Within(0.001f));
            Assert.That(definition.SecondsPerShotAtLevel(3), Is.EqualTo(0.64f).Within(0.001f));
            Object.DestroyImmediate(definition);
        }

        [Test]
        public void WaveDefinition_ExposesFiniteEnemySequenceAndBossClass()
        {
            var wave = ScriptableObject.CreateInstance<WaveDefinition>();
            var enemy = ScriptableObject.CreateInstance<EnemyDefinition>();
            Set(wave, "enemies", new[] { enemy, enemy });
            Set(wave, "bossWave", true);

            Assert.That(wave.Enemies, Has.Length.EqualTo(2));
            Assert.That(wave.IsBossWave, Is.True);
            Object.DestroyImmediate(wave);
            Object.DestroyImmediate(enemy);
        }

        private static void Set(object target, string field, object value)
        {
            target.GetType().GetField(field, BindingFlags.Instance | BindingFlags.NonPublic).SetValue(target, value);
        }
    }
}
