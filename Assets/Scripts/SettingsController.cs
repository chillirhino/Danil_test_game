using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Simple persistent settings: master volume + music/sfx toggles, saved to PlayerPrefs.
/// Applies AudioListener.volume so it takes effect game-wide even without a mixer.
/// </summary>
public class SettingsController : MonoBehaviour
{
    [SerializeField] private Slider volumeSlider;
    [SerializeField] private Text musicLabel;
    [SerializeField] private Text sfxLabel;

    private bool _music = true, _sfx = true;
    private float _vol = 0.8f;

    private void Awake()
    {
        _vol = PlayerPrefs.GetFloat("MasterVolume", 0.8f);
        _music = PlayerPrefs.GetInt("Music", 1) == 1;
        _sfx = PlayerPrefs.GetInt("SFX", 1) == 1;
        Apply();
    }

    private void OnEnable()
    {
        if (volumeSlider != null)
        {
            volumeSlider.SetValueWithoutNotify(_vol);
            volumeSlider.onValueChanged.RemoveListener(SetVolume);
            volumeSlider.onValueChanged.AddListener(SetVolume);
        }
        RefreshLabels();
    }

    public void SetVolume(float v) { _vol = v; PlayerPrefs.SetFloat("MasterVolume", v); Apply(); }
    public void ToggleMusic() { _music = !_music; PlayerPrefs.SetInt("Music", _music ? 1 : 0); Apply(); RefreshLabels(); }
    public void ToggleSFX() { _sfx = !_sfx; PlayerPrefs.SetInt("SFX", _sfx ? 1 : 0); Apply(); RefreshLabels(); }

    private void Apply()
    {
        // master volume affects everything; the music on/off is handled by SoundManager (mutes the
        // music source only), and the SFX toggle gates SoundManager's one-shots — so turning music
        // off no longer silences the sound effects.
        AudioListener.volume = _vol;
        if (SoundManager.Instance != null) SoundManager.Instance.ApplyAudioSettings();
        PlayerPrefs.Save();
    }

    private void RefreshLabels()
    {
        if (musicLabel != null) musicLabel.text = "MUSIC: " + (_music ? "ON" : "OFF");
        if (sfxLabel != null) sfxLabel.text = "SFX: " + (_sfx ? "ON" : "OFF");
    }
}
