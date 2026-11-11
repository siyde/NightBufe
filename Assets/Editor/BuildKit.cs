using NightBufe.Data;
using NightBufe.Gameplay;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace NightBufe.EditorTools
{
    /// <summary>Small helpers that create placeholder world objects and uGUI elements.</summary>
    internal class BuildKit
    {
        public readonly ArtLibrary Art;
        public readonly TMP_FontAsset Font;
        public readonly Material SpriteLit;

        public BuildKit(ArtLibrary art, TMP_FontAsset font)
        {
            Art = art;
            Font = font;
            SpriteLit = AssetDatabase.LoadAssetAtPath<Material>(EditorPaths.SpriteLitMaterial);
        }

        public Color Ink => new Color(0.15f, 0.12f, 0.1f);

        // ---------------- World ----------------

        public static GameObject Empty(string name, Transform parent, Vector2 position)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = position;
            return go;
        }

        /// <summary>A placeholder sprite whose look comes from ArtLibrary[key]. Size is applied via scale.</summary>
        public SpriteRenderer Sprite(string name, Transform parent, string key, Vector2 position, Vector2 size,
            int order, bool circle = false)
        {
            var go = Empty(name, parent, position);
            go.transform.localScale = new Vector3(size.x, size.y, 1f);
            var sr = go.AddComponent<SpriteRenderer>();
            if (SpriteLit != null) sr.sharedMaterial = SpriteLit;
            sr.sortingOrder = order;
            var slot = go.AddComponent<ArtSlot>();
            slot.library = Art;
            slot.artKey = key;
            slot.circlePlaceholder = circle;
            slot.Apply();
            return sr;
        }

        /// <summary>A sprite whose color is driven by code (no ArtSlot), e.g. LEDs and plate contents.</summary>
        public SpriteRenderer PlainSprite(string name, Transform parent, Vector2 position, Vector2 size, int order,
            Color color, bool circle = false)
        {
            var go = Empty(name, parent, position);
            go.transform.localScale = new Vector3(size.x, size.y, 1f);
            var sr = go.AddComponent<SpriteRenderer>();
            if (SpriteLit != null) sr.sharedMaterial = SpriteLit;
            sr.sprite = circle ? Art.placeholderCircle : Art.placeholderSquare;
            sr.color = color;
            sr.sortingOrder = order;
            return sr;
        }

        public TextMeshPro WorldLabel(string text, Transform parent, Vector2 position, float fontSize = 2.4f,
            int order = 50, Color? color = null)
        {
            var go = Empty("Label", parent, position);
            var tmp = go.AddComponent<TextMeshPro>();
            tmp.font = Font;
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = color ?? new Color(0.97f, 0.93f, 0.85f);
            tmp.textWrappingMode = TextWrappingModes.NoWrap;
            tmp.rectTransform.sizeDelta = new Vector2(3f, 0.6f);
            tmp.sortingOrder = order;
            return tmp;
        }

        /// <summary>Adds a collider of the given size and a WorldButton.</summary>
        public static WorldButton MakeClickable(GameObject go, Vector2 size, Vector2 offset = default)
        {
            var box = go.AddComponent<BoxCollider2D>();
            box.size = size;
            box.offset = offset;
            return go.AddComponent<WorldButton>();
        }

        // ---------------- UI ----------------

        public static Canvas Canvas(string name, int sortOrder = 0)
        {
            var go = new GameObject(name, typeof(RectTransform));
            var canvas = go.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = sortOrder;
            var scaler = go.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;
            go.AddComponent<GraphicRaycaster>();
            return canvas;
        }

        public static void EventSystem()
        {
            var go = new GameObject("EventSystem");
            go.AddComponent<EventSystem>();
            var module = go.AddComponent<InputSystemUIInputModule>();
            module.AssignDefaultActions();
        }

        public static RectTransform Rect(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax,
            Vector2 anchoredPosition, Vector2 sizeDelta, Vector2? pivot = null)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rt = (RectTransform)go.transform;
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.pivot = pivot ?? new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = anchoredPosition;
            rt.sizeDelta = sizeDelta;
            return rt;
        }

        /// <summary>Rect anchored at a single point (anchor = pivot).</summary>
        public static RectTransform At(string name, Transform parent, Vector2 anchor, Vector2 position, Vector2 size) =>
            Rect(name, parent, anchor, anchor, position, size, anchor);

        public static RectTransform Stretch(string name, Transform parent, float inset = 0f) =>
            Rect(name, parent, Vector2.zero, Vector2.one, Vector2.zero, new Vector2(-inset * 2f, -inset * 2f));

        public static Image Panel(RectTransform rt, Color color, bool raycast = true)
        {
            var image = rt.gameObject.AddComponent<Image>();
            image.color = color;
            image.raycastTarget = raycast;
            return image;
        }

        /// <summary>Image whose look comes from ArtLibrary[key].</summary>
        public Image ArtPanel(RectTransform rt, string key)
        {
            var image = rt.gameObject.AddComponent<Image>();
            var slot = rt.gameObject.AddComponent<ArtSlot>();
            slot.library = Art;
            slot.artKey = key;
            slot.Apply();
            return image;
        }

        public TextMeshProUGUI Text(RectTransform rt, string text, float size, Color color,
            TextAlignmentOptions align = TextAlignmentOptions.Center, bool raycast = false)
        {
            var tmp = rt.gameObject.AddComponent<TextMeshProUGUI>();
            tmp.font = Font;
            tmp.text = text;
            tmp.fontSize = size;
            tmp.color = color;
            tmp.alignment = align;
            tmp.raycastTarget = raycast;
            tmp.textWrappingMode = TextWrappingModes.Normal;
            return tmp;
        }

        public TextMeshProUGUI TextAt(string name, Transform parent, Vector2 anchor, Vector2 position, Vector2 size,
            string text, float fontSize, Color color, TextAlignmentOptions align = TextAlignmentOptions.Center) =>
            Text(At(name, parent, anchor, position, size), text, fontSize, color, align);

        public Button Button(string name, Transform parent, Vector2 anchor, Vector2 position, Vector2 size,
            string label, float fontSize = 30f, Color? background = null, Color? textColor = null)
        {
            var rt = At(name, parent, anchor, position, size);
            var image = Panel(rt, background ?? Art.uiAccent);
            var button = rt.gameObject.AddComponent<Button>();
            button.targetGraphic = image;
            var colors = button.colors;
            colors.highlightedColor = new Color(1f, 1f, 1f, 1f);
            colors.normalColor = new Color(0.9f, 0.9f, 0.9f, 1f);
            colors.pressedColor = new Color(0.7f, 0.7f, 0.7f, 1f);
            colors.disabledColor = new Color(0.45f, 0.45f, 0.45f, 0.6f);
            button.colors = colors;

            var labelRt = Stretch("Label", rt);
            Text(labelRt, label, fontSize, textColor ?? new Color(0.1f, 0.08f, 0.06f));
            return button;
        }

        public TMP_InputField InputField(string name, Transform parent, Vector2 anchor, Vector2 position, Vector2 size,
            string placeholder)
        {
            var rt = At(name, parent, anchor, position, size);
            var bg = Panel(rt, new Color(1f, 1f, 1f, 0.55f));
            var field = rt.gameObject.AddComponent<TMP_InputField>();
            field.targetGraphic = bg;

            var area = Stretch("Text Area", rt, 8f);
            area.gameObject.AddComponent<RectMask2D>();

            var ph = Text(Stretch("Placeholder", area), placeholder, 28f, new Color(0.3f, 0.25f, 0.2f, 0.55f),
                TextAlignmentOptions.Left);
            ph.fontStyle = FontStyles.Italic;
            ph.textWrappingMode = TextWrappingModes.NoWrap;

            var txt = Text(Stretch("Text", area), "", 28f, Ink, TextAlignmentOptions.Left);
            txt.textWrappingMode = TextWrappingModes.NoWrap;

            field.textViewport = area;
            field.textComponent = txt;
            field.placeholder = ph;
            field.fontAsset = Font;
            field.pointSize = 28f;
            field.characterLimit = 20;
            return field;
        }

        public static CanvasGroup Group(GameObject go, bool blocksRaycasts = false)
        {
            var group = go.AddComponent<CanvasGroup>();
            group.blocksRaycasts = blocksRaycasts;
            group.interactable = blocksRaycasts;
            return group;
        }
    }
}
