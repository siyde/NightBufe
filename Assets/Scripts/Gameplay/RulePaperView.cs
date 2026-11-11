using System;
using System.Collections.Generic;
using System.Text;
using NightBufe.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace NightBufe.Gameplay
{
    /// <summary>
    /// The previous owner's rule paper. Opens over the counter; can be flipped;
    /// the front has a signature line where the player may write their name.
    /// </summary>
    public class RulePaperView : MonoBehaviour
    {
        public GameObject panelRoot;
        public GameObject frontSide;
        public GameObject backSide;
        public TextMeshProUGUI frontText;
        public TextMeshProUGUI backText;

        [Header("İmza")]
        public GameObject signArea;
        public TMP_InputField nameInput;
        public Button signButton;
        public TextMeshProUGUI signatureText;

        [Header("Butonlar")]
        public Button flipButton;
        public TextMeshProUGUI flipLabel;
        public Button closeButton;

        public event Action<string> Signed;
        public event Action Flipped;
        public event Action Closed;

        public bool IsOpen => panelRoot != null && panelRoot.activeSelf;
        public bool ShowingBack { get; private set; }

        private void Awake()
        {
            if (panelRoot != null) panelRoot.SetActive(false);
            if (flipButton != null) flipButton.onClick.AddListener(Flip);
            if (closeButton != null) closeButton.onClick.AddListener(Close);
            if (signButton != null) signButton.onClick.AddListener(TrySign);
        }

        public void Open(IList<RuleData> rules, RuleData backRule, int curiosity, string signedName)
        {
            if (frontText != null) frontText.text = BuildFront(rules, curiosity);
            if (backText != null) backText.text = BuildBack(backRule, curiosity);
            SetSignature(signedName);
            ShowSide(false);
            if (panelRoot != null) panelRoot.SetActive(true);
        }

        public void Close()
        {
            if (!IsOpen) return;
            panelRoot.SetActive(false);
            Closed?.Invoke();
        }

        public void SetSignature(string signedName)
        {
            bool signed = !string.IsNullOrEmpty(signedName);
            if (signArea != null) signArea.SetActive(!signed);
            if (signatureText != null)
                signatureText.text = signed ? $"Yeni çalışan:  <i>{signedName}</i>" : "Yeni çalışan:  ________________";
        }

        private void Flip()
        {
            ShowSide(!ShowingBack);
            if (ShowingBack) Flipped?.Invoke();
        }

        private void ShowSide(bool back)
        {
            ShowingBack = back;
            if (frontSide != null) frontSide.SetActive(!back);
            if (backSide != null) backSide.SetActive(back);
            if (flipLabel != null) flipLabel.text = back ? "Önünü çevir" : "Arkasını çevir";
        }

        private void TrySign()
        {
            string typed = nameInput != null ? nameInput.text.Trim() : "";
            if (typed.Length == 0) return;
            if (typed.Length > 20) typed = typed.Substring(0, 20);
            SetSignature(typed);
            Signed?.Invoke(typed);
        }

        private static string BuildFront(IList<RuleData> rules, int curiosity)
        {
            var sb = new StringBuilder();
            sb.AppendLine("<b>BÜFEDE ÇALIŞANA</b>");
            sb.AppendLine("<size=80%>Bu kurallara harfi harfine uy.</size>");
            sb.AppendLine();
            int number = 1;
            foreach (var rule in rules)
            {
                if (rule == null) continue;
                sb.Append(number++).Append(". ").AppendLine(RuleLine(rule, curiosity));
            }
            return sb.ToString();
        }

        private static string BuildBack(RuleData backRule, int curiosity)
        {
            if (backRule == null) return "<i>Arkası boş.</i>";
            var sb = new StringBuilder();
            sb.AppendLine("<size=80%><i>Kâğıdın arkasında, kurşun kalemle yazılmış, silinmeye çalışılmış bir satır var:</i></size>");
            sb.AppendLine();
            sb.Append(RuleLine(backRule, curiosity));
            return sb.ToString();
        }

        private static string RuleLine(RuleData rule, int curiosity)
        {
            bool readable = rule.curiosityToRead <= 0 || curiosity >= rule.curiosityToRead;
            if (readable) return rule.text;
            return $"<alpha=#55><s>{rule.illegibleText}</s></alpha>";
        }
    }
}
