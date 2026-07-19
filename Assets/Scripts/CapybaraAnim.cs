using UnityEngine;

/// <summary>
/// Frame-based capybara animation: plays an idle loop when standing, a run loop when moving on the
/// ground, and maps jump-sheet frames to vertical velocity while airborne. The sprite art already
/// carries all the motion, so no procedural squash/tilt is applied. Facing (flipX) is handled by
/// PlayerController2D. Drives the SpriteRenderer found under this Visual object.
/// </summary>
public class CapybaraAnim : MonoBehaviour
{
    [SerializeField] private Rigidbody2D body;
    [SerializeField] private PlayerController2D controller;
    [SerializeField] private SpriteRenderer sr;

    [Header("Animation frames")]
    [SerializeField] private Sprite[] idleFrames;
    [SerializeField] private Sprite[] runFrames;
    [SerializeField] private Sprite[] jumpFrames;   // ordered: crouch/launch -> rise -> apex -> fall -> land
    [SerializeField] private Sprite[] swimFrames;   // paddle loop played while in a water zone

    [Header("Speeds")]
    [SerializeField] private float idleFps = 6f;
    [SerializeField] private float runFps = 12f;
    [SerializeField] private float swimFps = 10f;          // paddle loop speed while swimming
    [SerializeField] private float moveThreshold = 0.2f;   // |vx| below this = idle
    [SerializeField] private float moveSpeedRef = 5f;      // run animation speeds up toward this

    [Header("Water look")]
    [SerializeField] private Color waterTint = new Color(0.62f, 0.76f, 0.92f); // cool tint applied while swimming so the capybara reads as submerged

    private float _t;
    private bool _waterTinted;

    private void Awake()
    {
        var cfg = GameConfig.Instance;
        if (cfg != null)
        {
            idleFps = cfg.capyIdleFps; runFps = cfg.capyRunFps; swimFps = cfg.capySwimFps;
            moveThreshold = cfg.capyMoveThreshold; moveSpeedRef = cfg.capyMoveSpeedRef;
        }
        if (body == null) body = GetComponentInParent<Rigidbody2D>();
        if (controller == null) controller = GetComponentInParent<PlayerController2D>();
        if (sr == null) sr = GetComponentInChildren<SpriteRenderer>();
        // the frames self-animate; make sure no residual squash/tilt from a previous rig remains
        transform.localScale = Vector3.one;
        transform.localRotation = Quaternion.identity;
    }

    private void LateUpdate()
    {
        float dt = Time.deltaTime;
        if (dt <= 0f || sr == null) return;

        float vx = body ? body.linearVelocity.x : 0f;
        float vy = body ? body.linearVelocity.y : 0f;
        bool grounded = controller && controller.IsGrounded;
        bool inWater = controller && controller.IsInWater;

        Sprite frame = null;

        if (inWater && swimFrames != null && swimFrames.Length > 0)
        {
            // Continuous paddle loop while submerged; speeds up a touch with movement.
            float speed = Mathf.Sqrt(vx * vx + vy * vy);
            float fps = swimFps * Mathf.Clamp(speed / Mathf.Max(0.1f, moveSpeedRef) + 0.6f, 0.6f, 1.6f);
            _t += dt * fps;
            frame = swimFrames[((int)_t) % swimFrames.Length];
        }
        else if (!grounded && jumpFrames != null && jumpFrames.Length > 0)
        {
            float f = Mathf.InverseLerp(10f, -14f, vy);   // rising -> 0, falling -> 1
            int idx = Mathf.Clamp(Mathf.RoundToInt(f * (jumpFrames.Length - 1)), 0, jumpFrames.Length - 1);
            frame = jumpFrames[idx];
            _t = 0f;
        }
        else if (grounded && Mathf.Abs(vx) > moveThreshold && runFrames != null && runFrames.Length > 0)
        {
            float fps = runFps * Mathf.Clamp(Mathf.Abs(vx) / Mathf.Max(0.1f, moveSpeedRef), 0.5f, 1.5f);
            _t += dt * fps;
            frame = runFrames[((int)_t) % runFrames.Length];
        }
        else if (idleFrames != null && idleFrames.Length > 0)
        {
            _t += dt * idleFps;
            frame = idleFrames[((int)_t) % idleFrames.Length];
        }

        if (frame != null) sr.sprite = frame;

        // Underwater look: a cool-blue tint while swimming so the capybara reads as submerged.
        // (The water body is nearly opaque and renders behind the player, so it can't tint it
        // directly.) Yields to the star-power gold pulse so it never fights that effect.
        bool powered = controller && controller.IsPowered;
        if (inWater && !powered) { sr.color = waterTint; _waterTinted = true; }
        else if (_waterTinted && !powered) { sr.color = Color.white; _waterTinted = false; }
    }
}
