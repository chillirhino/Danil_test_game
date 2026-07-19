using UnityEngine;

/// <summary>
/// Calm patrol back and forth (turning at its range limit / walls / ledges). But when the player
/// comes within <see cref="aggroRadius"/>, the enemy goes aggressive: red eyes light up and it
/// charges toward the player at <see cref="chaseSpeed"/> (still refusing to run off a ledge).
/// Stomp from above destroys it; a side hit costs the player a life.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class EnemyPatrol : MonoBehaviour
{
    [SerializeField] private float speed = 2f;
    [SerializeField] private float range = 4f;        // half-range from start X
    [SerializeField] private float bounceForce = 14f;
    [SerializeField] private SpriteRenderer sprite;

    [Header("Aggro")]
    [SerializeField] private float aggroRadius = 5.5f;   // player within this (horizontally) triggers the charge
    [SerializeField] private float aggroHeight = 3f;     // ...and within this vertical band
    [SerializeField] private float chaseSpeed = 4.2f;    // speed while charging the player
    [SerializeField] private GameObject redEyes;         // glow overlay, shown only while aggressive
    [SerializeField] private float eyeOffsetX = 0.28f;   // how far toward the face the eyes sit

    [Header("Obstacle checks")]
    [SerializeField] private LayerMask groundMask;     // defaults to "Ground" if unset
    [SerializeField] private float wallCheck = 0.12f;  // extra reach past the body for walls
    [SerializeField] private float ledgeLookAhead = 0.25f;
    [SerializeField] private float ledgeDrop = 0.7f;   // turn if no ground within this drop ahead

    private Rigidbody2D _rb;
    private BoxCollider2D _box;
    private Transform _player;
    private float _startX;
    private int _dir = 1;
    private bool _aggro;

    private void Awake()
    {
        var cfg = GameConfig.Instance;
        if (cfg != null)
        {
            speed = cfg.enemySpeed; chaseSpeed = cfg.enemyChaseSpeed;
            aggroRadius = cfg.enemyAggroRadius; aggroHeight = cfg.enemyAggroHeight;
            bounceForce = cfg.enemyBounceForce;
            eyeOffsetX = cfg.enemyEyeOffsetX; wallCheck = cfg.enemyWallCheck;
            ledgeLookAhead = cfg.enemyLedgeLookAhead; ledgeDrop = cfg.enemyLedgeDrop;
        }
        _rb = GetComponent<Rigidbody2D>();
        _box = GetComponent<BoxCollider2D>();
        _startX = transform.position.x;
        if (sprite == null) sprite = GetComponentInChildren<SpriteRenderer>();
        if (groundMask.value == 0) groundMask = LayerMask.GetMask("Ground");
        if (sprite != null) sprite.flipX = _dir < 0;
        if (redEyes != null) redEyes.SetActive(false);
    }

    private void FixedUpdate()
    {
        UpdateAggro();

        if (_aggro)
        {
            float dx = _player.position.x - transform.position.x;
            _dir = dx >= 0f ? 1 : -1;
            if (sprite != null) sprite.flipX = _dir < 0;
            // charge, but stop at a wall/ledge instead of falling off
            _rb.linearVelocity = new Vector2(BlockedAhead() ? 0f : _dir * chaseSpeed, _rb.linearVelocity.y);
        }
        else
        {
            _rb.linearVelocity = new Vector2(_dir * speed, _rb.linearVelocity.y);
            bool flip = (_dir > 0 && transform.position.x > _startX + range)
                     || (_dir < 0 && transform.position.x < _startX - range)
                     || BlockedAhead();
            if (flip) Flip();
        }

        if (redEyes != null)
        {
            redEyes.SetActive(_aggro);
            if (_aggro)
            {
                var lp = redEyes.transform.localPosition;
                redEyes.transform.localPosition = new Vector3(Mathf.Abs(eyeOffsetX) * _dir, lp.y, lp.z);
            }
        }
    }

    private void UpdateAggro()
    {
        if (_player == null)
        {
            var p = GameObject.FindWithTag("Player");
            if (p != null) _player = p.transform;
        }
        _aggro = false;
        if (_player != null)
        {
            float dx = Mathf.Abs(_player.position.x - transform.position.x);
            float dy = Mathf.Abs(_player.position.y - transform.position.y);
            _aggro = dx <= aggroRadius && dy <= aggroHeight;
        }
    }

    /// <summary>Wall/step or ledge/drop just ahead in the current facing direction.</summary>
    private bool BlockedAhead()
    {
        if (_box == null) return false;
        Vector2 center = _rb.position + _box.offset;
        float halfW = _box.size.x * 0.5f;
        float halfH = _box.size.y * 0.5f;
        Vector2 wallOrigin = new Vector2(center.x, center.y - halfH + 0.15f);
        if (Physics2D.Raycast(wallOrigin, new Vector2(_dir, 0f), halfW + wallCheck, groundMask).collider != null)
            return true;
        Vector2 probe = new Vector2(center.x + _dir * (halfW + ledgeLookAhead), center.y - halfH + 0.1f);
        return Physics2D.Raycast(probe, Vector2.down, ledgeDrop, groundMask).collider == null;
    }

    private void Flip()
    {
        _dir = -_dir;
        if (sprite != null) sprite.flipX = _dir < 0;
    }

    private void OnCollisionEnter2D(Collision2D col)
    {
        if (!col.collider.CompareTag("Player")) return;
        var pc = col.collider.GetComponent<PlayerController2D>();
        if (pc == null) return;

        bool stomped = col.collider.bounds.min.y > transform.position.y;
        if (stomped)
        {
            pc.Bounce(bounceForce);
            SoundManager.Play("stomp");
            Destroy(gameObject);
        }
        else if (pc.IsPowered)
        {
            SoundManager.Play("stomp");
            Destroy(gameObject);
        }
        else if (GameManager2D.Instance != null)
        {
            GameManager2D.Instance.Damage(pc);
        }
        else
        {
            pc.Respawn(Vector3.zero);
        }
    }
}
