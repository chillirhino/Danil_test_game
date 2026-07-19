using UnityEngine;

namespace Sirvival
{
    /// <summary>
    /// Silences all audio while the Sirvival scene is active (kills any AudioSource,
    /// including a background music manager). Restores the previous volume on disable
    /// so other scenes aren't left muted.
    /// </summary>
    public class SirvivalAudio : MonoBehaviour
    {
        private float _prevVolume;

        private void OnEnable()
        {
            _prevVolume = AudioListener.volume;
            AudioListener.volume = 0f;
            AudioListener.pause = true;
        }

        private void OnDisable()
        {
            AudioListener.volume = _prevVolume;
            AudioListener.pause = false;
        }
    }
}
