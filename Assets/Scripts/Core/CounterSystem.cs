using System.Collections.Generic;

namespace NightBufe.Core
{
    /// <summary>Applies counter effects and one-shot choices to a GameState.</summary>
    public static class CounterSystem
    {
        public static void Apply(GameState state, CounterEffect effect)
        {
            int value = state.Get(effect.counter) + effect.amount;
            int max = CounterLimits.Max(effect.counter);
            if (value < CounterLimits.Min) value = CounterLimits.Min;
            if (value > max) value = max;
            state.Set(effect.counter, value);
        }

        public static void Apply(GameState state, IEnumerable<CounterEffect> effects)
        {
            if (effects == null) return;
            foreach (var effect in effects)
                Apply(state, effect);
        }

        /// <summary>
        /// Applies a choice's effects only the first time its id is seen.
        /// Returns false if the choice was already taken.
        /// </summary>
        public static bool TryApplyChoice(GameState state, string choiceId, IEnumerable<CounterEffect> effects)
        {
            if (string.IsNullOrEmpty(choiceId) || state.HasUsedChoice(choiceId))
                return false;

            state.usedChoices.Add(choiceId);
            Apply(state, effects);
            return true;
        }
    }
}
