using UnityEngine;
using UnityEngine.UI;

namespace Sirvival
{
    /// <summary>Reads RunManager + ChefHealth each frame and paints the HUD.</summary>
    public class Hud : MonoBehaviour
    {
        [SerializeField] private Image hpFill;     // Image type = Filled
        [SerializeField] private Image xpFill;
        [SerializeField] private Text levelText;
        [SerializeField] private Text timerText;
        [SerializeField] private Text killsText;
        [SerializeField] private ChefHealth health;

        private void Update()
        {
            var rm = RunManager.Instance;
            if (rm == null) return;
            if (health != null && hpFill != null) hpFill.fillAmount = health.Current / Mathf.Max(1f, health.Max);
            if (xpFill != null) xpFill.fillAmount = rm.XpToNext > 0 ? (float)rm.Xp / rm.XpToNext : 0f;
            if (levelText != null) levelText.text = "LV " + rm.Level;
            if (killsText != null) killsText.text = "☠ " + rm.Kills;
            if (timerText != null) { int s = (int)rm.Elapsed; timerText.text = (s / 60) + ":" + (s % 60).ToString("00"); }
        }
    }
}
