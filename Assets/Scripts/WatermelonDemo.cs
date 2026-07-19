using UnityEngine;

/// <summary>
/// Looping tutorial animation for the Level 3 billboard: the capybara walks to a watermelon,
/// eats it (turns gold / powered), then walks into an ocelot which flips over and fades out.
/// Uses unscaled time so it plays even while the game is paused. Call Evaluate() to pose a frame.
/// </summary>
public class WatermelonDemo : MonoBehaviour
{
    [SerializeField] private SpriteRenderer capy;
    [SerializeField] private Transform watermelon;
    [SerializeField] private Transform ocelot;

    [Header("Layout (local X)")]
    [SerializeField] private float leftX = -0.60f;
    [SerializeField] private float ocelotStopOffset = 0.20f; // how far left of the ocelot the capy stops
    [SerializeField] private float baseY = -0.14f;
    [SerializeField] private Color powerColor = new Color(1f, 0.82f, 0.15f);

    [Header("Timing (s)")]
    [SerializeField] private float walk1 = 1.4f;
    [SerializeField] private float eatPause = 0.45f;
    [SerializeField] private float walk2 = 1.4f;
    [SerializeField] private float dieDur = 0.55f;
    [SerializeField] private float endHold = 0.6f;

    private SpriteRenderer _melonSr, _ocelotSr;
    private float _melonX, _ocelotX, _ocelotBaseY;

    private void Awake() { Cache(); }

    private void Cache()
    {
        if (watermelon != null) { _melonSr = watermelon.GetComponent<SpriteRenderer>(); _melonX = watermelon.localPosition.x; }
        if (ocelot != null)     { _ocelotSr = ocelot.GetComponent<SpriteRenderer>();     _ocelotX = ocelot.localPosition.x; _ocelotBaseY = ocelot.localPosition.y; }
    }

    private void Update() { Evaluate(Time.unscaledTime % Cycle); }

    private float Cycle => walk1 + eatPause + walk2 + dieDur + endHold;

    public void Evaluate(float t)
    {
        if (_melonSr == null || _ocelotSr == null) Cache();

        float stopX = _ocelotX - ocelotStopOffset;
        float capX;
        bool powered, melonOn, ocelotOn = true;
        float flip = 0f, alpha = 1f, drop = 0f;

        if (t < walk1)                                   // walk to watermelon
        {
            capX = Mathf.Lerp(leftX, _melonX, t / walk1); powered = false; melonOn = true;
        }
        else if (t < walk1 + eatPause)                   // eat: melon gone, turn gold
        {
            capX = _melonX; powered = true; melonOn = false;
        }
        else if (t < walk1 + eatPause + walk2)           // walk to ocelot
        {
            capX = Mathf.Lerp(_melonX, stopX, (t - walk1 - eatPause) / walk2); powered = true; melonOn = false;
        }
        else if (t < walk1 + eatPause + walk2 + dieDur)  // ocelot dies (flip + fade + drop)
        {
            float k = (t - walk1 - eatPause - walk2) / dieDur;
            capX = stopX; powered = true; melonOn = false;
            flip = 180f * k; alpha = 1f - k; drop = -0.12f * k;
        }
        else                                             // hold: ocelot gone
        {
            capX = stopX; powered = true; melonOn = false; ocelotOn = false;
        }

        if (capy != null)
        {
            float bob = Mathf.Abs(Mathf.Sin(t * 8f)) * 0.02f;
            capy.transform.localPosition = new Vector3(capX, baseY + bob, capy.transform.localPosition.z);
            capy.color = powered ? powerColor : Color.white;
            capy.flipX = false;
        }
        if (_melonSr != null) _melonSr.enabled = melonOn;
        if (ocelot != null)
        {
            if (_ocelotSr != null)
            {
                _ocelotSr.enabled = ocelotOn && alpha > 0.02f;
                var c = _ocelotSr.color; c.a = alpha; _ocelotSr.color = c;
            }
            ocelot.localRotation = Quaternion.Euler(0f, 0f, flip);
            ocelot.localPosition = new Vector3(_ocelotX, _ocelotBaseY + drop, ocelot.localPosition.z);
        }
    }
}