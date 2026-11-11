using System.Collections.Generic;
using System.Text;
using NightBufe.Core;
using NightBufe.Data;
using TMPro;
using UnityEngine;

namespace NightBufe.Gameplay
{
    /// <summary>Shows what is on the serving plate, as placeholder shapes and as text.</summary>
    public class PlateView : MonoBehaviour
    {
        public GameDatabase database;
        public SpriteRenderer lavashLayer;
        public SpriteRenderer wrappedLayer;
        public SpriteRenderer drinkLayer;
        [Tooltip("İç malzemeler için küçük daireler (sırayla doldurulur).")]
        public List<SpriteRenderer> fillingDots = new List<SpriteRenderer>();
        public TextMeshProUGUI summaryText;

        public void Refresh(DurumBuilder plate)
        {
            bool hasBase = plate.HasBase;
            if (lavashLayer != null) lavashLayer.enabled = hasBase && !plate.IsWrapped;
            if (wrappedLayer != null) wrappedLayer.enabled = plate.IsWrapped;

            for (int i = 0; i < fillingDots.Count; i++)
            {
                var dot = fillingDots[i];
                if (dot == null) continue;
                bool show = i < plate.Fillings.Count && !plate.IsWrapped;
                dot.enabled = show;
                if (show) dot.color = ColorFor(plate.Fillings[i]);
            }

            if (drinkLayer != null)
            {
                drinkLayer.enabled = !string.IsNullOrEmpty(plate.Drink);
                if (drinkLayer.enabled) drinkLayer.color = ColorFor(plate.Drink);
            }

            if (summaryText != null) summaryText.text = Describe(plate);
        }

        public string Describe(DurumBuilder plate)
        {
            if (plate.IsEmpty) return "Tabak boş";

            var sb = new StringBuilder();
            if (plate.HasBase)
            {
                var baseIngredient = database != null ? database.GetFirstOfKind(IngredientKind.Base) : null;
                sb.Append(baseIngredient != null ? baseIngredient.displayName : "Lavaş");
                foreach (var filling in plate.Fillings)
                    sb.Append(" + ").Append(Name(filling));
                sb.Append(plate.IsWrapped ? "  (sarıldı)" : "  (açık)");
            }
            if (!string.IsNullOrEmpty(plate.Drink))
            {
                if (sb.Length > 0) sb.Append("  ·  ");
                sb.Append(Name(plate.Drink));
            }
            return sb.ToString();
        }

        private string Name(string id) => database != null ? database.IngredientName(id) : id;

        private Color ColorFor(string ingredientId)
        {
            if (database == null || database.art == null) return Color.white;
            var ingredient = database.GetIngredient(ingredientId);
            var entry = ingredient != null ? database.art.Find(ingredient.artKey) : null;
            return entry != null ? entry.placeholderColor : Color.white;
        }
    }
}
