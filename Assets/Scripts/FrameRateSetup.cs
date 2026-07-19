using UnityEngine;

/// <summary>
/// Mobile builds default to Application.targetFrameRate = 30, which makes the game feel choppy.
/// This runs automatically at startup (before the first scene loads) and lifts the cap to 60.
/// </summary>
public static class FrameRateSetup
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Init()
    {
        // vSyncCount must be 0 for targetFrameRate to be honored
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 60;
    }
}
