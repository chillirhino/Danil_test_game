using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

/// <summary>
/// Pause / escape menu. Toggled with Esc or the corner pause button. Freezes the game
/// (Time.timeScale = 0) and shows a popup with Resume / Restart / Quit. Ignores input while
/// the level is already won/lost (GameManager2D.GameEnded).
/// </summary>
public class PauseMenu : MonoBehaviour
{
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject openButton;   // corner button, hidden while paused
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button restartButton;
    [SerializeField] private Button mainMenuButton;
    [SerializeField] private Button quitButton;
    [SerializeField] private Button openButtonBtn;
    [SerializeField] private string mainMenuScene = "MainMenu";

    public bool IsPaused { get; private set; }

    private void Awake()
    {
        if (resumeButton != null) resumeButton.onClick.AddListener(Resume);
        if (restartButton != null) restartButton.onClick.AddListener(Restart);
        if (mainMenuButton != null) mainMenuButton.onClick.AddListener(GoToMainMenu);
        if (quitButton != null) quitButton.onClick.AddListener(Quit);
        if (openButtonBtn != null) openButtonBtn.onClick.AddListener(Pause);
        if (pausePanel != null) pausePanel.SetActive(false);
    }

    private void Update()
    {
        var kb = Keyboard.current;
        if (kb != null && kb.escapeKey.wasPressedThisFrame) Toggle();
    }

    private bool Ended => GameManager2D.Instance != null && GameManager2D.Instance.GameEnded;

    public void Toggle() { if (IsPaused) Resume(); else Pause(); }

    public void Pause()
    {
        if (Ended || IsPaused) return;
        IsPaused = true;
        Time.timeScale = 0f;
        if (pausePanel != null) pausePanel.SetActive(true);
        if (openButton != null) openButton.SetActive(false);
    }

    public void Resume()
    {
        IsPaused = false;
        Time.timeScale = 1f;
        if (pausePanel != null) pausePanel.SetActive(false);
        if (openButton != null) openButton.SetActive(true);
    }

    public void Restart()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void GoToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuScene);
    }

    public void Quit()
    {
        Time.timeScale = 1f;
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
