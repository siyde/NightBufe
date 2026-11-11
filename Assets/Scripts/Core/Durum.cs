using System;
using System.Collections.Generic;
using System.Linq;

namespace NightBufe.Core
{
    /// <summary>What a customer asks for. Ingredient ids match IngredientData ids.</summary>
    [Serializable]
    public class OrderSpec
    {
        /// <summary>True if the customer wants a wrapped dürüm.</summary>
        public bool wantsDurum;
        public List<string> fillings = new List<string>();
        /// <summary>Drink id, or empty for no drink.</summary>
        public string drink = "";

        public bool IsEmpty => !wantsDurum && string.IsNullOrEmpty(drink);
    }

    public enum BuildResult
    {
        Ok,
        NeedsBase,       // a filling or wrap without lavaş
        AlreadyHasBase,
        AlreadyWrapped,  // can't add after wrapping
        Duplicate,
        AlreadyHasDrink
    }

    /// <summary>
    /// The plate the player is preparing. Lavaş must come first; fillings are
    /// a set (order doesn't matter); "Sar" closes the dürüm; drink is separate.
    /// </summary>
    public class DurumBuilder
    {
        private readonly List<string> _fillings = new List<string>();

        public bool HasBase { get; private set; }
        public bool IsWrapped { get; private set; }
        public string Drink { get; private set; } = "";
        public IReadOnlyList<string> Fillings => _fillings;
        public bool IsEmpty => !HasBase && string.IsNullOrEmpty(Drink);

        public BuildResult AddBase()
        {
            if (HasBase) return BuildResult.AlreadyHasBase;
            HasBase = true;
            return BuildResult.Ok;
        }

        public BuildResult AddFilling(string id)
        {
            if (!HasBase) return BuildResult.NeedsBase;
            if (IsWrapped) return BuildResult.AlreadyWrapped;
            if (_fillings.Contains(id)) return BuildResult.Duplicate;
            _fillings.Add(id);
            return BuildResult.Ok;
        }

        public BuildResult Wrap()
        {
            if (!HasBase) return BuildResult.NeedsBase;
            if (IsWrapped) return BuildResult.AlreadyWrapped;
            IsWrapped = true;
            return BuildResult.Ok;
        }

        public BuildResult SetDrink(string id)
        {
            if (!string.IsNullOrEmpty(Drink)) return BuildResult.AlreadyHasDrink;
            Drink = id ?? "";
            return BuildResult.Ok;
        }

        public void Clear()
        {
            HasBase = false;
            IsWrapped = false;
            Drink = "";
            _fillings.Clear();
        }
    }

    public static class OrderEvaluator
    {
        /// <summary>True if the plate exactly matches the order.</summary>
        public static bool Matches(OrderSpec order, DurumBuilder plate)
        {
            if (order == null || plate == null) return false;

            if (order.wantsDurum)
            {
                if (!plate.HasBase || !plate.IsWrapped) return false;
                var wanted = new HashSet<string>(order.fillings ?? new List<string>());
                if (!wanted.SetEquals(plate.Fillings)) return false;
            }
            else if (plate.HasBase)
            {
                return false;
            }

            return string.Equals(order.drink ?? "", plate.Drink ?? "", StringComparison.Ordinal);
        }

        /// <summary>True if the plate contains the given drink, regardless of the rest.</summary>
        public static bool ContainsDrink(DurumBuilder plate, string drinkId) =>
            plate != null && !string.IsNullOrEmpty(drinkId) && plate.Drink == drinkId;

        public static int FillingCount(OrderSpec order) => order?.fillings?.Count(f => !string.IsNullOrEmpty(f)) ?? 0;
    }
}
