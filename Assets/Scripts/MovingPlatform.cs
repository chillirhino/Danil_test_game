using UnityEngine;

/// <summary>
/// A platform that ping-pongs between its start and start+offset. Anything with a
/// PlayerController2D standing on top is carried along by the platform's per-step delta.
/// Uses a kinematic Rigidbody2D so moving the collider is cheap and smooth.
/// </summary>
[RequireComponent(typeof(BoxCollider2D))]
public class MovingPlatform : MonoBehaviour
{
    [SerializeField] private Vector2 offset = new Vector2(4f, 0f); // travel from start
    [SerializeField] private float speed = 2f;                     // units / second

    private BoxCollider2D _box;
    private Rigidbody2D _rb;
    private Vector2 _start, _prev;

    private void Awake()
    {
        var cfg = GameConfig.Instance;
        if (cfg != null)
        {
            speed = cfg.movingPlatformSpeed;
        }
        _box = GetComponent<BoxCollider2D>();
        _rb = GetComponent<Rigidbody2D>();
        if (_rb == null) _rb = gameObject.AddComponent<Rigidbody2D>();
        _rb.bodyType = RigidbodyType2D.Kinematic;
        _rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        _start = transform.position;
        _prev = _start;
    }

    private void FixedUpdate()
    {
        float len = offset.magnitude;
        float t = len > 0.01f ? Mathf.PingPong(Time.time * speed / len, 1f) : 0f;
        Vector2 target = _start + offset * t;
        Vector2 delta = target - _prev;
        _rb.MovePosition(target);
        _prev = target;

        // carry riders standing on top
        Bounds b = _box.bounds;
        Vector2 c = new Vector2(b.center.x, b.max.y + 0.08f);
        Vector2 s = new Vector2(b.size.x * 0.95f, 0.16f);
        var hits = Physics2D.OverlapBoxAll(c, s, 0f);
        for (int i = 0; i < hits.Length; i++)
        {
            if (hits[i].GetComponent<PlayerController2D>() == null) continue;
            var prb = hits[i].attachedRigidbody;
            if (prb != null) prb.position += delta;
        }
    }
}
