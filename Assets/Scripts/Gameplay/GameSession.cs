using NightBufe.Core;
using NightBufe.Data;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace NightBufe.Gameplay
{
    public static class SceneNames
    {
        public const string Boot = "Boot";
        public const string MainMenu = "MainMenu";
        public const string Counter = "Counter";
        public const string Storage = "Storage";
        public const string NightEnd = "NightEnd";
        public const string Ending = "Ending";

        public static readonly string[] All = { Boot, MainMenu, Counter, Storage, NightEnd, Ending };
    }

    /// <summary>
    /// Cross-scene holder for the current GameState and database. Any scene can be
    /// played directly: its controller calls Init with its own database reference.
    /// </summary>
    public static class GameSession
    {
        private static SaveService _saves;

        public static GameDatabase Database { get; private set; }
        public static GameState State { get; private set; }
        public static SaveService Saves => _saves ??= new SaveService(Application.persistentDataPath);

        public static bool DevMode { get; set; }
        public static float DevTimeScale { get; set; } = 1f;

        /// <summary>Summary of the night that just ended, for the NightEnd screen.</summary>
        public static int LastNightNumber { get; set; }
        public static int LastNightEarnings { get; set; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            _saves = null;
            Database = null;
            State = null;
            DevMode = false;
            DevTimeScale = 1f;
            LastNightNumber = 0;
            LastNightEarnings = 0;
        }

        public static void Init(GameDatabase database)
        {
            if (database != null) Database = database;
            State ??= new GameState();
        }

        public static void NewGame()
        {
            State = new GameState();
            Saves.Delete();
        }

        public static bool Continue()
        {
            var loaded = Saves.Load();
            if (loaded == null) return false;
            State = loaded;
            return true;
        }

        public static void Save()
        {
            if (State != null) Saves.Save(State);
        }

        public static void Load(string sceneName) => SceneManager.LoadScene(sceneName);
    }
}
