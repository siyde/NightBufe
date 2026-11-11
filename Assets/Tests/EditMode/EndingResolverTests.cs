using System.Collections.Generic;
using NightBufe.Core;
using NUnit.Framework;

namespace NightBufe.Tests
{
    public class EndingResolverTests
    {
        [Test]
        public void FreshState_OnlyRegular()
        {
            CollectionAssert.AreEqual(new[] { EndingId.Regular }, EndingResolver.AvailableEndings(new GameState()));
        }

        [TestCase(2, false)]
        [TestCase(3, true)]
        [TestCase(4, true)]
        public void Loop_NeedsChildBond3(int childBond, bool expected)
        {
            var state = new GameState { childBond = childBond };
            Assert.AreEqual(expected, EndingResolver.IsAvailable(state, EndingId.Loop));
        }

        [TestCase(4, false)]
        [TestCase(5, true)]
        public void Awakening_NeedsReality5(int reality, bool expected)
        {
            var state = new GameState { reality = reality };
            Assert.AreEqual(expected, EndingResolver.IsAvailable(state, EndingId.Awakening));
        }

        [Test]
        public void Seal_NeedsCuriosity5AndThreePages()
        {
            var state = new GameState { curiosity = 5, diaryPagesFound = new List<int> { 1, 2 } };
            Assert.IsFalse(EndingResolver.IsAvailable(state, EndingId.Seal), "2 sayfa yetmez");

            state.AddDiaryPage(3);
            Assert.IsTrue(EndingResolver.IsAvailable(state, EndingId.Seal));

            state.curiosity = 4;
            Assert.IsFalse(EndingResolver.IsAvailable(state, EndingId.Seal), "M 4 yetmez");
        }

        [Test]
        public void Seal_DuplicateOrInvalidPagesDoNotCount()
        {
            var state = new GameState { curiosity = 6, diaryPagesFound = new List<int> { 1, 1, 2, 7 } };
            Assert.IsFalse(EndingResolver.IsAvailable(state, EndingId.Seal));
        }

        [Test]
        public void AddDiaryPage_IgnoresDuplicates()
        {
            var state = new GameState();
            state.AddDiaryPage(1);
            state.AddDiaryPage(1);
            Assert.AreEqual(1, state.diaryPagesFound.Count);
        }

        [Test]
        public void CarefulPlayer_CanOpenAllThreeSpecialEndings()
        {
            var state = new GameState { childBond = 3, curiosity = 5, reality = 5, diaryPagesFound = new List<int> { 1, 2, 3 } };
            CollectionAssert.AreEqual(
                new[] { EndingId.Regular, EndingId.Loop, EndingId.Seal, EndingId.Awakening },
                EndingResolver.AvailableEndings(state));
        }

        [TestCase(3, false)]
        [TestCase(4, true)]
        public void LastRule_ReadableAtCuriosity4(int curiosity, bool expected)
        {
            Assert.AreEqual(expected, EndingResolver.IsLastRuleReadable(new GameState { curiosity = curiosity }));
        }

        /// <summary>
        /// Plays every Reality choice from DESIGN.md nights 1–4 (radyo, taksi sohbet,
        /// taksiye servis, bağıran adam, radyodaki anne): Uyanış must be reachable.
        /// </summary>
        [Test]
        public void Awakening_ReachableThroughDesignedChoices()
        {
            var state = new GameState();
            CounterSystem.TryApplyChoice(state, "n1_radio", new[] { new CounterEffect(CounterType.Reality, 1) });
            CounterSystem.TryApplyChoice(state, "n1_taxi", new[] { new CounterEffect(CounterType.Reality, 1) });
            CounterSystem.TryApplyChoice(state, "n3_man", new[] { new CounterEffect(CounterType.Reality, 2) });
            Assert.IsFalse(EndingResolver.IsAvailable(state, EndingId.Awakening), "Gece 4 olmadan G=4");
            CounterSystem.TryApplyChoice(state, "n4_mother", new[] { new CounterEffect(CounterType.Reality, 2), new CounterEffect(CounterType.Agreement, -1) });
            Assert.AreEqual(6, state.reality);
            Assert.IsTrue(EndingResolver.IsAvailable(state, EndingId.Awakening));
        }
    }
}
