using System;
using KeySlaught.SceneGameplay;
using NUnit.Framework;
using UnityEngine;

namespace KeySlaught.Tests.EditMode
{
    public sealed class PathMathTests
    {
        private static readonly Vector3[] CornerPath =
        {
            new Vector3(0f, 0f, 0f),
            new Vector3(3f, 0f, 0f),
            new Vector3(3f, 4f, 0f)
        };

        [Test]
        public void CalculateLength_SumsEverySegment()
        {
            Assert.That(PathMath.CalculateLength(CornerPath), Is.EqualTo(7f));
        }

        [Test]
        public void EvaluateDistance_InterpolatesAcrossSegments()
        {
            Assert.That(
                PathMath.EvaluateDistance(CornerPath, 5f),
                Is.EqualTo(new Vector3(3f, 2f, 0f)));
        }

        [Test]
        public void EvaluateDistance_ClampsBeforeStartAndAfterEnd()
        {
            Assert.That(PathMath.EvaluateDistance(CornerPath, -2f), Is.EqualTo(CornerPath[0]));
            Assert.That(PathMath.EvaluateDistance(CornerPath, 20f), Is.EqualTo(CornerPath[2]));
        }

        [Test]
        public void EvaluateDistance_SkipsZeroLengthSegments()
        {
            var path = new[] { Vector3.zero, Vector3.zero, Vector3.right * 2f };

            Assert.That(
                PathMath.EvaluateDistance(path, 1f),
                Is.EqualTo(Vector3.right));
        }

        [Test]
        public void DistanceRemaining_ClampsAtZero()
        {
            Assert.That(PathMath.DistanceRemaining(CornerPath, 2f), Is.EqualTo(5f));
            Assert.That(PathMath.DistanceRemaining(CornerPath, 20f), Is.Zero);
        }

        [Test]
        public void PathOperations_RejectFewerThanTwoPoints()
        {
            Assert.That(
                () => PathMath.CalculateLength(new[] { Vector3.zero }),
                Throws.TypeOf<ArgumentException>());
        }
    }
}
