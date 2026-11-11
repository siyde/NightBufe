using System;
using System.IO;
using UnityEngine;

namespace NightBufe.Core
{
    /// <summary>
    /// Saves/loads GameState as JSON. The directory is injected so tests can use
    /// a temp folder; the game passes Application.persistentDataPath.
    /// </summary>
    public class SaveService
    {
        public const string DefaultFileName = "gece_bufesi_save.json";

        public string FilePath { get; }

        public SaveService(string directory, string fileName = DefaultFileName)
        {
            if (string.IsNullOrEmpty(directory)) throw new ArgumentException("Directory required", nameof(directory));
            FilePath = Path.Combine(directory, fileName);
        }

        public bool HasSave => File.Exists(FilePath);

        public void Save(GameState state)
        {
            if (state == null) throw new ArgumentNullException(nameof(state));
            Directory.CreateDirectory(Path.GetDirectoryName(FilePath));
            string json = ToJson(state);
            string temp = FilePath + ".tmp";
            File.WriteAllText(temp, json);
            if (File.Exists(FilePath)) File.Delete(FilePath);
            File.Move(temp, FilePath);
        }

        /// <summary>Returns the saved state, or null if there is none or it is corrupt.</summary>
        public GameState Load()
        {
            if (!HasSave) return null;
            try
            {
                return FromJson(File.ReadAllText(FilePath));
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[SaveService] Kayıt okunamadı: {e.Message}");
                return null;
            }
        }

        public void Delete()
        {
            if (HasSave) File.Delete(FilePath);
        }

        public static string ToJson(GameState state) => JsonUtility.ToJson(state, true);

        public static GameState FromJson(string json)
        {
            if (string.IsNullOrWhiteSpace(json)) return null;
            var state = JsonUtility.FromJson<GameState>(json);
            if (state == null) return null;
            state.diaryPagesFound ??= new System.Collections.Generic.List<int>();
            state.usedChoices ??= new System.Collections.Generic.List<string>();
            state.playerName ??= "";
            return state;
        }
    }
}
