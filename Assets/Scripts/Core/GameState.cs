using System;
using System.Collections.Generic;

namespace NightBufe.Core
{
    /// <summary>
    /// The whole persistent state of a run. Plain C#, serialized with JsonUtility,
    /// so only public fields of supported types are used.
    /// </summary>
    [Serializable]
    public class GameState
    {
        public const int FinalNight = 5;
        public const int SaveVersion = 1;

        public int version = SaveVersion;
        public int night = 1;
        /// <summary>0..1 progress of the current night (23:00 → ezan).</summary>
        public float nightProgress;
        public int money;

        public int agreement;
        public int curiosity;
        public int reality;
        public int childBond;

        /// <summary>Diary page numbers (1..3) found in the storage room.</summary>
        public List<int> diaryPagesFound = new List<int>();
        /// <summary>Ids of one-shot choices already taken this run.</summary>
        public List<string> usedChoices = new List<string>();
        /// <summary>Name written under the rule paper. Empty if never signed.</summary>
        public string playerName = "";

        public int Get(CounterType type)
        {
            switch (type)
            {
                case CounterType.Agreement: return agreement;
                case CounterType.Curiosity: return curiosity;
                case CounterType.Reality: return reality;
                case CounterType.ChildBond: return childBond;
                default: throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }

        public void Set(CounterType type, int value)
        {
            switch (type)
            {
                case CounterType.Agreement: agreement = value; break;
                case CounterType.Curiosity: curiosity = value; break;
                case CounterType.Reality: reality = value; break;
                case CounterType.ChildBond: childBond = value; break;
                default: throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }

        public bool HasUsedChoice(string choiceId) => usedChoices.Contains(choiceId);

        public bool HasSigned => !string.IsNullOrEmpty(playerName);

        public void AddDiaryPage(int page)
        {
            if (!diaryPagesFound.Contains(page))
                diaryPagesFound.Add(page);
        }

        /// <summary>Called when the night ends (ezan). Moves to the next night.</summary>
        public void AdvanceNight()
        {
            night++;
            nightProgress = 0f;
        }
    }
}
