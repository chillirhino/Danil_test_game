using UnityEngine;

/// <summary>
/// A hazard (e.g. spikes). Touching it respawns the player at the last respawn point.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class Hazard : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other) => TryKill(other);
    private void OnCollisionEnter2D(Collision2D col) => TryKill(col.collider);

    private void TryKill(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        var pc = other.GetComponent<PlayerController2D>();
        if (pc == null) return;
        if (GameManager2D.Instance != null)
            GameManager2D.Instance.Damage(pc);   // costs a life + i-frames; Game Over at 0
        else
            pc.Respawn(Vector3.zero);
    }
}
