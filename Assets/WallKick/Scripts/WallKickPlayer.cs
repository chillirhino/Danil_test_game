using UnityEngine;
using UnityEngine.InputSystem;

namespace WallKick
{
    /// <summary>
    /// Core Wall Kickers movement: the player sticks to a wall, and each tap
    /// launches them off it at a fixed angle toward the opposite wall.
    /// Uses a dynamic Rigidbody2D — taps set the launch velocity, gravity draws
    /// the parabola, wall collisions re-stick the player.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class WallKickPlayer : MonoBehaviour
    {
        [Header("Jump")]
        [Tooltip("Horizontal launch speed away from the wall.")]
        public float jumpSpeedX = 7f;
        [Tooltip("Vertical launch speed (controls how high each kick goes).")]
        public float jumpSpeedY = 12f;
        [Tooltip("Gravity applied while in flight.")]
        public float gravityScale = 3f;

        [Header("Wall")]
        [Tooltip("How fast the player slides down while stuck to a wall (0 = hang still).")]
        public float wallSlideSpeed = 1.5f;
        [Tooltip("Layer(s) treated as kickable walls.")]
        public LayerMask wallLayer = ~0;

        [Header("Feel")]
        [Tooltip("A tap this many seconds before touching a wall still fires on contact.")]
        public float jumpBufferTime = 0.12f;

        public enum State { OnWall, InFlight }
        public State CurrentState { get; private set; } = State.InFlight;

        // +1 = wall is on the left (launch right), -1 = wall is on the right (launch left).
        private int _wallSide;
        private float _bufferTimer;
        private Rigidbody2D _rb;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _rb.gravityScale = 0f;      // start hanging until first contact
            _rb.freezeRotation = true;
        }

        private void Update()
        {
            if (WasTapped())
                _bufferTimer = jumpBufferTime;
            else
                _bufferTimer -= Time.deltaTime;

            if (CurrentState == State.OnWall && _bufferTimer > 0f)
                Kick();
        }

        private void FixedUpdate()
        {
            if (CurrentState == State.OnWall)
            {
                // Hang / slow slide down the wall.
                _rb.linearVelocity = new Vector2(0f, -wallSlideSpeed);
            }
        }

        private void Kick()
        {
            _bufferTimer = 0f;
            CurrentState = State.InFlight;
            _rb.gravityScale = gravityScale;
            // Launch away from the wall at a fixed angle.
            _rb.linearVelocity = new Vector2(_wallSide * jumpSpeedX, jumpSpeedY);
        }

        private void OnCollisionEnter2D(Collision2D col)
        {
            TryStick(col);
        }

        private void OnCollisionStay2D(Collision2D col)
        {
            if (CurrentState == State.InFlight)
                TryStick(col);
        }

        private void TryStick(Collision2D col)
        {
            if ((wallLayer.value & (1 << col.gameObject.layer)) == 0)
                return;

            // Contact normal points from the wall toward the player.
            float nx = col.GetContact(0).normal.x;
            if (Mathf.Abs(nx) < 0.5f)
                return; // hit a floor/ceiling, not a side wall

            _wallSide = nx > 0f ? 1 : -1;   // normal +x => wall on our left
            CurrentState = State.OnWall;
            _rb.gravityScale = 0f;
            _rb.linearVelocity = Vector2.zero;

            // If a tap was buffered just before contact, fire immediately.
            if (_bufferTimer > 0f)
                Kick();
        }

        private static bool WasTapped()
        {
            var pointer = Pointer.current;
            return pointer != null && pointer.press.wasPressedThisFrame;
        }
    }
}
