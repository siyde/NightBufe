using System;
using System.Collections.Generic;
using NightBufe.Core;
using UnityEngine;

namespace NightBufe.Data
{
    [Serializable]
    public class TalkOption
    {
        [Tooltip("Buton yazısı (ör. 'Sohbet et').")]
        public string label;
        [TextArea] public string reply;
        [Tooltip("İsteğe bağlı: bu konuşma bir seçim sayılıyorsa.")]
        public ChoiceData choice;
    }

    [CreateAssetMenu(menuName = "Gece Büfesi/Müşteri", fileName = "Customer")]
    public class CustomerData : ScriptableObject
    {
        public string id;
        public string displayName;

        [Header("Görsel")]
        [Tooltip("ArtLibrary anahtarı.")]
        public string artKey;
        [Tooltip("Çocuk gibi kısa müşteriler için.")]
        public bool isSmall;
        [Range(0f, 1f), Tooltip("0 = normal, 1 = tamamen solgun.")]
        public float paleness;

        [Header("Zamanlama")]
        [Tooltip("Geliş saati, 'SS:dd' (ör. '23:05').")]
        public string arrivalTime = "23:05";

        [Header("Sipariş")]
        public OrderSpec order = new OrderSpec();
        [TextArea, Tooltip("Balonda görünen sipariş cümlesi.")]
        public string orderText;
        [TextArea] public string greeting;
        public int price = 60;
        [Tooltip("Parası yoksa servis ücretsizdir.")]
        public bool paysMoney = true;

        [Header("Tepkiler")]
        [TextArea] public string servedLine;
        [TextArea, Tooltip("Kâğıt imzalandıysa kullanılır. {isim} oyuncunun adıyla değişir.")]
        public string servedLineIfSigned;
        [TextArea] public string wrongOrderLine;
        [TextArea] public string refusedLine;

        [Header("Seçimler")]
        [Tooltip("Doğru servis edildiğinde uygulanır.")]
        public ChoiceData servedChoice;
        [Tooltip("'Kapalıyız' denince uygulanır.")]
        public ChoiceData refusedChoice;
        public List<TalkOption> talkOptions = new List<TalkOption>();

        public string GetServedLine(GameState state)
        {
            if (state != null && state.HasSigned && !string.IsNullOrEmpty(servedLineIfSigned))
                return servedLineIfSigned.Replace("{isim}", state.playerName);
            return servedLine;
        }
    }
}
