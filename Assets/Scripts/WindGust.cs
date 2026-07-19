using UnityEngine;

/// <summary>
/// A wind zone (Level 6 mechanic). While the player is inside its trigger, pushes them
/// horizontally by <see cref="windForce"/>. Can "gust" — toggle on/off on a cycle — for
/// timing challenges. Optional arrow renderers brighten while the wind blows and dim when it stops.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class WindGust : MonoBehaviour
{
    [SerializeField] private float windForce = 50f;   // horizontal push (accel) applied to the player
    [SerializeField] private float direction = 2f;    // +1 = push right, -1 = push left

    [Header("Gusting")]
    [SerializeField] private bool gusting = false;
    [SerializeField] private float gustOn = 1.6f;
    [SerializeField] private float gustOff = 1.2f;

    [Header("Visual")]
    [SerializeField] private SpriteRenderer[] arrows;

    private float _t;
    private bool _active = true;
    private bool _prevActive = true;
    private bool _playerIn;

    private void Awake()
    {
        var cfg = GameConfig.Instance;
        if (cfg != null)
        {
            windForce = cfg.windForce; direction = cfg.windDirection;
            gustOn = cfg.windGustOn; gustOff = cfg.windGustOff;
        }
    }

    private void Update()
    {
        if (gusting)
        {
            _t += Time.deltaTime;
            float cycle = Mathf.Max(0.1f, gustOn + gustOff);
            _active = (_t % cycle) < gustOn;
        }
        if (_active && !_prevActive && _playerIn) SoundManager.Play("wind"); // gust kicks in while player is inside
        _prevActive = _active;
        if (arrows != null)
        {
            float a = _active ? (0.55f + 0.45f * Mathf.Abs(Mathf.Sin(Time.time * 3f))) : 0.12f;
            foreach (var s in arrows)
                if (s != null) { var c = s.color; c.a = a; s.color = c; }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        _playerIn = true;
        if (_active) SoundManager.Play("wind"); // whoosh on stepping into the wind
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player")) _playerIn = false;
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (!_active) return;
        if (!other.CompareTag("Player")) return;
        var pc = other.GetComponent<PlayerController2D>();
        if (pc != null) pc.AddWind(direction * Mathf.Abs(windForce));
    }
}
