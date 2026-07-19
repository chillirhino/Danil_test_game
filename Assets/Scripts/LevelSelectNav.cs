using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

/// <summary>
/// Level-select screen. Each entry maps a level number to a scene; a button is interactable only
/// when that level is unlocked (LevelProgress), and locked buttons show a dimmed "lock" state.
/// </summary>
public class LevelSelectNav : MonoBehaviour
{
    [System.Serializable]
    public class Entry
    {
        public int level = 1;
        public string sceneName = "Platformer";
        public Button button;
        public GameObject lockIcon;        // shown when locked
        public GameObject completedIcon;   // shown when the level has been beaten
    }

    [SerializeField] private Entry[] entries;
    [SerializeField] private Button backButton;
    [SerializeField] private string mainMenuScene = "MainMenu";

    private void Awake()
    {
        Time.timeScale = 1f;
        foreach (var e in entries)
        {
            if (e.button == null) continue;
            // an entry with no scene (e.g. levels 6-7 that don't exist yet) is always locked
            bool available = !string.IsNullOrEmpty(e.sceneName);
            bool unlocked = available && LevelProgress.IsUnlocked(e.level);
            e.button.interactable = unlocked;
            if (e.lockIcon != null) e.lockIcon.SetActive(!unlocked);
            // "beaten" checkmark: only on an available level the player has actually completed
            if (e.completedIcon != null)
                e.completedIcon.SetActive(available && LevelProgress.IsCompleted(e.level));
            var scene = e.sceneName;
            if (unlocked) e.button.onClick.AddListener(() => SceneManager.LoadScene(scene));
        }
        if (backButton != null) backButton.onClick.AddListener(() => SceneManager.LoadScene(mainMenuScene));
    }

    private void Update()
    {
        var kb = Keyboard.current;
        if (kb != null && kb.escapeKey.wasPressedThisFrame) SceneManager.LoadScene(mainMenuScene);
    }
}
