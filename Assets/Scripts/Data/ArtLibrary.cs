using System;
using System.Collections.Generic;
using UnityEngine;

namespace NightBufe.Data
{
    [Serializable]
    public class ArtEntry
    {
        public string key;
        [Tooltip("Boşsa yer tutucu kutu + renk kullanılır.")]
        public Sprite sprite;
        public Color placeholderColor = Color.magenta;
        [Tooltip("Sprite atanınca rengi beyaza çek (görselin kendi renkleri görünsün).")]
        public bool tintSprite;
    }

    /// <summary>
    /// The single place where every visual is assigned. Drop a sprite from
    /// Assets/Art/ into an entry and every object using that key updates.
    /// </summary>
    [CreateAssetMenu(menuName = "Gece Büfesi/Görsel Kütüphanesi", fileName = "ArtLibrary")]
    public class ArtLibrary : ScriptableObject
    {
        [Tooltip("Yer tutucu için düz beyaz kare.")]
        public Sprite placeholderSquare;
        public Sprite placeholderCircle;

        [Header("Palet")]
        public Color lightWarm = new Color(1f, 0.82f, 0.5f);
        public Color lightTense = new Color(0.62f, 0.9f, 0.55f);
        public Color nightBlue = new Color(0.06f, 0.08f, 0.2f);
        public Color uiPanel = new Color(0.08f, 0.09f, 0.16f, 0.92f);
        public Color uiAccent = new Color(1f, 0.78f, 0.35f);
        public Color uiText = new Color(0.97f, 0.93f, 0.85f);

        public List<ArtEntry> entries = new List<ArtEntry>();

        public ArtEntry Find(string key)
        {
            foreach (var entry in entries)
                if (entry != null && entry.key == key)
                    return entry;
            return null;
        }

        /// <summary>Resolves a key to a sprite and color. Falls back to the placeholder square.</summary>
        public void Resolve(string key, bool circle, out Sprite sprite, out Color color)
        {
            var entry = Find(key);
            if (entry != null && entry.sprite != null)
            {
                sprite = entry.sprite;
                color = entry.tintSprite ? entry.placeholderColor : Color.white;
                return;
            }

            sprite = circle && placeholderCircle != null ? placeholderCircle : placeholderSquare;
            color = entry != null ? entry.placeholderColor : Color.magenta;
        }
    }
}
