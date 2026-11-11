using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NightBufe.Data;
using NightBufe.Gameplay;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

namespace NightBufe.EditorTools
{
    /// <summary>
    /// "Gece Büfesi > 3. Sahneleri Kur": regenerates every scene from code.
    /// Never edit the generated .unity files by hand; change this builder instead.
    /// </summary>
    public static class SceneBuilder
    {
        [MenuItem(EditorPaths.Menu + "3. Sahneleri Kur", priority = 3)]
        public static void Run()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            FontSetup.EnsureFont(BuildAll);
        }

        [MenuItem(EditorPaths.Menu + "Hepsini Kur (1 → 2 → 3)", priority = 20)]
        public static void RunEverything()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            FontSetup.EnsureFont(() =>
            {
                Night1DataBuilder.BuildAssets();
                BuildAll();
            });
        }

        private static void BuildAll()
        {
            CounterLayoutDefaults.EnsureBackgroundImported();
            var database = AssetDatabase.LoadAssetAtPath<GameDatabase>(EditorPaths.Database);
            if (database == null || database.art == null || database.GetNight(1) == null)
                database = Night1DataBuilder.BuildAssets();

            var font = FontSetup.LoadFont();
            if (font == null)
            {
                Debug.LogError("[Gece Büfesi] Türkçe font yok. Önce '1. TMP ve Türkçe Font Kur' komutunu çalıştır.");
                return;
            }

            var kit = new BuildKit(database.art, font);
            var profile = EnsureTensionProfile();
            Directory.CreateDirectory(EditorPaths.Scenes);

            Build(SceneNames.Boot, () => BuildBoot(kit, database));
            Build(SceneNames.MainMenu, () => BuildMainMenu(kit, database));
            Build(SceneNames.Counter, () => BuildCounter(kit, database, profile));
            Build(SceneNames.Storage, () => BuildStorage(kit, database));
            Build(SceneNames.NightEnd, () => BuildNightEnd(kit, database));
            Build(SceneNames.Ending, () => BuildEnding(kit, database));

            EditorBuildSettings.scenes = SceneNames.All
                .Select(n => new EditorBuildSettingsScene(EditorPaths.ScenePath(n), true))
                .ToArray();

            AssetDatabase.SaveAssets();
            EditorSceneManager.OpenScene(EditorPaths.ScenePath(SceneNames.Counter));
            Debug.Log("[Gece Büfesi] Sahneler kuruldu: " + string.Join(", ", SceneNames.All) +
                      ". Oynamak için Boot sahnesini aç ve Play'e bas (Counter'dan doğrudan da başlatılabilir).");
        }

        private static void Build(string sceneName, Action populate)
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            populate();
            EditorSceneManager.SaveScene(scene, EditorPaths.ScenePath(sceneName));
        }

        // =====================================================================
        // Shared pieces
        // =====================================================================

        private static Camera MakeCamera(Color background, bool worldClicks)
        {
            var go = new GameObject("Main Camera") { tag = "MainCamera" };
            go.transform.position = new Vector3(0f, 0f, -10f);
            var cam = go.AddComponent<Camera>();
            cam.orthographic = true;
            cam.orthographicSize = 5f;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = background;
            cam.GetUniversalAdditionalCameraData().renderPostProcessing = true;
            go.AddComponent<AudioListener>();
            if (worldClicks) go.AddComponent<Physics2DRaycaster>();
            return cam;
        }

        private static Light2D MakeGlobalLight(Color color, float intensity)
        {
            var go = new GameObject("Global Light 2D");
            var light = go.AddComponent<Light2D>();
            light.lightType = Light2D.LightType.Global;
            light.color = color;
            light.intensity = intensity;
            return light;
        }

        private static VolumeProfile EnsureTensionProfile()
        {
            var profile = AssetDatabase.LoadAssetAtPath<VolumeProfile>(EditorPaths.TensionProfile);
            if (profile != null) return profile;

            Directory.CreateDirectory(EditorPaths.Rendering);
            profile = ScriptableObject.CreateInstance<VolumeProfile>();
            AssetDatabase.CreateAsset(profile, EditorPaths.TensionProfile);

            AddOverride<Vignette>(profile, v =>
            {
                v.intensity.Override(0.5f);
                v.smoothness.Override(0.55f);
                v.color.Override(new Color(0.02f, 0.07f, 0.03f));
            });
            AddOverride<ColorAdjustments>(profile, c =>
            {
                c.saturation.Override(-35f);
                c.postExposure.Override(-0.35f);
                c.colorFilter.Override(new Color(0.78f, 1f, 0.8f));
            });
            AddOverride<FilmGrain>(profile, g => g.intensity.Override(0.55f));
            AddOverride<ChromaticAberration>(profile, a => a.intensity.Override(0.4f));

            EditorUtility.SetDirty(profile);
            AssetDatabase.SaveAssets();
            return profile;
        }

        private static void AddOverride<T>(VolumeProfile profile, Action<T> setup) where T : VolumeComponent
        {
            var component = profile.Add<T>(false);
            component.name = typeof(T).Name;
            setup(component);
            AssetDatabase.AddObjectToAsset(component, profile);
        }

        private static DevOverlay MakeDevOverlay(BuildKit kit, Transform canvas)
        {
            var root = BuildKit.Stretch("Dev", canvas);
            var overlay = root.gameObject.AddComponent<DevOverlay>();
            var panel = BuildKit.At("DevPanel", root, new Vector2(0f, 1f), new Vector2(20f, -100f), new Vector2(820f, 400f));
            BuildKit.Panel(panel, new Color(0f, 0f, 0f, 0.8f), false);
            var text = kit.Text(BuildKit.Stretch("Text", panel, 14f), "", 22f, new Color(0.6f, 1f, 0.6f), TextAlignmentOptions.TopLeft);
            overlay.panel = panel.gameObject;
            overlay.text = text;
            panel.gameObject.SetActive(false);
            return overlay;
        }

        private static FadingText MakeFadingText(BuildKit kit, Transform canvas, string name, Vector2 position,
            Vector2 size, float fontSize, Color textColor, Color? background)
        {
            var rt = BuildKit.At(name, canvas, new Vector2(0.5f, 0.5f), position, size);
            if (background.HasValue) BuildKit.Panel(rt, background.Value, false);
            var text = kit.Text(BuildKit.Stretch("Text", rt, 18f), "", fontSize, textColor);
            var fading = rt.gameObject.AddComponent<FadingText>();
            fading.text = text;
            fading.group = BuildKit.Group(rt.gameObject);
            fading.group.alpha = 0f;
            return fading;
        }

        /// <summary>Title + body + one button, used by the simple screens.</summary>
        private static (TextMeshProUGUI title, TextMeshProUGUI body, Button button, TextMeshProUGUI label) SimpleScreen(
            BuildKit kit, string title, string body, string buttonLabel)
        {
            MakeCamera(kit.Art.nightBlue, false);
            BuildKit.EventSystem();
            var canvas = BuildKit.Canvas("UI").transform;
            var center = new Vector2(0.5f, 0.5f);
            var titleText = kit.TextAt("Title", canvas, center, new Vector2(0f, 250f), new Vector2(1400f, 140f),
                title, 76f, kit.Art.uiAccent);
            var bodyText = kit.TextAt("Body", canvas, center, new Vector2(0f, 0f), new Vector2(1200f, 340f),
                body, 34f, kit.Art.uiText);
            var button = kit.Button("Button", canvas, center, new Vector2(0f, -270f), new Vector2(380f, 76f), buttonLabel, 34f);
            var label = button.GetComponentInChildren<TextMeshProUGUI>();
            return (titleText, bodyText, button, label);
        }

        // =====================================================================
        // Simple scenes
        // =====================================================================

        private static void BuildBoot(BuildKit kit, GameDatabase database)
        {
            MakeCamera(Color.black, false);
            var boot = new GameObject("Boot").AddComponent<BootController>();
            boot.database = database;
        }

        private static void BuildMainMenu(BuildKit kit, GameDatabase database)
        {
            MakeCamera(kit.Art.nightBlue, false);
            BuildKit.EventSystem();
            var canvas = BuildKit.Canvas("UI").transform;
            var center = new Vector2(0.5f, 0.5f);

            kit.TextAt("Title", canvas, center, new Vector2(0f, 260f), new Vector2(1400f, 160f),
                "Gece Büfesi", 120f, kit.Art.uiAccent);
            kit.TextAt("Subtitle", canvas, center, new Vector2(0f, 160f), new Vector2(1400f, 60f),
                "Sabaha kadar açığız. Kurallara uy.", 32f, kit.Art.uiText);

            var controller = new GameObject("MainMenu").AddComponent<MainMenuController>();
            controller.database = database;
            controller.newGameButton = kit.Button("NewGame", canvas, center, new Vector2(0f, 20f), new Vector2(400f, 76f), "Yeni Oyun", 34f);
            controller.continueButton = kit.Button("Continue", canvas, center, new Vector2(0f, -75f), new Vector2(400f, 76f), "Devam Et", 34f);
            controller.quitButton = kit.Button("Quit", canvas, center, new Vector2(0f, -170f), new Vector2(400f, 76f), "Çıkış", 34f);
            controller.infoText = kit.TextAt("Info", canvas, center, new Vector2(0f, -270f), new Vector2(1200f, 60f),
                "", 28f, kit.Art.uiText);
            kit.TextAt("Footer", canvas, new Vector2(0.5f, 0f), new Vector2(0f, 30f), new Vector2(1400f, 40f),
                "Kilometre Taşı 1  ·  F1: geliştirici modu  ·  Türkçe karakter testi: çğıöşü ÇĞİÖŞÜ", 22f,
                new Color(1f, 1f, 1f, 0.45f));
        }

        private static void BuildStorage(BuildKit kit, GameDatabase database)
        {
            var (_, body, button, _) = SimpleScreen(kit, "Depo", "", "Yukarı çık");
            var controller = new GameObject("Storage").AddComponent<StorageController>();
            controller.database = database;
            controller.bodyText = body;
            controller.backButton = button;
        }

        private static void BuildNightEnd(BuildKit kit, GameDatabase database)
        {
            var (title, body, button, label) = SimpleScreen(kit, "Gece bitti", "", "Devam");
            var controller = new GameObject("NightEnd").AddComponent<NightEndController>();
            controller.database = database;
            controller.titleText = title;
            controller.bodyText = body;
            controller.continueButton = button;
            controller.continueLabel = label;
        }

        private static void BuildEnding(BuildKit kit, GameDatabase database)
        {
            var (_, body, button, _) = SimpleScreen(kit, "Sabah Ezanı", "", "Ana menü");
            var controller = new GameObject("Ending").AddComponent<EndingController>();
            controller.database = database;
            controller.bodyText = body;
            controller.menuButton = button;
        }

        // =====================================================================
        // Counter (main gameplay)
        // =====================================================================

        private static void BuildCounter(BuildKit kit, GameDatabase database, VolumeProfile profile)
        {
            var art = kit.Art;
            MakeCamera(art.nightBlue, true);
            BuildKit.EventSystem();

            // ---- Light & tension ----
            var light = MakeGlobalLight(art.lightWarm, 0.95f);
            var volumeGo = new GameObject("Tension Volume");
            var volume = volumeGo.AddComponent<Volume>();
            volume.isGlobal = true;
            volume.priority = 10f;
            volume.weight = 0f;
            volume.sharedProfile = profile;
            var tension = new GameObject("Tension").AddComponent<TensionController>();
            tension.globalLight = light;
            tension.tensionVolume = volume;
            tension.warmColor = art.lightWarm;
            tension.tenseColor = art.lightTense;

            // Every world position comes from CounterLayout (pixels on the background art).
            var layout = CounterLayoutDefaults.Ensure();
            CounterLayoutDefaults.AssignBackground(art);
            var world = new GameObject("World").transform;

            // ---- Background art & placeholders ----
            var backdrop = new GameObject("Backdrop").AddComponent<ArtBackdrop>();
            backdrop.transform.SetParent(world, false);
            backdrop.library = art;
            backdrop.layout = layout;
            backdrop.backgroundKey = CounterLayoutDefaults.BackgroundKey;
            var background = BuildKit.Empty("BackgroundArt", world, Vector2.zero).AddComponent<SpriteRenderer>();
            if (kit.SpriteLit != null) background.sharedMaterial = kit.SpriteLit;
            background.sortingOrder = -200;
            backdrop.background = background;

            LayoutAnchor Anchor(string slotId, Transform parent, string name = null)
            {
                var anchor = BuildKit.Empty(name ?? slotId, parent, Vector2.zero).AddComponent<LayoutAnchor>();
                anchor.backdrop = backdrop;
                anchor.slotId = slotId;
                return anchor;
            }

            // Plain 1x1 child stretched over the slot.
            SpriteRenderer Stretched(LayoutAnchor anchor, string name, string key, int order, bool circle = false)
            {
                var sr = kit.Sprite(name, anchor.transform, key, Vector2.zero, Vector2.one, order, circle);
                anchor.stretchTargets.Add(sr.transform);
                return sr;
            }

            WorldButton Hotspot(LayoutAnchor anchor, string hoverLabel)
            {
                anchor.hitArea = anchor.gameObject.AddComponent<BoxCollider2D>();
                var button = anchor.gameObject.AddComponent<WorldButton>();
                button.highlight = kit.PlainSprite("Highlight", anchor.transform, Vector2.zero, Vector2.one, 90,
                    new Color(1f, 0.95f, 0.7f, 0.25f));
                anchor.stretchTargets.Add(button.highlight.transform);
                if (!string.IsNullOrEmpty(hoverLabel))
                {
                    var label = kit.WorldLabel(hoverLabel, anchor.transform, Vector2.zero, 2.6f, 95, new Color(1f, 0.92f, 0.6f));
                    label.fontStyle = FontStyles.Bold;
                    anchor.aboveTarget = label.transform;
                    button.hoverLabel = label.gameObject;
                    label.gameObject.SetActive(false);
                }
                button.highlight.enabled = false;
                return button;
            }

            var placeholders = BuildKit.Empty("Placeholders", world, Vector2.zero).transform;
            var special = new HashSet<string> { "doner", "plate" };
            foreach (var slot in layout.slots)
            {
                if (string.IsNullOrEmpty(slot.placeholderKey) || special.Contains(slot.id)) continue;
                var anchor = Anchor(slot.id, placeholders, "PH_" + slot.id);
                Stretched(anchor, "Visual", slot.placeholderKey, slot.sortingOrder, slot.placeholderCircle);
                backdrop.placeholderObjects.Add(anchor.gameObject);
            }

            // ---- Ingredient hotspots ----
            var hotspots = BuildKit.Empty("Hotspots", world, Vector2.zero).transform;
            var ingredientButtons = new List<IngredientButton>();
            foreach (var slot in layout.slots)
            {
                if (string.IsNullOrEmpty(slot.ingredientId)) continue;
                var anchor = Anchor(slot.id, hotspots, "Hotspot_" + slot.id);
                var data = database.GetIngredient(slot.ingredientId);
                Hotspot(anchor, string.IsNullOrEmpty(slot.hoverLabel) && data != null ? data.displayName : slot.hoverLabel);
                var button = anchor.gameObject.AddComponent<IngredientButton>();
                button.ingredient = data;
                ingredientButtons.Add(button);
            }

            // ---- Customer (feet on the counter line, scaled to the slot height) ----
            var customerAnchor = Anchor("customer", world, "Customer");
            customerAnchor.pivot = new Vector2(0.5f, 0f);
            customerAnchor.uniformScale = LayoutAnchor.UniformScale.ByHeight;
            customerAnchor.designSize = 3.85f;
            var customerView = customerAnchor.gameObject.AddComponent<CustomerView>();
            var figure = BuildKit.Empty("Figure", customerAnchor.transform, Vector2.zero).transform;
            customerView.figureRoot = figure;
            customerView.library = art;
            customerView.body = kit.PlainSprite("Body", figure, new Vector2(0f, 1.4f), new Vector2(1.7f, 2.8f), -20, Color.white);
            customerView.head = kit.PlainSprite("Head", figure, new Vector2(0f, 3.35f), new Vector2(1f, 1f), -19, Color.white, true);

            // ---- Döner (anomaly) ----
            var donerAnchor = Anchor("doner", world, "Doner");
            var doner = donerAnchor.gameObject.AddComponent<DonerView>();
            var spit = BuildKit.Empty("Spit", donerAnchor.transform, Vector2.zero).transform;
            var meat = kit.Sprite("Meat", spit, "doner_meat", Vector2.zero, Vector2.one, 5);
            donerAnchor.stretchTargets.Add(meat.transform);
            backdrop.placeholderObjects.Add(spit.gameObject);
            doner.spit = spit;
            doner.backdrop = backdrop;
            doner.slotId = "doner";
            doner.artOverlay = kit.PlainSprite("ArtOverlay", donerAnchor.transform, Vector2.zero, Vector2.one, 6, Color.white);
            doner.artOverlay.transform.localScale = Vector3.one;
            doner.artOverlay.enabled = false;

            // ---- Radio (red box on the shelf) ----
            var radioAnchor = Anchor("radio", world, "Radio");
            Hotspot(radioAnchor, "Radyo");
            var radio = radioAnchor.gameObject.AddComponent<RadioController>();
            var ledAnchor = Anchor("radio_led", world, "RadioLed");
            radio.led = kit.PlainSprite("Led", ledAnchor.transform, Vector2.zero, Vector2.one, 60, Color.black, true);
            ledAnchor.stretchTargets.Add(radio.led.transform);
            var staticAnchor = Anchor("radio_static", world, "RadioStatic");
            radio.staticText = kit.WorldLabel("cızzz...", staticAnchor.transform, Vector2.zero, 2.4f, 60, new Color(0.7f, 1f, 0.75f, 0.9f));
            radio.staticText.gameObject.SetActive(false);

            // ---- Rule paper (always drawn: not part of the painting) ----
            var paperAnchor = Anchor("rule_paper", world, "RulePaperCorner");
            var paperVisual = Stretched(paperAnchor, "Paper", "rule_paper", 30);
            paperVisual.transform.localRotation = Quaternion.Euler(0f, 0f, 4f);
            var paperButton = Hotspot(paperAnchor, "Kural kâğıdı");

            // ---- Serving plate (design units: 2.6 wide) ----
            var plateAnchor = Anchor("plate", world, "ServingPlate");
            plateAnchor.uniformScale = LayoutAnchor.UniformScale.ByWidth;
            plateAnchor.designSize = 2.6f;
            var plateView = plateAnchor.gameObject.AddComponent<PlateView>();
            plateView.database = database;
            var dish = kit.Sprite("Dish", plateAnchor.transform, "plate", Vector2.zero, new Vector2(2.6f, 1.1f), 18, true);
            backdrop.placeholderObjects.Add(dish.gameObject);
            plateView.lavashLayer = kit.Sprite("Lavash", plateAnchor.transform, "ing_lavas", new Vector2(-0.2f, 0.05f), new Vector2(1.7f, 0.55f), 21);
            plateView.wrappedLayer = kit.Sprite("Wrapped", plateAnchor.transform, "durum_wrapped", new Vector2(-0.2f, 0.1f), new Vector2(1.6f, 0.45f), 22);
            for (int i = 0; i < 4; i++)
                plateView.fillingDots.Add(kit.PlainSprite($"Filling{i}", plateAnchor.transform,
                    new Vector2(-0.8f + i * 0.4f, 0.1f), new Vector2(0.3f, 0.3f), 23, Color.white, true));
            plateView.drinkLayer = kit.PlainSprite("Drink", plateAnchor.transform, new Vector2(0.9f, 0.3f), new Vector2(0.4f, 0.75f), 24, Color.white);
            plateView.lavashLayer.enabled = false;
            plateView.wrappedLayer.enabled = false;
            plateView.drinkLayer.enabled = false;
            foreach (var dot in plateView.fillingDots) dot.enabled = false;

            // UI follow points
            var balloonPoint = Anchor("balloon", world, "BalloonPoint");
            var summaryPoint = Anchor("plate_summary", world, "PlateSummaryPoint");

            // =================== UI ===================
            var canvas = BuildKit.Canvas("UI").transform;
            var hud = BuildHud(kit, canvas);

            // Speech balloon
            var balloon = BuildKit.At("Balloon", canvas, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(580f, 240f));
            kit.ArtPanel(balloon, "ui_balloon");
            var follow = balloon.gameObject.AddComponent<UiFollowWorld>();
            follow.target = balloonPoint.transform;
            var tail = BuildKit.At("Tail", balloon, new Vector2(1f, 0.35f), new Vector2(14f, 0f), new Vector2(34f, 34f));
            tail.localRotation = Quaternion.Euler(0f, 0f, 45f);
            kit.ArtPanel(tail, "ui_balloon");
            var balloonText = kit.Text(BuildKit.Rect("Text", balloon, new Vector2(0f, 0.3f), Vector2.one, Vector2.zero, new Vector2(-44f, -30f)),
                "", 28f, kit.Ink, TextAlignmentOptions.TopLeft);
            var hintText = kit.Text(BuildKit.Rect("OrderHint", balloon, Vector2.zero, new Vector2(1f, 0.3f), Vector2.zero, new Vector2(-44f, -10f)),
                "", 24f, new Color(0.55f, 0.15f, 0.1f), TextAlignmentOptions.Left);
            hintText.fontStyle = FontStyles.Bold;
            customerView.balloonRoot = balloon.gameObject;
            customerView.balloonText = balloonText;
            customerView.orderHintText = hintText;

            // Talk options hang below the balloon (and follow it); they live outside the
            // balloon root so hiding the balloon doesn't fight CustomerView's button handling.
            var talkRow = BuildKit.At("TalkOptions", canvas, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(580f, 60f));
            var talkFollow = talkRow.gameObject.AddComponent<UiFollowWorld>();
            talkFollow.target = balloonPoint.transform;
            talkFollow.offset = new Vector2(0f, -165f);
            var row = talkRow.gameObject.AddComponent<HorizontalLayoutGroup>();
            row.spacing = 16f;
            row.childForceExpandWidth = true;
            row.childForceExpandHeight = true;
            row.childControlWidth = true;
            row.childControlHeight = true;
            for (int i = 0; i < 2; i++)
                customerView.talkButtons.Add(kit.Button($"Talk{i}", talkRow, new Vector2(0.5f, 0.5f), Vector2.zero,
                    new Vector2(300f, 60f), "...", 26f, new Color(0.2f, 0.22f, 0.36f), art.uiText));

            // Plate summary + actions
            var summaryRect = BuildKit.At("PlateSummary", canvas, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(700f, 44f));
            BuildKit.Panel(summaryRect, new Color(0f, 0f, 0f, 0.55f), false);
            plateView.summaryText = kit.Text(BuildKit.Stretch("Text", summaryRect, 6f), "Tabak boş", 24f, art.uiText);
            summaryRect.gameObject.AddComponent<UiFollowWorld>().target = summaryPoint.transform;

            // Actions: right column, top to bottom
            var actionAnchor = new Vector2(1f, 0f);
            var actionSize = new Vector2(230f, 66f);
            var trash = kit.Button("Trash", canvas, actionAnchor, new Vector2(-25f, 253f), actionSize, "Tabağı boşalt", 26f,
                new Color(0.3f, 0.3f, 0.32f), art.uiText);
            var wrap = kit.Button("Wrap", canvas, actionAnchor, new Vector2(-25f, 177f), actionSize, "Sar", 32f,
                new Color(0.88f, 0.74f, 0.5f));
            var serve = kit.Button("Serve", canvas, actionAnchor, new Vector2(-25f, 101f), actionSize, "Servis Et", 32f);
            var refuse = kit.Button("Refuse", canvas, actionAnchor, new Vector2(-25f, 25f), actionSize, "Kapalıyız", 32f,
                new Color(0.55f, 0.15f, 0.12f), art.uiText);

            // Messages
            var toast = MakeFadingText(kit, canvas, "Toast", new Vector2(200f, 380f), new Vector2(760f, 130f), 28f,
                art.uiText, new Color(art.uiPanel.r, art.uiPanel.g, art.uiPanel.b, 0.85f));
            var whisper = MakeFadingText(kit, canvas, "Whisper", new Vector2(0f, 120f), new Vector2(1400f, 240f), 110f,
                new Color(0.75f, 1f, 0.8f, 0.9f), null);
            whisper.text.fontStyle = FontStyles.Italic;
            whisper.fadeSeconds = 1.2f;

            var rulePaper = BuildRulePaper(kit, canvas);
            var dev = MakeDevOverlay(kit, canvas);

            // ---- Controller ----
            var controller = new GameObject("CounterController").AddComponent<CounterController>();
            controller.database = database;
            controller.hud = hud;
            controller.customerView = customerView;
            controller.plateView = plateView;
            controller.rulePaper = rulePaper;
            controller.radio = radio;
            controller.tension = tension;
            controller.doner = doner;
            controller.toast = toast;
            controller.whisper = whisper;
            controller.devOverlay = dev;
            controller.ingredientButtons = ingredientButtons;
            controller.wrapButton = wrap;
            controller.trashButton = trash;
            controller.rulePaperButton = paperButton;
            controller.serveButton = serve;
            controller.refuseButton = refuse;
        }

        private static HudView BuildHud(BuildKit kit, Transform canvas)
        {
            var art = kit.Art;
            var bar = BuildKit.Rect("Hud", canvas, new Vector2(0f, 1f), new Vector2(1f, 1f), Vector2.zero, new Vector2(0f, 80f), new Vector2(0.5f, 1f));
            BuildKit.Panel(bar, art.uiPanel, false);
            var hud = bar.gameObject.AddComponent<HudView>();

            hud.titleText = kit.TextAt("Title", bar, new Vector2(0f, 0.5f), new Vector2(30f, 0f), new Vector2(560f, 60f),
                "", 30f, art.uiAccent, TextAlignmentOptions.Left);

            var clockBg = BuildKit.Rect("ClockBar", bar, new Vector2(0.33f, 0.5f), new Vector2(0.69f, 0.5f), Vector2.zero, new Vector2(0f, 20f));
            BuildKit.Panel(clockBg, new Color(0.16f, 0.17f, 0.3f), false);
            var fill = BuildKit.Rect("Fill", clockBg, Vector2.zero, new Vector2(0f, 1f), Vector2.zero, Vector2.zero, new Vector2(0f, 0.5f));
            BuildKit.Panel(fill, art.uiAccent, false);
            hud.clockFill = fill;

            kit.TextAt("Start", clockBg, new Vector2(0f, 0.5f), new Vector2(-90f, 0f), new Vector2(80f, 40f), "23:00", 22f, art.uiText, TextAlignmentOptions.Right);
            kit.TextAt("End", clockBg, new Vector2(1f, 0.5f), new Vector2(90f, 0f), new Vector2(80f, 40f), "Ezan", 22f, art.uiText, TextAlignmentOptions.Left);

            hud.timeText = kit.TextAt("Time", bar, new Vector2(0.78f, 0.5f), Vector2.zero, new Vector2(160f, 60f), "23:00", 40f, art.uiText);
            hud.moneyText = kit.TextAt("Money", bar, new Vector2(1f, 0.5f), new Vector2(-30f, 0f), new Vector2(300f, 60f),
                "Kasa: 0 TL", 30f, art.uiText, TextAlignmentOptions.Right);
            return hud;
        }

        private static RulePaperView BuildRulePaper(BuildKit kit, Transform canvas)
        {
            var root = BuildKit.Stretch("RulePaper", canvas);
            var view = root.gameObject.AddComponent<RulePaperView>();

            var dim = BuildKit.Stretch("Panel", root);
            BuildKit.Panel(dim, new Color(0f, 0f, 0f, 0.65f));
            view.panelRoot = dim.gameObject;

            var paper = BuildKit.At("Paper", dim, new Vector2(0.5f, 0.5f), new Vector2(0f, -20f), new Vector2(820f, 920f));
            paper.localRotation = Quaternion.Euler(0f, 0f, -1.2f);
            kit.ArtPanel(paper, "rule_paper");

            // Front
            var front = BuildKit.Stretch("Front", paper);
            view.frontSide = front.gameObject;
            view.frontText = kit.Text(BuildKit.Rect("Rules", front, new Vector2(0f, 0.34f), Vector2.one, new Vector2(0f, -10f), new Vector2(-100f, -80f)),
                "", 34f, kit.Ink, TextAlignmentOptions.TopLeft);
            view.signatureText = kit.Text(BuildKit.Rect("Signature", front, new Vector2(0f, 0.25f), new Vector2(1f, 0.33f), Vector2.zero, new Vector2(-100f, 0f)),
                "", 30f, kit.Ink, TextAlignmentOptions.Left);
            var signArea = BuildKit.Rect("SignArea", front, new Vector2(0f, 0.15f), new Vector2(1f, 0.24f), Vector2.zero, new Vector2(-100f, 0f));
            view.signArea = signArea.gameObject;
            view.nameInput = kit.InputField("NameInput", signArea, new Vector2(0f, 0.5f), Vector2.zero, new Vector2(440f, 62f), "Adını yaz...");
            view.signButton = kit.Button("Sign", signArea, new Vector2(1f, 0.5f), Vector2.zero, new Vector2(200f, 62f), "İmzala", 28f);

            // Back
            var back = BuildKit.Stretch("Back", paper);
            view.backSide = back.gameObject;
            view.backText = kit.Text(BuildKit.Rect("BackText", back, new Vector2(0f, 0.2f), Vector2.one, new Vector2(0f, -10f), new Vector2(-100f, -80f)),
                "", 34f, kit.Ink, TextAlignmentOptions.TopLeft);
            back.gameObject.SetActive(false);

            // Buttons
            view.flipButton = kit.Button("Flip", paper, new Vector2(0.5f, 0f), new Vector2(-150f, 40f), new Vector2(270f, 64f),
                "Arkasını çevir", 28f, new Color(0.25f, 0.2f, 0.16f), kit.Art.uiText);
            view.flipLabel = view.flipButton.GetComponentInChildren<TextMeshProUGUI>();
            view.closeButton = kit.Button("Close", paper, new Vector2(0.5f, 0f), new Vector2(150f, 40f), new Vector2(270f, 64f),
                "Kapat", 28f, new Color(0.25f, 0.2f, 0.16f), kit.Art.uiText);

            dim.gameObject.SetActive(false);
            return view;
        }
    }
}
