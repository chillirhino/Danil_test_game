using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using Sirvival;

namespace Sirvival.EditorTools
{
    /// <summary>
    /// Idempotent builder for the Phase-1 gameplay layer: enemy/projectile prefabs,
    /// RunManager + spawner + auto-attack + chef health, and the HUD / level-up /
    /// game-over UI, all wired up. Safe to re-run. Menu: Sirvival ▸ Build Gameplay.
    /// </summary>
    public static class SirvivalGameplayBuilder
    {
        const string Dir = "Assets/Art/Sprites/Sirvival/";

        [MenuItem("Sirvival/Build Gameplay")]
        public static void BuildGameplay()
        {
            var px = AssetDatabase.LoadAssetAtPath<Sprite>(Dir + "_px.png");
            var circle = AssetDatabase.LoadAssetAtPath<Sprite>(Dir + "_circle.png");

            var enemyPrefab = MakeEnemyPrefab(px);
            var projPrefab = MakeProjectilePrefab(circle);

            var chef = GameObject.Find("Chef");
            var canvasGo = GameObject.Find("UICanvas");

            // ── Systems: RunManager + spawner ────────────────────────────────
            var systems = GameObject.Find("Systems");
            if (systems == null) systems = new GameObject("Systems");
            Ensure<RunManager>(systems);
            var spawner = Ensure<EnemySpawner>(systems);
            SetRef(spawner, "enemyPrefab", enemyPrefab);

            // ── Chef: health + auto-attack ───────────────────────────────────
            ChefHealth health = null;
            if (chef != null)
            {
                health = Ensure<ChefHealth>(chef);
                var atk = Ensure<ChefAutoAttack>(chef);
                SetRef(atk, "projectilePrefab", projPrefab);
            }

            // ── Canvas: ensure it draws above the world ──────────────────────
            var canvas = canvasGo.GetComponent<Canvas>();
            canvas.sortingOrder = 100;

            // ── HUD ──────────────────────────────────────────────────────────
            var hud = BuildHud(canvasGo.transform, px, health);

            // ── Level-up panel ───────────────────────────────────────────────
            BuildLevelUpPanel(canvasGo.transform, px);

            // ── Game-over panel ──────────────────────────────────────────────
            BuildGameOverPanel(canvasGo.transform, px);

            // ── Build settings ───────────────────────────────────────────────
            const string scenePath = "Assets/Scenes/Sirvival.unity";
            var scenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
            if (!scenes.Exists(s => s.path == scenePath))
                scenes.Add(new EditorBuildSettingsScene(scenePath, true));
            EditorBuildSettings.scenes = scenes.ToArray();

            var scene = SceneManager.GetActiveScene();
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log("SIRVIVAL_GAMEPLAY_BUILT");
        }

        // ── prefabs ──────────────────────────────────────────────────────────

        static Enemy MakeEnemyPrefab(Sprite px)
        {
            const string path = Dir + "Enemy.prefab";
            var existing = AssetDatabase.LoadAssetAtPath<Enemy>(path);
            if (existing != null) return existing;
            var go = new GameObject("Enemy");
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = px; sr.color = new Color(0.85f, 0.27f, 0.22f); sr.sortingOrder = 0;
            var rb = go.AddComponent<Rigidbody2D>(); rb.gravityScale = 0f; rb.freezeRotation = true;
            go.AddComponent<CircleCollider2D>().radius = 0.42f;
            go.AddComponent<Enemy>();
            var prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
            Object.DestroyImmediate(go);
            return prefab.GetComponent<Enemy>();
        }

        static Projectile MakeProjectilePrefab(Sprite circle)
        {
            const string path = Dir + "Projectile.prefab";
            var existing = AssetDatabase.LoadAssetAtPath<Projectile>(path);
            if (existing != null) return existing;
            var go = new GameObject("Projectile");
            go.transform.localScale = Vector3.one * 0.4f;
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = circle; sr.color = new Color(1f, 0.85f, 0.3f); sr.sortingOrder = 5;
            var rb = go.AddComponent<Rigidbody2D>(); rb.bodyType = RigidbodyType2D.Kinematic;
            var cc = go.AddComponent<CircleCollider2D>(); cc.radius = 0.35f; cc.isTrigger = true;
            go.AddComponent<Projectile>();
            var prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
            Object.DestroyImmediate(go);
            return prefab.GetComponent<Projectile>();
        }

