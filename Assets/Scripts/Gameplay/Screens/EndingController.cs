using System.Text;
using NightBufe.Core;
using NightBufe.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace NightBufe.Gameplay
{
    /// <summary>Ending screen. For now lists which endings the counters opened.</summary>
    public class EndingController : MonoBehaviour
    {
        public GameDatabase database;
        public TextMeshProUGUI bodyText;
        public Button menuButton;

        private void Start()
        {
            GameSession.Init(database);
            var sb = new StringBuilder();
            sb.AppendLine("Açılan sonlar:");
            sb.AppendLine();
            foreach (var ending in EndingResolver.AvailableEndings(GameSession.State))
                sb.AppendLine("• " + DisplayName(ending));
            sb.AppendLine();
            sb.Append("<size=75%><i>Son sahneler sonraki kilometre taşlarında gelecek.</i></size>");
            bodyText.text = sb.ToString();
            menuButton.onClick.AddListener(() => GameSession.Load(SceneNames.MainMenu));
        }

        public static string DisplayName(EndingId ending)
        {
            switch (ending)
            {
                case EndingId.Regular: return "Müdavim";
                case EndingId.Loop: return "Döngü";
                case EndingId.Seal: return "Mühür";
                case EndingId.Awakening: return "Uyanış";
                default: return ending.ToString();
            }
        }
    }
}
