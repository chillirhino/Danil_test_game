using UnityEngine;

/// <summary>
/// Central tuning config for the capybara platformer. Create ONE asset at
/// <c>Assets/Resources/GameConfig.asset</c> (menu: Create ▸ Capybara ▸ Game Config) and edit all
/// gameplay values there — or via the web editor in <c>tools/config-editor/</c>. Scripts read from
/// <see cref="Instance"/> at Awake and fall back to their own serialized defaults if no config asset
/// exists (so nothing breaks). Only GLOBAL mechanic values live here; per-object layout (platform
/// travel offsets, enemy patrol range, individual placements) stays on the scene objects.
/// </summary>
[CreateAssetMenu(fileName = "GameConfig", menuName = "Capybara/Game Config", order = 0)]
public class GameConfig : ScriptableObject
{
    [Header("Player — Movement")]
    public float walkSpeed = 3.1f;
    public float moveSpeed = 5f;
    public float speedRampTime = 0.7f;
    public float driftSpeedThreshold = 2.8f;
    public float acceleration = 60f;
    public float deceleration = 70f;

    [Header("Player — Jump")]
    public float jumpForce = 12f;
    public int maxAirJumps = 1;
    public float coyoteTime = 0.1f;
    public float jumpBufferTime = 0.1f;
    public float lowJumpMultiplier = 3f;
    public float fallMultiplier = 2.5f;

    [Header("Player — Ground Check")]
    public float groundCheckRadius = 0.12f;
    public float groundCheckWidth = 0.9f;

    [Header("Player — Water (swim)")]
    public float waterGravityScale = 0.42f;
    public float swimUpAccel = 24f;
    public float swimMaxRise = 3.4f;
    public float swimMaxSink = 2.6f;
    public float waterDrag = 2.4f;
    public float swimMoveMul = 0.72f;

    [Header("Player — Misc")]
    public float fallDeathY = -20f;
    public float flashInterval = 0.1f;

    [Header("Enemy")]
    public float enemySpeed = 2f;
    public float enemyChaseSpeed = 4.2f;
    public float enemyAggroRadius = 5.5f;
    public float enemyAggroHeight = 3f;
    public float enemyBounceForce = 14f;
    public float enemyEyeOffsetX = 0.28f;
    public float enemyWallCheck = 0.12f;
    public float enemyLedgeLookAhead = 0.25f;
    public float enemyLedgeDrop = 0.7f;

    [Header("Boss (Harpy)")]
    public int bossMaxHP = 3;
    public float bossArenaLeft = -8f;
    public float bossArenaRight = 8f;
    public float bossCruiseHeight = 3.5f;
    public float bossGroundY = -1.3f;
    public float bossCruiseSpeed = 4f;
    public float bossDiveSpeed = 16f;
    public float bossRiseSpeed = 9f;
    public float bossCruiseTime = 2f;
    public float bossTelegraphTime = 0.8f;
    public float bossGroundedTime = 1.5f;
    public float bossInvulnTime = 1f;
    public float bossStompBounce = 16f;
    public float bossFeatherSpawnY = 5f;
    public float bossShadowY = -2.2f;

    [Header("Falling Feather (boss projectile)")]
    public float featherFallSpeed = 6f;
    public float featherSpin = 140f;
    public float featherKillY = -4f;

    [Header("Moving Platform")]
    public float movingPlatformSpeed = 2f;

    [Header("Crumbling Platform")]
    public float crumbleDelay = 0.55f;
    public float crumbleRespawnDelay = 3f;
    public float crumbleShakeAmount = 0.06f;

    [Header("Collectibles")]
    public int coinValue = 1;
    public float coinSpinSpeed = 0f;   // 0 = flat orange sprites only bob; spinning a 2D sprite around Y makes it go edge-on/invisible
    public float coinBobHeight = 0.12f;
    public float coinBobSpeed = 2f;
    public float starDuration = 6f;
    public float starSpinSpeed = 120f;
    public float starBobHeight = 0.15f;
    public float starBobSpeed = 2f;

    [Header("Environment")]
    public float waterCurrent = 0f;
    public float windForce = 50f;
    public float windDirection = 2f;
    public float windGustOn = 1.6f;
    public float windGustOff = 1.2f;

    [Header("Camera")]
    public float cameraOffsetX = 0f;
    public float cameraOffsetY = 1f;
    public float cameraSmoothTime = 0.15f;
    public float cameraMinY = -1.5f;

    [Header("Checkpoint")]
    public float checkpointLoweredY = -1.7f;
    public float checkpointRaisedY = 0f;
    public float checkpointRaiseTime = 0.35f;
    public float checkpointRespawnYOffset = 1f;

    [Header("Animation")]
    public float capyIdleFps = 6f;
    public float capyRunFps = 12f;
    public float capyMoveThreshold = 0.2f;
    public float capyMoveSpeedRef = 5f;
    public float capySwimFps = 10f;
    public float enemyIdleFps = 6f;
    public float enemyRunFps = 12f;

    [Header("Input (touch)")]
    public float touchDeadZone = 30f;
    public float touchSplitX01 = 0.5f;
    public float dragDeadZone = 12f;
    public float dragMaxLead = 160f;

    [Header("BouncePad")]
    public float bouncePadForce = 20f;

    [Header("Lives")]
    public int maxLives = 3;
    public float invincibilityTime = 1.2f;
    public float heartBreakFrameTime = 0.08f;

    // ---- singleton access (loaded once from Resources) ----
    private static GameConfig _instance;
    private static bool _searched;

    /// <summary>The active config, or null if no asset exists in Resources (scripts then use their own defaults).</summary>
    public static GameConfig Instance
    {
        get
        {
            if (_instance == null && !_searched)
            {
                _instance = Resources.Load<GameConfig>("GameConfig");
                _searched = true;
            }
            return _instance;
        }
    }
}
