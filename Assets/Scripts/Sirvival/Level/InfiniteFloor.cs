using UnityEngine;

namespace Sirvival
{
    /// <summary>
    /// Makes a Tiled SpriteRenderer floor look infinite: every frame it snaps to the
    /// camera in whole-tile steps. Because the tile pattern's period equals
    /// <see cref="tileWorldSize"/>, a whole-tile jump is visually seamless, so the
    /// floor appears to extend forever in every direction.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class InfiniteFloor : MonoBehaviour
    {
        [SerializeField] private float tileWorldSize = 1f;
        private Transform _cam;

        private void Awake()
        {
            if (Camera.main != null) _cam = Camera.main.transform;
        }

        private void LateUpdate()
        {
            if (_cam == null)
            {
                if (Camera.main == null) return;
                _cam = Camera.main.transform;
            }
            float t = Mathf.Max(0.001f, tileWorldSize);
            float x = Mathf.Round(_cam.position.x / t) * t;
            float y = Mathf.Round(_cam.position.y / t) * t;
            transform.position = new Vector3(x, y, transform.position.z);
        }
    }
}
