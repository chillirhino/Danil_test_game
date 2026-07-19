using UnityEngine;
using UnityEngine.EventSystems;

namespace Sirvival
{
    /// <summary>
    /// Fixed on-screen virtual joystick. Reports <see cref="Direction"/> as a
    /// Vector2 with magnitude 0..1. Put this on the joystick "background" UI object;
    /// it drags a child "handle" within a pixel range.
    /// </summary>
    public class VirtualJoystick : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        [SerializeField] private RectTransform background;
        [SerializeField] private RectTransform handle;
        [SerializeField] private float handleRange = 90f; // px the handle can travel from center

        /// <summary>Current stick direction, magnitude clamped to 1.</summary>
        public Vector2 Direction { get; private set; }

        private void Awake()
        {
            if (background == null) background = transform as RectTransform;
        }

        public void OnPointerDown(PointerEventData e) => OnDrag(e);

        public void OnDrag(PointerEventData e)
        {
            if (background == null) return;
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    background, e.position, e.pressEventCamera, out Vector2 local))
                return;

            Vector2 radius = background.sizeDelta * 0.5f;
            if (radius.x <= 0f || radius.y <= 0f) return;
            Vector2 norm = new Vector2(local.x / radius.x, local.y / radius.y);
            Direction = norm.magnitude > 1f ? norm.normalized : norm;

            if (handle != null) handle.anchoredPosition = Direction * handleRange;
        }

        public void OnPointerUp(PointerEventData e)
        {
            Direction = Vector2.zero;
            if (handle != null) handle.anchoredPosition = Vector2.zero;
        }
    }
}
