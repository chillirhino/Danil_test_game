using UnityEngine;
using UnityEngine.UI;

namespace Sirvival
{
    /// <summary>Shows 3 styled upgrade cards on level-up (paused); applying resumes.</summary>
    public class LevelUpPanel : MonoBehaviour
    {
        [SerializeField] private GameObject root;
        [SerializeField] private Button[] buttons;
        [SerializeField] private Text[] nameTexts;
        [SerializeField] private Text[] descTexts;
        [SerializeField] private Text[] rarityTexts;
        [SerializeField] private Image[] rarityStrips;
        [SerializeField] private Image[] icons;
        // icon lookup (id -> sprite), filled by the builder so it works in builds too
        [SerializeField] private string[] iconIds;
        [SerializeField] private Sprite[] iconSprites;

        private void Awake()
        {
            if (RunManager.Instance != null) RunManager.Instance.OnLevelUp += Show;
            if (root != null) root.SetActive(false);
        }

        private void OnDestroy()
        {
            if (RunManager.Instance != null) RunManager.Instance.OnLevelUp -= Show;
        }

        private Sprite IconFor(string id)
        {
            if (iconIds == null) return null;
            for (int i = 0; i < iconIds.Length; i++)
                if (iconIds[i] == id) return i < iconSprites.Length ? iconSprites[i] : null;
            return null;
        }

        private void Show(Upgrade[] choices)
        {
            if (root != null) root.SetActive(true);
            for (int i = 0; i < buttons.Length; i++)
            {
                var u = i < choices.Length ? choices[i] : null;
                buttons[i].gameObject.SetActive(u != null);
                if (u == null) continue;

                if (i < nameTexts.Length && nameTexts[i]) nameTexts[i].text = u.Title;
                if (i < descTexts.Length && descTexts[i]) descTexts[i].text = u.Desc;
                if (i < rarityTexts.Length && rarityTexts[i]) rarityTexts[i].text = Upgrade.RarityLabel(u.Rarity);
                if (i < rarityStrips.Length && rarityStrips[i]) rarityStrips[i].color = Upgrade.RarityColor(u.Rarity);
                if (i < icons.Length && icons[i]) icons[i].sprite = IconFor(u.IconId);

                buttons[i].onClick.RemoveAllListeners();
                var picked = u;
                buttons[i].onClick.AddListener(() =>
                {
                    RunManager.Instance.ChooseUpgrade(picked);
                    if (root != null) root.SetActive(false);
                });
            }
        }
    }
}
