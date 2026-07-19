using UnityEngine;
using UnityEngine.InputSystem;

namespace PoK.Player
{
    /// <summary>
    /// On-rails first-person controller for the Path of Kings prototype.
    /// The player only walks forward along the path — no strafing, no free look.
    /// On mobile there is no joystick: movement is straight ahead (auto or hold).
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public class FirstPersonController : MonoBehaviour
    {
        [Header("Forward movement")]
        public float walkSpeed = 3f;
        public float gravity = -20f;

        [Tooltip("If true, the player walks forward on its own. If false, movement only happens while 'hold to walk' is active (touch / left mouse / W).")]
        public bool autoWalk = true;

        [Tooltip("When true, the player stops (e.g. an enemy blocks the path). Combat logic sets this later.")]
        public bool blocked = false;

        CharacterController _cc;
        float _verticalVelocity;

        // Exposed so the arms / head-bob script can react to movement.
        public float CurrentPlanarSpeed { get; private set; }

        void Awake()
        {
            _cc = GetComponent<CharacterController>();
        }

        void Update()
        {
            bool wantsToWalk = !blocked && (autoWalk || HoldToWalkPressed());

            Vector3 planar = wantsToWalk ? transform.forward * walkSpeed : Vector3.zero;
            CurrentPlanarSpeed = new Vector2(planar.x, planar.z).magnitude;

            if (_cc.isGrounded && _verticalVelocity < 0f)
                _verticalVelocity = -2f;
            _verticalVelocity += gravity * Time.deltaTime;

            Vector3 velocity = planar + Vector3.up * _verticalVelocity;
            _cc.Move(velocity * Time.deltaTime);
        }

        bool HoldToWalkPressed()
        {
            var kb = Keyboard.current;
            if (kb != null && kb.wKey.isPressed) return true;

            var mouse = Mouse.current;
            if (mouse != null && mouse.leftButton.isPressed) return true;

            var ts = Touchscreen.current;
            if (ts != null && ts.primaryTouch.press.isPressed) return true;

            return false;
        }
    }
}
