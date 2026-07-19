using UnityEngine;

/// <summary>
/// Jump pad. When the player lands on top, launches them upward with bounceForce
/// (reuses PlayerController2D.Bounce). Only triggers from above.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class BouncePad : MonoBehaviour
{
    [SerializeField] private float bounceForce = 20f;
    [SerializeField] private Transform squashVisual; // optional: quick squash on bounce

    private Collider2D _col;

    private void Awake()
    {
        _col = GetComponent<Collider2D>();
        var cfg = GameConfig.Instance;
        if (cfg != null) bounceForce = cfg.bouncePadForce;
    }

    private void OnCollisionEnter2D(Collision2D col)
    {
        var pc = col.collider.GetComponent<PlayerController2D>();
        if (pc == null) return;
        // only when the player is on top of the pad
        if (col.collider.bounds.min.y > _col.bounds.center.y)
        {
            pc.Bounce(bounceForce);
            SoundManager.Play("bounce");
            if (squashVisual != null) StopAllCoroutines();
            if (squashVisual != null) StartCoroutine(Squash());
        }
    }

    private System.Collections.IEnumerator Squash()
    {
        Vector3 baseS = squashVisual.localScale;
        squashVisual.localScale = new Vector3(baseS.x * 1.15f, baseS.y * 0.6f, baseS.z);
        float t = 0f;
        while (t < 0.18f)
        {
            t += Time.deltaTime;
            squashVisual.localScale = Vector3.Lerp(squashVisual.localScale, baseS, t / 0.18f);
            yield return null;
        }
        squashVisual.localScale = baseS;
    }
}
