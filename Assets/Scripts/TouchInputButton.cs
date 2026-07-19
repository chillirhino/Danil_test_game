using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// On-screen touch control that feeds the capybara controller. Works for touch and mouse via the
/// EventSystem. MoveLeft/MoveRight hold movement while pressed; Jump presses/releases the jump.
/// </summary>
public class TouchInputButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public enum Act { MoveLeft, MoveRight, Jump }
    [SerializeField] private Act action;

    private PlayerController2D _pc;

    private PlayerController2D PC
    {
        get { if (_pc == null) _pc = FindFirstObjectByType<PlayerController2D>(); return _pc; }
    }

    public void OnPointerDown(PointerEventData e)
    {
        var pc = PC; if (pc == null) return;
        switch (action)
        {
            case Act.MoveLeft: pc.SetMove(-1f); break;
            case Act.MoveRight: pc.SetMove(1f); break;
            case Act.Jump: pc.PressJump(); break;
        }
    }

    public void OnPointerUp(PointerEventData e)
    {
        var pc = PC; if (pc == null) return;
        if (action == Act.Jump) pc.ReleaseJump();
        else pc.SetMove(0f);
    }
}
