using System;
using UnityEngine;
using UnityEditor;
using NUnit.Framework;
using Lockstep;

namespace MonoTests.Lockstep {
    [TestFixture]
    public class FixedMathTest {
        
        [SetUp]
        public void GetReady() {
        }

        [TearDown]
        public void Clean() {
        }

        [Test]
        public void CreateLongTest() {
            Assert.AreEqual(0L, FixedMath.Create(0L));
            Assert.AreEqual(1L << 16, FixedMath.Create(1L));
            Assert.AreEqual(-1L << 16, FixedMath.Create(-1L));
            Assert.AreEqual(132414LL << 16, FixedMath.Create(132414L));
        }

        [Test]
        public void AddSubIntTest() {
            long n1 = FixedMath.Create(-1L);
            long p1 = FixedMath.Create(1L);

            // Assert 1 + -1 = 0
            Assert.AreEqual(0, n1.Add(p1));

            // Assert 1 + 1 = 2
            Assert.AreEqual(2 << 16, p1.Add(p1));

            // Assert -1 + -1 = -2
            Assert.AreEqual(-2 << 16, n1.Add(n1));

            // Assert 12 - 7 = 5
            Assert.AreEqual(5 << 16, FixedMath.Create(12L).Sub(FixedMath.Create(7L)));

            // Assert 33 - 66 = -33
            Assert.AreEqual(-33 << 16, FixedMath.Create(33L).Sub(FixedMath.Create(66L)));

            // Assert -33 - -66 = 33
            Assert.AreEqual(33 << 16, FixedMath.Create(-33L).Sub(FixedMath.Create(-66L)));
        }

        [Test]
        public void CreateFloatTest() {
            // The left 48 bits in the number are the whole number
            // and the right 16 are the fraction. This test ensures
            // the fractional numbers are being assigned correctly.
            Assert.AreEqual(0, FixedMath.Create(0f));
            // Assert 1.5 = ..001|100..
            Assert.AreEqual((1 << 16) + (1 << 15), FixedMath.Create(1.5f));
            // Assert .75 = ..0000|1100..
            Assert.AreEqual((1 << 14) + (1 << 15), FixedMath.Create(.75f));
            Assert.AreEqual((1 << 26) + (1 << 15), FixedMath.Create(1024.5f));
            Assert.AreEqual(-((1 << 26) + (1 << 15)), FixedMath.Create(-1024.5f));
        }

        [Test]
        public void AddSubFloatTest() {
            long half = FixedMath.Create(0.5f);
            Assert.AreEqual(1 << 16, half.Add(half));

            long quarter = FixedMath.Create(0.5f);
            Assert.AreEqual(1 << 16, half.Add(half));
        }

        [Test]
        public void RoundTest() {
            // Keep in mind the current rounding uses 0.5 -> 0
            Assert.AreEqual(0, FixedMath.Round(FixedMath.Create(0.5f)));
            Assert.AreEqual(1 << 16, FixedMath.Round(FixedMath.Create(0.6f)));
            Assert.AreEqual(0, FixedMath.Round(FixedMath.Create(0.4f)));
            Assert.AreEqual(0, FixedMath.Round(FixedMath.Create(0.1f)));

            // Keep in mind the current rounding uses 0.5 -> -1
            Assert.AreEqual(-1 << 16, FixedMath.Round(FixedMath.Create(-0.5f)));
            Assert.AreEqual(-1 << 16, FixedMath.Round(FixedMath.Create(-0.6f)));
            Assert.AreEqual(0, FixedMath.Round(FixedMath.Create(-0.4f)));
            Assert.AreEqual(0, FixedMath.Round(FixedMath.Create(-0.1f)));
        }

        [Test]
        public void CreateFromFractionTest() {
            Assert.AreEqual(0, FixedMath.Create(0, 100));
            Assert.AreEqual(1 << 15, FixedMath.Create(1, 2));
            Assert.AreEqual(-1 << 15, FixedMath.Create(-1, 2));
            Assert.AreEqual(-1 << 15, FixedMath.Create(1, -2));
            Assert.AreEqual(1 << 15, FixedMath.Create(-1, -2));
        }

        [Test]
        public void MulDivTest() {
            long two = FixedMath.Create(2);
            // Assert 2x2 = 4
            Assert.AreEqual(1 << 18, two.Mul(two));

            // Assert 2/2 = 1
            Assert.AreEqual(1 << 16, two.Div(two));

            long max = FixedMath.Create(int.MaxValue);
            long answer = FixedMath.Create(1073741823.5);
            Assert.AreEqual(70368744144896L, answer);

            Assert.AreEqual(answer, max.Div(two));
            Assert.AreEqual(1 << 16, max.Div(max));
        }

        [Test]
        public void ToIntTest() {
            Assert.AreEqual(0, FixedMath.Create(0).ToInt());
            Assert.AreEqual(1, FixedMath.Create(1).ToInt());
            Assert.AreEqual(-1, FixedMath.Create(-1).ToInt());
        }
    }
}

