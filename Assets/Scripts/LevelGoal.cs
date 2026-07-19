using UnityEngine;

/// <summary>
/// The end-of-level goal (finish flag). The finish is INERT until every orange in the level has
/// been collected: while locked it dims its sprites and ignores the player; once all oranges are
/// collected it lights up and the next player touch wins the level.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class LevelGoal : MonoBehaviour
{
    [Tooltip("Multiplied onto each sprite's base color while the finish is locked (oranges missing).")]
    [SerializeField] private Color lockedTint = new Color(0.5f, 0.5f, 0.58f, 1f);

    private SpriteRenderer[] _sprites;
    private Color[] _baseColors;
    private bool _unlocked = true;

    private void Reset()
    {
        var col = GetComponent<Collider2D>();
        if (col != null) col.isTrigger = true;
    }

    private void Awake()
    {
        _sprites = GetComponentsInChildren<SpriteRenderer>(true);
        _baseColors = new Color[_sprites.Length];
        for (int i = 0; i < _sprites.Length; i++) _baseColors[i] = _sprites[i].color;
        ApplyTint(false); // start locked; Update flips it once totals are known
    }

    private static bool AllCollected =>
        GameManager2D.Instance == null || GameManager2D.Instance.AllCoinsCollected;

    private void Update()
    {
        bool ok = AllCollected;
        if (ok != _unlocked) ApplyTint(ok);
    }

    private void ApplyTint(bool unlocked)
    {
        _unlocked = unlocked;
        if (_sprites == null) return;
        for (int i = 0; i < _sprites.Length; i++)
            if (_sprites[i] != null)
                _sprites[i].color = unlocked ? _baseColors[i] : _baseColors[i] * lockedTint;
    }

    // Enter covers walking in with everything already collected; Stay covers collecting the
    // last orange while standing on the finish (Enter wouldn't fire again).
    private void OnTriggerEnter2D(Collider2D other) => TryFinish(other);
    private void OnTriggerStay2D(Collider2D other) => TryFinish(other);

    private void TryFinish(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        if (!AllCollected) return; // finish is inert until every orange is collected
        if (GameManager2D.Instance != null) GameManager2D.Instance.WinLevel();
    }
}
