using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Full-area touch zone (the bottom half of the screen) that steers the capybara by dragging.
/// Drag your finger right to move right, left to move left; keep it held off-centre to keep moving.
/// Coexists with the on-screen arrow buttons: since the EventSystem delivers a pointer only to the
/// top-most UI element, pressing an arrow still works, while dragging anywhere else on the zone moves.
/// </summary>
public class DragMoveZone : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    [Tooltip("Pixels of horizontal travel from the anchor before movement kicks in.")]
    [SerializeField] private float deadZone = 12f;

    [Tooltip("Finger distance (pixels) from the anchor that maps to full speed. Farther = faster, capped here.")]
    [SerializeField] private float maxLead = 160f;

    private PlayerController2D _pc;
    private float _anchorX;
    private bool _active;

    private PlayerController2D PC
    {
        get { if (_pc == null) _pc = FindFirstObjectByType<PlayerController2D>(); return _pc; }
    }

    private void Awake()
    {
        var cfg = GameConfig.Instance;
        if (cfg != null)
        {
            deadZone = cfg.dragDeadZone; maxLead = cfg.dragMaxLead;
        }
    }

    public void OnPointerDown(PointerEventData e)
    {
        _anchorX = e.position.x;
        _active = true;
    }

    public void OnDrag(PointerEventData e)
    {
        if (!_active) return;

        float dx = e.position.x - _anchorX;
        // Let the anchor trail the finger so a quick direction reversal responds immediately.
        if (dx > maxLead) { _anchorX = e.position.x - maxLead; dx = maxLead; }
        else if (dx < -maxLead) { _anchorX = e.position.x + maxLead; dx = -maxLead; }

        // Analog: speed grows with how far the finger is dragged from the anchor (past the dead zone),
        // ramping from 0 at the dead-zone edge up to full speed at maxLead.
        float mag = Mathf.Abs(dx);
        float move = mag < deadZone
            ? 0f
            : Mathf.Sign(dx) * Mathf.Clamp01((mag - deadZone) / (maxLead - deadZone));
        var pc = PC; if (pc != null) pc.SetMove(move);
    }

    public void OnPointerUp(PointerEventData e)
    {
        _active = false;
        var pc = PC; if (pc != null) pc.SetMove(0f);
    }
}
