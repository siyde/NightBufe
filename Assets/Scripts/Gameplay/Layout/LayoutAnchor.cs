using System.Collections.Generic;
using NightBufe.Core;
using UnityEngine;

namespace NightBufe.Gameplay
{
    /// <summary>
    /// Places this object on a CounterLayout slot. Optionally stretches child visuals
    /// and a collider to the slot's size, or scales uniformly (customer, plate).
    /// </summary>
    [ExecuteAlways]
    public class LayoutAnchor : MonoBehaviour
    {
        public enum UniformScale { None, ByWidth, ByHeight }

        public ArtBackdrop backdrop;
        public string slotId;
        [Tooltip("Nesnenin alandaki konumu: (0.5, 0.5) orta, (0.5, 0) alt orta.")]
        public Vector2 pivot = new Vector2(0.5f, 0.5f);
        [Tooltip("Ölçeği alanın boyutuna eşitlenecek çocuklar (1x1 birimlik yer tutucular).")]
        public List<Transform> stretchTargets = new List<Transform>();
        [Tooltip("Alanın üst kenarının biraz üstüne yerleştirilir (ör. ad etiketi).")]
        public Transform aboveTarget;
        public BoxCollider2D hitArea;

        public UniformScale uniformScale = UniformScale.None;
        [Tooltip("Ölçek 1'deki tasarım boyu/genişliği (dünya birimi).")]
        public float designSize = 1f;

        private void OnEnable()
        {
            if (backdrop != null) backdrop.LayoutChanged += Apply;
            Apply();
        }

        private void OnDisable()
        {
            if (backdrop != null) backdrop.LayoutChanged -= Apply;
        }

        public void Apply()
        {
            if (backdrop == null || !backdrop.TryGetWorldRect(slotId, out LayoutRect r)) return;

            transform.position = new Vector3(r.x + r.width * pivot.x, r.y + r.height * pivot.y, transform.position.z);

            float scale = 1f;
            if (uniformScale == UniformScale.ByWidth && designSize > 0f) scale = r.width / designSize;
            else if (uniformScale == UniformScale.ByHeight && designSize > 0f) scale = r.height / designSize;
            if (uniformScale != UniformScale.None) transform.localScale = new Vector3(scale, scale, 1f);

            // Rect center relative to the pivot, in local (unscaled) units.
            var centerOffset = new Vector2((0.5f - pivot.x) * r.width, (0.5f - pivot.y) * r.height) / scale;
            var size = new Vector2(r.width, r.height) / scale;

            foreach (var target in stretchTargets)
            {
                if (target == null) continue;
                target.localPosition = new Vector3(centerOffset.x, centerOffset.y, target.localPosition.z);
                target.localScale = new Vector3(size.x, size.y, 1f);
            }

            if (aboveTarget != null)
                aboveTarget.localPosition = new Vector3(centerOffset.x, centerOffset.y + size.y * 0.5f + 0.3f / scale, aboveTarget.localPosition.z);

            if (hitArea != null)
            {
                hitArea.size = size;
                hitArea.offset = centerOffset;
            }
        }
    }
}
