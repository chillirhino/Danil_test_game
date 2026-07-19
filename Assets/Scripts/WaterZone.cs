using UnityEngine;

/// <summary>
/// A body of water for Level 7. While the capybara overlaps this trigger it swims:
/// reduced gravity (buoyancy), heavy drag, hold-jump to stroke up. An optional steady
/// horizontal current can nudge the player (reuses the wind push channel).
/// Use one large collider per contiguous pool so buoyancy doesn't flicker on overlaps.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class WaterZone : MonoBehaviour
{
    [Tooltip("Optional steady horizontal push while submerged (world units/sec^2). 0 = still water.")]
    [SerializeField] private float current = 0f;

    private void Awake()
    {
        var cfg = GameConfig.Instance;
        if (cfg != null)
        {
            current = cfg.waterCurrent;
        }
    }

    private void Reset()
    {
        var c = GetComponent<Collider2D>();
        if (c != null) c.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        var pc = other.GetComponent<PlayerController2D>();
        if (pc != null) { pc.SetWater(true); SoundManager.Play("splash"); }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        var pc = other.GetComponent<PlayerController2D>();
        if (pc != null) pc.SetWater(false);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (Mathf.Approximately(current, 0f)) return;
        var pc = other.GetComponent<PlayerController2D>();
        if (pc != null) pc.AddWind(current);
    }
}
