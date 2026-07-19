using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace Sirvival
{
    /// <summary>
    /// Top-down movement for the chef. Reads the on-screen <see cref="VirtualJoystick"/>,
    /// with a keyboard fallback (WASD / arrows) so it's testable in the editor.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class ChefMovement : MonoBehaviour
    {
        [SerializeField] private VirtualJoystick joystick;
        [SerializeField] private float moveSpeed = 5f;

        private Rigidbody2D _rb;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _rb.gravityScale = 0f;
            _rb.freezeRotation = true;
        }

        public void SetJoystick(VirtualJoystick j) => joystick = j;

        private Vector2 ReadInput()
        {
            Vector2 v = joystick != null ? joystick.Direction : Vector2.zero;
            if (v.sqrMagnitude >= 0.01f) return v;

#if ENABLE_INPUT_SYSTEM
            var k = Keyboard.current;
            if (k != null)
            {
                float x = (k.dKey.isPressed || k.rightArrowKey.isPressed ? 1f : 0f)
                        - (k.aKey.isPressed || k.leftArrowKey.isPressed ? 1f : 0f);
                float y = (k.wKey.isPressed || k.upArrowKey.isPressed ? 1f : 0f)
                        - (k.sKey.isPressed || k.downArrowKey.isPressed ? 1f : 0f);
                v = new Vector2(x, y);
                if (v.sqrMagnitude > 1f) v = v.normalized;
            }
#endif
            return v;
        }

        private void FixedUpdate()
        {
            _rb.linearVelocity = ReadInput() * moveSpeed;
        }
    }
}
