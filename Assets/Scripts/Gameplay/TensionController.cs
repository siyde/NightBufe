using NightBufe.Core;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace NightBufe.Gameplay
{
    /// <summary>
    /// Drives the "gerilim" value into the global 2D light (warm yellow → greenish,
    /// with flicker) and the weight of the tension post-processing Volume.
    /// </summary>
    public class TensionController : MonoBehaviour
    {
        public Light2D globalLight;
        public Volume tensionVolume;

        [Header("Işık")]
        public Color warmColor = new Color(1f, 0.82f, 0.5f);
        public Color tenseColor = new Color(0.62f, 0.9f, 0.55f);
        public float baseIntensity = 0.9f;
        [Tooltip("Gerilim 1 iken titreme genliği.")]
        public float maxFlickerAmplitude = 0.35f;
        public float flickerSpeed = 14f;

        [Header("Test")]
        [Tooltip("-1 değilse gerilimi bu değere sabitler (Inspector'dan denemek için).")]
        [Range(-1f, 1f)] public float debugOverride = -1f;

        public TensionModel Model { get; } = new TensionModel();
        public float Value => debugOverride >= 0f ? debugOverride : Model.Value;

        private float _forcedFlickerTimer;

        public void SetBase(float baseTension) => Model.BaseTension = baseTension;

        public void Spike(float amount) => Model.AddSpike(amount);

        /// <summary>Strong on/off flicker for a short moment (e.g. signing the paper).</summary>
        public void Flicker(float seconds) => _forcedFlickerTimer = Mathf.Max(_forcedFlickerTimer, seconds);

        private void Update()
        {
            Model.Tick(Time.deltaTime);
            float t = Value;

            if (globalLight != null)
            {
                globalLight.color = Color.Lerp(warmColor, tenseColor, t);

                float noise = Mathf.PerlinNoise(Time.time * flickerSpeed, 0.37f) * 2f - 1f;
                float intensity = baseIntensity + noise * maxFlickerAmplitude * t * t;

                if (_forcedFlickerTimer > 0f)
                {
                    _forcedFlickerTimer -= Time.deltaTime;
                    intensity = Random.value < 0.45f ? 0.15f : baseIntensity * 1.1f;
                }

                globalLight.intensity = Mathf.Max(0.05f, intensity);
            }

            if (tensionVolume != null)
                tensionVolume.weight = t;
        }
    }
}
