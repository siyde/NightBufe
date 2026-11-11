using System.Collections.Generic;
using NightBufe.Core;
using NightBufe.Data;
using UnityEditor;
using UnityEngine;

namespace NightBufe.EditorTools
{
    /// <summary>
    /// Creates (or regenerates) the Night 1 data: ingredients, customers, rules,
    /// choices and the GameDatabase. The content itself is plain data, so new nights
    /// can be authored in the Inspector without code.
    /// </summary>
    public static class Night1DataBuilder
    {
        public const string SignName = "n1_sign_name";
        public const string FlipPaper = "n1_flip_paper";
        public const string RadioWhisper = "n1_radio_whisper";
        public const string TaxiChat = "n1_taxi_chat";
        public const string ChildFreeAyran = "n1_child_free_ayran";

        [MenuItem(EditorPaths.Menu + "2. Gece 1 Verisini Oluştur", priority = 2)]
        public static void Run()
        {
            var database = BuildAssets();
            Selection.activeObject = database;
            Debug.Log($"[Gece Büfesi] Gece 1 verisi hazır: {EditorPaths.Database}");
        }

        public static GameDatabase BuildAssets()
        {
            var factory = new AssetDatabaseFactory();
            var square = PlaceholderArt.EnsureSquare();
            var circle = PlaceholderArt.EnsureCircle();
            CounterLayoutDefaults.EnsureBackgroundImported();

            var database = Build(factory);
            PlaceholderArt.FillLibrary(database.art, square, circle);
            CounterLayoutDefaults.Ensure();
            CounterLayoutDefaults.AssignBackground(database.art);
            factory.SaveAll();
            return database;
        }

        /// <summary>Builds the whole data graph using the given factory (assets or in-memory).</summary>
        public static GameDatabase Build(IDataAssetFactory f)
        {
            var database = f.Get<GameDatabase>(EditorPaths.Database);
            var art = f.Get<ArtLibrary>(EditorPaths.ArtLibrary);
            database.art = art;

            database.ingredients = new List<IngredientData>
            {
                Ingredient(f, "lavas", "Lavaş", IngredientKind.Base, "ing_lavas"),
                Ingredient(f, "tavuk", "Tavuk", IngredientKind.Filling, "ing_tavuk"),
                Ingredient(f, "marul", "Marul", IngredientKind.Filling, "ing_marul"),
                Ingredient(f, "domates", "Domates", IngredientKind.Filling, "ing_domates"),
                Ingredient(f, "acisos", "Acı sos", IngredientKind.Filling, "ing_acisos"),
                Ingredient(f, "ayran", "Ayran", IngredientKind.Drink, "drink_ayran"),
                Ingredient(f, "kola", "Kola", IngredientKind.Drink, "drink_kola"),
            };

            var night = BuildNight1(f);
            if (!database.nights.Contains(night)) database.nights.Add(night);
            database.nights.RemoveAll(n => n == null);
            return database;
        }

