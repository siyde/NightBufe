using UnityEngine;

namespace NightBufe.Gameplay
{
    /// <summary>Keeps a UI element over a world point (speech balloon, plate summary).</summary>
    [RequireComponent(typeof(RectTransform))]
    public class UiFollowWorld : MonoBehaviour
    {
        public Transform target;
        [Tooltip("Ekran (Canvas) biriminde ek kaydırma.")]
        public Vector2 offset;
        public Camera worldCamera;

        private RectTransform _rect;

        private void Awake() => _rect = (RectTransform)transform;

        private void LateUpdate()
        {
            if (target == null) return;
            var cam = worldCamera != null ? worldCamera : Camera.main;
            if (cam == null) return;

            var parent = _rect.parent as RectTransform;
            if (parent == null) return;

            Vector3 screen = cam.WorldToScreenPoint(target.position);
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(parent, screen, null, out Vector2 local))
                _rect.localPosition = new Vector3(local.x + offset.x, local.y + offset.y, 0f);
        }
    }
}
