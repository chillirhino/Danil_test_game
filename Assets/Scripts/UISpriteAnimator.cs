using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Cycles a set of sprites on a UI Image at a fixed frame rate (unscaled time, so it keeps
/// playing on menus where Time.timeScale may be 0). Used for the animated level-select islands.
/// </summary>
[RequireComponent(typeof(Image))]
public class UISpriteAnimator : MonoBehaviour
{
    [SerializeField] private Sprite[] frames;
    [SerializeField] private float fps = 4f;

    private Image _img;

    private void Awake() => _img = GetComponent<Image>();

    private void Update()
    {
        if (frames == null || frames.Length == 0 || _img == null) return;
        _img.sprite = frames[((int)(Time.unscaledTime * Mathf.Max(0.01f, fps))) % frames.Length];
    }
}
