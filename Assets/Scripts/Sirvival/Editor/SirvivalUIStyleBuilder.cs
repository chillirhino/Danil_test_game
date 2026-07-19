using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using Sirvival;

namespace Sirvival.EditorTools
{
    /// <summary>Restyles HUD / Level-Up / Game-Over to match the Lovable mockups
    /// (Titan One font, rounded cards, warm palette). Menu: Sirvival ▸ Style UI.</summary>
    public static class SirvivalUIStyleBuilder
    {
        const string UI = "Assets/Art/Sprites/Sirvival/ui/";
        static Font _font;
        static Sprite _rr;

        // palette
        static Color Cream = new Color(0.985f, 0.937f, 0.835f);
        static Color DarkTx = new Color(0.22f, 0.15f, 0.11f);
        static Color GrayTx = new Color(0.52f, 0.47f, 0.44f);
        static Color Red = new Color(0.90f, 0.29f, 0.22f);
        static Color Yellow = new Color(0.96f, 0.69f, 0.18f);
        static Color XpBlue = new Color(0.25f, 0.62f, 1f);
        static Color DarkPill = new Color(0f, 0f, 0f, 0.42f);
        static Color LevelBg = new Color(0.10f, 0.085f, 0.075f, 0.95f);
        static Color GoverBg = new Color(0.16f, 0.075f, 0.065f, 0.97f);

        [MenuItem("Sirvival/Style UI")]
        public static void StyleUI()
        {
            EnsureAssets();
            var canvas = GameObject.Find("UICanvas").transform;

            DestroyMatches(canvas, "HUD");
            DestroyMatches(canvas, "LevelUpPanel");
            DestroyMatches(canvas, "GameOverPanel");

            BuildHud(canvas);
            BuildLevelUp(canvas);
            BuildGameOver(canvas);

            var scene = SceneManager.GetActiveScene();
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log("SIRVIVAL_UI_STYLED");
        }

        static void EnsureAssets()
        {
            _font = AssetDatabase.LoadAssetAtPath<Font>("Assets/Fonts/TitanOne-Regular.ttf");
            string rrPath = UI + "rr.png";
            var ti = (TextureImporter)AssetImporter.GetAtPath(rrPath);
            ti.textureType = TextureImporterType.Sprite;
            ti.spriteImportMode = SpriteImportMode.Single;
            var s = new TextureImporterSettings(); ti.ReadTextureSettings(s);
            s.spriteBorder = new Vector4(24, 24, 24, 24);
            s.spriteMeshType = SpriteMeshType.FullRect;
            s.spriteMode = (int)SpriteImportMode.Single;
            ti.SetTextureSettings(s); ti.SaveAndReimport();
            _rr = AssetDatabase.LoadAssetAtPath<Sprite>(rrPath);
            foreach (var n in new[] { "ic_sauce", "ic_pin", "ic_boots", "ic_garlic", "ic_arrow", "heart" })
            {
                var t = (TextureImporter)AssetImporter.GetAtPath(UI + n + ".png");
                if (t != null && t.textureType != TextureImporterType.Sprite) { t.textureType = TextureImporterType.Sprite; t.SaveAndReimport(); }
            }
        }

        static Sprite Icon(string id) => AssetDatabase.LoadAssetAtPath<Sprite>(UI + (id == "heart" ? "heart" : "ic_" + id) + ".png");

