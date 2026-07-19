using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// Flying harpy boss. Circles the arena, telegraphs, then dives at the player's last position.
/// When it's low (diving / grounded) the player can stomp its head to damage it. 3 stomps to
/// defeat; it speeds up and shortens its wait as HP drops (phase escalation). Side contact
/// damages the player (routed through GameManager2D so i-frames apply).
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class HarpyBoss : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] private int maxHP = 3;

    [Header("Arena (world X range + heights)")]
    [SerializeField] private float arenaLeft = -8f;
    [SerializeField] private float arenaRight = 8f;
    [SerializeField] private float cruiseHeight = 3.5f;
    [SerializeField] private float groundY = -1.3f;      // lowest point of a dive (stomp window height)

    [Header("Movement")]
    [SerializeField] private float cruiseSpeed = 4f;
    [SerializeField] private float diveSpeed = 16f;
    [SerializeField] private float riseSpeed = 9f;

    [Header("Timing")]
    [SerializeField] private float cruiseTime = 2.0f;    // wait between dives (phase 1)
    [SerializeField] private float telegraphTime = 0.8f; // hover-above-player warning
    [SerializeField] private float groundedTime = 1.5f;  // vulnerable window on the ground
    [SerializeField] private float invulnTime = 1.0f;    // after a hit

    [Header("Refs")]
    [SerializeField] private Transform player;
    [SerializeField] private SpriteRenderer sr;
    [SerializeField] private Sprite[] flyFrames;
    [SerializeField] private Sprite[] diveFrames;
    [SerializeField] private float stompBounce = 16f;
    [SerializeField] private bool winOnDefeat = true;   // trigger GameManager2D.WinLevel() on death

    [Header("Phase 2+ feathers")]
    [SerializeField] private Sprite featherSprite;
    [SerializeField] private float featherSpawnY = 5f;

    [Header("Dive telegraph")]
    [SerializeField] private Transform diveShadow;   // ground marker shown where the dive will land
    [SerializeField] private float shadowY = -2.2f;

    public int HP { get; private set; }
    public int MaxHP => maxHP;
    public bool Defeated { get; private set; }
    /// <summary>Fired once when the boss dies (wire the finish / win screen to this).</summary>
    public event Action OnDefeated;

    private enum St { Cruise, Telegraph, Dive, Grounded, Rise, Dead }
    private St _state;
    private float _t;
    private int _dir = 1;
    private float _diveTargetX;
    private bool _invuln;
    private Rigidbody2D _rb;
    private BoxCollider2D _bodyCol;
    private float _animT;

    private void Awake()
    {
        var cfg = GameConfig.Instance;
        if (cfg != null)
        {
            maxHP = cfg.bossMaxHP; arenaLeft = cfg.bossArenaLeft; arenaRight = cfg.bossArenaRight;
            cruiseHeight = cfg.bossCruiseHeight; groundY = cfg.bossGroundY;
            cruiseSpeed = cfg.bossCruiseSpeed; diveSpeed = cfg.bossDiveSpeed; riseSpeed = cfg.bossRiseSpeed;
            cruiseTime = cfg.bossCruiseTime; telegraphTime = cfg.bossTelegraphTime;
            groundedTime = cfg.bossGroundedTime; invulnTime = cfg.bossInvulnTime;
            stompBounce = cfg.bossStompBounce; featherSpawnY = cfg.bossFeatherSpawnY; shadowY = cfg.bossShadowY;
        }
        _rb = GetComponent<Rigidbody2D>();
        _rb.bodyType = RigidbodyType2D.Kinematic;
        _rb.gravityScale = 0f;
        _bodyCol = GetComponent<BoxCollider2D>();
        HP = maxHP;
        if (player == null) { var p = GameObject.FindGameObjectWithTag("Player"); if (p != null) player = p.transform; }
        if (sr == null) sr = GetComponentInChildren<SpriteRenderer>();
        _state = St.Cruise;
        transform.position = new Vector3(arenaLeft, cruiseHeight, 0f);
    }

    // 0 at full HP, → 1 as it nears death; drives the phase escalation.
    private float Phase => maxHP <= 1 ? 0f : Mathf.Clamp01(1f - (HP - 1f) / (maxHP - 1f));
    private float SpeedMul => 1f + Phase * 0.3f;
    private float WaitMul => 1f - Phase * 0.2f;

    private void Update()
    {
        if (Defeated) return;
        _t += Time.deltaTime;
        switch (_state)
        {
            case St.Cruise: Cruise(); break;
            case St.Telegraph: Telegraph(); break;
            case St.Dive: Dive(); break;
            case St.Grounded: if (_t >= groundedTime * WaitMul) { _t = 0f; _state = St.Rise; } break;
            case St.Rise: Rise(); break;
        }
        Animate();
        if (diveShadow != null && diveShadow.gameObject.activeSelf != (_state == St.Telegraph))
            diveShadow.gameObject.SetActive(_state == St.Telegraph);
    }

    private void Cruise()
    {
        float y = cruiseHeight + Mathf.Sin(Time.time * 3f) * 0.25f;
        float x = transform.position.x + _dir * cruiseSpeed * SpeedMul * Time.deltaTime;
        if (x > arenaRight) { x = arenaRight; _dir = -1; }
        else if (x < arenaLeft) { x = arenaLeft; _dir = 1; }
        transform.position = new Vector3(x, y, 0f);
        Face(_dir);
        if (_t >= cruiseTime * WaitMul) { _t = 0f; _state = St.Telegraph; }
    }

    private void Telegraph()
    {
        if (player != null)
        {
            float px = Mathf.Clamp(player.position.x, arenaLeft, arenaRight);
            Face(px >= transform.position.x ? 1 : -1);
            float x = Mathf.MoveTowards(transform.position.x, px, cruiseSpeed * 2f * Time.deltaTime);
            transform.position = new Vector3(x, cruiseHeight, 0f);
        }
        if (diveShadow != null)
        {
            float sx = player != null ? Mathf.Clamp(player.position.x, arenaLeft, arenaRight) : transform.position.x;
            diveShadow.position = new Vector3(sx, shadowY, 0f);
            float s = Mathf.Lerp(0.4f, 1.1f, _t / telegraphTime);
            diveShadow.localScale = new Vector3(s, s * 0.35f, 1f);
        }
        if (sr != null) sr.color = (Mathf.FloorToInt(_t * 10f) % 2 == 0) ? Color.white : new Color(1f, 0.55f, 0.55f);
        if (_t >= telegraphTime)
        {
            if (sr != null) sr.color = Color.white;
            _diveTargetX = player != null ? Mathf.Clamp(player.position.x, arenaLeft, arenaRight) : transform.position.x;
            _t = 0f; _state = St.Dive;
        }
    }

    private void Dive()
    {
        Vector3 target = new Vector3(_diveTargetX, groundY, 0f);
        transform.position = Vector3.MoveTowards(transform.position, target, diveSpeed * SpeedMul * Time.deltaTime);
        if (Vector3.Distance(transform.position, target) < 0.05f) { _t = 0f; _state = St.Grounded; if (HP <= maxHP - 1) SpawnFeathers(); }
    }

    private void SpawnFeathers()
    {
        if (featherSprite == null) return;
        SoundManager.Play("feather"); // one whoosh for the whole volley (not per feather)
        int count = HP <= 1 ? 10 : 6; // more in the final phase
        // spread feathers across the whole visible screen, not just the arena around the boss
        float spawnLeft = arenaLeft, spawnRight = arenaRight;
        var cam = Camera.main;
        if (cam != null && cam.orthographic)
        {
            float halfW = cam.orthographicSize * cam.aspect;
            spawnLeft = cam.transform.position.x - halfW + 0.3f;
            spawnRight = cam.transform.position.x + halfW - 0.3f;
        }
        for (int i = 0; i < count; i++)
        {
            float fx = UnityEngine.Random.Range(spawnLeft, spawnRight);
            var g = new GameObject("Feather");
            g.transform.position = new Vector3(fx, featherSpawnY, 0f);
            g.transform.localScale = Vector3.one * 0.5f;
            var sr = g.AddComponent<SpriteRenderer>();
            sr.sprite = featherSprite;
            sr.sortingOrder = 20;
            var rb = g.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.gravityScale = 0f;
            var col = g.AddComponent<CircleCollider2D>();
            col.isTrigger = true;
            col.radius = 0.7f;
            g.AddComponent<FallingFeather>();
        }
    }

    private void Rise()
    {
        float y = Mathf.MoveTowards(transform.position.y, cruiseHeight, riseSpeed * Time.deltaTime);
        transform.position = new Vector3(transform.position.x, y, 0f);
        if (Mathf.Abs(y - cruiseHeight) < 0.05f) { _t = 0f; _state = St.Cruise; }
    }

    private void OnTriggerEnter2D(Collider2D c) => HandlePlayer(c);
    private void OnTriggerStay2D(Collider2D c) => HandlePlayer(c);

    private void HandlePlayer(Collider2D col)
    {
        if (Defeated) return;
        if (!col.CompareTag("Player")) return;
        var pc = col.GetComponent<PlayerController2D>();
        if (pc == null) return;
        // Reliable stomp test off the actual colliders: the player is "on top" when
        // its feet (collider bottom) are above the boss body's vertical middle.
        float bossMidY = _bodyCol != null ? _bodyCol.bounds.center.y : transform.position.y;
        bool fromAbove = col.bounds.min.y > bossMidY - 0.15f;
        if (_invuln) return;
        if (fromAbove)
        {
            pc.Bounce(stompBounce);
            TakeHit();
        }
        else
        {
            if (GameManager2D.Instance != null) GameManager2D.Instance.Damage(pc);
        }
    }

    private void TakeHit()
    {
        HP--;
        if (HP <= 0) { StartCoroutine(Die()); return; }
        SoundManager.Play("bosshit");
        StartCoroutine(Invuln());
        _t = 0f; _state = St.Rise; // knocked back up, then resumes faster (higher Phase)
    }

    private IEnumerator Invuln()
    {
        _invuln = true;
        float e = 0f;
        while (e < invulnTime)
        {
            if (sr != null) sr.enabled = Mathf.FloorToInt(e * 12f) % 2 == 0;
            e += Time.deltaTime;
            yield return null;
        }
        if (sr != null) sr.enabled = true;
        _invuln = false;
    }

    private IEnumerator Die()
    {
        Defeated = true;
        _state = St.Dead;
        SoundManager.Play("bossdeath");
        float e = 0f;
        while (e < 1.3f)
        {
            transform.position += Vector3.down * 2.2f * Time.deltaTime;
            transform.Rotate(0f, 0f, 220f * Time.deltaTime);
            e += Time.deltaTime;
            yield return null;
        }
        OnDefeated?.Invoke();
        if (winOnDefeat && GameManager2D.Instance != null) GameManager2D.Instance.WinLevel();
        gameObject.SetActive(false);
    }

    private void Face(int d)
    {
        if (sr == null || d == 0) return;
        var s = sr.transform.localScale;
        s.x = Mathf.Abs(s.x) * (d > 0 ? 1f : -1f);
        sr.transform.localScale = s;
    }

    private void Animate()
    {
        if (sr == null) return;
        // nose-down banking while diving (procedural, no frame jitter)
        float targetTilt = _state == St.Dive ? -35f * Mathf.Sign(sr.transform.localScale.x) : 0f;
        sr.transform.localRotation = Quaternion.Lerp(sr.transform.localRotation, Quaternion.Euler(0f, 0f, targetTilt), Time.deltaTime * 8f);
        Sprite[] frames = _state == St.Dive ? diveFrames : flyFrames;
        if (frames == null || frames.Length == 0) return;
        _animT += Time.deltaTime * 10f;
        sr.sprite = frames[Mathf.FloorToInt(_animT) % frames.Length];
    }
}
