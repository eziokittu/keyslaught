using KeySlaught.Gameplay;
using NUnit.Framework;

namespace KeySlaught.Tests.EditMode
{
    public sealed class LibraryStateTests
    {
        [Test]
        public void DamageFromArrival_EqualsRemainingLetterCount()
        {
            var enemy = new EnemyWordState("MILK");
            enemy.TryConsume('M');

            Assert.That(LibraryDamageRules.DamageFromArrival(enemy), Is.EqualTo(3));
        }

        [Test]
        public void DamageFromArrival_IsZeroForDefeatedEnemy()
        {
            var enemy = new EnemyWordState("A");
            enemy.TryConsume('A');

            Assert.That(LibraryDamageRules.DamageFromArrival(enemy), Is.Zero);
        }

        [Test]
        public void ApplyEnemyArrival_ReducesHealthAndClampsAtZero()
        {
            var library = new LibraryState(3);
            var enemy = new EnemyWordState("MILK");

            var damage = library.ApplyEnemyArrival(enemy);

            Assert.That(damage, Is.EqualTo(4));
            Assert.That(library.CurrentHealth, Is.Zero);
            Assert.That(library.IsDestroyed, Is.True);
        }

        [Test]
        public void Repair_RestoresHealthWithoutExceedingMaximum()
        {
            var library = new LibraryState(10);
            library.ApplyEnemyArrival(new EnemyWordState("BOOK"));

            Assert.That(library.Repair(3), Is.EqualTo(3));
            Assert.That(library.CurrentHealth, Is.EqualTo(9));
            Assert.That(library.Repair(50), Is.EqualTo(1));
            Assert.That(library.CurrentHealth, Is.EqualTo(10));
        }
    }
}
