using UnityEngine;

/// <summary>
/// Frame-based enemy (ocelot) animation: plays a run loop while the patrol is moving and an idle
/// loop when (near-)stationary. Facing (flipX) is handled by EnemyPatrol. The art carries the
/// motion, so no procedural squash is applied. Drives the SpriteRenderer on this object.
/// </summary>
public class EnemyAnim : MonoBehaviour
{
    [SerializeField] private Rigidbody2D body;
    [SerializeField] private SpriteRenderer sr;

    [Header("Frames")]
    [SerializeField] private Sprite[] idleFrames;
    [SerializeField] private Sprite[] runFrames;

    [Header("Speeds")]
    [SerializeField] private float idleFps = 6f;
    [SerializeField] private float runFps = 12f;
    [SerializeField] private float moveThreshold = 0.1f;
    [SerializeField] private float refSpeed = 2.4f;

    private float _t;

    private void Awake()
    {
        var cfg = GameConfig.Instance;
        if (cfg != null)
        {
            idleFps = cfg.enemyIdleFps; runFps = cfg.enemyRunFps;
        }
        if (body == null) body = GetComponentInParent<Rigidbody2D>();
        if (sr == null) sr = GetComponent<SpriteRenderer>();
    }

    private void LateUpdate()
    {
        float dt = Time.deltaTime;
        if (dt <= 0f || sr == null) return;

        float sp = body != null ? Mathf.Abs(body.linearVelocity.x) : 0f;
        Sprite frame = null;

        if (sp > moveThreshold && runFrames != null && runFrames.Length > 0)
        {
            float fps = runFps * Mathf.Clamp(sp / Mathf.Max(0.1f, refSpeed), 0.5f, 1.6f);
            _t += dt * fps;
            frame = runFrames[((int)_t) % runFrames.Length];
        }
        else if (idleFrames != null && idleFrames.Length > 0)
        {
            _t += dt * idleFps;
            frame = idleFrames[((int)_t) % idleFrames.Length];
        }

        if (frame != null) sr.sprite = frame;
    }
}
