using UnityEngine;

namespace NightBufe.Data
{
    public enum IngredientKind
    {
        Base,     // lavaş: must come first
        Filling,  // tavuk, marul, domates, acı sos
        Drink     // ayran, kola
    }

    [CreateAssetMenu(menuName = "Gece Büfesi/Malzeme", fileName = "Ingredient")]
    public class IngredientData : ScriptableObject
    {
        [Tooltip("Siparişlerde kullanılan sabit kimlik (ör. 'tavuk').")]
        public string id;
        [Tooltip("Oyunda görünen ad.")]
        public string displayName;
        public IngredientKind kind;
        [Tooltip("ArtLibrary anahtarı.")]
        public string artKey;
    }
}
