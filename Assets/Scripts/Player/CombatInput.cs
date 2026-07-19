using UnityEngine;
using UnityEngine.InputSystem;

namespace PoK.Player
{
    /// <summary>
    /// Fires the "Attack" trigger on the first-person body's Animator
    /// when the player taps / clicks / presses space.
    /// </summary>
    public class CombatInput : MonoBehaviour
    {
        Animator _anim;

        void Awake()
        {
            _anim = GetComponentInChildren<Animator>();
        }

        void Update()
        {
            if (_anim != null && AttackPressed())
                _anim.SetTrigger("Attack");
        }

        bool AttackPressed()
        {
            var mouse = Mouse.current;
            if (mouse != null && mouse.leftButton.wasPressedThisFrame) return true;
            var kb = Keyboard.current;
            if (kb != null && kb.spaceKey.wasPressedThisFrame) return true;
            var ts = Touchscreen.current;
            if (ts != null && ts.primaryTouch.press.wasPressedThisFrame) return true;
            return false;
        }
    }
}
