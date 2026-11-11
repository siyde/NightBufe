using System;
using System.Collections.Generic;
using UnityEngine;

namespace NightBufe.Data
{
    [Serializable]
    public class LayoutSlot
    {
        [Tooltip("Sabit kimlik (ör. 'bin_lavas', 'radio', 'plate').")]
        public string id;
        [Tooltip("Arka plan görselinde piksel olarak: x, y = sol üst köşe; genişlik, yükseklik.")]
        public Rect pixelRect;

        [Header("Tıklanabilir")]
        [Tooltip("Doluysa tıklanınca bu malzeme eklenir.")]
        public string ingredientId;
        [Tooltip("Fare üstüne gelince görünen ad.")]
        public string hoverLabel;

        [Header("Yer tutucu (görsel yokken)")]
        [Tooltip("Arka plan görseli yokken bu alana çizilecek kutunun ArtLibrary anahtarı.")]
        public string placeholderKey;
        public bool placeholderCircle;
        public int sortingOrder;
    }

    /// <summary>
    /// Where everything sits on the counter, measured in pixels on the background image.
    /// When the art changes, update the rects here (no code change, no scene rebuild).
    /// </summary>
    [CreateAssetMenu(menuName = "Gece Büfesi/Tezgâh Yerleşimi", fileName = "CounterLayout")]
    public class CounterLayout : ScriptableObject
    {
        [Tooltip("Ölçümlerin yapıldığı görselin piksel boyutu.")]
        public Vector2 referenceSize = new Vector2(1536f, 1024f);
        public List<LayoutSlot> slots = new List<LayoutSlot>();

        public LayoutSlot Find(string id)
        {
            foreach (var slot in slots)
                if (slot != null && slot.id == id)
                    return slot;
            return null;
        }
    }
}
