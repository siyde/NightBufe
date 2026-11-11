using System;
using System.Collections.Generic;
using UnityEngine;

namespace NightBufe.Data
{
    public enum AnomalyType
    {
        DonerFlip,       // döner şişi bir anlığına ters döner
        ExtraWindows,    // karşı binada ışıklı pencere sayısı artar
        EmptyTaxi        // taksi büfenin önünde, içi boş
    }

    [Serializable]
    public class AnomalyEvent
    {
        public AnomalyType type;
        [Tooltip("'SS:dd'")]
        public string time = "01:00";
        public float durationSeconds = 0.5f;
    }

    [CreateAssetMenu(menuName = "Gece Büfesi/Gece", fileName = "Night")]
    public class NightData : ScriptableObject
    {
        public int nightNumber = 1;
        public string title = "Gece 1 – İlk Vardiya";
        [Tooltip("23:00'ten ezana kadar gerçek süre (saniye).")]
        public float durationSeconds = 420f;
        [Range(0f, 1f)] public float baseTension = 0.05f;
        [TextArea, Tooltip("Gece başında kısa süre görünen yazı.")]
        public string introText;
        [TextArea, Tooltip("Gece bitince NightEnd ekranında görünen yazı.")]
        public string endText = "Ezan okunuyor. Kepengi indiriyorsun.";

        [Header("Müşteriler (geliş sırasına göre)")]
        public List<CustomerData> customers = new List<CustomerData>();

        [Header("Kural kâğıdı")]
        [Tooltip("Bu gece kâğıda eklenen kurallar. Önceki gecelerinkiler de görünür.")]
        public List<RuleData> newRules = new List<RuleData>();
        [Tooltip("Kâğıdın arkasındaki silik kural (boş bırakılırsa önceki gecedeki kullanılır).")]
        public RuleData backRule;
        public ChoiceData signNameChoice;
        public ChoiceData flipPaperChoice;

        [Header("Radyo")]
        public string whisperTime = "02:30";
        [Tooltip("Fısıltının duyulabileceği süre (oyun dakikası).")]
        public int whisperWindowMinutes = 45;
        public string whisperText = "...uyan...";
        public ChoiceData radioWhisperChoice;

        [Header("Anomaliler")]
        public List<AnomalyEvent> anomalies = new List<AnomalyEvent>();
    }
}
