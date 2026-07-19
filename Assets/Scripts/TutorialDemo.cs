using UnityEngine;

/// <summary>
/// Little in-world demo animation for a tutorial billboard's capybara icon:
/// Move = waddles left/right (and flips to face travel); Jump = hops with a squash on landing.
/// Uses unscaled time so it keeps animating regardless of Time.timeScale.
/// </summary>
public class TutorialDemo : MonoBehaviour
{
    public enum Mode { Move, Jump, DoubleJump }

    [SerializeField] private Mode mode = Mode.Move;
    [SerializeField] private float moveAmplitude = 0.55f;
    [SerializeField] private float jumpHeight = 0.6f;
    [SerializeField] private float speed = 2.2f;

    [Header("Sprite frames (optional loop)")]
    [SerializeField] private Sprite[] frames;   // if set, cycles these on the SpriteRenderer
    [SerializeField] private float fps = 9f;

    private Vector3 _base;
    private Vector3 _baseScale;
    private SpriteRenderer _sr;

    private void Awake()
    {
        _base = transform.localPosition;
        _baseScale = transform.localScale;
        _sr = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        float t = Time.unscaledTime * speed;
        if (mode == Mode.Move)
        {
            float x = Mathf.Sin(t) * moveAmplitude;
            float bob = Mathf.Abs(Mathf.Sin(t * 2f)) * 0.06f;
            transform.localPosition = _base + new Vector3(x, bob, 0f);
            if (_sr != null) _sr.flipX = Mathf.Cos(t) < 0f; // face the way it's walking
        }
        else if (mode == Mode.Jump)
        {
            float y = Mathf.Max(0f, Mathf.Sin(t)) * jumpHeight;
            transform.localPosition = _base + new Vector3(0f, y, 0f);
            float grounded = 1f - Mathf.Clamp01(y / (jumpHeight * 0.25f)); // 1 near ground
            transform.localScale = new Vector3(_baseScale.x * (1f + grounded * 0.12f),
                                               _baseScale.y * (1f - grounded * 0.12f),
                                               _baseScale.z);
        }
        else // DoubleJump: a low first hop, then a higher second hop (reads as double jump)
        {
            float p = Mathf.Repeat(t, Mathf.PI * 2f) / (Mathf.PI * 2f); // 0..1
            float y;
            if (p < 0.42f) y = Mathf.Sin(p / 0.42f * Mathf.PI) * jumpHeight * 0.5f;        // hop 1 (low)
            else if (p < 0.9f) y = Mathf.Sin((p - 0.42f) / 0.48f * Mathf.PI) * jumpHeight;  // hop 2 (high)
            else y = 0f;
            transform.localPosition = _base + new Vector3(0f, y, 0f);
            float grounded = 1f - Mathf.Clamp01(y / (jumpHeight * 0.25f));
            transform.localScale = new Vector3(_baseScale.x * (1f + grounded * 0.10f),
                                               _baseScale.y * (1f - grounded * 0.10f),
                                               _baseScale.z);
        }

        if (frames != null && frames.Length > 0 && _sr != null)
            _sr.sprite = frames[((int)(Time.unscaledTime * fps)) % frames.Length];
    }
}
