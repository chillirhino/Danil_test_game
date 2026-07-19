using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

/// <summary>
/// Main-menu ambience: animates a flat background by overlaying moving elements — drifting leaves,
/// twinkling sparkles, flying birds, a scrolling waterfall shimmer, a pulsing sun glow, a breathing
/// capybara, and a subtle tilt parallax. All FX are created at runtime as UI Images under the Canvas.
/// </summary>
public class MenuFx : MonoBehaviour
{
    [Header("FX sprites")]
    public Sprite glowSprite, shimmerSprite, leafSprite, sparkleSprite, birdSprite, butterflySprite;

    [Header("Scene refs")]
    public RectTransform bg;    // background image (parallax)
    public RectTransform capy;  // capybara root (breathing)

    [Header("Feature positions (canvas coords, 1920x1080, center origin)")]
    public Vector2 sunPos = new Vector2(180, 275);
    public Vector2 waterfallPos = new Vector2(168, -95);

    [Header("Counts")]
    public int leaves = 13;
    public int sparkles = 12;
    public int birds = 3;
    public int butterflies = 5;

    RectTransform _layer;
    Image _glow;
    readonly List<RectTransform> _leaf = new(); readonly List<Vector2> _leafVel = new(); readonly List<float> _leafSpin = new();
    readonly List<RectTransform> _spark = new(); readonly List<float> _sparkPh = new(); readonly List<float> _sparkSp = new();
    readonly List<RectTransform> _bird = new(); readonly List<float> _birdSp = new(); readonly List<float> _birdPh = new();
    readonly List<RectTransform> _shim = new(); readonly List<float> _shimY0 = new(); readonly List<Vector2> _shimBase = new();
    readonly List<RectTransform> _bfly = new(); readonly List<Vector2> _bflyHome = new(); readonly List<float> _bflyPh = new();
    Vector3 _capyScale; Vector2 _capyPos; Vector2 _bgPos, _layerPos;
    float _t; Vector2 _parallax;
    System.Random _rng = new System.Random(7);
    float R(float a, float b) => a + (float)_rng.NextDouble() * (b - a);