        // ── HUD ────────────────────────────────────────────────────────────────

        static Hud BuildHud(Transform canvas, Sprite px, ChefHealth health)
        {
            if (GameObject.Find("HUD") != null) return GameObject.Find("HUD").GetComponent<Hud>();
            var hudGo = new GameObject("HUD", typeof(RectTransform));
            hudGo.transform.SetParent(canvas, false);
            Stretch((RectTransform)hudGo.transform);

            // XP bar across the very top
            var xpBg = MakeImage("XpBg", hudGo.transform, px, new Color(0f, 0f, 0f, 0.5f));
            Anchor(xpBg, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0, -20), new Vector2(1000, 26));
            var xpFill = MakeImage("XpFill", xpBg.transform, px, new Color(0.35f, 0.6f, 1f, 1f));
            Stretch((RectTransform)xpFill.transform);
            var xpImg = xpFill.GetComponent<Image>(); xpImg.type = Image.Type.Filled; xpImg.fillMethod = Image.FillMethod.Horizontal; xpImg.fillOrigin = 0; xpImg.fillAmount = 0f;

            var level = MakeText("Level", hudGo.transform, "LV 1", 44, TextAnchor.UpperLeft);
            Anchor(level.gameObject, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(40, -70), new Vector2(300, 60));
            var kills = MakeText("Kills", hudGo.transform, "☠ 0", 44, TextAnchor.UpperRight);
            Anchor(kills.gameObject, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-40, -70), new Vector2(300, 60));
            var timer = MakeText("Timer", hudGo.transform, "0:00", 60, TextAnchor.UpperCenter);
            Anchor(timer.gameObject, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0, -70), new Vector2(400, 80));

            // HP bar under the timer
            var hpBg = MakeImage("HpBg", hudGo.transform, px, new Color(0f, 0f, 0f, 0.5f));
            Anchor(hpBg, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0, -160), new Vector2(560, 34));
            var hpFill = MakeImage("HpFill", hpBg.transform, px, new Color(0.9f, 0.3f, 0.25f, 1f));
            Stretch((RectTransform)hpFill.transform);
            var hpImg = hpFill.GetComponent<Image>(); hpImg.type = Image.Type.Filled; hpImg.fillMethod = Image.FillMethod.Horizontal; hpImg.fillOrigin = 0; hpImg.fillAmount = 1f;

            var hud = hudGo.AddComponent<Hud>();
            SetRef(hud, "hpFill", hpImg);
            SetRef(hud, "xpFill", xpImg);
            SetRef(hud, "levelText", level);
            SetRef(hud, "timerText", timer);
            SetRef(hud, "killsText", kills);
            SetRef(hud, "health", health);
            return hud;
        }

        // ── Level-up panel ───────────────────────────────────────────────────

        static void BuildLevelUpPanel(Transform canvas, Sprite px)
        {
            if (GameObject.Find("LevelUpPanel") != null) return;
            var panelHost = new GameObject("LevelUpPanel", typeof(RectTransform));
            panelHost.transform.SetParent(canvas, false);
            Stretch((RectTransform)panelHost.transform);

            var dim = MakeImage("Dim", panelHost.transform, px, new Color(0f, 0f, 0f, 0.8f));
            Stretch((RectTransform)dim.transform);

            var title = MakeText("Title", dim.transform, "УРОВЕНЬ!", 70, TextAnchor.UpperCenter);
            Anchor(title.gameObject, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0, -260), new Vector2(800, 100));

            var buttons = new Button[3];
            var labels = new Text[3];
            for (int i = 0; i < 3; i++)
            {
                var b = MakeButton("Choice" + i, dim.transform, px, out var lbl);
                Anchor(b.gameObject, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, 220 - i * 220), new Vector2(820, 190));
                buttons[i] = b; labels[i] = lbl;
            }

            var lp = panelHost.AddComponent<LevelUpPanel>();
            SetRef(lp, "root", dim); // toggling the dim container shows/hides the whole panel
            SetArray(lp, "buttons", buttons);
            SetArray(lp, "labels", labels);
        }

        // ── Game-over panel ──────────────────────────────────────────────────

        static void BuildGameOverPanel(Transform canvas, Sprite px)
        {
            if (GameObject.Find("GameOverPanel") != null) return;
            var host = new GameObject("GameOverPanel", typeof(RectTransform));
            host.transform.SetParent(canvas, false);
            Stretch((RectTransform)host.transform);

            var dim = MakeImage("Dim", host.transform, px, new Color(0f, 0f, 0f, 0.85f));
            Stretch((RectTransform)dim.transform);

            var summary = MakeText("Summary", dim.transform, "", 56, TextAnchor.MiddleCenter);
            Anchor(summary.gameObject, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, 120), new Vector2(900, 300));

            var btn = MakeButton("Restart", dim.transform, px, out var lbl);
            lbl.text = "ЗАНОВО"; lbl.fontSize = 48;
            Anchor(btn.gameObject, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, -180), new Vector2(500, 150));

            var go = host.AddComponent<GameOverPanel>();
            SetRef(go, "root", dim);
            SetRef(go, "summary", summary);
            SetRef(go, "restart", btn);
        }

        // ── helpers ──────────────────────────────────────────────────────────

        static T Ensure<T>(GameObject go) where T : Component
        {
            var c = go.GetComponent<T>();
            if (c == null) c = go.AddComponent<T>();
            return c;
        }

        static Font UIFont()
        {
            var f = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (f == null) f = Resources.GetBuiltinResource<Font>("Arial.ttf");
            return f;
        }

        static GameObject MakeImage(string name, Transform parent, Sprite spr, Color col)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var img = go.AddComponent<Image>();
            img.sprite = spr; img.color = col;
            return go;
        }

        static Text MakeText(string name, Transform parent, string txt, int size, TextAnchor anchor)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var t = go.AddComponent<Text>();
            t.text = txt; t.font = UIFont(); t.fontSize = size; t.alignment = anchor;
            t.color = Color.white; t.horizontalOverflow = HorizontalWrapMode.Overflow; t.verticalOverflow = VerticalWrapMode.Overflow;
            return t;
        }

        static Button MakeButton(string name, Transform parent, Sprite px, out Text label)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var img = go.AddComponent<Image>();
            img.sprite = px; img.color = new Color(0.22f, 0.22f, 0.28f, 0.98f);
            var btn = go.AddComponent<Button>();
            label = MakeText(name + "_label", go.transform, "", 40, TextAnchor.MiddleCenter);
            Stretch((RectTransform)label.transform);
            return btn;
        }

        static void Stretch(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
        }

        static void Anchor(GameObject go, Vector2 aMin, Vector2 aMax, Vector2 pivot, Vector2 pos, Vector2 size)
        {
            var rt = (RectTransform)go.transform;
            rt.anchorMin = aMin; rt.anchorMax = aMax; rt.pivot = pivot;
            rt.anchoredPosition = pos; rt.sizeDelta = size;
        }

        static void SetRef(Component c, string field, Object value)
        {
            var so = new SerializedObject(c);
            var p = so.FindProperty(field);
            if (p != null) { p.objectReferenceValue = value; so.ApplyModifiedPropertiesWithoutUndo(); }
            else Debug.LogWarning("SetRef: no field '" + field + "' on " + c.GetType().Name);
        }

        static void SetArray(Component c, string field, Object[] values)
        {
            var so = new SerializedObject(c);
            var p = so.FindProperty(field);
            if (p == null) { Debug.LogWarning("SetArray: no field '" + field + "'"); return; }
            p.arraySize = values.Length;
            for (int i = 0; i < values.Length; i++)
                p.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
            so.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
