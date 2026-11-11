using System.Collections.Generic;

namespace NightBufe.Core
{
    public enum ServeOutcome
    {
        Empty,   // nothing on the plate; nothing happens
        Correct,
        Wrong
    }

    public struct ServeResult
    {
        public ServeOutcome outcome;
        public int moneyEarned;
        public bool choiceApplied;
    }

    /// <summary>Serving / refusing a customer, independent of any MonoBehaviour.</summary>
    public static class ServiceResolver
    {
        /// <summary>
        /// Correct order: pay (if the customer pays) and apply the "served" choice.
        /// Wrong order: no money, no choice. Empty plate: no-op.
        /// </summary>
        public static ServeResult Serve(GameState state, OrderSpec order, DurumBuilder plate, int price, bool paysMoney,
            string servedChoiceId = null, IEnumerable<CounterEffect> servedEffects = null)
        {
            var result = new ServeResult();
            if (plate == null || plate.IsEmpty)
            {
                result.outcome = ServeOutcome.Empty;
                return result;
            }

            if (!OrderEvaluator.Matches(order, plate))
            {
                result.outcome = ServeOutcome.Wrong;
                return result;
            }

            result.outcome = ServeOutcome.Correct;
            if (paysMoney && price > 0)
            {
                state.money += price;
                result.moneyEarned = price;
            }
            result.choiceApplied = CounterSystem.TryApplyChoice(state, servedChoiceId, servedEffects);
            return result;
        }

        /// <summary>"Kapalıyız": applies the refusal choice, if any.</summary>
        public static bool Refuse(GameState state, string refusedChoiceId = null, IEnumerable<CounterEffect> refusedEffects = null) =>
            CounterSystem.TryApplyChoice(state, refusedChoiceId, refusedEffects);
    }
}
