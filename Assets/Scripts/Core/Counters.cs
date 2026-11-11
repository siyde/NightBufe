using System;

namespace NightBufe.Core
{
    /// <summary>The four hidden counters. Never shown to the player outside dev mode.</summary>
    public enum CounterType
    {
        Agreement,  // A - Anlaşma
        Curiosity,  // M - Merak
        Reality,    // G - Gerçeklik
        ChildBond   // Ç - Çocukla bağ
    }

    /// <summary>A single change to one counter, e.g. A +1 or G -1.</summary>
    [Serializable]
    public struct CounterEffect
    {
        public CounterType counter;
        public int amount;

        public CounterEffect(CounterType counter, int amount)
        {
            this.counter = counter;
            this.amount = amount;
        }

        public override string ToString() => $"{CounterLimits.ShortName(counter)} {(amount >= 0 ? "+" : "")}{amount}";
    }

    public static class CounterLimits
    {
        public const int Min = 0;

        public static int Max(CounterType type)
        {
            switch (type)
            {
                case CounterType.Agreement: return 7;
                case CounterType.Curiosity: return 6;
                case CounterType.Reality: return 7;
                case CounterType.ChildBond: return 4;
                default: throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }

        public static string ShortName(CounterType type)
        {
            switch (type)
            {
                case CounterType.Agreement: return "A";
                case CounterType.Curiosity: return "M";
                case CounterType.Reality: return "G";
                case CounterType.ChildBond: return "Ç";
                default: return "?";
            }
        }
    }
}
