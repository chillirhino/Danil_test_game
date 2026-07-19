using UnityEngine;

/// <summary>
/// A feather projectile the harpy drops in later phases. Falls, spins, damages the
/// player on contact (via GameManager2D so i-frames apply), and self-destroys off-screen.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class FallingFeather : MonoBehaviour
{
    [SerializeField] private float fallSpeed = 6f;
    [SerializeField] private float spin = 140f;
    [SerializeField] private float killY = -4f;

    private void Awake()
    {
        var cfg = GameConfig.Instance;
        if (cfg != null)
        {
            fallSpeed = cfg.featherFallSpeed; spin = cfg.featherSpin; killY = cfg.featherKillY;
        }
    }

    private void Update()
    {
        transform.position += Vector3.down * fallSpeed * Time.deltaTime;
        transform.Rotate(0f, 0f, spin * Time.deltaTime);
        if (transform.position.y < killY) Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        var pc = other.GetComponent<PlayerController2D>();
        if (pc != null && GameManager2D.Instance != null) GameManager2D.Instance.Damage(pc);
        Destroy(gameObject);
    }
}
