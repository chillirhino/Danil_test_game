using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

namespace FlappyGame
{
    /// <summary>
    /// Self-contained Flappy Bird. Drop this on one GameObject in a scene and press Play.
    /// Builds the bird, pipes, scrolling ground, drifting clouds and a mobile-friendly UI
    /// at runtime from sprites in Resources/Flappy. Tap / click / space to flap.
    /// Works on touch, mouse and keyboard. Uses an orthographic camera + SpriteRenderers.
    /// </summary>
    public class FlappyBird : MonoBehaviour
    {
        [Header("Bird")]
        public float gravity = 22f;        // downward accel (world units / s^2)
        public float flapVelocity = 8f;    // upward velocity applied on a tap
        public float birdRadius = 0.35f;   // collision radius
        public float birdXFactor = -0.45f; // horizontal position as fraction of half-width

        [Header("Pipes")]
        public float pipeSpeed = 3.2f;
        public float pipeHalfWidth = 0.6f;
        public float gapHalfHeight = 1.55f;
        public float spawnInterval = 1.6f;

        [Header("World")]
        public float orthoSize = 6f;
        public float groundHeight = 1.1f;

        // Sorting layers (all sprites sit at z=0; order decides draw order).
        const int ORDER_CLOUD = -20, ORDER_PIPE = -10, ORDER_GROUND = 10, ORDER_BIRD = 20;

        enum State { Ready, Playing, Dead }
        State _state;

        Camera _cam;
        SpriteRenderer _birdSR;
        Transform _bird;
        readonly List<PipePair> _pipes = new List<PipePair>();
        Transform _pipeRoot;

        readonly SpriteRenderer[] _groundTiles = new SpriteRenderer[2];
        float _groundScroll;
        readonly List<Transform> _clouds = new List<Transform>();

        float _velocity;
        float _spawnTimer;
        int _score, _best;
        float _animTimer;
        int _animFrame;

        float _halfH, _halfW, _birdX, _spawnX, _despawnX, _groundTopY;

        // Sprites
        Sprite[] _birdFrames;
        Sprite _pipeSprite, _groundSprite, _cloudSprite;

        Canvas _canvas;
        Text _scoreText, _messageText;

        class PipePair
        {
            public SpriteRenderer top, bottom;
            public float x, centerY;
            public bool scored;
        }

        void Start()
        {
            LoadSprites();
            SetupCamera();
            BuildBird();
            BuildGround();
            BuildClouds();
            _pipeRoot = new GameObject("Pipes").transform;
            _pipeRoot.SetParent(transform, false);
            BuildUI();
            RecomputeBounds();
            ResetGame();
        }

        void LoadSprites()
        {
            _birdFrames = new[]
            {
                Resources.Load<Sprite>("Flappy/bird1"),
                Resources.Load<Sprite>("Flappy/bird2"),
            };
            _pipeSprite = Resources.Load<Sprite>("Flappy/pipe_a");
            _groundSprite = Resources.Load<Sprite>("Flappy/ground");
            _cloudSprite = Resources.Load<Sprite>("Flappy/cloud");
        }

        void SetupCamera()
        {
            _cam = Camera.main;
            if (_cam == null)
            {
                var go = new GameObject("Main Camera");
                go.tag = "MainCamera";
                _cam = go.AddComponent<Camera>();
            }
            _cam.orthographic = true;
            _cam.orthographicSize = orthoSize;
            _cam.transform.position = new Vector3(0f, 0f, -10f);
            _cam.transform.rotation = Quaternion.identity;
            _cam.nearClipPlane = 0.1f;
            _cam.farClipPlane = 100f;
            _cam.clearFlags = CameraClearFlags.SolidColor;
            _cam.backgroundColor = new Color(0.32f, 0.70f, 0.90f); // sky blue
        }

        static SpriteRenderer MakeSR(string name, Sprite sprite, int order, Transform parent)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.sortingOrder = order;
            return sr;
        }

