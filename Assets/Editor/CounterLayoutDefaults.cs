using System.Collections.Generic;
using System.IO;
using NightBufe.Data;
using UnityEditor;
using UnityEngine;

namespace NightBufe.EditorTools
{
    /// <summary>
    /// Default counter layout, measured in pixels on the concept image
    /// (Assets/Art/Backgrounds/counter.png, 1536x1024). Existing slots in
    /// CounterLayout.asset are never overwritten, so hand-tuned rects survive rebuilds.
    /// </summary>
    public static class CounterLayoutDefaults
    {
        public const string BackgroundKey = "bg_counter";

        private static LayoutSlot S(string id, float x, float y, float w, float h, string placeholderKey = null,
            int order = 0, string ingredient = null, string hover = null, bool circle = false) =>
            new LayoutSlot
            {
                id = id,
                pixelRect = new Rect(x, y, w, h),
                placeholderKey = placeholderKey,
                sortingOrder = order,
                ingredientId = ingredient,
                hoverLabel = hover,
                placeholderCircle = circle
            };

        public static List<LayoutSlot> Slots() => new List<LayoutSlot>
        {
            // ---- Sadece yer tutucu (boyalı görselde zaten var) ----
            S("street", 250, 150, 850, 450, "bg_sky", -100),
            S("building", 790, 170, 310, 300, "bg_building", -95),
            S("road", 250, 470, 850, 130, "bg_street", -94),
            S("taxi", 680, 365, 210, 140, "taxi", -80),
            S("wall_top", 0, 0, 1536, 150, "booth_frame", -90),
            S("wall_left", 0, 150, 250, 450, "booth_frame", -90),
            S("wall_right", 1100, 150, 436, 450, "booth_frame", -90),
            S("shelf", 1150, 385, 386, 25, "drink_rack", -50),
            S("counter_top", 0, 595, 1536, 55, "counter_top", 0),
            S("workspace", 0, 650, 1536, 374, "workspace", -10),

            // ---- Döner (anomali) ----
            S("doner", 0, 205, 150, 280, "doner_meat", 5),

            // ---- Malzemeler (tıklanabilir) ----
            S("bin_lavas", 0, 650, 265, 255, "ing_lavas", 20, "lavas", "Lavaş"),
            S("grill_tavuk", 268, 660, 382, 110, "grill", 15, "tavuk", "Tavuk"),
            S("bin_tavuk", 690, 658, 200, 82, "ing_tavuk", 20, "tavuk", "Tavuk"),
            S("bin_marul_top", 690, 748, 200, 58, "ing_marul", 20, "marul", "Marul"),
            S("bin_marul", 283, 812, 212, 100, "ing_marul", 20, "marul", "Marul"),
            S("bin_acisos", 500, 812, 195, 100, "ing_acisos", 20, "acisos", "Acı sos"),
            S("bin_domates", 705, 812, 215, 100, "ing_domates", 20, "domates", "Domates"),
            S("bottle_acisos", 893, 515, 77, 185, "ing_acisos", 20, "acisos", "Acı sos"),
            S("bottle_ayran", 1312, 285, 50, 112, "drink_ayran", -40, "ayran", "Ayran"),
            S("bottle_kola", 1418, 282, 58, 135, "drink_kola", -40, "kola", "Kola"),

            // ---- Radyo (raftaki kırmızı kutu) ----
            S("radio", 1158, 338, 100, 50, "radio", -40, null, "Radyo"),
            S("radio_led", 1236, 344, 14, 14),
            S("radio_static", 1110, 290, 200, 40),

            // ---- Görselde olmayan, her zaman çizilenler ----
            S("rule_paper", 975, 885, 150, 55, null, 30, null, "Kural kâğıdı"),
            S("customer", 440, 280, 280, 320),
            S("plate", 955, 690, 435, 190, "plate", 18),
            S("balloon", 190, 195, 340, 190),
            S("plate_summary", 955, 655, 435, 30),
        };

        public static CounterLayout Ensure()
        {
            var layout = AssetDatabase.LoadAssetAtPath<CounterLayout>(EditorPaths.CounterLayout);
            if (layout == null)
            {
                Directory.CreateDirectory(Path.GetDirectoryName(EditorPaths.CounterLayout));
                layout = ScriptableObject.CreateInstance<CounterLayout>();
                layout.referenceSize = new Vector2(1536f, 1024f);
                AssetDatabase.CreateAsset(layout, EditorPaths.CounterLayout);
            }

            foreach (var slot in Slots())
                if (layout.Find(slot.id) == null)
                    layout.slots.Add(slot);

            EditorUtility.SetDirty(layout);
            return layout;
        }

        /// <summary>
        /// Imports Assets/Art/Backgrounds/counter.png as a sprite. Call this BEFORE loading
        /// other assets: a reimport can reload (and invalidate) objects already in memory.
        /// </summary>
        public static void EnsureBackgroundImported()
        {
            if (!File.Exists(EditorPaths.CounterBackground)) return;

            var importer = (TextureImporter)AssetImporter.GetAtPath(EditorPaths.CounterBackground);
            if (importer != null && importer.textureType != TextureImporterType.Sprite)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.spritePixelsPerUnit = 100f;
                importer.mipmapEnabled = false;
                importer.maxTextureSize = 4096;
                importer.textureCompression = TextureImporterCompression.CompressedHQ;
                importer.SaveAndReimport();
            }
        }

        /// <summary>Puts the counter background into ArtLibrary["bg_counter"] if that entry has no sprite yet.</summary>
        public static void AssignBackground(ArtLibrary art)
        {
            if (art == null || !File.Exists(EditorPaths.CounterBackground)) return;

            var entry = art.Find(BackgroundKey);
            if (entry == null)
            {
                entry = new ArtEntry { key = BackgroundKey, placeholderColor = Color.black };
                art.entries.Add(entry);
            }
            if (entry.sprite == null)
                entry.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(EditorPaths.CounterBackground);
            EditorUtility.SetDirty(art);
        }
    }
}
