using UnityEngine;

/// <summary>
/// Smoothly follows a target (the capybara) on X and Y with an offset and optional Y clamp,
/// so the camera doesn't dip below the level floor.
/// </summary>
public class CameraFollow2D : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private Vector2 offset = new Vector2(0f, 1f);
    [SerializeField] private float smoothTime = 0.15f;
    [SerializeField] private float minY = -1.5f;

    private Vector3 _velocity;

    private void Awake()
    {
        var cfg = GameConfig.Instance;
        if (cfg != null)
        {
            offset = new Vector2(cfg.cameraOffsetX, cfg.cameraOffsetY);
            smoothTime = cfg.cameraSmoothTime;
            // minY is PER-SCENE (each level's floor / basin bottom sits at a different Y — e.g.
            // Level 7's deep water needs -6.5). Do NOT globalize it, or the camera stops following
            // the player down into deeper levels. Keeps the value serialized in each scene.
        }
    }

    public void SetTarget(Transform t) => target = t;

    private void LateUpdate()
    {
        if (target == null) return;
        Vector3 desired = new Vector3(target.position.x + offset.x,
                                      Mathf.Max(target.position.y + offset.y, minY),
                                      transform.position.z);
        transform.position = Vector3.SmoothDamp(transform.position, desired, ref _velocity, smoothTime);
    }
}