        // ── HUD ──────────────────────────────────────────────────────────────
        static void BuildHud(Transform canvas)
        {
            var hud = New("HUD", canvas); Stretch(hud);
            var chef = GameObject.Find("Chef");
            var health = chef != null ? chef.GetComponent<ChefHealth>() : null;

            // XP bar (top full width)
            var xpbg = Panel("XpBg", hud.transform, new Color(0, 0, 0, 0.4f));
            Anchor(xpbg, V(0, 1), V(1, 1), V(0.5f, 1), V(0, -16), new Vector2(-60, 26));
            var xpFill = Bar("XpFill", xpbg.transform, XpBlue); ((Image)xpFill).fillAmount = 0f;

            // Level badge
            var lv = Panel("LvBadge", hud.transform, Cream);
            Anchor(lv, V(0, 1), V(0, 1), V(0, 1), V(28, -54), new Vector2(150, 64));
            var lvTxt = Txt("LvTxt", lv.transform, "LV 1", 30, TextAnchor.MiddleCenter, DarkTx); StretchT(lvTxt);

            // Kills + Coins pills (top-right)
            var kills = Pill("Kills", hud.transform, V(1, 1), new Vector2(-198, -54), Red, "0", out var killsTxt);
            var coins = Pill("Coins", hud.transform, V(1, 1), new Vector2(-28, -54), Yellow, "0", out _);

            // Timer
            var timer = Txt("Timer", hud.transform, "0:00", 60, TextAnchor.UpperCenter, Color.white);
            Anchor(timer.gameObject, V(0.5f, 1), V(0.5f, 1), V(0.5f, 1), V(0, -66), new Vector2(500, 90));

            // HP bar + heart
            var hpbg = Panel("HpBg", hud.transform, new Color(0, 0, 0, 0.4f));
            Anchor(hpbg, V(0.5f, 1), V(0.5f, 1), V(0.5f, 1), V(0, -168), new Vector2(560, 40));
            var hpFill = Bar("HpFill", hpbg.transform, Red); ((Image)hpFill).fillAmount = 1f;
            var heart = Img("Heart", hud.transform, Icon("heart"), Red);
            Anchor(heart, V(0.5f, 1), V(0.5f, 1), V(0.5f, 1), V(-292, -168), new Vector2(46, 46));

            // Weapon slots (bottom-left, decorative)
            string[] wi = { "sauce", "pin", "garlic" };
            for (int i = 0; i < 3; i++)
            {
                var slot = Panel("Slot" + i, hud.transform, Cream);
                Anchor(slot, V(0, 0), V(0, 0), V(0, 0), new Vector2(30 + i * 108, 300), new Vector2(96, 96));
                var ic = Img("i", slot.transform, Icon(wi[i]), DarkTx);
                Anchor(ic, V(0.5f, 0.5f), V(0.5f, 0.5f), V(0.5f, 0.5f), Vector2.zero, new Vector2(64, 64));
            }

            var comp = hud.AddComponent<Hud>();
            SetRef(comp, "hpFill", hpFill); SetRef(comp, "xpFill", xpFill);
            SetRef(comp, "levelText", lvTxt); SetRef(comp, "timerText", timer);
            SetRef(comp, "killsText", killsTxt); SetRef(comp, "health", health);
        }

        // ── Level Up ─────────────────────────────────────────────────────────
        static void BuildLevelUp(Transform canvas)
        {
            var host = New("LevelUpPanel", canvas); Stretch(host);
            var dim = Panel("Dim", host.transform, LevelBg); Stretch(dim.GetComponent<RectTransform>());

            var title = Txt("Title", dim.transform, "LEVEL UP!", 78, TextAnchor.MiddleCenter, Yellow);
            Anchor(title.gameObject, V(0.5f, 0.5f), V(0.5f, 0.5f), V(0.5f, 0.5f), V(0, 600), new Vector2(900, 120));
            var sub = Txt("Sub", dim.transform, "CHOOSE AN UPGRADE", 30, TextAnchor.MiddleCenter, Yellow);
            Anchor(sub.gameObject, V(0.5f, 0.5f), V(0.5f, 0.5f), V(0.5f, 0.5f), V(0, 510), new Vector2(900, 50));

            var buttons = new Button[3]; var names = new Text[3]; var descs = new Text[3];
            var rar = new Text[3]; var strips = new Image[3]; var icons = new Image[3];
            for (int i = 0; i < 3; i++)
            {
                var card = Panel("Card" + i, dim.transform, Cream);
                Anchor(card, V(0.5f, 0.5f), V(0.5f, 0.5f), V(0.5f, 0.5f), V(0, 260 - i * 240), new Vector2(880, 210));
                buttons[i] = card.AddComponent<Button>();

                var strip = Img("Strip", card.transform, _rr, Red);
                ((Image)strip).type = Image.Type.Sliced;
                Anchor(strip, V(0, 0), V(0, 1), V(0, 0.5f), V(18, 0), new Vector2(20, -36)); strips[i] = (Image)strip;

                var box = Panel("IconBox", card.transform, Color.white);
                Anchor(box, V(0, 0.5f), V(0, 0.5f), V(0, 0.5f), V(56, 0), new Vector2(130, 130));
                var ic = Img("Icon", box.transform, null, DarkTx);
                Anchor(ic, V(0.5f, 0.5f), V(0.5f, 0.5f), V(0.5f, 0.5f), Vector2.zero, new Vector2(88, 88)); icons[i] = (Image)ic;

                names[i] = Txt("Name", card.transform, "", 38, TextAnchor.MiddleLeft, DarkTx);
                Anchor(names[i].gameObject, V(0, 0.5f), V(0, 0.5f), V(0, 0.5f), V(214, 34), new Vector2(560, 54));
                descs[i] = Txt("Desc", card.transform, "", 26, TextAnchor.MiddleLeft, GrayTx);
                Anchor(descs[i].gameObject, V(0, 0.5f), V(0, 0.5f), V(0, 0.5f), V(214, -30), new Vector2(600, 44));

                var badge = Panel("Badge", card.transform, new Color(0.93f, 0.90f, 0.84f));
                Anchor(badge, V(1, 0.5f), V(1, 0.5f), V(1, 0.5f), V(-28, 62), new Vector2(150, 52));
                rar[i] = Txt("R", badge.transform, "", 22, TextAnchor.MiddleCenter, DarkTx); StretchT(rar[i]);
            }

            var comp = host.AddComponent<LevelUpPanel>();
            SetRef(comp, "root", dim);
            SetArr(comp, "buttons", buttons); SetArr(comp, "nameTexts", names); SetArr(comp, "descTexts", descs);
            SetArr(comp, "rarityTexts", rar); SetArr(comp, "rarityStrips", strips); SetArr(comp, "icons", icons);
            string[] ids = { "sauce", "pin", "boots", "heart", "garlic", "arrow" };
            var sprites = new Sprite[ids.Length];
            for (int i = 0; i < ids.Length; i++) sprites[i] = Icon(ids[i]);
            SetStrArr(comp, "iconIds", ids); SetArr(comp, "iconSprites", sprites);
        }

