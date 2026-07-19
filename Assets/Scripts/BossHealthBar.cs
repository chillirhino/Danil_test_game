using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Drives a boss HP bar: a Filled (Horizontal) fill Image whose fillAmount eases toward the
/// boss's HP ratio (so damage drains smoothly instead of snapping). Hides the bar on defeat.
/// </summary>
public class BossHealthBar : MonoBehaviour
{
    [SerializeField] private HarpyBoss boss;
    [SerializeField] private Image fill;      // Image: Type=Filled, Horizontal, Origin=Left
    [SerializeField] private GameObject bar;  // visible root to hide on death
    [SerializeField] private float drainSpeed = 0.5f; // fillAmount units per second

    private float _displayed = 1f;

    private void Update()
    {
        if (boss == null) return;
        float target = boss.MaxHP > 0 ? Mathf.Clamp01((float)boss.HP / boss.MaxHP) : 0f;
        _displayed = Mathf.MoveTowards(_displayed, target, drainSpeed * Time.deltaTime);
        if (fill != null) fill.fillAmount = _displayed;
        if (boss.Defeated && bar != null && bar.activeSelf && _displayed <= 0.001f) bar.SetActive(false);
    }
}
