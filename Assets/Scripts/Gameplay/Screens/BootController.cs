using NightBufe.Data;
using UnityEngine;

namespace NightBufe.Gameplay
{
    /// <summary>First scene: prepares the session and goes to the main menu.</summary>
    public class BootController : MonoBehaviour
    {
        public GameDatabase database;

        private void Start()
        {
            GameSession.Init(database);
            GameSession.Load(SceneNames.MainMenu);
        }
    }
}
