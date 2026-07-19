using UnityEngine;

/// <summary>
/// A collectible coin. Spins around the vertical axis (a 3D "coin flip" look) and gently bobs,
/// and is collected when the player touches it. Works for both a 3D mesh coin and a sprite.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class Coin : MonoBehaviour
{
    [SerializeField] private int value = 1;

    /// <summary>Point value of this collectible (used to compute the level total).</summary>
    public int Value => value;

    [SerializeField] private float spinSpeed = 160f;   // degrees/sec around Y
    [SerializeField] private float bobHeight = 0.12f;
    [SerializeField] private float bobSpeed = 2f;

    private Vector3 _basePos;
    private float _phase;

    private void Awake()
    {
        var cfg = GameConfig.Instance;
        if (cfg != null)
        {
            value = cfg.coinValue; spinSpeed = cfg.coinSpinSpeed;
            bobHeight = cfg.coinBobHeight; bobSpeed = cfg.coinBobSpeed;
        }
    }

    private void Start()
    {
        // use LOCAL position so a coin parented to a moving platform bobs relative to it
        // (and keeps riding the platform) instead of snapping back to a fixed world point
        _basePos = transform.localPosition;
        _phase = transform.position.x; // desync coins so they don't animate in lockstep
    }

    private void Update()
    {
        transform.Rotate(0f, spinSpeed * Time.deltaTime, 0f, Space.World);
        float bob = Mathf.Sin((Time.time + _phase) * bobSpeed) * bobHeight;
        transform.localPosition = _basePos + Vector3.up * bob;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        if (GameManager2D.Instance != null) GameManager2D.Instance.AddCoin(value);
        Destroy(gameObject);
    }
}
