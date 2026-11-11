using System.Collections.Generic;
using NightBufe.Core;
using NUnit.Framework;

namespace NightBufe.Tests
{
    public class DurumBuilderTests
    {
        private static OrderSpec Order(bool durum, string drink, params string[] fillings) =>
            new OrderSpec { wantsDurum = durum, drink = drink, fillings = new List<string>(fillings) };

        [Test]
        public void FillingBeforeLavas_IsRejected()
        {
            var plate = new DurumBuilder();
            Assert.AreEqual(BuildResult.NeedsBase, plate.AddFilling("tavuk"));
            Assert.AreEqual(BuildResult.NeedsBase, plate.Wrap());
        }

        [Test]
        public void NothingCanBeAddedAfterWrapping()
        {
            var plate = new DurumBuilder();
            plate.AddBase();
            plate.Wrap();
            Assert.AreEqual(BuildResult.AlreadyWrapped, plate.AddFilling("marul"));
        }

        [Test]
        public void FillingOrderDoesNotMatter()
        {
            var plate = new DurumBuilder();
            plate.AddBase();
            plate.AddFilling("domates");
            plate.AddFilling("tavuk");
            plate.Wrap();
            plate.SetDrink("kola");
            Assert.IsTrue(OrderEvaluator.Matches(Order(true, "kola", "tavuk", "domates"), plate));
        }

        [Test]
        public void UnwrappedDurum_DoesNotMatch()
        {
            var plate = new DurumBuilder();
            plate.AddBase();
            plate.AddFilling("tavuk");
            Assert.IsFalse(OrderEvaluator.Matches(Order(true, "", "tavuk"), plate));
        }

        [Test]
        public void ExtraFilling_DoesNotMatch()
        {
            var plate = new DurumBuilder();
            plate.AddBase();
            plate.AddFilling("tavuk");
            plate.AddFilling("acisos");
            plate.Wrap();
            Assert.IsFalse(OrderEvaluator.Matches(Order(true, "", "tavuk"), plate));
        }

        [Test]
        public void MissingOrWrongDrink_DoesNotMatch()
        {
            var plate = new DurumBuilder();
            plate.AddBase();
            plate.AddFilling("tavuk");
            plate.Wrap();
            Assert.IsFalse(OrderEvaluator.Matches(Order(true, "ayran", "tavuk"), plate));
            plate.SetDrink("kola");
            Assert.IsFalse(OrderEvaluator.Matches(Order(true, "ayran", "tavuk"), plate));
        }

        [Test]
        public void DrinkOnlyOrder_RejectsDurum()
        {
            var plate = new DurumBuilder();
            plate.SetDrink("ayran");
            Assert.IsTrue(OrderEvaluator.Matches(Order(false, "ayran"), plate));
            plate.AddBase();
            Assert.IsFalse(OrderEvaluator.Matches(Order(false, "ayran"), plate));
        }

        [Test]
        public void Serve_EmptyPlate_IsNoOp()
        {
            var state = new GameState();
            var result = ServiceResolver.Serve(state, Order(false, "ayran"), new DurumBuilder(), 30, true);
            Assert.AreEqual(ServeOutcome.Empty, result.outcome);
            Assert.AreEqual(0, state.money);
        }

        [Test]
        public void Serve_Wrong_NoMoneyNoChoice()
        {
            var state = new GameState();
            var plate = new DurumBuilder();
            plate.SetDrink("kola");
            var result = ServiceResolver.Serve(state, Order(false, "ayran"), plate, 30, true, "c",
                new[] { new CounterEffect(CounterType.Agreement, 1) });
            Assert.AreEqual(ServeOutcome.Wrong, result.outcome);
            Assert.AreEqual(0, state.money);
            Assert.AreEqual(0, state.agreement);
        }
    }

    public class NightClockTests
    {
        [Test]
        public void StartsAt2300_EndsAt0600()
        {
            var clock = new NightClock(420f);
            Assert.AreEqual("23:00", clock.FormatTime());
            Assert.AreEqual(420, clock.TotalGameMinutes);
            Assert.IsTrue(clock.Advance(420f));
            Assert.IsTrue(clock.IsFinished);
            Assert.AreEqual("06:00", clock.FormatTime());
        }

        [Test]
        public void CrossesMidnight()
        {
            var clock = new NightClock(420f);
            clock.Advance(75f); // 1 real second = 1 game minute here
            Assert.AreEqual("00:15", clock.FormatTime());
        }

        [Test]
        public void AdvanceReturnsTrueOnlyOnce()
        {
            var clock = new NightClock(10f);
            Assert.IsFalse(clock.Advance(5f));
            Assert.IsTrue(clock.Advance(6f));
            Assert.IsFalse(clock.Advance(1f));
        }

        [Test]
        public void ProgressAt_HandlesTimesAfterMidnight()
        {
            var clock = new NightClock(420f);
            Assert.AreEqual(0f, clock.ProgressAt("23:00"), 1e-4f);
            Assert.AreEqual(60f / 420f, clock.ProgressAt("00:00"), 1e-4f);
            Assert.AreEqual(1f, clock.ProgressAt("06:00"), 1e-4f);
        }

        [TestCase("25:00")]
        [TestCase("ab:cd")]
        [TestCase("")]
        public void TryParseTime_RejectsInvalid(string text)
        {
            Assert.IsFalse(NightClock.TryParseTime(text, out _));
        }
    }

    public class NightEventTests
    {
        [Test]
        public void Whisper_FiresOnceOnlyIfRadioOnInWindow()
        {
            var tracker = new RadioWhisperTracker(0.5f, 0.1f);
            Assert.IsFalse(tracker.Update(true, 0.4f), "henüz erken");
            Assert.IsFalse(tracker.Update(false, 0.55f), "radyo kapalı");
            Assert.IsTrue(tracker.Update(true, 0.56f));
            Assert.IsFalse(tracker.Update(true, 0.57f), "bir kez");
        }

        [Test]
        public void Whisper_MissedAfterWindow()
        {
            var tracker = new RadioWhisperTracker(0.5f, 0.1f);
            Assert.IsFalse(tracker.Update(true, 0.7f));
            Assert.IsTrue(tracker.IsMissed(0.7f));
        }

        [Test]
        public void CustomerQueue_WaitsForArrivalAndFreeCounter()
        {
            var queue = new CustomerQueue(new[] { 0.1f, 0.3f });
            Assert.AreEqual(-1, queue.TryDequeue(0.05f, false));
            Assert.AreEqual(0, queue.TryDequeue(0.1f, false));
            Assert.AreEqual(-1, queue.TryDequeue(0.35f, true), "tezgâh dolu");
            Assert.AreEqual(1, queue.TryDequeue(0.35f, false));
            Assert.IsTrue(queue.IsDone);
        }

        [Test]
        public void Tension_SpikeDecaysToBase()
        {
            var tension = new TensionModel(0.1f) { DecayPerSecond = 0.5f };
            tension.AddSpike(0.4f);
            Assert.AreEqual(0.5f, tension.Value, 1e-4f);
            tension.Tick(10f);
            Assert.AreEqual(0.1f, tension.Value, 1e-4f);
        }
    }
}
