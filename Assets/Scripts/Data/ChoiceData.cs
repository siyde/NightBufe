using System.Collections.Generic;
using NightBufe.Core;
using UnityEngine;

namespace NightBufe.Data
{
    /// <summary>
    /// A one-shot meaningful choice and its effect on the hidden counters.
    /// Each id is applied at most once per run.
    /// </summary>
    [CreateAssetMenu(menuName = "Gece Büfesi/Seçim", fileName = "Choice")]
    public class ChoiceData : ScriptableObject
    {
        [Tooltip("Kayıtta tutulan benzersiz kimlik (ör. 'n1_sign_name').")]
        public string id;
        [Tooltip("Buton ya da eylem adı (gerekirse).")]
        public string label;
        [TextArea, Tooltip("Seçim yapıldığında ekranda kısa süre görünen yazı.")]
        public string resultText;
        public List<CounterEffect> effects = new List<CounterEffect>();

        [Header("Atmosfer")]
        [Range(0f, 1f), Tooltip("Gerilim değerine anlık eklenen sıçrama.")]
        public float tensionSpike;
        [Tooltip("Işıklar bir an titresin mi?")]
        public bool flickerLights;
    }
}