        // ── Game Over ────────────────────────────────────────────────────────
        static void BuildGameOver(Transform canvas)
        {
            var host = New("GameOverPanel", canvas); Stretch(host);
            var dim = Panel("Dim", host.transform, GoverBg); Stretch(dim.GetComponent<RectTransform>());

            var title = Txt("Title", dim.transform, "TIME'S UP!", 74, TextAnchor.MiddleCenter, Red);
            Anchor(title.gameObject, V(0.5f, 0.5f), V(0.5f, 0.5f), V(0.5f, 0.5f), V(0, 600), new Vector2(900, 120));
            var sub = Txt("Sub", dim.transform, "the kitchen got you", 28, TextAnchor.MiddleCenter, new Color(0.8f, 0.6f, 0.55f));
            Anchor(sub.gameObject, V(0.5f, 0.5f), V(0.5f, 0.5f), V(0.5f, 0.5f), V(0, 520), new Vector2(900, 50));

            var t = StatCard(dim.transform, "SURVIVED", new Vector2(-220, 250));
            var k = StatCard(dim.transform, "KILLS", new Vector2(220, 250));
            var c = StatCard(dim.transform, "COINS", new Vector2(-220, 40));
            var l = StatCard(dim.transform, "LEVEL", new Vector2(220, 40));

            var play = BigButton(dim.transform, "PLAY AGAIN", new Vector2(0, -230), Red, Color.white, 44);
            var menu = BigButton(dim.transform, "MENU", new Vector2(0, -400), Yellow, DarkTx, 40);

            var comp = host.AddComponent<GameOverPanel>();
            SetRef(comp, "root", dim);
            SetRef(comp, "timeText", t); SetRef(comp, "killsText", k);
            SetRef(comp, "coinsText", c); SetRef(comp, "levelText", l);
            SetRef(comp, "playAgain", play); SetRef(comp, "menu", menu);
        }

        static Text StatCard(Transform parent, string label, Vector2 pos)
        {
            var card = Panel("Card_" + label, parent, Cream);
            Anchor(card, V(0.5f, 0.5f), V(0.5f, 0.5f), V(0.5f, 0.5f), pos, new Vector2(400, 180));
            var lab = Txt("Label", card.transform, label, 24, TextAnchor.UpperLeft, GrayTx);
            Anchor(lab.gameObject, V(0, 1), V(0, 1), V(0, 1), V(30, -26), new Vector2(340, 40));
            var val = Txt("Val", card.transform, "-", 56, TextAnchor.LowerLeft, DarkTx);
            Anchor(val.gameObject, V(0, 0), V(0, 0), V(0, 0), V(30, 24), new Vector2(340, 80));
            return val;
        }

        static Button BigButton(Transform parent, string label, Vector2 pos, Color bg, Color txt, int size)
        {
            var b = Panel("Btn_" + label, parent, bg);
            Anchor(b, V(0.5f, 0.5f), V(0.5f, 0.5f), V(0.5f, 0.5f), pos, new Vector2(820, 150));
            var btn = b.AddComponent<Button>();
            var t = Txt("t", b.transform, label, size, TextAnchor.MiddleCenter, txt); StretchT(t);
            return btn;
        }

