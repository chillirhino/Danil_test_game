using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace Sirvival
{
    /// <summary>Shows the run summary (4 stat cards) on death; restart / menu buttons.</summary>
    public class GameOverPanel : MonoBehaviour
    {
        [SerializeField] private GameObject root;
        [SerializeField] private Text timeText;
        [SerializeField] private Text killsText;
        [SerializeField] private Text coinsText;
        [SerializeField] private Text levelText;
        [SerializeField] private Button playAgain;
        [SerializeField] private Button menu;

        private void Awake()
        {
            if (RunManager.Instance != null) RunManager.Instance.OnGameOver += Show;
            if (root != null) root.SetActive(false);
            if (playAgain != null) playAgain.onClick.AddListener(Restart);
            if (menu != null) menu.onClick.AddListener(Restart); // no menu scene yet -> restart
        }

        private void OnDestroy()
        {
            if (RunManager.Instance != null) RunManager.Instance.OnGameOver -= Show;
        }

        private void Restart()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        private void Show()
        {
            var rm = RunManager.Instance;
            int s = (int)rm.Elapsed;
            if (timeText != null) timeText.text = (s / 60) + ":" + (s % 60).ToString("00");
            if (killsText != null) killsText.text = rm.Kills.ToString();
            if (coinsText != null) coinsText.text = "0";
            if (levelText != null) levelText.text = rm.Level.ToString();
            if (root != null) root.SetActive(true);
        }
    }
}
