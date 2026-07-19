using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;

/// <summary>One-shot editor builder: adds a Credits button to the main menu and a Credits panel
/// (attribution required by the audio-asset licenses), wired into MainMenuNav.</summary>
public static class CreditsBuild
{
    public static void Run()
    {
        var scene = EditorSceneManager.OpenScene("Assets/Scenes/MainMenu.unity", OpenSceneMode.Single);
        var canvasGo = GameObject.Find("Canvas");
        var canvas = canvasGo.transform;
        var nav = Object.FindFirstObjectByType<MainMenuNav>();
        var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        var playBtn = GameObject.Find("Canvas/PlayBtn");
        var blue = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/wood/button-blue.png");
        var green = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/wood/button-green.png");
        var panelSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/wood/panel-dialog.png");

        // --- remove any previous build so re-running is clean ---
        var oldBtn = GameObject.Find("Canvas/CreditsBtn"); if (oldBtn != null) Object.DestroyImmediate(oldBtn);
        var oldPanel = GameObject.Find("Canvas/CreditsPanel"); if (oldPanel != null) Object.DestroyImmediate(oldPanel);

        // --- Credits button on the menu (clone PlayBtn for identical styling) ---
        var creditsBtnGo = Object.Instantiate(playBtn, canvas);
        creditsBtnGo.name = "CreditsBtn";
        var cbRt = creditsBtnGo.GetComponent<RectTransform>();
        cbRt.anchorMin = new Vector2(0.5f, 0f); cbRt.anchorMax = new Vector2(0.5f, 0f); cbRt.pivot = new Vector2(0.5f, 0f);
        cbRt.anchoredPosition = new Vector2(0f, 34f);
        cbRt.localScale = Vector3.one * 0.6f;
        var cbImg = creditsBtnGo.GetComponent<Image>(); if (cbImg != null && blue != null) cbImg.sprite = blue;
        SetLabel(creditsBtnGo.transform, "CREDITS");
        var creditsButton = creditsBtnGo.GetComponent<Button>();

        // --- Credits panel: full-screen dimmer + dialog + title + body + back ---
        var panelGo = new GameObject("CreditsPanel", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        panelGo.transform.SetParent(canvas, false);
        var pRt = panelGo.GetComponent<RectTransform>();
        pRt.anchorMin = Vector2.zero; pRt.anchorMax = Vector2.one; pRt.offsetMin = Vector2.zero; pRt.offsetMax = Vector2.zero;
        var dim = panelGo.GetComponent<Image>(); dim.color = new Color(0f, 0f, 0f, 0.86f); dim.raycastTarget = true;

        // Dialog box
        var dlg = new GameObject("Dialog", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        dlg.transform.SetParent(panelGo.transform, false);
        var dRt = dlg.GetComponent<RectTransform>();
        dRt.anchorMin = new Vector2(0.5f, 0.5f); dRt.anchorMax = new Vector2(0.5f, 0.5f); dRt.pivot = new Vector2(0.5f, 0.5f);
        dRt.sizeDelta = new Vector2(860f, 940f); dRt.anchoredPosition = Vector2.zero;
        var dImg = dlg.GetComponent<Image>();
        if (panelSprite != null) { dImg.sprite = panelSprite; dImg.type = Image.Type.Sliced; dImg.pixelsPerUnitMultiplier = 3.2f; }
        else dImg.color = new Color(0.15f, 0.1f, 0.06f, 0.98f);

        // Title
        MakeText(dlg.transform, "Title", "CREDITS", font, 66, FontStyle.Bold,
            new Vector2(0.5f, 1f), new Vector2(0f, -70f), new Vector2(700f, 90f),
            new Color(1f, 0.92f, 0.6f), TextAnchor.MiddleCenter);

        // Body (attribution)
        string body =
            "Capybara Platformer\n\n" +
            "— MUSIC —\n" +
            "HAI Soundworks\n" +
            "haisoundworks.com\n\n" +
            "— SOUND FX —\n" +
            "Leohpaz  •  RPG Essentials\n" +
            "Krishna Palacio  •  Minifantasy\n" +
            "Kenney  •  CC0\n\n" +
            "Made with Unity";
        MakeText(dlg.transform, "Body", body, font, 34, FontStyle.Normal,
            new Vector2(0.5f, 0.5f), new Vector2(0f, 30f), new Vector2(720f, 560f),
            new Color(0.98f, 0.95f, 0.88f), TextAnchor.MiddleCenter);

        // Back button (clone PlayBtn, green)
        var backGo = Object.Instantiate(playBtn, dlg.transform);
        backGo.name = "BackBtn";
        var bRt = backGo.GetComponent<RectTransform>();
        bRt.anchorMin = new Vector2(0.5f, 0f); bRt.anchorMax = new Vector2(0.5f, 0f); bRt.pivot = new Vector2(0.5f, 0f);
        bRt.anchoredPosition = new Vector2(0f, 60f);
        bRt.localScale = Vector3.one * 0.66f;
        var bImg = backGo.GetComponent<Image>(); if (bImg != null && green != null) bImg.sprite = green;
        SetLabel(backGo.transform, "BACK");
        var backButton = backGo.GetComponent<Button>();

        panelGo.SetActive(false);

        // --- wire MainMenuNav refs ---
        var so = new SerializedObject(nav);
        so.FindProperty("creditsButton").objectReferenceValue = creditsButton;
        so.FindProperty("creditsPanel").objectReferenceValue = panelGo;
        so.FindProperty("creditsCloseButton").objectReferenceValue = backButton;
        so.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(nav);

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("CREDITSBUILD done: button + panel built and wired.");
    }

    static void SetLabel(Transform btn, string txt)
    {
        var label = btn.Find("Label");
        if (label != null) { var t = label.GetComponent<Text>(); if (t != null) t.text = txt; }
    }

    static void MakeText(Transform parent, string name, string txt, Font font, int size, FontStyle style,
        Vector2 anchor, Vector2 pos, Vector2 sizeDelta, Color color, TextAnchor align)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
        go.transform.SetParent(parent, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = anchor; rt.anchorMax = anchor; rt.pivot = anchor;
        rt.anchoredPosition = pos; rt.sizeDelta = sizeDelta;
        var t = go.GetComponent<Text>();
        t.text = txt; t.font = font; t.fontSize = size; t.fontStyle = style;
        t.color = color; t.alignment = align; t.horizontalOverflow = HorizontalWrapMode.Wrap; t.verticalOverflow = VerticalWrapMode.Overflow;
        var sh = go.AddComponent<Shadow>(); sh.effectColor = new Color(0f, 0f, 0f, 0.55f); sh.effectDistance = new Vector2(2f, -2f);
    }
}
