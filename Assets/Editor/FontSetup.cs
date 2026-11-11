using System;
using System.IO;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.TextCore.LowLevel;

namespace NightBufe.EditorTools
{
    /// <summary>
    /// Imports TMP Essential Resources (if missing) and creates a dynamic SDF font
    /// asset from LiberationSans (OFL). Glyphs (incl. ç ğ ı İ ö ş ü) are rendered from
    /// the source font on demand; TryAddCharacters verifies the font covers them.
    /// It becomes the TMP default font and is assigned to every generated text.
    /// </summary>
    public static class FontSetup
    {
        public const string TurkishCharacters = "çğıöşüÇĞİÖŞÜâîûÂÎÛ";
        private const string BasicCharacters =
            " !\"#$%&'()*+,-./0123456789:;<=>?@ABCDEFGHIJKLMNOPQRSTUVWXYZ[\\]^_`abcdefghijklmnopqrstuvwxyz{|}~–—’“”…·•";

        private static Action _onReady;
        private static bool _importPending;

        [MenuItem(EditorPaths.Menu + "1. TMP ve Türkçe Font Kur", priority = 1)]
        public static void Run() => EnsureFont(null);

        public static bool IsFontReady => AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(EditorPaths.TurkishFont) != null;

        public static TMP_FontAsset LoadFont() => AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(EditorPaths.TurkishFont);

        /// <summary>Makes sure the font exists. onReady runs once it does (maybe after an async package import).</summary>
        public static void EnsureFont(Action onReady)
        {
            if (!File.Exists(EditorPaths.TmpSettings) || !File.Exists(EditorPaths.TmpSourceFont))
            {
                ImportEssentials(onReady);
                return;
            }

            CreateFontAsset();
            onReady?.Invoke();
        }

        private static void ImportEssentials(Action onReady)
        {
            var info = UnityEditor.PackageManager.PackageInfo.FindForAssetPath("Packages/com.unity.ugui");
            if (info == null)
            {
                Debug.LogError("[Gece Büfesi] com.unity.ugui paketi bulunamadı; TMP Essential Resources içe aktarılamadı.");
                return;
            }

            string package = Path.Combine(info.resolvedPath, "Package Resources", "TMP Essential Resources.unitypackage");
            if (!File.Exists(package))
            {
                Debug.LogError($"[Gece Büfesi] Paket bulunamadı: {package}\nWindow > TextMeshPro > Import TMP Essential Resources ile elle içe aktar ve komutu tekrar çalıştır.");
                return;
            }

            _onReady = onReady;
            _importPending = true;
            AssetDatabase.importPackageCompleted += OnImported;
            AssetDatabase.importPackageFailed += OnImportFailed;
            Debug.Log("[Gece Büfesi] TMP Essential Resources içe aktarılıyor...");
            AssetDatabase.ImportPackage(package, false);

            // In batch mode the import can finish synchronously; continue right away then.
            if (_importPending && File.Exists(EditorPaths.TmpSettings) && File.Exists(EditorPaths.TmpSourceFont))
                OnImported(package);
        }

        private static void OnImported(string packageName)
        {
            if (!_importPending) return;
            _importPending = false;
            Unsubscribe();
            AssetDatabase.Refresh();
            CreateFontAsset();
            var next = _onReady;
            _onReady = null;
            next?.Invoke();
        }

        private static void OnImportFailed(string packageName, string error)
        {
            _importPending = false;
            Unsubscribe();
            _onReady = null;
            Debug.LogError($"[Gece Büfesi] TMP Essential Resources içe aktarılamadı: {error}");
        }

        private static void Unsubscribe()
        {
            AssetDatabase.importPackageCompleted -= OnImported;
            AssetDatabase.importPackageFailed -= OnImportFailed;
        }

        private static void CreateFontAsset()
        {
            var existing = LoadFont();
            if (existing == null)
            {
                var source = AssetDatabase.LoadAssetAtPath<Font>(EditorPaths.TmpSourceFont);
                if (source == null)
                {
                    Debug.LogError($"[Gece Büfesi] Kaynak font yok: {EditorPaths.TmpSourceFont}");
                    return;
                }

                Directory.CreateDirectory(EditorPaths.Fonts);
                var font = TMP_FontAsset.CreateFontAsset(source, 90, 9, GlyphRenderMode.SDFAA, 1024, 1024,
                    AtlasPopulationMode.Dynamic, true);
                font.name = Path.GetFileNameWithoutExtension(EditorPaths.TurkishFont);
                AssetDatabase.CreateAsset(font, EditorPaths.TurkishFont);

                // The atlas texture and material must live inside the font asset.
                var atlas = font.atlasTexture;
                atlas.name = font.name + " Atlas";
                AssetDatabase.AddObjectToAsset(atlas, font);
                font.material.name = font.name + " Material";
                AssetDatabase.AddObjectToAsset(font.material, font);
                existing = font;

                // Coverage check while the source face is loaded. Dynamic glyph data is
                // cleared on save and regenerated at runtime, so only check on creation.
                if (!font.TryAddCharacters(BasicCharacters + TurkishCharacters, out string missing) && !string.IsNullOrEmpty(missing))
                    Debug.LogWarning($"[Gece Büfesi] Fontta olmayan karakterler: {missing}");
            }

            EditorUtility.SetDirty(existing);

            var settings = TMP_Settings.instance;
            if (settings != null)
            {
                TMP_Settings.defaultFontAsset = existing;
                EditorUtility.SetDirty(settings);
            }

            AssetDatabase.SaveAssets();
            Debug.Log($"[Gece Büfesi] Türkçe font hazır: {EditorPaths.TurkishFont} (test: {TurkishCharacters})");
        }
    }
}
