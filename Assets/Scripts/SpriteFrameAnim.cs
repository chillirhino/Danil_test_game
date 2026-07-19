using UnityEngine;

/// <summary>
/// Cycles a SpriteRenderer through a set of frames at a fixed fps. Used for the animated
/// wind gusts in Level 6 (replacing the static arrows).
/// </summary>
public class SpriteFrameAnim : MonoBehaviour
{
    [SerializeField] private SpriteRenderer sr;
    [SerializeField] private Sprite[] frames;
    [SerializeField] private float fps = 14f;

    private float _t;

    private void Awake()
    {
        if (sr == null) sr = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        if (sr == null || frames == null || frames.Length == 0) return;
        _t += Time.deltaTime * fps;
        sr.sprite = frames[Mathf.FloorToInt(_t) % frames.Length];
    }
}
