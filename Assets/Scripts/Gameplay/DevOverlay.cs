using System.Text;
using NightBufe.Core;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

namespace NightBufe.Gameplay
{
    /// <summary>
    /// Developer mode. F1: show hidden counters. F2 (dev mode only): 10x time.
    /// Lives in every scene that needs it; state is kept in GameSession.
    /// </summary>
    public class DevOverlay : MonoBehaviour
    {
        public GameObject panel;
        public TextMeshProUGUI text;
        public float fastTimeScale = 10f;

        /// <summary>Optional extra line from the current scene (e.g. clock).</summary>
        public System.Func<string> extraInfo;

        private void Start() => ApplyVisibility();

        private void Update()
        {
            var keyboard = Keyboard.current;
            if (keyboard != null)
            {
                if (keyboard.f1Key.wasPressedThisFrame)
                {
                    GameSession.DevMode = !GameSession.DevMode;
                    if (!GameSession.DevMode) GameSession.DevTimeScale = 1f;
                    ApplyVisibility();
                }

                if (GameSession.DevMode && keyboard.f2Key.wasPressedThisFrame)
                    GameSession.DevTimeScale = GameSession.DevTimeScale > 1f ? 1f : fastTimeScale;
            }

            if (GameSession.DevMode && text != null)
                text.text = Build();
        }

        private void ApplyVisibility()
        {
            if (panel != null) panel.SetActive(GameSession.DevMode);
        }

        private string Build()
        {
            var state = GameSession.State;
            var sb = new StringBuilder();
            sb.AppendLine("<b>GELİŞTİRİCİ (F1)</b>");
            if (state == null)
            {
                sb.AppendLine("Oyun durumu yok.");
                return sb.ToString();
            }

            sb.AppendLine($"Gece {state.night}   Kasa {state.money} TL");
            sb.AppendLine($"A (Anlaşma)     {state.agreement}/{CounterLimits.Max(CounterType.Agreement)}");
            sb.AppendLine($"M (Merak)       {state.curiosity}/{CounterLimits.Max(CounterType.Curiosity)}");
            sb.AppendLine($"G (Gerçeklik)   {state.reality}/{CounterLimits.Max(CounterType.Reality)}");
            sb.AppendLine($"Ç (Çocukla bağ) {state.childBond}/{CounterLimits.Max(CounterType.ChildBond)}");
            sb.AppendLine($"Günlük sayfaları: {state.diaryPagesFound.Count}/3");
            sb.AppendLine($"İmza: {(state.HasSigned ? state.playerName : "-")}");
            sb.AppendLine($"Seçimler: {(state.usedChoices.Count == 0 ? "-" : string.Join(", ", state.usedChoices))}");
            sb.AppendLine($"Açık sonlar: {string.Join(", ", EndingResolver.AvailableEndings(state))}");
            if (extraInfo != null) sb.AppendLine(extraInfo());
            sb.Append($"<size=80%>F2: zaman x{(GameSession.DevTimeScale > 1f ? GameSession.DevTimeScale : 1f):0}</size>");
            return sb.ToString();
        }
    }
}
