using NightBufe.Data;
using UnityEngine;
using UnityEngine.UI;

namespace NightBufe.Gameplay
{
    /// <summary>
    /// Pulls this object's sprite/color from the ArtLibrary by key, so replacing
    /// art means editing one entry in ArtLibrary.asset. Works with SpriteRenderer or Image.
    /// </summary>
    [ExecuteAlways]
    public class ArtSlot : MonoBehaviour
    {
        public ArtLibrary library;
        public string artKey;
        [Tooltip("Sprite yoksa daire yer tutucu kullan.")]
        public bool circlePlaceholder;

        public Color BaseColor { get; private set; } = Color.white;

        private void OnEnable() => Apply();

#if UNITY_EDITOR
        private void OnValidate() => Apply();
#endif

        public void Apply()
        {
            if (library == null || string.IsNullOrEmpty(artKey)) return;
            library.Resolve(artKey, circlePlaceholder, out var sprite, out var color);
            BaseColor = color;

            if (TryGetComponent(out SpriteRenderer sr))
            {
                sr.sprite = sprite;
                sr.color = color;
            }
            else if (TryGetComponent(out Image image))
            {
                image.sprite = sprite;
                image.color = color;
            }
        }

        public void SetKey(string key)
        {
            artKey = key;
            Apply();
        }
    }
}
