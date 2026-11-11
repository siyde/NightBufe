using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace NightBufe.Gameplay
{
    /// <summary>
    /// Makes a world area clickable through the EventSystem (needs a Collider2D and a
    /// Physics2DRaycaster on the camera). UI on top blocks it automatically.
    /// Hover shows an optional highlight and name label, so it works over painted art too.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class WorldButton : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
    {
        public bool interactable = true;
        [Tooltip("Fare üstündeyken görünen yarı saydam vurgu.")]
        public SpriteRenderer highlight;
        [Tooltip("Fare üstündeyken görünen ad etiketi.")]
        public GameObject hoverLabel;

        public event Action Clicked;

        private void Awake() => SetHover(false);

        public void OnPointerClick(PointerEventData eventData)
        {
            if (!interactable || eventData.button != PointerEventData.InputButton.Left) return;
            Clicked?.Invoke();
        }

        public void OnPointerEnter(PointerEventData eventData) => SetHover(interactable);

        public void OnPointerExit(PointerEventData eventData) => SetHover(false);

        private void OnDisable() => SetHover(false);

        private void SetHover(bool on)
        {
            if (highlight != null) highlight.enabled = on;
            if (hoverLabel != null) hoverLabel.SetActive(on);
        }
    }
}
