using System.Collections;
using UnityEngine;

/// <summary>
/// A checkpoint. Starts as a bare pole with no flag. When the player first touches it, it becomes
/// the active respawn point (GameManager2D.RespawnPoint) and the flag quickly raises up the pole.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class Checkpoint : MonoBehaviour
{
    [SerializeField] private Transform flag;          // the flag sprite that slides up the pole
    [SerializeField] private SpriteRenderer flagSr;   // its renderer (hidden until activated)
    [SerializeField] private float loweredY = -1.7f;  // flag start (down / hidden behind the base)
    [SerializeField] private float raisedY = 0f;      // flag end (top of the pole)
    [SerializeField] private float raiseTime = 0.35f; // quick raise
    [SerializeField] private float respawnYOffset = 1f;

    private bool _activated;

    private void Awake()
    {
        var cfg = GameConfig.Instance;
        if (cfg != null)
        {
            loweredY = cfg.checkpointLoweredY; raisedY = cfg.checkpointRaisedY;
            raiseTime = cfg.checkpointRaiseTime; respawnYOffset = cfg.checkpointRespawnYOffset;
        }
        if (flag != null)
            flag.localPosition = new Vector3(flag.localPosition.x, loweredY, flag.localPosition.z);
        if (flagSr != null) flagSr.enabled = false; // just the pole until reached
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (_activated || !other.CompareTag("Player")) return;
        _activated = true;
        if (GameManager2D.Instance != null)
            GameManager2D.Instance.SetRespawn(transform.position + Vector3.up * respawnYOffset);
        if (flagSr != null) flagSr.enabled = true;
        SoundManager.Play("checkpoint");
        StartCoroutine(Raise());
    }

    private IEnumerator Raise()
    {
        if (flag == null) yield break;
        float x = flag.localPosition.x, z = flag.localPosition.z, t = 0f;
        while (t < raiseTime)
        {
            t += Time.deltaTime;
            float k = Mathf.Clamp01(t / raiseTime);
            k = 1f - (1f - k) * (1f - k); // ease-out: snappy start, settle at top
            flag.localPosition = new Vector3(x, Mathf.Lerp(loweredY, raisedY, k), z);
            yield return null;
        }
        flag.localPosition = new Vector3(x, raisedY, z);
    }
}
