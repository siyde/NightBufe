using System.Collections.Generic;
using System.IO;
using NightBufe.Data;
using UnityEditor;
using UnityEngine;

namespace NightBufe.EditorTools
{
    /// <summary>
    /// Placeholder sprites (white square / circle) and the default ArtLibrary keys
    /// with their placeholder colors. Existing entries (and assigned sprites) are kept.
    /// </summary>
    public static class PlaceholderArt
    {
        public static readonly (string key, Color color)[] DefaultEntries =
        {
            // Sokak / arka plan
            ("bg_sky", new Color(0.05f, 0.07f, 0.18f)),
            ("bg_building", new Color(0.13f, 0.13f, 0.22f)),
            ("bg_window_lit", new Color(1f, 0.84f, 0.45f)),
            ("bg_window_dark", new Color(0.07f, 0.08f, 0.13f)),
            ("bg_street", new Color(0.11f, 0.11f, 0.15f)),
            ("bg_streetlight", new Color(1f, 0.9f, 0.6f)),
            ("taxi", new Color(0.95f, 0.76f, 0.12f)),
            ("taxi_wheel", new Color(0.04f, 0.04f, 0.05f)),
            // Büfe
            ("booth_frame", new Color(0.24f, 0.2f, 0.18f)),
            ("awning", new Color(0.62f, 0.12f, 0.12f)),
            ("counter_top", new Color(0.5f, 0.32f, 0.17f)),
            ("workspace", new Color(0.32f, 0.2f, 0.12f)),
            ("doner_rod", new Color(0.72f, 0.72f, 0.75f)),
            ("doner_meat", new Color(0.58f, 0.34f, 0.15f)),
            ("grill", new Color(0.2f, 0.2f, 0.22f)),
            ("container", new Color(0.72f, 0.74f, 0.78f)),
            ("drink_rack", new Color(0.3f, 0.22f, 0.15f)),
            ("plate", new Color(0.92f, 0.92f, 0.94f)),
            ("durum_wrapped", new Color(0.88f, 0.74f, 0.5f)),
            ("wrap_board", new Color(0.62f, 0.46f, 0.3f)),
            ("trash", new Color(0.28f, 0.3f, 0.3f)),
            ("radio", new Color(0.36f, 0.26f, 0.18f)),
            ("rule_paper", new Color(0.93f, 0.9f, 0.8f)),
            // Malzemeler
            ("ing_lavas", new Color(0.94f, 0.86f, 0.66f)),
            ("ing_tavuk", new Color(0.82f, 0.56f, 0.3f)),
            ("ing_marul", new Color(0.36f, 0.76f, 0.3f)),
            ("ing_domates", new Color(0.86f, 0.2f, 0.15f)),
            ("ing_acisos", new Color(0.55f, 0.04f, 0.04f)),
            ("drink_ayran", new Color(0.97f, 0.97f, 0.95f)),
            ("drink_kola", new Color(0.28f, 0.09f, 0.05f)),
            // Müşteriler
            ("customer_student", new Color(0.26f, 0.42f, 0.72f)),
            ("customer_nurse", new Color(0.62f, 0.8f, 0.86f)),
            ("customer_taxi", new Color(0.38f, 0.32f, 0.26f)),
            ("customer_watchman", new Color(0.22f, 0.28f, 0.24f)),
            ("customer_child", new Color(0.82f, 0.1f, 0.1f)),
            // Arayüz
            ("ui_balloon", new Color(0.97f, 0.95f, 0.9f)),
        };

        public static Sprite EnsureSquare() => EnsureSprite(EditorPaths.SquareSprite, 32, false);
        public static Sprite EnsureCircle() => EnsureSprite(EditorPaths.CircleSprite, 64, true);

        private static Sprite EnsureSprite(string path, int size, bool circle)
        {
            if (!File.Exists(path))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(path));
                var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
                float r = size * 0.5f;
                var pixels = new Color32[size * size];
                for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                {
                    byte a = 255;
                    if (circle)
                    {
                        float dx = x + 0.5f - r, dy = y + 0.5f - r;
                        float d = Mathf.Sqrt(dx * dx + dy * dy);
                        a = (byte)(Mathf.Clamp01(r - d) * 255f);
                    }
                    pixels[y * size + x] = new Color32(255, 255, 255, a);
                }
                tex.SetPixels32(pixels);
                File.WriteAllBytes(path, tex.EncodeToPNG());
                Object.DestroyImmediate(tex);
                AssetDatabase.ImportAsset(path);
            }

            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            if (importer != null && (importer.textureType != TextureImporterType.Sprite || importer.spritePixelsPerUnit != size))
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.spritePixelsPerUnit = size;
                importer.mipmapEnabled = false;
                importer.alphaIsTransparency = true;
                importer.filterMode = FilterMode.Bilinear;
                importer.SaveAndReimport();
            }

            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }

        /// <summary>Fills missing keys; never overwrites a sprite or color the user already set.</summary>
        public static void FillLibrary(ArtLibrary library, Sprite square, Sprite circle)
        {
            if (square != null) library.placeholderSquare = square;
            if (circle != null) library.placeholderCircle = circle;
            library.entries ??= new List<ArtEntry>();
            foreach (var (key, color) in DefaultEntries)
                if (library.Find(key) == null)
                    library.entries.Add(new ArtEntry { key = key, placeholderColor = color });
        }
    }
}