        public static NightData BuildNight1(IDataAssetFactory f)
        {
            string root = EditorPaths.Night1;
            var night = f.Get<NightData>($"{root}/Night01.asset");
            night.nightNumber = 1;
            night.title = "Gece 1 – İlk Vardiya";
            night.durationSeconds = 420f;
            night.baseTension = 0.05f;
            night.introText = "Gece 1 – İlk Vardiya\nTezgâhın altında eski sahibin bıraktığı bir kâğıt var.";
            night.endText = "Ezan okunuyor. Kepengi indiriyorsun.\nDöner şişi... hep böyle mi dönüyordu?";

            // ---- Seçimler (DESIGN.md, Gece 1 tablosu) ----
            var sign = Choice(f, root, SignName, "Kural kâğıdının altına adını yaz",
                "Işıklar bir an titredi. Büfe seni kabul etti.", 0.2f, true,
                new CounterEffect(CounterType.Agreement, 1));
            var flip = Choice(f, root, FlipPaper, "Kâğıdın arkasını çevir",
                "Arkada silik, okunamayan bir kural daha var.", 0.05f, false,
                new CounterEffect(CounterType.Curiosity, 1));
            var radio = Choice(f, root, RadioWhisper, "Radyoyu açık bırak",
                "Parazitin içinden biri fısıldadı. Sana mı?", 0.3f, false,
                new CounterEffect(CounterType.Reality, 1));
            var taxiChat = Choice(f, root, TaxiChat, "Taksi şoförüyle sohbet et",
                "", 0.1f, false,
                new CounterEffect(CounterType.Reality, 1));
            var childAyran = Choice(f, root, ChildFreeAyran, "Çocuğa bedava ayran ver",
                "Çocuk ayranı iki eliyle tuttu. Sana uzun uzun baktı.", 0.05f, false,
                new CounterEffect(CounterType.ChildBond, 1));

            night.signNameChoice = sign;
            night.flipPaperChoice = flip;
            night.radioWhisperChoice = radio;

            // ---- Kurallar ----
            night.newRules = new List<RuleData>
            {
                Rule(f, root, "n1_rule_1", "İsteneni ver, fazlasını değil."),
                Rule(f, root, "n1_rule_2", "Ezan okunmadan tezgâhı bırakma."),
                Rule(f, root, "n1_rule_3", "Radyoyu kapalı tut, parazit yapar.", isLie: true),
                Rule(f, root, "n1_rule_4", "Kimseye adını söyleme."),
            };
            night.backRule = Rule(f, root, "n1_rule_back", "Çocuğa servis yapma.",
                illegible: "Ç..u.a   s..v.s   y..ma.", curiosityToRead: EndingResolver.LastRuleCuriosityThreshold);

            // ---- Radyo ----
            night.whisperTime = "03:15";
            night.whisperWindowMinutes = 45;
            night.whisperText = "...uyan...";

            // ---- Anomali: döner şişi bir anlığına ters döner ----
            night.anomalies = new List<AnomalyEvent>
            {
                new AnomalyEvent { type = AnomalyType.DonerFlip, time = "01:05", durationSeconds = 0.6f }
            };

            // ---- Müşteriler ----
            var student = Customer(f, root, "student", "Üniversiteli Öğrenci", "customer_student", "23:08");
            student.greeting = "Kolay gelsin! Vize haftası, uyku yok.";
            student.orderText = "Bir tavuk dürüm, bol marullu. Yanına da ayran.";
            student.order = Order(true, "ayran", "tavuk", "marul");
            student.price = 90;
            student.servedLine = "Ellerine sağlık! Bu beni sabaha kadar idare eder.";
            student.wrongOrderLine = "Bu... benim istediğim değil ama neyse. Açım zaten.";
            student.refusedLine = "Kapalı mı? Işıklar yanıyor ama...";
            student.talkOptions = new List<TalkOption>();

            var nurse = Customer(f, root, "nurse", "Hemşire", "customer_nurse", "00:20");
            nurse.greeting = "Vardiya arası, beş dakikam var.";
            nurse.orderText = "Acısız bir tavuk dürüm; marul, domates olsun. Bir de kola.";
            nurse.order = Order(true, "kola", "tavuk", "marul", "domates");
            nurse.price = 110;
            nurse.servedLine = "Sağ ol. Yoğun bakım bu gece çok sessiz... garip.";
            nurse.wrongOrderLine = "Hmm. Neyse, yolda yerim.";
            nurse.refusedLine = "Peki... Hastane kantini de kapalıydı zaten.";
            nurse.talkOptions = new List<TalkOption>
            {
                new TalkOption
                {
                    label = "Hastane yakın mı?",
                    reply = "Şu köşeyi dönünce. Bu gece bir kaza vakası geldi, genç biri. Hâlâ uyanmadı."
                }
            };

            var taxi = Customer(f, root, "taxi_driver", "Taksi Şoförü", "customer_taxi", "01:30");
            taxi.greeting = "Hayırlı işler. Taksi kapıda, çabuk olsun.";
            taxi.orderText = "Acılı bir tavuk dürüm, domatesli. Bir de kola, buz gibi.";
            taxi.order = Order(true, "kola", "tavuk", "domates", "acisos");
            taxi.price = 110;
            taxi.servedLine = "Eline sağlık. Bu saatte açık yer bulmak zor.";
            taxi.wrongOrderLine = "Bu değil ama... boş ver, acelem var.";
            taxi.refusedLine = "Kapalı mı? Peki. Ben buralardayım.";
            taxi.talkOptions = new List<TalkOption>
            {
                new TalkOption
                {
                    label = "Sohbet et",
                    reply = "Köşede az önce kaza olmuş. Bir taksi, karşıdan gelen kamyon... Ambulans çabuk geldi ama. Neyse, bize ne.",
                    choice = taxiChat
                }
            };

            var watchman = Customer(f, root, "watchman", "Mahalle Bekçisi", "customer_watchman", "02:40");
            watchman.greeting = "Hayırlı nöbetler.";
            watchman.orderText = "Bana sadece bir ayran ver.";
            watchman.order = Order(false, "ayran");
            watchman.price = 30;
            watchman.servedLine = "Eski sahibi de her gece bir ayran verirdi bana. Allah rahmet eylesin.";
            watchman.wrongOrderLine = "Ayran dedim, ayran.";
            watchman.refusedLine = "Peki, peki. Gözün açık olsun.";
            watchman.talkOptions = new List<TalkOption>
            {
                new TalkOption
                {
                    label = "Eski sahibi sor",
                    reply = "Yirmi yıl bu tezgâhta durdu. Bir sabah ezan okunurken kepengi indirdi, bir daha açmadı. Kâğıdını hâlâ tezgâhın altında mı tutuyorsun?"
                }
            };

            var child = Customer(f, root, "child", "Kırmızı Kapüşonlu Çocuk", "customer_child", "04:10");
            child.isSmall = true;
            child.paleness = 0.15f;
            child.greeting = "";
            child.orderText = "Bir ayran alabilir miyim? ...Param yok ama.";
            child.order = Order(false, "ayran");
            child.price = 0;
            child.paysMoney = false;
            child.servedLine = "Teşekkür ederim.";
            child.servedLineIfSigned = "Teşekkür ederim, {isim}.";
            child.wrongOrderLine = "Ben ayran istemiştim...";
            child.refusedLine = "...";
            child.servedChoice = childAyran;
            child.talkOptions = new List<TalkOption>();

            night.customers = new List<CustomerData> { student, nurse, taxi, watchman, child };
            return night;
        }

