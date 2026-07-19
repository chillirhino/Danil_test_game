using UnityEngine;

/// <summary>
/// Purely decorative fish for the Level 7 water level: drifts back and forth within a
/// horizontal range with a gentle vertical bob, flipping to face its travel direction.
/// No collider / no damage — atmosphere only, never blocks the player.
/// </summary>
public class FishSwim : MonoBehaviour
{
    [SerializeField] private float speed = 1.2f;
    [SerializeField] private float range = 3f;
    [SerializeField] private float bobAmp = 0.22f;
    [SerializeField] private float bobFreq = 2f;

    private Vector3 _origin;
    private float _dir = 1f;
    private float _t;
    private SpriteRenderer _sr;

    private void Start()
    {
        _origin = transform.position;
        _sr = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        _t += Time.deltaTime;
        var p = transform.position;
        p.x += _dir * speed * Time.deltaTime;
        if (p.x > _origin.x + range) _dir = -1f;
        else if (p.x < _origin.x - range) _dir = 1f;
        p.y = _origin.y + Mathf.Sin(_t * bobFreq) * bobAmp;
        transform.position = p;
        if (_sr != null) _sr.flipX = _dir < 0f;
    }
}
