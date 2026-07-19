using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 2D platformer controller for the capybara.
/// Uses the new Input System (keyboard for editor testing; mobile buttons can call
/// SetMove / PressJump / ReleaseJump). Includes coyote time and jump buffering for a
/// responsive, "juicy" jump feel.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController2D : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float walkSpeed = 3.1f;      // slow speed right after pressing move
    [SerializeField] private float moveSpeed = 5f;        // top speed reached after holding a while
    [SerializeField] private float speedRampTime = 0.7f;  // how long to hold before hitting top speed
    [SerializeField] private float driftSpeedThreshold = 2.8f; // reverse above this speed -> drift/skid anim
    [SerializeField] private float acceleration = 60f;
    [SerializeField] private float deceleration = 70f;

    [Header("Jump")]
    [SerializeField] private float jumpForce = 12f;
    [SerializeField] private int maxAirJumps = 1;          // extra mid-air jumps (1 = double jump)
    [SerializeField] private float coyoteTime = 0.1f;      // grace period after leaving ground
    [SerializeField] private float jumpBufferTime = 0.1f;  // remembers an early jump press
    [SerializeField] private float lowJumpMultiplier = 3f; // shorter hop when jump released early
    [SerializeField] private float fallMultiplier = 2.5f;  // snappier fall

    [Header("Ground Check")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.12f;
    // Feet-wide ground probe. Kept a bit narrower than the body so it never reaches a side wall
    // the body isn't already touching. Spanning the feet (instead of a single center point) keeps
    // the player "grounded" while standing on a block corner — so the jump animation no longer
    // flickers when only the outer edge of the feet is over the ledge.
    [SerializeField] private float groundCheckWidth = 0.9f;
    [SerializeField] private LayerMask groundLayer;

    [Header("Damage")]
    [SerializeField] private float flashInterval = 0.1f; // blink speed during i-frames

    [Header("Water (swim)")]
    [SerializeField] private float waterGravityScale = 0.42f; // reduced gravity while submerged (slow sink)
    [SerializeField] private float swimUpAccel = 24f;         // upward stroke accel while jump held under water
    [SerializeField] private float swimMaxRise = 3.4f;        // clamp on how fast you can swim up
    [SerializeField] private float swimMaxSink = 2.6f;        // clamp on how fast you sink
    [SerializeField] private float waterDrag = 2.4f;          // watery damping on velocity
    [SerializeField] private float swimMoveMul = 0.72f;       // horizontal speed multiplier under water

    [Header("Abyss")]
    [SerializeField] private float fallDeathY = -20f; // fall below this Y -> respawn at last checkpoint

    [Header("Power (watermelon)")]
    [SerializeField] private ParticleSystem powerParticles; // aura emitted while powered

    private Rigidbody2D _rb;
    private SpriteRenderer _sprite;
    private float _moveInput;
    private float _keyMove;    // keyboard horizontal
    private float _touchMove;  // on-screen buttons horizontal
    private float _windAccel;   // external wind push (WindGust zones), applied & cleared each FixedUpdate
    private bool _inWater;      // inside a WaterZone -> swim physics
    private float _origGravity; // gravityScale to restore on leaving water
    private float _moveHoldTime; // how long the current move direction has been held (for speed ramp)
    private float _lastMoveSign; // direction held last frame (reset ramp when it changes)
    private float _lastDir;      // last NON-ZERO move direction (persists through brief 0-input gaps)
    private bool _jumpHeld;
    private float _coyoteCounter;
    private float _jumpBufferCounter;
    private int _airJumpsLeft;
    private bool _hasGroundJump;   // the initial (ground) jump; kept even after walking off a ledge
    private float _jumpLockCounter; // brief lock after a jump: ignore ground-contact refills while the feet box still overlaps (prevents a phantom extra ground jump)
    private const float JumpRefillLock = 0.1f;
    private bool _isGrounded;
    private bool _wasGrounded;  // previous-frame grounded state (to detect the landing moment)
    private bool _invincible;
    private bool _powered;
    private Coroutine _invincibilityCo; // active i-frame blink routine (so a restart can cancel it)
    private Coroutine _powerCo;         // active power-up pulse routine
    private CapybaraAnim _anim;      // owns the sprite; plays the eat animation

    /// <summary>True while the player has damage-immunity (i-frames) after being hit.</summary>
    public bool IsInvincible => _invincible;

    /// <summary>True while a star power-up is active (invincible + defeats enemies on touch).</summary>
    public bool IsPowered => _powered;

    /// <summary>Any damage-immune state (i-frames or star power).</summary>
    public bool IsImmune => _invincible || _powered;

    /// <summary>True when the ground check currently overlaps the ground (used by CapybaraAnim).</summary>
    public bool IsGrounded => _isGrounded;

    /// <summary>True while the player is inside a WaterZone (swim physics active). Used by CapybaraAnim.</summary>
    public bool IsInWater => _inWater;

    private void Awake()
    {
        ApplyConfig();
        _rb = GetComponent<Rigidbody2D>();
        _sprite = GetComponentInChildren<SpriteRenderer>();
        _anim = GetComponentInChildren<CapybaraAnim>();
        _rb.freezeRotation = true;
    }

    /// <summary>Pull tuning values from the central <see cref="GameConfig"/> if one exists (else keep the serialized defaults).</summary>
    private void ApplyConfig()
    {
        var c = GameConfig.Instance;
        if (c == null) return;
        walkSpeed = c.walkSpeed; moveSpeed = c.moveSpeed; speedRampTime = c.speedRampTime;
        driftSpeedThreshold = c.driftSpeedThreshold; acceleration = c.acceleration; deceleration = c.deceleration;
        jumpForce = c.jumpForce; maxAirJumps = c.maxAirJumps; coyoteTime = c.coyoteTime;
        jumpBufferTime = c.jumpBufferTime; lowJumpMultiplier = c.lowJumpMultiplier; fallMultiplier = c.fallMultiplier;
        groundCheckRadius = c.groundCheckRadius; groundCheckWidth = c.groundCheckWidth;
        waterGravityScale = c.waterGravityScale; swimUpAccel = c.swimUpAccel; swimMaxRise = c.swimMaxRise;
        swimMaxSink = c.swimMaxSink; waterDrag = c.waterDrag; swimMoveMul = c.swimMoveMul;
        fallDeathY = c.fallDeathY; flashInterval = c.flashInterval;
    }

    private void Update()
    {
        // Fell into the abyss (past any KillZone): respawn at the last checkpoint.
        // Guarded by IsImmune so it fires once per fall, not every frame during i-frames.
        if (transform.position.y < fallDeathY && !IsImmune)
            FallToCheckpoint();

        ReadKeyboard();
        // keyboard wins when a key is held, otherwise use the on-screen buttons
        _moveInput = Mathf.Abs(_keyMove) > 0.01f ? _keyMove : _touchMove;

        // Speed ramp: a fresh press starts slow (walkSpeed) and accelerates to moveSpeed
        // the longer the button is held. Changing direction resets the ramp back to slow.
        if (Mathf.Abs(_moveInput) > 0.01f)
        {
            float sign = Mathf.Sign(_moveInput);
            if (sign != _lastMoveSign) _moveHoldTime = 0f;
            _moveHoldTime += Time.deltaTime;
            _lastMoveSign = sign;
            _lastDir = sign;
        }
        else
        {
            _moveHoldTime = 0f;
            _lastMoveSign = 0f;
            // NOTE: _lastDir is intentionally NOT reset here
        }

        // Ground check: a thin box spanning the feet width (robust on block corners/ledges),
        // rather than a single center point that goes false as soon as the center clears the edge.
        _isGrounded = groundCheck != null &&
                      Physics2D.OverlapBox(groundCheck.position,
                          new Vector2(groundCheckWidth, groundCheckRadius * 2f), 0f, groundLayer);
        // For the ~0.1s right after a jump the feet box is still inside the ground it launched from.
        // Only treat contact as a *landing* refill once that lock has expired, otherwise a second tap
        // spends a phantom "ground jump" before the air jump — a stray triple jump.
        _jumpLockCounter -= Time.deltaTime;
        bool refillFromGround = _isGrounded && _jumpLockCounter <= 0f;
        if (refillFromGround) { _airJumpsLeft = maxAirJumps; _hasGroundJump = true; } // refill jumps on landing
        if (_isGrounded && !_wasGrounded) SoundManager.Play("land"); // just touched down
        _wasGrounded = _isGrounded;

        // Coyote time (also gated by the post-jump lock so it can't re-arm a ground jump mid-rise)
        _coyoteCounter = refillFromGround ? coyoteTime : _coyoteCounter - Time.deltaTime;

        // Jump buffer countdown
        _jumpBufferCounter -= Time.deltaTime;

        // Perform buffered jump: ground/coyote first, otherwise a mid-air jump.
        // Under water, jumping is handled by the swim branch in FixedUpdate (hold to stroke up).
        if (!_inWater && _jumpBufferCounter > 0f)
        {
            // First jump: from the ground, during coyote time, OR retained after simply walking off
            // a ledge (so you can still do the full ground jump + double jump when falling).
            if (_coyoteCounter > 0f || _hasGroundJump)
            {
                _rb.linearVelocity = new Vector2(_rb.linearVelocity.x, jumpForce);
                _jumpBufferCounter = 0f;
                _coyoteCounter = 0f;
                _hasGroundJump = false;
                _jumpLockCounter = JumpRefillLock; // don't let the still-overlapping feet box refill this jump
                SoundManager.Play("jump");
            }
            else if (_airJumpsLeft > 0)
            {
                _rb.linearVelocity = new Vector2(_rb.linearVelocity.x, jumpForce);
                _jumpBufferCounter = 0f;
                _airJumpsLeft--;
                _jumpLockCounter = JumpRefillLock;
                SoundManager.Play("doublejump");
            }
        }

        // Facing direction
        if (_sprite != null && Mathf.Abs(_moveInput) > 0.01f)
            _sprite.flipX = _moveInput < 0f;
    }

    private void FixedUpdate()
    {
        // Horizontal movement with acceleration/deceleration.
        // Current max speed ramps from walkSpeed up to moveSpeed based on how long move is held.
        float speed01 = speedRampTime > 0f ? Mathf.Clamp01(_moveHoldTime / speedRampTime) : 1f;
        float currentMaxSpeed = Mathf.Lerp(walkSpeed, moveSpeed, speed01);
        if (_inWater) currentMaxSpeed *= swimMoveMul;
        float targetSpeed = _moveInput * currentMaxSpeed;
        float speedDiff = targetSpeed - _rb.linearVelocity.x;
        float accelRate = Mathf.Abs(targetSpeed) > 0.01f ? acceleration : deceleration;
        float movement = speedDiff * accelRate * Time.fixedDeltaTime;
        _rb.linearVelocity = new Vector2(_rb.linearVelocity.x + movement, _rb.linearVelocity.y);

        // Wind gusts (Level 6): horizontal push accumulated by WindGust zones this step, then cleared.
        if (_windAccel != 0f)
        {
            _rb.linearVelocity += new Vector2(_windAccel * Time.fixedDeltaTime, 0f);
            _windAccel = 0f;
        }

        // Swim physics (Level 7 water): hold jump to stroke up, release to sink slowly; heavy drag.
        // Gravity is reduced via gravityScale on enter, so buoyancy feels floaty. Skips jump-gravity shaping.
        if (_inWater)
        {
            var v = _rb.linearVelocity;
            if (_jumpHeld) v.y += swimUpAccel * Time.fixedDeltaTime;   // upward stroke while held
            float dragK = 1f - Mathf.Clamp01(waterDrag * Time.fixedDeltaTime);
            v.x *= dragK;                                              // watery horizontal damping
            if (v.y > 0f) v.y *= dragK;                               // damp upward glide
            v.y = Mathf.Clamp(v.y, -swimMaxSink, swimMaxRise);
            _rb.linearVelocity = v;
            return;
        }

        // Better jump gravity
        if (_rb.linearVelocity.y < 0f)
            _rb.linearVelocity += Vector2.up * Physics2D.gravity.y * (fallMultiplier - 1f) * Time.fixedDeltaTime;
        else if (_rb.linearVelocity.y > 0f && !_jumpHeld)
            _rb.linearVelocity += Vector2.up * Physics2D.gravity.y * (lowJumpMultiplier - 1f) * Time.fixedDeltaTime;
    }

    private void ReadKeyboard()
    {
        var kb = Keyboard.current;
        if (kb == null) return;

        float x = 0f;
        if (kb.aKey.isPressed || kb.leftArrowKey.isPressed) x -= 1f;
        if (kb.dKey.isPressed || kb.rightArrowKey.isPressed) x += 1f;
        _keyMove = x;

        if (kb.spaceKey.wasPressedThisFrame || kb.wKey.wasPressedThisFrame || kb.upArrowKey.wasPressedThisFrame)
            PressJump();
        if (kb.spaceKey.wasReleasedThisFrame || kb.wKey.wasReleasedThisFrame || kb.upArrowKey.wasReleasedThisFrame)
            ReleaseJump();
    }

    // ---- Public API (also usable by on-screen mobile buttons) ----
    public void SetMove(float x) => _touchMove = Mathf.Clamp(x, -1f, 1f);

    public void PressJump()
    {
        _jumpBufferCounter = jumpBufferTime;
        _jumpHeld = true;
    }

    public void ReleaseJump() => _jumpHeld = false;

    /// <summary>
    /// Handles falling off the level. Routes through the same damage flow as hazards/KillZones
    /// (costs a life + i-frames, respawns at the checkpoint, Game Over at 0 lives). Falls back to a
    /// plain respawn if there is no GameManager (e.g. a bare test scene).
    /// </summary>
    private void FallToCheckpoint()
    {
        if (GameManager2D.Instance != null)
            GameManager2D.Instance.Damage(this);
        else
            Respawn(Vector3.zero);
    }

    /// <summary>Teleports the player to a position and zeroes velocity (used on death).</summary>
    public void Respawn(Vector3 position)
    {
        transform.position = position;
        if (_rb == null) _rb = GetComponent<Rigidbody2D>();
        _rb.linearVelocity = Vector2.zero;
        _moveInput = 0f;
    }

    /// <summary>Applies an upward bounce (used when stomping an enemy).</summary>
    public void Bounce(float force)
    {
        if (_rb == null) _rb = GetComponent<Rigidbody2D>();
        _rb.linearVelocity = new Vector2(_rb.linearVelocity.x, force);
    }

    /// <summary>Accumulate a horizontal wind push (world units/sec²) for this physics step. Called by WindGust zones.</summary>
    public void AddWind(float accelX) => _windAccel += accelX;

    /// <summary>Enter/leave swim mode (Level 7 water). Lowers gravity for buoyancy; a small breach hop on exit.</summary>
    public void SetWater(bool on)
    {
        if (on == _inWater) return;
        _inWater = on;
        if (_rb == null) _rb = GetComponent<Rigidbody2D>();
        if (on)
        {
            _origGravity = _rb.gravityScale;
            _rb.gravityScale = waterGravityScale;
        }
        else
        {
            _rb.gravityScale = _origGravity;
            // breach: keep a little upward pop so you can hop out onto a ledge
            if (_rb.linearVelocity.y > 0.1f)
                _rb.linearVelocity = new Vector2(_rb.linearVelocity.x, Mathf.Max(_rb.linearVelocity.y, jumpForce * 0.62f));
        }
    }

    /// <summary>Grants temporary damage-immunity and flashes the sprite. Called after taking a hit.</summary>
    public void StartInvincibility(float duration)
    {
        if (!isActiveAndEnabled) return;
        // StopCoroutine(nameof(...)) would NOT match a coroutine started by IEnumerator reference,
        // so cancel via the stored handle instead, then restart cleanly (sprite left visible).
        if (_invincibilityCo != null) StopCoroutine(_invincibilityCo);
        if (_sprite != null) _sprite.enabled = true;
        _invincibilityCo = StartCoroutine(InvincibilityRoutine(duration));
    }

    private IEnumerator InvincibilityRoutine(float duration)
    {
        _invincible = true;
        float t = 0f;
        while (t < duration)
        {
            if (_sprite != null) _sprite.enabled = !_sprite.enabled;
            yield return new WaitForSeconds(flashInterval);
            t += flashInterval;
        }
        if (_sprite != null) _sprite.enabled = true;
        _invincible = false;
    }

    /// <summary>Grants a star power-up: temporary invincibility + a gold glowing pulse.</summary>
    public void StartPower(float duration)
    {
        if (!isActiveAndEnabled) return;
        // Cancel any running pulse via its handle (name-based stop wouldn't match the reference start),
        // so grabbing a second power-up refreshes the timer instead of spawning a duplicate routine.
        if (_powerCo != null) StopCoroutine(_powerCo);
        _powerCo = StartCoroutine(PowerRoutine(duration));
    }

    private IEnumerator PowerRoutine(float duration)
    {
        _powered = true;
        if (powerParticles != null) powerParticles.Play();
        float t = 0f;
        while (t < duration)
        {
            if (_sprite != null)
            {
                float k = Mathf.Sin(t * 18f) * 0.5f + 0.5f;
                _sprite.color = Color.Lerp(Color.white, new Color(1f, 0.82f, 0.15f), k); // gold pulse
            }
            t += Time.deltaTime;
            yield return null;
        }
        if (_sprite != null) _sprite.color = Color.white;
        if (powerParticles != null) powerParticles.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        _powered = false;
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck == null) return;
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(groundCheck.position, new Vector3(groundCheckWidth, groundCheckRadius * 2f, 0f));
    }
}