        // ── helpers ──────────────────────────────────────────────────────────
        static Vector2 V(float x, float y) => new Vector2(x, y);
        static GameObject New(string n, Transform p) { var g = new GameObject(n, typeof(RectTransform)); g.transform.SetParent(p, false); return g; }
        static void DestroyMatches(Transform parent, string n)
        { for (int i = parent.childCount - 1; i >= 0; i--) { var c = parent.GetChild(i); if (c.name == n) Object.DestroyImmediate(c.gameObject); } }

        static GameObject Panel(string n, Transform p, Color col)
        {
            var g = New(n, p); var im = g.AddComponent<Image>();
            im.sprite = _rr; im.type = Image.Type.Sliced; im.color = col; return g;
        }
        static Image Bar(string n, Transform p, Color col)
        {
            var g = New(n, p); Stretch(g.GetComponent<RectTransform>());
            var im = g.AddComponent<Image>(); im.sprite = _rr; im.type = Image.Type.Filled;
            im.fillMethod = Image.FillMethod.Horizontal; im.fillOrigin = 0; im.color = col; return im;
        }
        static Image Img(string n, Transform p, Sprite spr, Color col)
        {
            var g = New(n, p); var im = g.AddComponent<Image>();
            im.sprite = spr; im.color = col; im.preserveAspect = true; im.raycastTarget = false; return im;
        }
        static Text Txt(string n, Transform p, string s, int size, TextAnchor a, Color col)
        {
            var g = New(n, p); var t = g.AddComponent<Text>();
            t.text = s; t.font = _font; t.fontSize = size; t.alignment = a; t.color = col;
            t.horizontalOverflow = HorizontalWrapMode.Overflow; t.verticalOverflow = VerticalWrapMode.Overflow;
            t.raycastTarget = false; return t;
        }
        static GameObject Pill(string n, Transform p, Vector2 anchor, Vector2 pos, Color dot, string val, out Text txt)
        {
            var g = Panel(n, p, DarkPill);
            Anchor(g, anchor, anchor, anchor, pos, new Vector2(160, 64));
            var d = Img("dot", g.transform, AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Sprites/Sirvival/_circle.png"), dot);
            Anchor(d, V(0, 0.5f), V(0, 0.5f), V(0, 0.5f), V(20, 0), new Vector2(34, 34));
            txt = Txt("v", g.transform, val, 30, TextAnchor.MiddleRight, Color.white);
            Anchor(txt.gameObject, V(1, 0.5f), V(1, 0.5f), V(1, 0.5f), V(-22, 0), new Vector2(100, 50));
            return g;
        }

        static void Stretch(RectTransform rt) { rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one; rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero; }
        static void Stretch(Transform t) => Stretch((RectTransform)t);
        static void Stretch(GameObject g) => Stretch((RectTransform)g.transform);
        static void StretchT(Text t) => Stretch((RectTransform)t.transform);
        static void Anchor(GameObject g, Vector2 aMin, Vector2 aMax, Vector2 piv, Vector2 pos, Vector2 size)
        { var rt = (RectTransform)g.transform; rt.anchorMin = aMin; rt.anchorMax = aMax; rt.pivot = piv; rt.anchoredPosition = pos; rt.sizeDelta = size; }
        static void Anchor(Image im, Vector2 aMin, Vector2 aMax, Vector2 piv, Vector2 pos, Vector2 size) => Anchor(im.gameObject, aMin, aMax, piv, pos, size);
        static void Anchor(Text tx, Vector2 aMin, Vector2 aMax, Vector2 piv, Vector2 pos, Vector2 size) => Anchor(tx.gameObject, aMin, aMax, piv, pos, size);

        static void SetRef(Component c, string f, Object v)
        { var so = new SerializedObject(c); var p = so.FindProperty(f); if (p != null) { p.objectReferenceValue = v; so.ApplyModifiedPropertiesWithoutUndo(); } else Debug.LogWarning("no field " + f); }
        static void SetArr(Component c, string f, Object[] vals)
        { var so = new SerializedObject(c); var p = so.FindProperty(f); p.arraySize = vals.Length; for (int i = 0; i < vals.Length; i++) p.GetArrayElementAtIndex(i).objectReferenceValue = vals[i]; so.ApplyModifiedPropertiesWithoutUndo(); }
        static void SetStrArr(Component c, string f, string[] vals)
        { var so = new SerializedObject(c); var p = so.FindProperty(f); p.arraySize = vals.Length; for (int i = 0; i < vals.Length; i++) p.GetArrayElementAtIndex(i).stringValue = vals[i]; so.ApplyModifiedPropertiesWithoutUndo(); }
    }
}

// force recompile
