using TMPro;
using UnityEngine;

namespace NightBufe.Gameplay
{
    /// <summary>Short fading message (feedback, choice results, radio whisper).</summary>
    public class FadingText : MonoBehaviour
    {
        public TextMeshProUGUI text;
        public CanvasGroup group;
        public float fadeSeconds = 0.6f;

        private float _timer;
        private float _holdSeconds;

        private void Awake()
        {
            if (group != null) group.alpha = 0f;
        }

        public void Show(string message, float holdSeconds = 2.5f)
        {
            if (string.IsNullOrEmpty(message)) return;
            if (text != null) text.text = message;
            _holdSeconds = holdSeconds;
            _timer = holdSeconds + fadeSeconds * 2f;
        }

        private void Update()
        {
            if (group == null) return;
            if (_timer <= 0f)
            {
                group.alpha = 0f;
                return;
            }

            _timer -= Time.deltaTime;
            float total = _holdSeconds + fadeSeconds * 2f;
            float elapsed = total - _timer;
            if (elapsed < fadeSeconds) group.alpha = elapsed / fadeSeconds;
            else if (_timer < fadeSeconds) group.alpha = Mathf.Max(0f, _timer / fadeSeconds);
            else group.alpha = 1f;
        }
    }
}
