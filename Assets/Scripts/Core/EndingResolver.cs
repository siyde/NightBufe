using System.Collections.Generic;

namespace NightBufe.Core
{
    public enum EndingId
    {
        Regular,    // Müdavim - always available
        Loop,       // Döngü   - Ç >= 3
        Seal,       // Mühür   - M >= 5 and all 3 diary pages
        Awakening   // Uyanış  - G >= 5
    }

    /// <summary>Decides which final options open on night 5 (see DESIGN.md).</summary>
    public static class EndingResolver
    {
        public const int LoopChildBondThreshold = 3;
        public const int SealCuriosityThreshold = 5;
        public const int SealDiaryPagesRequired = 3;
        public const int AwakeningRealityThreshold = 5;
        public const int LastRuleCuriosityThreshold = 4;

        public static bool IsAvailable(GameState state, EndingId ending)
        {
            switch (ending)
            {
                case EndingId.Regular:
                    return true;
                case EndingId.Loop:
                    return state.childBond >= LoopChildBondThreshold;
                case EndingId.Seal:
                    return state.curiosity >= SealCuriosityThreshold
                           && CountValidDiaryPages(state) >= SealDiaryPagesRequired;
                case EndingId.Awakening:
                    return state.reality >= AwakeningRealityThreshold;
                default:
                    return false;
            }
        }

        /// <summary>All endings the player may pick on night 5, Regular first.</summary>
        public static List<EndingId> AvailableEndings(GameState state)
        {
            var result = new List<EndingId>();
            foreach (EndingId ending in System.Enum.GetValues(typeof(EndingId)))
                if (IsAvailable(state, ending))
                    result.Add(ending);
            return result;
        }

        /// <summary>Night 5, step 2: the faded last rule becomes readable.</summary>
        public static bool IsLastRuleReadable(GameState state) =>
            state.curiosity >= LastRuleCuriosityThreshold;

        private static int CountValidDiaryPages(GameState state)
        {
            var distinct = new HashSet<int>();
            foreach (int page in state.diaryPagesFound)
                if (page >= 1 && page <= SealDiaryPagesRequired)
                    distinct.Add(page);
            return distinct.Count;
        }
    }
}