        // Uniformly scale a transform so the sprite's world height equals worldH.
        static void FitHeight(Transform t, Sprite s, float worldH)
        {
            if (s == null) { t.localScale = Vector3.one; return; }
            float k = worldH / s.bounds.size.y;
            t.localScale = new Vector3(k, k, 1f);
        }

        void BuildBird()
        {
            _birdSR = MakeSR("Bird", _birdFrames != null ? _birdFrames[0] : null, ORDER_BIRD, transform);
            _bird = _birdSR.transform;
            FitHeight(_bird, _birdSR.sprite, birdRadius * 2.1f);
        }

        void BuildGround()
        {
            for (int i = 0; i < 2; i++)
                _groundTiles[i] = MakeSR("Ground" + i, _groundSprite, ORDER_GROUND, transform);
        }

        void BuildClouds()
        {
            // Deterministic-ish spread; positions are set for real in UpdateClouds.
            for (int i = 0; i < 4; i++)
            {
                var sr = MakeSR("Cloud" + i, _cloudSprite, ORDER_CLOUD, transform);
                sr.color = new Color(1f, 1f, 1f, 0.9f);
                FitHeight(sr.transform, _cloudSprite, 1.1f + i * 0.25f);
                _clouds.Add(sr.transform);
            }
        }

        SpriteRenderer MakePipe(string name)
        {
            return MakeSR(name, _pipeSprite, ORDER_PIPE, _pipeRoot);
        }

        void BuildUI()
        {
            var canvasGo = new GameObject("Canvas");
            canvasGo.transform.SetParent(transform, false);
            _canvas = canvasGo.AddComponent<Canvas>();
            _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080f, 1920f);
            scaler.matchWidthOrHeight = 0.5f;
            canvasGo.AddComponent<GraphicRaycaster>();

            var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            _scoreText = MakeText("Score", font, 110, TextAnchor.UpperCenter);
            var sRt = _scoreText.rectTransform;
            sRt.anchorMin = sRt.anchorMax = new Vector2(0.5f, 1f);
            sRt.pivot = new Vector2(0.5f, 1f);
            sRt.anchoredPosition = new Vector2(0f, -70f);
            sRt.sizeDelta = new Vector2(600f, 220f);

