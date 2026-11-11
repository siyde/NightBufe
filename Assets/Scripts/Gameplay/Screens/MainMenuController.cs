using NightBufe.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace NightBufe.Gameplay
{
    public class MainMenuController : MonoBehaviour
    {
        public GameDatabase database;
        public Button newGameButton;
        public Button continueButton;
        public Button quitButton;
        public TextMeshProUGUI infoText;

        private void Start()
        {
            GameSession.Init(database);
            newGameButton.onClick.AddListener(NewGame);
            continueButton.onClick.AddListener(Continue);
            quitButton.onClick.AddListener(Quit);
            continueButton.interactable = GameSession.Saves.HasSave;
            if (infoText != null) infoText.text = "";
        }

        private void NewGame()
        {
            GameSession.NewGame();
            GameSession.Load(SceneNames.Counter);
        }

        private void Continue()
        {
            if (!GameSession.Continue())
            {
                if (infoText != null) infoText.text = "Kayıt okunamadı.";
                return;
            }

            if (database.GetNight(GameSession.State.night) == null)
            {
                if (infoText != null)
                    infoText.text = $"Gece {GameSession.State.night} henüz hazır değil. (Kilometre Taşı 2)";
                return;
            }

            GameSession.Load(SceneNames.Counter);
        }

        private void Quit()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
