using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Juicy press feedback for a UI button: squashes down while held, then pops back up with a little
/// overshoot bounce on release. Uses unscaled time so it still animates while the game is paused
/// (Time.timeScale = 0). Added automatically to every button by <see cref="SoundManager"/>.
/// </summary>
public class ButtonBounce : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] private float pressScale = 0.88f;   // squash while held
    [SerializeField] private float popScale = 1.15f;     // overshoot on release
    [SerializeField] private float downTime = 0.06f;
    [SerializeField] private float popTime = 0.09f;
    [SerializeField] private float settleTime = 0.13f;

    private Vector3 _base;
    private Coroutine _co;

    private void Awake() { _base = transform.localScale; }
    private void OnDisable() { if (_co != null) StopCoroutine(_co); transform.localScale = _base; }

    public void OnPointerDown(PointerEventData e)
    {
        if (_co != null) StopCoroutine(_co);
        _co = StartCoroutine(ScaleTo(_base * pressScale, downTime));
    }

    public void OnPointerUp(PointerEventData e)
    {
        if (_co != null) StopCoroutine(_co);
        _co = StartCoroutine(Pop());
    }

    private IEnumerator ScaleTo(Vector3 target, float dur)
    {
        Vector3 start = transform.localScale;
        float t = 0f;
        while (t < dur)
        {
            t += Time.unscaledDeltaTime;
            transform.localScale = Vector3.Lerp(start, target, t / dur);
            yield return null;
        }
        transform.localScale = target;
    }

    private IEnumerator Pop()
    {
        Vector3 start = transform.localScale;
        Vector3 over = _base * popScale;
        float t = 0f;
        while (t < popTime)
        {
            t += Time.unscaledDeltaTime;
            transform.localScale = Vector3.Lerp(start, over, t / popTime);
            yield return null;
        }
        t = 0f;
        while (t < settleTime)
        {
            t += Time.unscaledDeltaTime;
            float k = t / settleTime;
            k = 1f - (1f - k) * (1f - k); // ease-out settle
            transform.localScale = Vector3.Lerp(over, _base, k);
            yield return null;
        }
        transform.localScale = _base;
    }
}
