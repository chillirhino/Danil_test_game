using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Idle animation for the main-menu capybara: cycles sprite frames and adds a gentle
/// breathing/sniffing bob. Works on a UI Image (or falls back to a SpriteRenderer).
/// </summary>
public class MenuCapyAnim : MonoBehaviour
{
    [SerializeField] private Sprite[] frames;
    [SerializeField] private float fps = 6f;
    [SerializeField] private float bobAmount = 7f;
    [SerializeField] private float bobSpeed = 1.6f;

    private Image _img;
    private RectTransform _rt;
    private Vector2 _base;
    private float _t;

    private void Awake()
    {
        _img = GetComponent<Image>();
        _rt = GetComponent<RectTransform>();
        if (_rt != null) _base = _rt.anchoredPosition;
    }

    private void Update()
    {
        _t += Time.deltaTime;
        if (_img != null && frames != null && frames.Length > 0)
            _img.sprite = frames[Mathf.FloorToInt(_t * fps) % frames.Length];
        if (_rt != null)
            _rt.anchoredPosition = _base + new Vector2(0f, Mathf.Sin(_t * bobSpeed) * bobAmount);
    }
}
