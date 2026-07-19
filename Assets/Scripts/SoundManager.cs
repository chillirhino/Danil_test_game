using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// Global sound system. Auto-boots before the first scene, persists across scenes, and plays
/// retro SFX (loaded from Resources/Audio) plus a looping music track. Respects the SFX / Music
/// toggles and master volume stored in PlayerPrefs by <see cref="SettingsController"/>.
/// Call <c>SoundManager.Play("jump")</c> from anywhere; it is null-safe.
/// </summary>
public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

    private AudioSource _sfx;
    private AudioSource _music;
    private readonly Dictionary<string, AudioClip> _clips = new Dictionary<string, AudioClip>();

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Bootstrap()
    {
        if (Instance != null) return;
        var go = new GameObject("SoundManager");
        go.AddComponent<SoundManager>();
    }

    private static bool SfxOn => PlayerPrefs.GetInt("SFX", 1) == 1;
    private static bool MusicOn => PlayerPrefs.GetInt("Music", 1) == 1;
    private static float MasterVol => PlayerPrefs.GetFloat("MasterVolume", 0.8f);

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        _sfx = gameObject.AddComponent<AudioSource>();
        _sfx.playOnAwake = false;
        _sfx.volume = 0.5f; // SFX a touch quieter than the music tracks
        _music = gameObject.AddComponent<AudioSource>();
        _music.playOnAwake = false;
        _music.loop = true;
        _music.volume = 0.45f;

        foreach (var c in Resources.LoadAll<AudioClip>("Audio"))
            _clips[c.name] = c;

        AudioListener.volume = MasterVol;
        _music.mute = !MusicOn;
        EnsureAudioListener();
        ApplySceneMusic(SceneManager.GetActiveScene().name);

        SceneManager.sceneLoaded += OnSceneLoaded;
        HookButtons();
    }

    private void OnDestroy()
    {
        if (Instance == this) SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene s, LoadSceneMode m)
    {
        EnsureAudioListener();
        ApplySceneMusic(s.name);
        HookButtons();
    }

    /// <summary>Guarantee exactly one AudioListener — the level scenes ship without one, so nothing (music or SFX) is audible there. Adds one to the main camera when the scene has none.</summary>
    private void EnsureAudioListener()
    {
        if (FindObjectsByType<AudioListener>(FindObjectsInactive.Exclude, FindObjectsSortMode.None).Length > 0) return;
        var cam = Camera.main;
        if (cam != null && cam.GetComponent<AudioListener>() == null)
            cam.gameObject.AddComponent<AudioListener>();
        else if (cam == null)
            gameObject.AddComponent<AudioListener>();
    }

    /// <summary>Pick the music track for a scene (menu gets its own track) and switch only when it changes.</summary>
    private void ApplySceneMusic(string sceneName)
    {
        // MainMenu → "menu_music"; every other scene → "music_<scene>" (e.g. music_platformer, music_level5).
        // Falls back to the generic "music" clip if a per-scene track isn't present.
        string wanted = sceneName == "MainMenu" ? "menu_music" : "music_" + sceneName.ToLower();
        if (!_clips.ContainsKey(wanted)) wanted = "music";
        if (!_clips.TryGetValue(wanted, out var clip) || clip == null) return;
        if (_music.clip == clip && _music.isPlaying) return;
        _music.clip = clip;
        _music.Play();
    }

    /// <summary>Add a click sound + press-bounce to every UI Button in the freshly loaded scene.</summary>
    private void HookButtons()
    {
        foreach (var b in FindObjectsByType<Button>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            b.onClick.AddListener(PlayClick);
            if (b.GetComponent<ButtonBounce>() == null) b.gameObject.AddComponent<ButtonBounce>();
        }
    }

    private void PlayClick() { PlaySfx("click"); }

    /// <summary>Re-read the Music/Volume prefs and apply them live (called by SettingsController).</summary>
    public void ApplyAudioSettings()
    {
        AudioListener.volume = MasterVol;
        if (_music != null) _music.mute = !MusicOn;
    }

    private void PlaySfx(string name)
    {
        if (!SfxOn) return;
        if (_clips.TryGetValue(name, out var c) && c != null)
            _sfx.PlayOneShot(c);
    }

    /// <summary>Play a one-shot SFX by name (from Resources/Audio). Null-safe if the manager isn't up yet.</summary>
    public static void Play(string name)
    {
        if (Instance != null) Instance.PlaySfx(name);
    }
}
