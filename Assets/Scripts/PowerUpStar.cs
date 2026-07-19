using UnityEngine;

/// <summary>
/// A star power-up pickup. Spins and bobs; when the player touches it, grants a temporary
/// star power (PlayerController2D.StartPower) and disappears.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class PowerUpStar : MonoBehaviour
{
    [SerializeField] private float duration = 6f;
    [SerializeField] private float spinSpeed = 120f;
    [SerializeField] private float bobHeight = 0.15f;
    [SerializeField] private float bobSpeed = 2f;
    [SerializeField] private ParticleSystem collectBurst; // juicy pop when eaten

    private Vector3 _base;
    private float _phase;

    private void Awake()
    {
        var cfg = GameConfig.Instance;
        if (cfg != null)
        {
            duration = cfg.starDuration; spinSpeed = cfg.starSpinSpeed;
            bobHeight = cfg.starBobHeight; bobSpeed = cfg.starBobSpeed;
        }
    }

    private void Start()
    {
        _base = transform.position;
        _phase = _base.x;
    }

    private void Update()
    {
        transform.Rotate(0f, 0f, spinSpeed * Time.deltaTime);
        transform.position = _base + Vector3.up * (Mathf.Sin((Time.time + _phase) * bobSpeed) * bobHeight);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        var pc = other.GetComponent<PlayerController2D>();
        if (pc == null) return;
        if (collectBurst != null)
        {
            collectBurst.transform.SetParent(null, true); // detach so it outlives the pickup
            collectBurst.Play();
            Destroy(collectBurst.gameObject, 2f);
        }
        pc.StartPower(duration);
        SoundManager.Play("powerup");
        Destroy(gameObject);
    }
}
