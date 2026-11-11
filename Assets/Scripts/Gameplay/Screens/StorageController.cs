using NightBufe.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace NightBufe.Gameplay
{
    /// <summary>Storage room. Placeholder for milestone 2 (diary pages, supplies).</summary>
    public class StorageController : MonoBehaviour
    {
        public GameDatabase database;
        public TextMeshProUGUI bodyText;
        public Button backButton;

        private void Start()
        {
            GameSession.Init(database);
            if (bodyText != null)
                bodyText.text = "Rutubet ve kızartma yağı kokusu. Raflar karanlık.\n\n<size=75%><i>Depo, Kilometre Taşı 2'de açılacak.</i></size>";
            backButton.onClick.AddListener(() => GameSession.Load(SceneNames.Counter));
        }
    }
}