        private static IngredientData Ingredient(IDataAssetFactory f, string id, string name, IngredientKind kind, string artKey)
        {
            var ingredient = f.Get<IngredientData>($"{EditorPaths.Ingredients}/{id}.asset");
            ingredient.id = id;
            ingredient.displayName = name;
            ingredient.kind = kind;
            ingredient.artKey = artKey;
            return ingredient;
        }

        private static ChoiceData Choice(IDataAssetFactory f, string root, string id, string label, string result,
            float tensionSpike, bool flicker, params CounterEffect[] effects)
        {
            var choice = f.Get<ChoiceData>($"{root}/Choices/{id}.asset");
            choice.id = id;
            choice.label = label;
            choice.resultText = result;
            choice.tensionSpike = tensionSpike;
            choice.flickerLights = flicker;
            choice.effects = new List<CounterEffect>(effects);
            return choice;
        }

        private static RuleData Rule(IDataAssetFactory f, string root, string id, string text,
            string illegible = "", int curiosityToRead = 0, bool isLie = false)
        {
            var rule = f.Get<RuleData>($"{root}/Rules/{id}.asset");
            rule.id = id;
            rule.text = text;
            rule.illegibleText = illegible;
            rule.curiosityToRead = curiosityToRead;
            rule.isLie = isLie;
            return rule;
        }

        private static CustomerData Customer(IDataAssetFactory f, string root, string id, string name, string artKey, string arrival)
        {
            var customer = f.Get<CustomerData>($"{root}/Customers/{id}.asset");
            customer.id = id;
            customer.displayName = name;
            customer.artKey = artKey;
            customer.arrivalTime = arrival;
            customer.isSmall = false;
            customer.paleness = 0f;
            customer.paysMoney = true;
            customer.servedLineIfSigned = "";
            customer.servedChoice = null;
            customer.refusedChoice = null;
            return customer;
        }

        private static OrderSpec Order(bool durum, string drink, params string[] fillings) =>
            new OrderSpec { wantsDurum = durum, drink = drink ?? "", fillings = new List<string>(fillings) };
    }
}
