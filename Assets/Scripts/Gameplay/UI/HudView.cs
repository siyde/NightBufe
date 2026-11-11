using TMPro;
using UnityEngine;

namespace NightBufe.Gameplay
{
    /// <summary>Top bar: night title, clock bar to ezan, time and money.</summary>
    public class HudView : MonoBehaviour
    {
        public TextMeshProUGUI titleText;
        public TextMeshProUGUI timeText;
        public TextMeshProUGUI moneyText;
        [Tooltip("Sabaha kalan süre çubuğu; anchorMax.x ilerlemeye göre ayarlanır.")]
        public RectTransform clockFill;

        public void SetTitle(string title)
        {
            if (titleText != null) titleText.text = title;
        }

        public void SetClock(float progress, string time)
        {
            if (clockFill != null)
            {
                var max = clockFill.anchorMax;
                max.x = Mathf.Clamp01(progress);
                clockFill.anchorMax = max;
            }
            if (timeText != null) timeText.text = time;
        }

        public void SetMoney(int money)
        {
            if (moneyText != null) moneyText.text = $"Kasa: {money} TL";
        }
    }
}
