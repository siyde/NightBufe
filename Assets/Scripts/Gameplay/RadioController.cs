using System;
using TMPro;
using UnityEngine;

namespace NightBufe.Gameplay
{
    /// <summary>The old radio on the counter. Click to toggle; shows static while on.</summary>
    [RequireComponent(typeof(WorldButton))]
    public class RadioController : MonoBehaviour
    {
        public SpriteRenderer led;
        public Color ledOn = new Color(1f, 0.35f, 0.2f);
        public Color ledOff = new Color(0.2f, 0.12f, 0.1f);
        [Tooltip("Açıkken titreşen parazit yazısı (dünya uzayında).")]
        public TextMeshPro staticText;
        public bool startsOn;

        public bool IsOn { get; private set; }
        public event Action<bool> Toggled;

        private static readonly string[] StaticFrames = { "cızzz...", "kşşşt...", "...cızz", "zzzt...", "..şşş.." };
        private float _frameTimer;
        private int _frame;

        private void Awake()
        {
            GetComponent<WorldButton>().Clicked += Toggle;
            SetOn(startsOn, false);
        }

        public void Toggle() => SetOn(!IsOn, true);

        public void SetOn(bool on, bool notify)
        {
            IsOn = on;
            if (led != null) led.color = on ? ledOn : ledOff;
            if (staticText != null) staticText.gameObject.SetActive(on);
            if (notify) Toggled?.Invoke(on);
        }

        private void Update()
        {
            if (!IsOn || staticText == null) return;
            _frameTimer -= Time.deltaTime;
            if (_frameTimer > 0f) return;
            _frameTimer = UnityEngine.Random.Range(0.25f, 0.6f);
            _frame = (_frame + 1) % StaticFrames.Length;
            staticText.text = StaticFrames[_frame];
        }
    }
}
