using UnityEngine;

/// <summary>
/// Fixes "ghost walls": tilemap walls that are visible but let the player pass through because
/// their <see cref="CompositeCollider2D"/> baked geometry went stale (e.g. tiles added by an editor
/// script without a persisted regen, or Generation Type = Manual).
///
/// On every scene load (Play mode) this forces every CompositeCollider2D in the scene to rebuild its
/// geometry from the current tilemap/colliders, so newly-placed wall tiles become solid. Harmless
/// where the geometry is already correct. No component to attach — a static runtime hook does it.
/// </summary>
public static class CompositeColliderAutoRebuild
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void RebuildAll()
    {
        var composites = Object.FindObjectsByType<CompositeCollider2D>(FindObjectsSortMode.None);
        foreach (var cc in composites)
        {
            if (cc != null) cc.GenerateGeometry();
        }
    }
}
