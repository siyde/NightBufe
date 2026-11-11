namespace NightBufe.EditorTools
{
    internal static class EditorPaths
    {
        public const string Menu = "Gece Büfesi/";

        public const string Data = "Assets/Data";
        public const string Database = Data + "/GameDatabase.asset";
        public const string ArtLibrary = Data + "/ArtLibrary.asset";
        public const string Ingredients = Data + "/Ingredients";
        public const string CounterLayout = Data + "/CounterLayout.asset";
        public const string CounterBackground = "Assets/Art/Backgrounds/counter.png";
        public const string Night1 = Data + "/Nights/Night01";

        public const string Art = "Assets/Art";
        public const string Placeholders = Art + "/Placeholders";
        public const string SquareSprite = Placeholders + "/Square.png";
        public const string CircleSprite = Placeholders + "/Circle.png";

        public const string Fonts = "Assets/Fonts";
        public const string TurkishFont = Fonts + "/GeceBufesi_LiberationSans SDF.asset";
        public const string TmpEssentialsFolder = "Assets/TextMesh Pro";
        public const string TmpSettings = TmpEssentialsFolder + "/Resources/TMP Settings.asset";
        public const string TmpSourceFont = TmpEssentialsFolder + "/Fonts/LiberationSans.ttf";

        public const string Rendering = "Assets/Rendering";
        public const string TensionProfile = Rendering + "/TensionVolumeProfile.asset";
        public const string SpriteLitMaterial = "Packages/com.unity.render-pipelines.universal/Runtime/Materials/Sprite-Lit-Default.mat";

        public const string Scenes = "Assets/Scenes";

        public static string ScenePath(string sceneName) => $"{Scenes}/{sceneName}.unity";
    }
}
