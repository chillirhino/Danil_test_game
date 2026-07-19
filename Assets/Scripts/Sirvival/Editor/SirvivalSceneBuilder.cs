using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace Sirvival.EditorTools
{
    /// <summary>
    /// One-shot, idempotent builder that wires the Sirvival scene's control layer:
    /// chef physics + movement, top-down camera follow, an EventSystem, and an
    /// on-screen virtual joystick. Safe to re-run.
    /// </summary>
    public static class SirvivalSceneBuilder
    {
        [MenuItem("Sirvival/Build Control Layer")]
        public static void Build()
        {
            var circle = EnsureCircleSprite();
            var scene = EditorSceneManager.GetActiveScene();

            // ── Chef: physics + movement ─────────────────────────────────────
            // NOTE: use Unity-aware "== null" checks, not C# "??" — GetComponent
            // returns a fake-null that "??" mishandles.
            var chef = GameObject.Find("Chef");
            Sirvival.ChefMovement movement = null;
            if (chef != null)
            {
                var rb = chef.GetComponent<Rigidbody2D>();
                if (rb == null) rb = chef.AddComponent<Rigidbody2D>();
                rb.gravityScale = 0f;
                rb.freezeRotation = true;
                rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
                if (chef.GetComponent<CircleCollider2D>() == null)
                    chef.AddComponent<CircleCollider2D>().radius = 0.45f;
                movement = chef.GetComponent<Sirvival.ChefMovement>();
                if (movement == null) movement = chef.AddComponent<Sirvival.ChefMovement>();
            }

            // ── Camera: top-down follow ──────────────────────────────────────
            var cam = Camera.main;
            if (cam != null)
            {
                var follow = cam.GetComponent<Sirvival.SurvivorsCameraFollow>();
                if (follow == null) follow = cam.gameObject.AddComponent<Sirvival.SurvivorsCameraFollow>();
                if (chef != null) follow.SetTarget(chef.transform);
            }

            // ── EventSystem (new Input System UI module) ─────────────────────
            if (Object.FindFirstObjectByType<EventSystem>() == null)
            {
                var es = new GameObject("EventSystem");
                es.AddComponent<EventSystem>();
                es.AddComponent<InputSystemUIInputModule>();
            }

            // ── Canvas (Screen Space - Camera so it renders through Main Camera) ─
            var canvasGo = GameObject.Find("UICanvas");
            if (canvasGo == null)
            {
                canvasGo = new GameObject("UICanvas");
                var canvas = canvasGo.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceCamera;
                canvas.worldCamera = cam;
                canvas.planeDistance = 5f;
                var scaler = canvasGo.AddComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1080, 1920);
                scaler.matchWidthOrHeight = 0.5f;
                canvasGo.AddComponent<GraphicRaycaster>();
            }

            // ── Virtual joystick (base + handle) ─────────────────────────────
            if (GameObject.Find("JoystickBase") == null)
            {
                var baseGo = new GameObject("JoystickBase", typeof(RectTransform));
                baseGo.transform.SetParent(canvasGo.transform, false);
                var brt = (RectTransform)baseGo.transform;
                brt.anchorMin = brt.anchorMax = new Vector2(0f, 0f);
                brt.pivot = new Vector2(0.5f, 0.5f);
                brt.sizeDelta = new Vector2(320f, 320f);
                brt.anchoredPosition = new Vector2(240f, 300f);
                var bimg = baseGo.AddComponent<Image>();
                bimg.sprite = circle;
                bimg.color = new Color(1f, 1f, 1f, 0.16f);

                var handleGo = new GameObject("Handle", typeof(RectTransform));
                handleGo.transform.SetParent(baseGo.transform, false);
                var hrt = (RectTransform)handleGo.transform;
                hrt.anchorMin = hrt.anchorMax = new Vector2(0.5f, 0.5f);
                hrt.pivot = new Vector2(0.5f, 0.5f);
                hrt.sizeDelta = new Vector2(150f, 150f);
                hrt.anchoredPosition = Vector2.zero;
                var himg = handleGo.AddComponent<Image>();
                himg.sprite = circle;
                himg.color = new Color(0.91f, 0.46f, 0.23f, 0.85f);
                himg.raycastTarget = false;

                var vj = baseGo.AddComponent<Sirvival.VirtualJoystick>();
                var so = new SerializedObject(vj);
                so.FindProperty("background").objectReferenceValue = brt;
                so.FindProperty("handle").objectReferenceValue = hrt;
                so.ApplyModifiedPropertiesWithoutUndo();

                if (movement != null)
                {
                    var so2 = new SerializedObject(movement);
                    so2.FindProperty("joystick").objectReferenceValue = vj;
                    so2.ApplyModifiedPropertiesWithoutUndo();
                }
            }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log("SIRVIVAL_BUILD_DONE");
        }

        private static Sprite EnsureCircleSprite()
        {
            const string path = "Assets/Art/Sprites/Sirvival/_circle.png";
            if (!File.Exists(path))
            {
                const int S = 64;
                var tex = new Texture2D(S, S, TextureFormat.RGBA32, false);
                var px = new Color32[S * S];
                float r = S * 0.5f;
                for (int y = 0; y < S; y++)
                for (int x = 0; x < S; x++)
                {
                    float dx = x + 0.5f - r, dy = y + 0.5f - r;
                    float d = Mathf.Sqrt(dx * dx + dy * dy);
                    float a = Mathf.Clamp01((r - d) / 1.5f);
                    px[y * S + x] = new Color32(255, 255, 255, (byte)(a * 255f));
                }
                tex.SetPixels32(px);
                tex.Apply();
                File.WriteAllBytes(path, tex.EncodeToPNG());
                Object.DestroyImmediate(tex);
                AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
                var ti = (TextureImporter)AssetImporter.GetAtPath(path);
                ti.textureType = TextureImporterType.Sprite;
                ti.alphaIsTransparency = true;
                ti.SaveAndReimport();
            }
            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }
    }
}
