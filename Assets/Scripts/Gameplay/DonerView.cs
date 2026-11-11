using NightBufe.Core;
using UnityEngine;

namespace NightBufe.Gameplay
{
    /// <summary>
    /// The döner spit. Night 1 anomaly flips it upside down for a moment.
    /// With placeholder art the placeholder spit rotates; with a painted background,
    /// the döner area is cut out of the background and drawn upside down on top of it.
    /// </summary>
    public class DonerView : MonoBehaviour
    {
        [Header("Yer tutucu")]
        public Transform spit;
        public float wobbleDegrees = 2f;
        public float wobbleSpeed = 1.3f;

        [Header("Boyalı arka plan")]
        public ArtBackdrop backdrop;
        public string slotId = "doner";
        [Tooltip("Arka plandan kesilen döner parçasını gösterir (normalde kapalı).")]
        public SpriteRenderer artOverlay;

        private float _flipTimer;
        private Sprite _cutout;

        public void Flip(float seconds) => _flipTimer = seconds;

        private void OnEnable()
        {
            if (backdrop != null) backdrop.LayoutChanged += InvalidateCutout;
            if (artOverlay != null) artOverlay.enabled = false;
        }

        private void OnDisable()
        {
            if (backdrop != null) backdrop.LayoutChanged -= InvalidateCutout;
        }

        private void Update()
        {
            bool flipping = _flipTimer > 0f;
            if (flipping) _flipTimer -= Time.deltaTime;

            if (backdrop != null && backdrop.HasArt)
            {
                UpdateArtOverlay(flipping);
                return;
            }

            if (spit == null) return;
            float z = Mathf.Sin(Time.time * wobbleSpeed) * wobbleDegrees + (flipping ? 180f : 0f);
            spit.localRotation = Quaternion.Euler(0f, 0f, z);
        }

        private void UpdateArtOverlay(bool flipping)
        {
            if (artOverlay == null) return;
            if (!flipping)
            {
                artOverlay.enabled = false;
                return;
            }

            if (_cutout == null) _cutout = BuildCutout();
            if (_cutout == null) return;

            artOverlay.sprite = _cutout;
            artOverlay.color = Color.white;
            artOverlay.transform.localRotation = Quaternion.Euler(0f, 0f, 180f);
            artOverlay.enabled = true;
        }

        private Sprite BuildCutout()
        {
            var source = backdrop.BackgroundSprite;
            var slot = backdrop.layout != null ? backdrop.layout.Find(slotId) : null;
            if (source == null || slot == null || !backdrop.TryGetWorldRect(slotId, out LayoutRect world)) return null;

            var reference = backdrop.ReferenceSize;
            var texRect = source.textureRect;
            float sx = texRect.width / reference.x;
            float sy = texRect.height / reference.y;
            var px = slot.pixelRect;

            // Image pixels are y-down; texture pixels are y-up.
            var cut = new Rect(texRect.x + px.x * sx, texRect.y + (reference.y - px.y - px.height) * sy,
                px.width * sx, px.height * sy);
            float lossyScale = Mathf.Abs(artOverlay.transform.lossyScale.x);
            float pixelsPerUnit = cut.width / (world.width / Mathf.Max(lossyScale, 1e-4f));

            // The overlay sits at the slot center (this object is anchored with pivot 0.5, 0.5).
            artOverlay.transform.localPosition = Vector3.zero;
            return Sprite.Create(source.texture, cut, new Vector2(0.5f, 0.5f), pixelsPerUnit);
        }

        private void InvalidateCutout()
        {
            if (_cutout != null)
            {
                if (Application.isPlaying) Destroy(_cutout);
                else DestroyImmediate(_cutout);
            }
            _cutout = null;
        }
    }
}
