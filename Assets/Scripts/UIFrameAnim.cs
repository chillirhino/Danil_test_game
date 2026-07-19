using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Cycles a UI <see cref="Image"/> through a set of sprite frames at a given fps.
/// Ping-pong mode plays the frames forward then backward for a smooth idle/breathing loop.
/// Uses unscaled time so it keeps animating while the game is paused.
/// </summary>
public class UIFrameAnim : MonoBehaviour
{
    public Sprite[] frames;
    public float fps = 5f;
    public bool pingPong = true;

    private Image _img;
    private float _t;

    private void Awake() { _img = GetComponent<Image>(); }

    private void Update()
    {
        if (_img == null || frames == null || frames.Length == 0) return;
        _t += Time.unscaledDeltaTime * fps;
        int n = frames.Length;
        int idx;
        if (pingPong && n > 1)
        {
            int period = n * 2 - 2;
            int p = (int)_t % period;
            idx = p < n ? p : period - p;
        }
        else idx = (int)_t % n;
        _img.sprite = frames[idx];
    }
}
