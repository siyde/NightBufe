using System;
using NightBufe.Data;
using UnityEngine;

namespace NightBufe.Gameplay
{
    /// <summary>A clickable ingredient container (lavaş kabı, ayran şişesi...).</summary>
    [RequireComponent(typeof(WorldButton))]
    public class IngredientButton : MonoBehaviour
    {
        public IngredientData ingredient;

        public event Action<IngredientData> Picked;

        private void Awake() => GetComponent<WorldButton>().Clicked += () => Picked?.Invoke(ingredient);
    }
}
