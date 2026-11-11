using NightBufe.Core;
using NightBufe.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace NightBufe.Gameplay
{
    /// <summary>Shown after the ezan: short summary, then the next night (or menu).</summary>
    public class NightEndController : MonoBehaviour
    {
        public GameDatabase database;
        public TextMeshProUGUI titleText;
        public TextMeshProUGUI bodyText;
        public Button continueButton;
        public TextMeshProUGUI continueLabel;

        private string _nextScene;

        private void Start()
        {
            GameSession.Init(database);
            var state = GameSession.State;
            int endedNight = GameSession.LastNightNumber > 0 ? GameSession.LastNightNumber : Mathf.Max(1, state.night - 1);
            var ended = database.GetNight(endedNight);

            titleText.text = $"Gece {endedNight} bitti";
            string end = ended != null ? ended.endText : "Ezan okunuyor.";
            string body = $"{end}\n\nBu geceki kazanç: {GameSession.LastNightEarnings} TL\nKasa: {state.money} TL";

            if (state.night > GameState.FinalNight)
            {
                _nextScene = SceneNames.Ending;
                continueLabel.text = "Son";
            }
            else if (database.GetNight(state.night) != null)
            {
                _nextScene = SceneNames.Counter;
                continueLabel.text = $"Gece {state.night}";
            }
            else
            {
                _nextScene = SceneNames.MainMenu;
                continueLabel.text = "Ana menü";
                body += $"\n\n<size=75%><i>Gece {state.night} henüz hazır değil (Kilometre Taşı 2). İlerlemen kaydedildi.</i></size>";
            }

            bodyText.text = body;
            continueButton.onClick.AddListener(() => GameSession.Load(_nextScene));
        }
    }
}