    Image MakeImg(string name, Sprite sp, Vector2 pos, float width)
    {
        var go = new GameObject(name, typeof(RectTransform));
        var rt = go.GetComponent<RectTransform>();
        rt.SetParent(_layer, false);
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f); rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos;
        float h = (sp != null && sp.rect.width > 0) ? width * sp.rect.height / sp.rect.width : width;
        rt.sizeDelta = new Vector2(width, h);
        var img = go.AddComponent<Image>(); img.sprite = sp; img.raycastTarget = false; img.preserveAspect = true;
        return img;
    }
    Vector2 RandTop() => new Vector2(R(-980, 980), R(560, 760));
    Vector2 RandAny() => new Vector2(R(-940, 940), R(-460, 500));

    void Start()
    {
        var canvas = GetComponentInParent<Canvas>(); if (canvas == null) canvas = FindFirstObjectByType<Canvas>();
        var go = new GameObject("MenuFxLayer", typeof(RectTransform));
        _layer = go.GetComponent<RectTransform>(); _layer.SetParent(canvas.transform, false);
        _layer.anchorMin = Vector2.zero; _layer.anchorMax = Vector2.one; _layer.offsetMin = Vector2.zero; _layer.offsetMax = Vector2.zero;
        _layer.SetSiblingIndex(1); // above background, below capy/title/buttons

        _glow = MakeImg("SunGlow", glowSprite, sunPos, 300); _glow.color = new Color(1f, 0.97f, 0.8f, 0.4f);

        // water glints twinkling on the pool at the base of the waterfall (measured pool center)
        Vector2 pool = new Vector2(154f, -194f);
        for (int i = 0; i < 6; i++)
        {
            var pos = pool + new Vector2(R(-72, 72), R(-6, 6));
            var im = MakeImg("WaterGlint" + i, glowSprite, pos, 30);
            im.preserveAspect = false; im.rectTransform.sizeDelta = new Vector2(R(24, 42), R(7, 11)); im.color = new Color(1f, 1f, 1f, 0.3f);
            _shim.Add(im.rectTransform); _shimY0.Add(R(0, 6.28f)); _shimBase.Add(pos);
        }

        for (int i = 0; i < leaves; i++) { var im = MakeImg("Leaf" + i, leafSprite, RandTop(), 26); im.color = Color.Lerp(new Color(0.5f,0.7f,0.3f), new Color(0.85f,0.6f,0.25f), (float)_rng.NextDouble()); var rt = im.rectTransform; rt.localEulerAngles = new Vector3(0, 0, R(0, 360)); _leaf.Add(rt); _leafVel.Add(new Vector2(R(-22, -8), R(-46, -28))); _leafSpin.Add(R(-40, 40)); }

        for (int i = 0; i < sparkles; i++) { var im = MakeImg("Spark" + i, sparkleSprite, RandAny(), R(12, 24)); _spark.Add(im.rectTransform); _sparkPh.Add(R(0, 6.28f)); _sparkSp.Add(R(1.5f, 3.5f)); }

        for (int i = 0; i < birds; i++) { var im = MakeImg("Bird" + i, birdSprite, new Vector2(R(-980, 980), R(170, 430)), R(36, 54)); im.color = new Color(0.32f, 0.24f, 0.17f, 0.9f); _bird.Add(im.rectTransform); _birdSp.Add(R(38, 72)); _birdPh.Add(R(0, 6.28f)); }

        for (int i = 0; i < butterflies; i++) { var home = new Vector2(R(-820, 840), R(-380, 90)); var im = MakeImg("Bfly" + i, butterflySprite, home, R(22, 34)); im.color = Color.Lerp(new Color(1f, 0.55f, 0.35f), new Color(0.55f, 0.6f, 1f), (float)_rng.NextDouble()); _bfly.Add(im.rectTransform); _bflyHome.Add(home); _bflyPh.Add(R(0, 6.28f)); }

        if (capy != null) { _capyScale = capy.localScale; _capyPos = capy.anchoredPosition; }
        if (bg != null) _bgPos = bg.anchoredPosition;
        _layerPos = _layer.anchoredPosition;

        if (Accelerometer.current != null) InputSystem.EnableDevice(Accelerometer.current); // for tilt parallax on phone
    }

    void Update()
    {
        float dt = Time.unscaledDeltaTime; _t += dt;

        // capybara: the breathing is done by real frames (UIFrameAnim). Here only a gentle
        // occasional hop + parallax so the scale isn't fought over.
        if (capy != null)
        {
            float cyc = _t % 5f;
            float sniff = cyc < 0.5f ? Mathf.Sin(cyc / 0.5f * Mathf.PI) : 0f;
            capy.anchoredPosition = _capyPos + new Vector2(0, sniff * 7f) + _parallax * 1.4f;
        }

        // sun glow pulse
        if (_glow != null)
        {
            float p = 0.5f + 0.5f * Mathf.Sin(_t * 1.2f);
            _glow.color = new Color(1f, 0.97f, 0.8f, 0.28f + 0.22f * p);
            _glow.rectTransform.localScale = Vector3.one * (0.94f + 0.10f * p);
        }

        // leaves drift down + sway + spin, recycle at top
        for (int i = 0; i < _leaf.Count; i++)
        {
            var rt = _leaf[i]; var pos = rt.anchoredPosition;
            pos += _leafVel[i] * dt; pos.x += Mathf.Sin(_t * 1.5f + i) * 14f * dt;
            rt.Rotate(0, 0, _leafSpin[i] * dt);
            if (pos.y < -600f) { pos = RandTop(); }
            rt.anchoredPosition = pos;
        }

        // sparkles twinkle
        for (int i = 0; i < _spark.Count; i++)
        {
            float a = Mathf.Abs(Mathf.Sin(_t * _sparkSp[i] + _sparkPh[i]));
            var img = _spark[i].GetComponent<Image>(); if (img != null) img.color = new Color(1f, 1f, 0.95f, a * 0.9f);
            _spark[i].localScale = Vector3.one * (0.6f + 0.4f * a);
        }

        // birds fly across + wing flap (scale.y) + bob
        for (int i = 0; i < _bird.Count; i++)
        {
            var rt = _bird[i]; var pos = rt.anchoredPosition;
            pos.x += _birdSp[i] * dt; pos.y += Mathf.Sin(_t * 0.8f + _birdPh[i]) * 6f * dt;
            if (pos.x > 1040f) { pos.x = -1040f; pos.y = R(170, 430); }
            rt.anchoredPosition = pos;
            float flap = 0.6f + 0.5f * Mathf.Abs(Mathf.Sin(_t * 8f + _birdPh[i]));
            rt.localScale = new Vector3(1f, flap, 1f);
        }

        // butterflies wander in gentle loops + flutter (wing flap = width squash)
        for (int i = 0; i < _bfly.Count; i++)
        {
            var rt = _bfly[i]; float tt = _t * 0.8f + _bflyPh[i];
            var pos = _bflyHome[i] + new Vector2(Mathf.Sin(tt) * 70f + Mathf.Sin(tt * 1.7f) * 24f,
                                                 Mathf.Cos(tt * 1.3f) * 42f + Mathf.Sin(tt * 2.3f) * 16f);
            rt.anchoredPosition = pos + _parallax * 0.4f;
            float bf = 0.35f + 0.65f * Mathf.Abs(Mathf.Sin(_t * 13f + _bflyPh[i]));
            rt.localScale = new Vector3(bf, 1f, 1f);
        }

        // water glints twinkle on the pool
        for (int i = 0; i < _shim.Count; i++)
        {
            var rt = _shim[i];
            float tw = Mathf.Abs(Mathf.Sin(_t * 2.2f + _shimY0[i]));
            var im = rt.GetComponent<Image>(); if (im != null) im.color = new Color(1f, 1f, 1f, 0.12f + 0.38f * tw);
            rt.localScale = new Vector3(0.7f + 0.5f * tw, 1f, 1f);
            rt.anchoredPosition = _shimBase[i] + new Vector2(Mathf.Sin(_t * 0.8f + _shimY0[i]) * 4f, 0f) + _parallax * 0.5f;
        }

        // subtle tilt/mouse parallax (new Input System)
        Vector2 target = Vector2.zero;
        var acc = Accelerometer.current;
        if (acc != null)
        {
            var a = acc.acceleration.ReadValue();
            target = new Vector2(Mathf.Clamp(a.x, -1f, 1f), Mathf.Clamp(a.y + 0.5f, -1f, 1f)) * 22f;
        }
        else if (Mouse.current != null)
        {
            var mp = Mouse.current.position.ReadValue();
            target = new Vector2((mp.x / Mathf.Max(1, Screen.width)) - 0.5f, (mp.y / Mathf.Max(1, Screen.height)) - 0.5f) * 30f;
        }
        _parallax = Vector2.Lerp(_parallax, target, dt * 3f);
        if (bg != null) bg.anchoredPosition = _bgPos - _parallax * 0.6f;
        _layer.anchoredPosition = _layerPos + _parallax * 0.3f;
    }
}
