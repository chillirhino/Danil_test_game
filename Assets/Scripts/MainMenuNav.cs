using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>Main-menu navigation: PLAY loads the level, SETTINGS toggles a panel, EXIT quits.</summary>
public class MainMenuNav : MonoBehaviour
{
    [SerializeField] private Button playButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button exitButton;
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private Button settingsCloseButton;
    [SerializeField] private Button creditsButton;
    [SerializeField] private GameObject creditsPanel;
    [SerializeField] private Button creditsCloseButton;
    [SerializeField] private string levelScene = "Platformer";

    private void Awake()
    {
        Time.timeScale = 1f;
        if (playButton != null) playButton.onClick.AddListener(Play);
        if (settingsButton != null) settingsButton.onClick.AddListener(OpenSettings);
        if (exitButton != null) exitButton.onClick.AddListener(Quit);
        if (settingsCloseButton != null) settingsCloseButton.onClick.AddListener(CloseSettings);
        if (settingsPanel != null) settingsPanel.SetActive(false);
        if (creditsButton != null) creditsButton.onClick.AddListener(OpenCredits);
        if (creditsCloseButton != null) creditsCloseButton.onClick.AddListener(CloseCredits);
        if (creditsPanel != null) creditsPanel.SetActive(false);
    }

    public void Play() => SceneManager.LoadScene("LevelSelect");
    public void OpenSettings() { if (settingsPanel != null) settingsPanel.SetActive(true); }
    public void CloseSettings() { if (settingsPanel != null) settingsPanel.SetActive(false); }
    public void OpenCredits() { if (creditsPanel != null) creditsPanel.SetActive(true); }
    public void CloseCredits() { if (creditsPanel != null) creditsPanel.SetActive(false); }

    public void Quit()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
