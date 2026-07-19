using UnityEngine;

namespace Sirvival
{
    /// <summary>
    /// Simple top-down smooth camera follow (no offset, no floor clamp) — kept separate
    /// from the platformer's <c>CameraFollow2D</c>, which is coupled to GameConfig and
    /// clamps minY for a side-scroller.
    /// </summary>
    public class SurvivorsCameraFollow : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private float smoothTime = 0.12f;

        private Vector3 _vel;

        public void SetTarget(Transform t) => target = t;

        private void LateUpdate()
        {
            if (target == null) return;
            Vector3 desired = new Vector3(target.position.x, target.position.y, transform.position.z);
            transform.position = Vector3.SmoothDamp(transform.position, desired, ref _vel, smoothTime);
        }
    }
}