            _messageText = MakeText("Message", font, 62, TextAnchor.MiddleCenter);
            var mRt = _messageText.rectTransform;
            mRt.anchorMin = mRt.anchorMax = new Vector2(0.5f, 0.5f);
            mRt.pivot = new Vector2(0.5f, 0.5f);
            mRt.anchoredPosition = Vector2.zero;
            mRt.sizeDelta = new Vector2(1000f, 500f);
        }

        Text MakeText(string name, Font font, int size, TextAnchor anchor)
        {
            var go = new GameObject(name);
            go.transform.SetParent(_canvas.transform, false);
            var t = go.AddComponent<Text>();
            t.font = font;
            t.fontSize = size;
            t.fontStyle = FontStyle.Bold;
            t.alignment = anchor;
            t.color = Color.white;
            t.horizontalOverflow = HorizontalWrapMode.Overflow;
            t.verticalOverflow = VerticalWrapMode.Overflow;
            var outline = go.AddComponent<Outline>();
            outline.effectColor = new Color(0f, 0f, 0f, 0.85f);
            outline.effectDistance = new Vector2(4f, -4f);
            return t;
        }

        void RecomputeBounds()
        {
            _halfH = _cam.orthographicSize;
            _halfW = _halfH * _cam.aspect;
            _birdX = _halfW * birdXFactor;
            _spawnX = _halfW + pipeHalfWidth + 0.5f;
            _despawnX = -_halfW - pipeHalfWidth - 0.5f;
            _groundTopY = -_halfH + groundHeight;
        }

        void Update()
        {
            RecomputeBounds();
            LayoutGround();
            bool scroll = _state != State.Dead;
            if (scroll) UpdateGroundScroll();
            UpdateClouds(scroll);
            AnimateBird(scroll);

            bool tap = TapThisFrame();

            switch (_state)
            {
                case State.Ready:
                    _bird.position = new Vector3(_birdX, Mathf.Sin(Time.time * 3f) * 0.35f, 0f);
                    _bird.rotation = Quaternion.identity;
                    if (tap) StartPlaying();
                    break;

                case State.Playing:
                    if (tap) _velocity = flapVelocity;
                    _velocity -= gravity * Time.deltaTime;
                    float y = _bird.position.y + _velocity * Time.deltaTime;
                    y = Mathf.Min(y, _halfH - birdRadius);
                    _bird.position = new Vector3(_birdX, y, 0f);
                    _bird.rotation = Quaternion.Euler(0f, 0f, Mathf.Clamp(_velocity * 5f, -70f, 35f));

                    UpdatePipes();
                    CheckCollisions();
                    break;

                case State.Dead:
                    if (tap) ResetGame();
                    break;
            }
        }

        void LayoutGround()
        {
            float tileW = _halfW * 2f;
            float cy = _groundTopY - groundHeight * 0.5f;
            foreach (var g in _groundTiles)
            {
                if (g.sprite == null) continue;
                // Slightly wider than tileW so adjacent tiles overlap and hide the seam.
                g.transform.localScale = new Vector3((tileW + 0.2f) / g.sprite.bounds.size.x,
                                                     groundHeight / g.sprite.bounds.size.y, 1f);
                var p = g.transform.position;
                g.transform.position = new Vector3(p.x, cy, 0f);
            }
        }

        void UpdateGroundScroll()
        {
            float tileW = _halfW * 2f;
            _groundScroll += pipeSpeed * Time.deltaTime;
            if (_groundScroll >= tileW) _groundScroll -= tileW;
            for (int i = 0; i < 2; i++)
            {
                var p = _groundTiles[i].transform.position;
                float x = -_halfW + tileW * 0.5f - _groundScroll + i * tileW;
                _groundTiles[i].transform.position = new Vector3(x, p.y, 0f);
            }
        }

        bool _cloudsInit;

        void UpdateClouds(bool scroll)
        {
            if (!_cloudsInit)
            {
                _cloudsInit = true;
                for (int i = 0; i < _clouds.Count; i++)
                    _clouds[i].position = new Vector3(
                        -_halfW + (i + 0.5f) / _clouds.Count * (_halfW * 2f),
                        _halfH * (0.35f + 0.14f * i), 0f);
            }

            float speed = pipeSpeed * 0.3f;
            for (int i = 0; i < _clouds.Count; i++)
            {
                var c = _clouds[i];
                float x = c.position.x - (scroll ? speed * Time.deltaTime : 0f);
                float y = c.position.y;
                if (x < -_halfW - 1.5f) { x = _halfW + 1.5f; y = _halfH * (0.30f + 0.15f * ((i * 37) % 4)); }
                c.position = new Vector3(x, y, 0f);
            }
        }

        void AnimateBird(bool animate)
        {
            if (_birdFrames == null || _birdFrames[0] == null) return;
            if (animate)
            {
                _animTimer += Time.deltaTime;
                if (_animTimer >= 0.12f)
                {
                    _animTimer = 0f;
                    _animFrame = 1 - _animFrame;
                    _birdSR.sprite = _birdFrames[_animFrame];
                }
            }
        }

        bool _wasPressed;

        bool TapThisFrame()
        {
            bool pressed = false;
            if (Keyboard.current != null && Keyboard.current.spaceKey.isPressed) pressed = true;
            if (Mouse.current != null && Mouse.current.leftButton.isPressed) pressed = true;
            if (Touchscreen.current != null &&
                Touchscreen.current.primaryTouch.press.isPressed) pressed = true;

            bool tapped = pressed && !_wasPressed;
            _wasPressed = pressed;
            return tapped;
        }

        void StartPlaying()
        {
            _state = State.Playing;
            _velocity = flapVelocity;
            _spawnTimer = spawnInterval;
            _messageText.text = "";
        }

        void ResetGame()
        {
            _state = State.Ready;
            _score = 0;
            _velocity = 0f;
            ClearPipes();
            _bird.position = new Vector3(_birdX, 0f, 0f);
            _bird.rotation = Quaternion.identity;
            _scoreText.text = "0";
            _messageText.text = "FLAPPY\n\nTap to start";
        }

        void UpdatePipes()
        {
            _spawnTimer += Time.deltaTime;
            if (_spawnTimer >= spawnInterval)
            {
                _spawnTimer = 0f;
                SpawnPipe();
            }

            for (int i = _pipes.Count - 1; i >= 0; i--)
            {
                var p = _pipes[i];
                p.x -= pipeSpeed * Time.deltaTime;
                PositionPipe(p);

                if (!p.scored && p.x < _birdX)
                {
                    p.scored = true;
                    _score++;
                    _scoreText.text = _score.ToString();
                }

                if (p.x < _despawnX)
                {
                    Destroy(p.top.gameObject);
                    Destroy(p.bottom.gameObject);
                    _pipes.RemoveAt(i);
                }
            }
        }

        void SpawnPipe()
        {
            float margin = 0.6f;
            float minC = _groundTopY + gapHalfHeight + margin;
            float maxC = _halfH - gapHalfHeight - margin;
            if (maxC < minC) { float m = (minC + maxC) * 0.5f; minC = maxC = m; }

            var pair = new PipePair
            {
                top = MakePipe("PipeTop"),
                bottom = MakePipe("PipeBottom"),
                x = _spawnX,
                centerY = Random.Range(minC, maxC),
                scored = false
            };
            pair.top.flipY = true; // cap points down for the top pipe
            _pipes.Add(pair);
            PositionPipe(pair);
        }

        void PositionPipe(PipePair p)
        {
            float w = pipeHalfWidth * 2f;

            // Bottom pipe: cap at the gap's lower edge, body extends down past the screen.
            float botTop = p.centerY - gapHalfHeight;
            float botLen = botTop - (-_halfH - 1f);
            SetPipe(p.bottom, p.x, botTop - botLen * 0.5f, w, botLen);

            // Top pipe: cap at the gap's upper edge (sprite flipped), body extends up.
            float topBot = p.centerY + gapHalfHeight;
            float topLen = (_halfH + 1f) - topBot;
            SetPipe(p.top, p.x, topBot + topLen * 0.5f, w, topLen);
        }

        void SetPipe(SpriteRenderer sr, float x, float cy, float worldW, float worldH)
        {
            if (sr.sprite != null)
            {
                sr.transform.localScale = new Vector3(worldW / sr.sprite.bounds.size.x,
                                                      Mathf.Max(0.01f, worldH) / sr.sprite.bounds.size.y, 1f);
            }
            sr.transform.position = new Vector3(x, cy, 0f);
        }

        void CheckCollisions()
        {
            float by = _bird.position.y;
            if (by - birdRadius <= _groundTopY) { Die(); return; }

            foreach (var p in _pipes)
            {
                if (Mathf.Abs(p.x - _birdX) <= pipeHalfWidth + birdRadius)
                {
                    bool insideGap = by < p.centerY + gapHalfHeight - birdRadius &&
                                     by > p.centerY - gapHalfHeight + birdRadius;
                    if (!insideGap) { Die(); return; }
                }
            }
        }

        void Die()
        {
            _state = State.Dead;
            if (_score > _best) _best = _score;
            _bird.position = new Vector3(_birdX, _groundTopY + birdRadius, 0f);
            _messageText.text = "GAME OVER\n\nScore: " + _score + "   Best: " + _best + "\n\nTap to retry";
        }

        void ClearPipes()
        {
            foreach (var p in _pipes)
            {
                if (p.top) Destroy(p.top.gameObject);
                if (p.bottom) Destroy(p.bottom.gameObject);
            }
            _pipes.Clear();
        }
    }
}
