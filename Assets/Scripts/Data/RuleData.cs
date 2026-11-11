using UnityEngine;

namespace NightBufe.Data
{
    [CreateAssetMenu(menuName = "Gece Büfesi/Kural", fileName = "Rule")]
    public class RuleData : ScriptableObject
    {
        public string id;
        [TextArea] public string text;
        [TextArea, Tooltip("Okunamaz hâli (silik kural için).")]
        public string illegibleText;
        [Tooltip("Okunabilir olması için gereken Merak (M) değeri. 0 = her zaman okunur.")]
        public int curiosityToRead;
        [Tooltip("Tasarım notu: bu kural yalan mı? Oyuncuya gösterilmez.")]
        public bool isLie;
    }
}
