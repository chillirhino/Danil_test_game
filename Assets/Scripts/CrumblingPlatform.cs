using System.Collections;
using UnityEngine;

/// <summary>
/// A platform that crumbles shortly after the player stands on it: it shakes for `crumbleDelay`,
/// then disappears (collider + sprite off) for `respawnDelay`, then comes back.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class CrumblingPlatform : MonoBehaviour
{
    [SerializeField] private float crumbleDelay = 0.55f;
    [SerializeField] private float respawnDelay = 3f;
    [SerializeField] private float shakeAmount = 0.06f;
    [SerializeField] private Transform visual;
    [SerializeField] private SpriteRenderer sr;

    private Collider2D _col;
    private Vector3 _baseVisualPos;
    private bool _triggered;

    private void Awake()
    {
        var cfg = GameConfig.Instance;
        if (cfg != null)
        {
            crumbleDelay = cfg.crumbleDelay; respawnDelay = cfg.crumbleRespawnDelay;
            shakeAmount = cfg.crumbleShakeAmount;
        }
        _col = GetComponent<Collider2D>();
        if (visual == null) visual = transform;
        if (sr == null) sr = GetComponentInChildren<SpriteRenderer>();
        _baseVisualPos = visual.localPosition;
    }

    private void OnCollisionEnter2D(Collision2D c)
    {
        if (_triggered) return;
        var pc = c.collider.GetComponent<PlayerController2D>();
        if (pc == null) return;
        if (c.collider.bounds.min.y > _col.bounds.center.y) // landed on top
            StartCoroutine(Crumble());
    }

    private IEnumerator Crumble()
    {
        _triggered = true;
        SoundManager.Play("crumble");
        float t = 0f;
        while (t < crumbleDelay)
        {
            t += Time.deltaTime;
            visual.localPosition = _baseVisualPos + (Vector3)(Random.insideUnitCircle * shakeAmount);
            yield return null;
        }
        visual.localPosition = _baseVisualPos;
        _col.enabled = false;
        if (sr != null) sr.enabled = false;

        yield return new WaitForSeconds(respawnDelay);

        _col.enabled = true;
        if (sr != null) sr.enabled = true;
        _triggered = false;
    }
}
