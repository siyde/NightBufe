using System;
using System.Collections.Generic;
using NightBufe.Core;
using NightBufe.Data;
using UnityEngine;

namespace NightBufe.Gameplay
{
    /// <summary>
    /// Draws the counter background from ArtLibrary (if a sprite is assigned), fits it
    /// to the camera and converts layout slots (pixels on the image) to world rects.
    /// Without art, the layout is spread over the whole view and placeholder boxes are shown.
    /// </summary>
    [ExecuteAlways]
    [DefaultExecutionOrder(-100)]
    public class ArtBackdrop : MonoBehaviour
    {
        public ArtLibrary library;
        public string backgroundKey = "bg_counter";
        public CounterLayout layout;
        public SpriteRenderer background;
        [Tooltip("Açık: ekranı doldur, taşan kısmı kırp. Kapalı: tamamı görünsün, kenarlarda boşluk kalsın.")]
        public bool cover = true;
        public Camera targetCamera;
        [Tooltip("Arka plan görseli atanınca gizlenen yer tutucu nesneler.")]
        public List<GameObject> placeholderObjects = new List<GameObject>();

        public event Action LayoutChanged;

        public bool HasArt { get; private set; }
        public Sprite BackgroundSprite { get; private set; }
        public LayoutRect ImageWorldRect { get; private set; }

        private bool _computed;
        private float _lastAspect = -1f;

        private Camera Cam => targetCamera != null ? targetCamera : Camera.main;

        private void OnEnable() => Refresh();

        private void Update()
        {
            var cam = Cam;
            if (cam != null && !Mathf.Approximately(cam.aspect, _lastAspect))
                Refresh();
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            // Defer: SetActive is not allowed inside OnValidate.
            UnityEditor.EditorApplication.delayCall += () => { if (this != null) Refresh(); };
        }
#endif

        public void Refresh()
        {
            var cam = Cam;
            float viewHeight = cam != null && cam.orthographic ? cam.orthographicSize * 2f : 10f;
            float aspect = cam != null ? cam.aspect : 16f / 9f;
            float viewWidth = viewHeight * aspect;
            Vector2 center = cam != null ? (Vector2)cam.transform.position : Vector2.zero;
            _lastAspect = aspect;

            var entry = library != null ? library.Find(backgroundKey) : null;
            BackgroundSprite = entry != null ? entry.sprite : null;
            HasArt = BackgroundSprite != null;

            LayoutRect rect;
            if (HasArt)
            {
                var size = BackgroundSprite.rect.size;
                rect = LayoutMath.FitImage(size.x, size.y, viewWidth, viewHeight, cover);
            }
            else
            {
                rect = new LayoutRect(-viewWidth * 0.5f, -viewHeight * 0.5f, viewWidth, viewHeight);
            }
            rect.x += center.x;
            rect.y += center.y;
            ImageWorldRect = rect;
            _computed = true;

            if (background != null)
            {
                background.enabled = HasArt;
                if (HasArt)
                {
                    background.sprite = BackgroundSprite;
                    background.color = Color.white;
                    var bounds = BackgroundSprite.bounds.size;
                    background.transform.position = new Vector3(rect.CenterX, rect.CenterY, background.transform.position.z);
                    background.transform.localScale = new Vector3(rect.width / bounds.x, rect.height / bounds.y, 1f);
                }
            }

            foreach (var go in placeholderObjects)
                if (go != null && go.activeSelf == HasArt)
                    go.SetActive(!HasArt);

            LayoutChanged?.Invoke();
        }

        public Vector2 ReferenceSize
        {
            get
            {
                if (layout != null && layout.referenceSize.x > 0f && layout.referenceSize.y > 0f) return layout.referenceSize;
                return HasArt ? BackgroundSprite.rect.size : new Vector2(1920f, 1080f);
            }
        }

        public bool TryGetWorldRect(string slotId, out LayoutRect worldRect)
        {
            worldRect = default;
            if (!_computed) Refresh();
            var slot = layout != null ? layout.Find(slotId) : null;
            if (slot == null) return false;

            var px = slot.pixelRect;
            var reference = ReferenceSize;
            worldRect = LayoutMath.PixelRectToWorld(new LayoutRect(px.x, px.y, px.width, px.height),
                reference.x, reference.y, ImageWorldRect);
            return true;
        }
    }
}
