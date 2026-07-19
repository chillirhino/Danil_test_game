using UnityEngine;

namespace Sirvival
{
    /// <summary>Cycles the chef's walk frames (frame-by-frame) and flips to face movement.</summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class ChefAnim : MonoBehaviour
    {
        [SerializeField] private Sprite[] frames;
        [SerializeField] private float fps = 8f;

        private SpriteRenderer _sr;
        private Rigidbody2D _rb;
        private int _i;
        private float _t;

        private void Awake()
        {
            _sr = GetComponent<SpriteRenderer>();
            _rb = GetComponent<Rigidbody2D>();
            if (frames != null && frames.Length > 0) _sr.sprite = frames[0];
        }

        private void Update()
        {
            if (frames == null || frames.Length == 0) return;
            _t += Time.deltaTime * fps;
            if (_t >= 1f)
            {
                _t -= 1f;
                _i = (_i + 1) % frames.Length;
                _sr.sprite = frames[_i];
            }
            if (_rb != null && Mathf.Abs(_rb.linearVelocity.x) > 0.05f)
                _sr.flipX = _rb.linearVelocity.x < 0f;
        }
    }
}
