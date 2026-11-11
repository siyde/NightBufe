using System.Collections.Generic;
using UnityEngine;

namespace NightBufe.Data
{
    /// <summary>Root data asset: all nights, ingredients and the art library.</summary>
    [CreateAssetMenu(menuName = "Gece Büfesi/Oyun Veritabanı", fileName = "GameDatabase")]
    public class GameDatabase : ScriptableObject
    {
        public List<NightData> nights = new List<NightData>();
        public List<IngredientData> ingredients = new List<IngredientData>();
        public ArtLibrary art;

        public NightData GetNight(int number)
        {
            foreach (var night in nights)
                if (night != null && night.nightNumber == number)
                    return night;
            return null;
        }

        public IngredientData GetIngredient(string id)
        {
            foreach (var ingredient in ingredients)
                if (ingredient != null && ingredient.id == id)
                    return ingredient;
            return null;
        }

        public IngredientData GetFirstOfKind(IngredientKind kind)
        {
            foreach (var ingredient in ingredients)
                if (ingredient != null && ingredient.kind == kind)
                    return ingredient;
            return null;
        }

        public string IngredientName(string id)
        {
            var ingredient = GetIngredient(id);
            return ingredient != null ? ingredient.displayName : id;
        }

        /// <summary>All rules visible on the paper up to and including the given night.</summary>
        public List<RuleData> RulesUpTo(int nightNumber)
        {
            var rules = new List<RuleData>();
            foreach (var night in nights)
                if (night != null && night.nightNumber <= nightNumber)
                    rules.AddRange(night.newRules);
            return rules;
        }

        /// <summary>The most recent back-side rule defined up to this night.</summary>
        public RuleData BackRuleFor(int nightNumber)
        {
            RuleData result = null;
            int best = int.MinValue;
            foreach (var night in nights)
                if (night != null && night.backRule != null && night.nightNumber <= nightNumber && night.nightNumber > best)
                {
                    best = night.nightNumber;
                    result = night.backRule;
                }
            return result;
        }
    }
}
