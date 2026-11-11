using System;
using System.Collections.Generic;
using NightBufe.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace NightBufe.Gameplay
{
    /// <summary>The customer standing across the counter, plus their speech balloon.</summary>
    public class CustomerView : MonoBehaviour
    {
        [Header("Dünya")]
        public Transform figureRoot;
        public SpriteRenderer body;
        public SpriteRenderer head;
        [Tooltip("Müşteri görselleri CustomerData.artKey ile buradan çözülür.")]
        public ArtLibrary library;
        public float smallScale = 0.62f;
        public Color paleColor = new Color(0.86f, 0.87f, 0.9f);
        public float fadeSeconds = 0.4f;

        [Header("Balon")]
        public GameObject balloonRoot;
        public TextMeshProUGUI balloonText;
        public TextMeshProUGUI orderHintText;
        public List<Button> talkButtons = new List<Button>();

        private Vector3 _baseScale = Vector3.one;
        private Color _bodyColor = Color.white;
        private Color _headColor = Color.white;
        private float _alpha;
        private float _targetAlpha;

        public bool IsVisible => _targetAlpha > 0f;

        private void Awake()
        {
            if (figureRoot != null) _baseScale = figureRoot.localScale;
            SetAlpha(0f);
            if (balloonRoot != null) balloonRoot.SetActive(false);
            HideTalkOptions();
        }

        public void Show(CustomerData customer, float paleness)
        {
            _bodyColor = Color.white;
            if (library != null && body != null)
            {
                library.Resolve(customer.artKey, false, out var sprite, out var color);
                body.sprite = sprite;
                _bodyColor = color;
            }
            _bodyColor = Color.Lerp(_bodyColor, paleColor, paleness);
            _headColor = Color.Lerp(new Color(0.93f, 0.78f, 0.64f), paleColor, paleness);

            if (figureRoot != null)
                figureRoot.localScale = customer.isSmall ? _baseScale * smallScale : _baseScale;

            _targetAlpha = 1f;
            if (balloonRoot != null) balloonRoot.SetActive(true);
        }

        public void Hide()
        {
            _targetAlpha = 0f;
            if (balloonRoot != null) balloonRoot.SetActive(false);
            HideTalkOptions();
        }

        public void Say(string line, string orderHint = null)
        {
            if (balloonText != null) balloonText.text = line;
            if (orderHintText != null) orderHintText.text = orderHint ?? "";
        }

        /// <summary>Shows one button per option; clicking invokes the callback with the option index.</summary>
        public void ShowTalkOptions(IList<TalkOption> options, Func<int, bool> isAvailable, Action<int> onPicked)
        {
            HideTalkOptions();
            if (options == null) return;

            for (int i = 0; i < options.Count && i < talkButtons.Count; i++)
            {
                if (!isAvailable(i)) continue;
                int index = i;
                var button = talkButtons[i];
                button.gameObject.SetActive(true);
                var label = button.GetComponentInChildren<TextMeshProUGUI>();
                if (label != null) label.text = options[i].label;
                button.onClick.AddListener(() => onPicked(index));
            }
        }

        public void HideTalkOptions()
        {
            foreach (var button in talkButtons)
            {
                if (button == null) continue;
                button.onClick.RemoveAllListeners();
                button.gameObject.SetActive(false);
            }
        }

        private void Update()
        {
            if (Mathf.Approximately(_alpha, _targetAlpha)) return;
            _alpha = Mathf.MoveTowards(_alpha, _targetAlpha, Time.deltaTime / Mathf.Max(0.01f, fadeSeconds));
            SetAlpha(_alpha);
        }

        private void SetAlpha(float a)
        {
            _alpha = a;
            if (body != null) body.color = new Color(_bodyColor.r, _bodyColor.g, _bodyColor.b, a);
            if (head != null) head.color = new Color(_headColor.r, _headColor.g, _headColor.b, a);
        }
    }
}
