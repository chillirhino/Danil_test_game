using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

/// <summary>
/// Tracks oranges collected and the player's respawn point, and updates a simple UI counter.
/// Also owns the win/lose flow (finish screen, restart). Singleton via GameManager2D.Instance.
/// </summary>
public class GameManager2D : MonoBehaviour
{
    public static GameManager2D Instance;

    public int Coins { get; private set; }
    public int TotalCoins { get; private set; }

    /// <summary>True once every collectible in the level has been picked up (or the level has none).
    /// The finish stays inert until this is true.</summary>
    public bool AllCoinsCollected => TotalCoins <= 0 || Coins >= TotalCoins;
    public int Lives { get; private set; }
    public Vector3 RespawnPoint;
    public bool GameEnded { get; private set; }

    [SerializeField] private Text coinText;
    [SerializeField] private GameObject winPanel;

    [Header("Lives")]
    [SerializeField] private int maxLives = 3;
    [SerializeField] private Image[] hearts;          // one icon per life, hidden as lives are lost
    [SerializeField] private GameObject losePanel;    // "GAME OVER" screen
    [SerializeField] private float invincibilityTime = 1.2f;

    [Header("Heart break animation")]
    [SerializeField] private Sprite[] heartFrames;    // [0]=full, [1..]=cracking frames played on loss
    [SerializeField] private float heartBreakFrameTime = 0.08f;

    [Header("Level")]
    [SerializeField] private int levelNumber = 1;     // used to unlock the next level on win
    [SerializeField] private string levelSelectScene = "LevelSelect";
    [SerializeField] private string mainMenuScene = "MainMenu";
    private bool _won;

    [Header("End-screen buttons")]
    [SerializeField] private Button winNextButton;
    [SerializeField] private Button winRestartButton;
    [SerializeField] private Button winMenuButton;
    [SerializeField] private Button loseRestartButton;
    [SerializeField] private Button loseMenuButton;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        var cfg = GameConfig.Instance;
        if (cfg != null) { maxLives = cfg.maxLives; invincibilityTime = cfg.invincibilityTime; heartBreakFrameTime = cfg.heartBreakFrameTime; }
        Lives = maxLives;
        Time.timeScale = 1f; // reset in case we returned from a paused end screen

        if (winNextButton != null) winNextButton.onClick.AddListener(GoToLevelSelect);
        if (winRestartButton != null) winRestartButton.onClick.AddListener(RestartLevel);
        if (winMenuButton != null) winMenuButton.onClick.AddListener(GoToMainMenu);
        if (loseRestartButton != null) loseRestartButton.onClick.AddListener(RestartLevel);
        if (loseMenuButton != null) loseMenuButton.onClick.AddListener(GoToMainMenu);
    }

    public void GoToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuScene);
    }

    private void Start()
    {
        // Respawn at the player's start until a checkpoint is reached.
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) RespawnPoint = player.transform.position;

        // Count every collectible present at level start so the UI can show "collected / total".
        TotalCoins = 0;
        foreach (var c in FindObjectsByType<Coin>(FindObjectsInactive.Include))
            TotalCoins += c.Value;

        UpdateUI();
        UpdateHearts();
    }

    private void Update()
    {
        if (!GameEnded) return;
        var kb = Keyboard.current;
        if (kb != null && kb.rKey.wasPressedThisFrame)
        {
            if (_won) GoToLevelSelect();  // after a win, R returns to the level-select screen
            else RestartLevel();          // after Game Over, R retries the level
        }
    }

    public void GoToLevelSelect()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(levelSelectScene);
    }

    public void AddCoin(int amount = 1)
    {
        Coins += amount;
        UpdateUI();
        SoundManager.Play("coin");
    }

    public void SetRespawn(Vector3 p) => RespawnPoint = p;
    public void SetCoinText(Text t) { coinText = t; UpdateUI(); }
    public void SetWinPanel(GameObject g) => winPanel = g;
    public void SetHearts(Image[] h) { hearts = h; UpdateHearts(); }
    public void SetLosePanel(GameObject g) => losePanel = g;

    /// <summary>
    /// Central damage entry point (called by hazards and enemy side-hits). Costs one life,
    /// grants i-frames and respawns; at zero lives triggers Game Over. No-op while immune or ended.
    /// </summary>
    public void Damage(PlayerController2D pc)
    {
        if (GameEnded) return;
        if (pc != null && pc.IsImmune) return;   // i-frames or star power

        int lost = Lives - 1;   // index of the heart being lost
        Lives--;
        bool dead = Lives <= 0;
        if (!dead) SoundManager.Play("hurt");   // the fatal hit plays "lose" via GameOver instead

        bool canAnimate = lost >= 0 && hearts != null && lost < hearts.Length && hearts[lost] != null
                          && heartFrames != null && heartFrames.Length > 1;
        if (canAnimate)
            StartCoroutine(BreakHeart(hearts[lost], dead)); // GameOver fires after it finishes breaking
        else
        {
            UpdateHearts();
            if (dead) GameOver();
        }

        if (!dead && pc != null)
        {
            pc.Respawn(RespawnPoint);
            pc.StartInvincibility(invincibilityTime);
        }
    }

    /// <summary>Plays the crack frames on a single heart, hides it, then ends the game if it was the last one.</summary>
    private IEnumerator BreakHeart(Image img, bool dead)
    {
        for (int f = 1; f < heartFrames.Length; f++)
        {
            if (heartFrames[f] != null) img.sprite = heartFrames[f];
            yield return new WaitForSecondsRealtime(heartBreakFrameTime); // realtime: survives Time.timeScale = 0
        }
        yield return new WaitForSecondsRealtime(0.06f);
        img.enabled = false;
        if (dead) GameOver();
    }

    /// <summary>Called by the LevelGoal when the player reaches the finish.</summary>
    public void WinLevel()
    {
        if (GameEnded) return;
        GameEnded = true;
        _won = true;
        LevelProgress.Complete(levelNumber);   // unlock the next level
        if (winPanel != null) winPanel.SetActive(true);
        SoundManager.Play("win");
        Time.timeScale = 0f; // freeze the world on the victory screen
    }

    private void GameOver()
    {
        GameEnded = true;
        if (losePanel != null) losePanel.SetActive(true);
        SoundManager.Play("lose");
        Time.timeScale = 0f; // freeze the world on the game-over screen
    }

    public void RestartLevel()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void UpdateUI()
    {
        if (coinText != null) coinText.text = Coins + "/" + TotalCoins;
    }

    private void UpdateHearts()
    {
        if (hearts == null) return;
        for (int i = 0; i < hearts.Length; i++)
            if (hearts[i] != null) hearts[i].enabled = i < Lives;
    }
}
