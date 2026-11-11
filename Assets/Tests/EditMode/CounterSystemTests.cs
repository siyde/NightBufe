using NightBufe.Core;
using NUnit.Framework;

namespace NightBufe.Tests
{
    public class CounterSystemTests
    {
        [Test]
        public void Apply_AddsAndSubtracts()
        {
            var state = new GameState();
            CounterSystem.Apply(state, new CounterEffect(CounterType.Reality, 2));
            CounterSystem.Apply(state, new CounterEffect(CounterType.Reality, -1));
            Assert.AreEqual(1, state.reality);
        }

        [Test]
        public void Apply_NeverGoesBelowZero()
        {
            var state = new GameState();
            CounterSystem.Apply(state, new CounterEffect(CounterType.Agreement, -1));
            Assert.AreEqual(0, state.agreement);
        }

        [TestCase(CounterType.Agreement, 7)]
        [TestCase(CounterType.Curiosity, 6)]
        [TestCase(CounterType.Reality, 7)]
        [TestCase(CounterType.ChildBond, 4)]
        public void Apply_ClampsAtDesignMaximum(CounterType type, int max)
        {
            var state = new GameState();
            for (int i = 0; i < 20; i++)
                CounterSystem.Apply(state, new CounterEffect(type, 1));
            Assert.AreEqual(max, state.Get(type));
        }

        [Test]
        public void TryApplyChoice_OnlyOnce()
        {
            var state = new GameState();
            var effects = new[] { new CounterEffect(CounterType.ChildBond, 1) };
            Assert.IsTrue(CounterSystem.TryApplyChoice(state, "x", effects));
            Assert.IsFalse(CounterSystem.TryApplyChoice(state, "x", effects));
            Assert.AreEqual(1, state.childBond);
            CollectionAssert.Contains(state.usedChoices, "x");
        }

        [Test]
        public void TryApplyChoice_MixedEffects()
        {
            // e.g. Gece 2: taksi şoförüne servis yap → G +1, A −1
            var state = new GameState { agreement = 2 };
            CounterSystem.TryApplyChoice(state, "n2_taxi_serve",
                new[] { new CounterEffect(CounterType.Reality, 1), new CounterEffect(CounterType.Agreement, -1) });
            Assert.AreEqual(1, state.reality);
            Assert.AreEqual(1, state.agreement);
        }

        [Test]
        public void TryApplyChoice_EmptyIdIsIgnored()
        {
            var state = new GameState();
            Assert.IsFalse(CounterSystem.TryApplyChoice(state, null, new[] { new CounterEffect(CounterType.Reality, 1) }));
            Assert.AreEqual(0, state.reality);
        }
    }
}
