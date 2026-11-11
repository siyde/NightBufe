using System.Linq;
using NightBufe.Core;
using NightBufe.Data;
using NightBufe.EditorTools;
using NUnit.Framework;

namespace NightBufe.Tests
{
    /// <summary>
    /// Builds the Night 1 data in memory (same code as the editor command) and checks
    /// it against the Night 1 table in DESIGN.md.
    /// </summary>
    public class Night1DataTests
    {
        private InMemoryAssetFactory _factory;
        private GameDatabase _db;
        private NightData _night;

        [SetUp]
        public void SetUp()
        {
            _factory = new InMemoryAssetFactory();
            _db = Night1DataBuilder.Build(_factory);
            _night = _db.GetNight(1);
        }

        [TearDown]
        public void TearDown() => _factory.DestroyAll();

        private ChoiceData FindChoice(string id)
        {
            var all = new[] { _night.signNameChoice, _night.flipPaperChoice, _night.radioWhisperChoice }
                .Concat(_night.customers.Select(c => c.servedChoice))
                .Concat(_night.customers.Select(c => c.refusedChoice))
                .Concat(_night.customers.SelectMany(c => c.talkOptions).Select(t => t.choice))
                .Where(c => c != null);
            return all.Single(c => c.id == id);
        }

        [Test]
        public void Night1_HasFiveCustomers_InArrivalOrder()
        {
            Assert.AreEqual(5, _night.customers.Count);
            var clock = new NightClock(_night.durationSeconds);
            var arrivals = _night.customers.Select(c => clock.ProgressAt(c.arrivalTime)).ToList();
            CollectionAssert.IsOrdered(arrivals);
            Assert.That(arrivals.Last(), Is.LessThan(1f));
            Assert.AreEqual("child", _night.customers.Last().id);
        }

        [Test]
        public void Night1_AllOrdersUseKnownIngredients()
        {
            foreach (var customer in _night.customers)
            {
                foreach (var filling in customer.order.fillings)
                    Assert.AreEqual(IngredientKind.Filling, _db.GetIngredient(filling)?.kind, $"{customer.id}: {filling}");
                if (!string.IsNullOrEmpty(customer.order.drink))
                    Assert.AreEqual(IngredientKind.Drink, _db.GetIngredient(customer.order.drink)?.kind, customer.id);
            }
        }

        [TestCase(Night1DataBuilder.SignName, CounterType.Agreement)]
        [TestCase(Night1DataBuilder.FlipPaper, CounterType.Curiosity)]
        [TestCase(Night1DataBuilder.RadioWhisper, CounterType.Reality)]
        [TestCase(Night1DataBuilder.TaxiChat, CounterType.Reality)]
        [TestCase(Night1DataBuilder.ChildFreeAyran, CounterType.ChildBond)]
        public void Night1_EachChoiceMatchesDesignTable(string choiceId, CounterType counter)
        {
            var choice = FindChoice(choiceId);
            Assert.AreEqual(1, choice.effects.Count);
            Assert.AreEqual(counter, choice.effects[0].counter);
            Assert.AreEqual(1, choice.effects[0].amount);

            var state = new GameState();
            CounterSystem.TryApplyChoice(state, choice.id, choice.effects);
            Assert.AreEqual(1, state.Get(counter));
        }

        [Test]
        public void Night1_AllChoices_GiveA1_M1_G2_C1()
        {
            var state = new GameState();
            foreach (var id in new[]
                     {
                         Night1DataBuilder.SignName, Night1DataBuilder.FlipPaper, Night1DataBuilder.RadioWhisper,
                         Night1DataBuilder.TaxiChat, Night1DataBuilder.ChildFreeAyran
                     })
            {
                var choice = FindChoice(id);
                CounterSystem.TryApplyChoice(state, choice.id, choice.effects);
                CounterSystem.TryApplyChoice(state, choice.id, choice.effects); // repeating does nothing
            }

            Assert.AreEqual(1, state.agreement);
            Assert.AreEqual(1, state.curiosity);
            Assert.AreEqual(2, state.reality);
            Assert.AreEqual(1, state.childBond);
        }

        [Test]
        public void Child_FreeAyran_GivesChildBondButNoMoney()
        {
            var child = _night.customers.Single(c => c.id == "child");
            var state = new GameState();
            var plate = new DurumBuilder();
            plate.SetDrink("ayran");

            var result = ServiceResolver.Serve(state, child.order, plate, child.price, child.paysMoney,
                child.servedChoice.id, child.servedChoice.effects);

            Assert.AreEqual(ServeOutcome.Correct, result.outcome);
            Assert.AreEqual(0, state.money);
            Assert.AreEqual(1, state.childBond);
        }

        [Test]
        public void Child_WrongDrink_NoChildBond()
        {
            var child = _night.customers.Single(c => c.id == "child");
            var state = new GameState();
            var plate = new DurumBuilder();
            plate.SetDrink("kola");

            var result = ServiceResolver.Serve(state, child.order, plate, child.price, child.paysMoney,
                child.servedChoice.id, child.servedChoice.effects);

            Assert.AreEqual(ServeOutcome.Wrong, result.outcome);
            Assert.AreEqual(0, state.childBond);
        }

        [Test]
        public void Child_RefusingDoesNotChangeCounters()
        {
            var child = _night.customers.Single(c => c.id == "child");
            var state = new GameState();
            ServiceResolver.Refuse(state, child.refusedChoice?.id, child.refusedChoice?.effects);
            Assert.AreEqual(0, state.childBond);
        }

        [Test]
        public void Student_CorrectOrder_PaysMoney()
        {
            var student = _night.customers.First();
            var state = new GameState();
            var plate = new DurumBuilder();
            plate.AddBase();
            plate.AddFilling("marul");
            plate.AddFilling("tavuk");
            plate.Wrap();
            plate.SetDrink("ayran");

            var result = ServiceResolver.Serve(state, student.order, plate, student.price, student.paysMoney);
            Assert.AreEqual(ServeOutcome.Correct, result.outcome);
            Assert.AreEqual(student.price, state.money);
        }

        [Test]
        public void BackRule_IsIllegibleUntilCuriosity4()
        {
            Assert.AreEqual(EndingResolver.LastRuleCuriosityThreshold, _night.backRule.curiosityToRead);
            Assert.AreEqual(4, _db.RulesUpTo(1).Count);
            Assert.AreSame(_night.backRule, _db.BackRuleFor(1));
        }

        [Test]
        public void ChildServedLine_UsesPlayerNameWhenSigned()
        {
            var child = _night.customers.Single(c => c.id == "child");
            Assert.AreEqual("Teşekkür ederim.", child.GetServedLine(new GameState()));
            Assert.AreEqual("Teşekkür ederim, Deniz.", child.GetServedLine(new GameState { playerName = "Deniz" }));
        }
    }
}
