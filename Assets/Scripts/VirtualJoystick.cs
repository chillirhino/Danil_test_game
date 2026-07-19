using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Floating on-screen joystick for the capybara. Occupies a large touch zone (the left half of
/// the screen). Touch anywhere inside the zone and a joystick base + handle spawns under the
/// finger; drag left/right to steer. Only the horizontal axis is used (this is a side-scroller),
/// feeding an analog value to <see cref="PlayerController2D.SetMove"/>. Releasing recenters the
/// handle and stops movement. Coexists with the separate Jump button on the right half.
/// </summary>
[RequireComponent(typeof(RectTransform))]
public class VirtualJoystick : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    [Header("Refs")]
    [SerializeField] private RectTransform joystickBase; // the ring that appears under the finger
    [SerializeField] private RectTransform handle;       // the knob that tracks the finger

    [Header("Feel")]
    [SerializeField] private float radius = 130f;                     // px of handle travel that maps to full speed
    [SerializeField, Range(0f, 0.9f)] private float deadZone = 0.12f; // ignore tiny nudges (fraction of radius)
    [SerializeField] private bool hideWhenIdle = true;               // hide base/handle while not touched

    private RectTransform _rt;
    private PlayerController2D _pc;
    private int _activePointerId = int.MinValue; // track a single finger (int.MinValue = none)
    private Vector2 _center;                      // local-space point where the current touch began

    private PlayerController2D PC
    {
        get { if (_pc == null) _pc = FindFirstObjectByType<PlayerController2D>(); return _pc; }
    }

    private void Awake()
    {
        _rt = GetComponent<RectTransform>();
        if (hideWhenIdle) SetVisible(false);
    }

    public void OnPointerDown(PointerEventData e)
    {
        if (_activePointerId != int.MinValue) return; // already steering with another finger
        _activePointerId = e.pointerId;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(_rt, e.position, e.pressEventCamera, out _center);
        if (joystickBase != null) joystickBase.anchoredPosition = _center;
        if (handle != null) handle.anchoredPosition = _center;
        SetVisible(true);
        UpdateHandle(e); // respond on the very first frame of the press
    }

    public void OnDrag(PointerEventData e)
    {
        if (e.pointerId != _activePointerId) return;
        UpdateHandle(e);
    }

    public void OnPointerUp(PointerEventData e)
    {
        if (e.pointerId != _activePointerId) return;
        _activePointerId = int.MinValue;
        if (handle != null) handle.anchoredPosition = _center; // snap knob back to center
        if (hideWhenIdle) SetVisible(false);
        var pc = PC; if (pc != null) pc.SetMove(0f);
    }

    private void UpdateHandle(PointerEventData e)
    {
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(_rt, e.position, e.pressEventCamera, out Vector2 local))
            return;

        Vector2 offset = local - _center;
        // Visual: let the knob follow the finger in all directions, clamped to the ring.
        Vector2 clamped = Vector2.ClampMagnitude(offset, radius);
        if (handle != null) handle.anchoredPosition = _center + clamped;

        // Movement: horizontal only. Rescale so the throw past the dead zone spans a full 0..1.
        float x = Mathf.Clamp(clamped.x / radius, -1f, 1f);
        float mag = Mathf.Abs(x);
        x = mag < deadZone ? 0f : Mathf.Sign(x) * (mag - deadZone) / (1f - deadZone);

        var pc = PC; if (pc != null) pc.SetMove(x);
    }

    private void SetVisible(bool on)
    {
        if (joystickBase != null) joystickBase.gameObject.SetActive(on);
        if (handle != null) handle.gameObject.SetActive(on);
    }
}
