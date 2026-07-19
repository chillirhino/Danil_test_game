using UnityEngine;

/// <summary>
/// Snaps this renderer's world position to the screen-pixel grid every frame.
/// With a static camera this removes the sub-pixel "shimmer / stripes" that crisp
/// pixel-art shows while moving. Put it on the visual child; the parent (logic/collider)
/// keeps its exact unsnapped position, only the rendered sprite snaps.
/// </summary>
[DefaultExecutionOrder(1000)]
public class PixelSnapRender : MonoBehaviour
{
    private void LateUpdate()
    {
        var cam = Camera.main;
        if (cam == null || !cam.orthographic) return;
        float unit = (2f * cam.orthographicSize) / Mathf.Max(1, cam.pixelHeight); // world size of one screen pixel
        Vector3 p = transform.position; // this renderer's own world position (already follows the parent)
        p.x = Mathf.Round(p.x / unit) * unit;
        p.y = Mathf.Round(p.y / unit) * unit;
        transform.position = p;
    }
}
