using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// A big on-screen move zone (e.g. the left half of the screen). Wherever the finger is, the
/// horizontal side decides the walk direction: touching the left part moves left, the right part
/// moves right. Sliding the finger across the middle flips the direction WITHOUT lifting it.
/// </summary>
[RequireComponent(typeof(RectTransform))]
public class TouchMoveZone : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    [SerializeField] private float deadZone = 30f;                 // small neutral band around the split
    [SerializeField, Range(0f, 1f)] private float splitX01 = 0.5f; // left/right boundary (0=left edge, 1=right edge)

    private RectTransform _rt;
    private PlayerController2D _pc;

    private PlayerController2D PC
    {
        get { if (_pc == null) _pc = FindFirstObjectByType<PlayerController2D>(); return _pc; }
    }

    private void Awake()
    {
        var cfg = GameConfig.Instance;
        if (cfg != null)
        {
            deadZone = cfg.touchDeadZone; splitX01 = cfg.touchSplitX01;
        }
        _rt = GetComponent<RectTransform>();
    }

    private void Apply(PointerEventData e)
    {
        var pc = PC; if (pc == null) return;
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(_rt, e.position, e.pressEventCamera, out Vector2 local))
            return;
        float splitLocalX = _rt.rect.xMin + splitX01 * _rt.rect.width; // boundary between left/right
        float x = local.x - splitLocalX;
        if (Mathf.Abs(x) < deadZone) return;   // keep last direction in the tiny middle band
        pc.SetMove(x < 0f ? -1f : 1f);
    }

    public void OnPointerDown(PointerEventData e) => Apply(e);
    public void OnDrag(PointerEventData e) => Apply(e);
    public void OnPointerUp(PointerEventData e) { var pc = PC; if (pc != null) pc.SetMove(0f); }
}
