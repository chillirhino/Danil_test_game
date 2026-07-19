using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEngine.EventSystems;
using PoK.Player;

public class ArmsSetup
{
    static Sprite ImportSprite(string path)
    {
        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
        var ti = AssetImporter.GetAtPath(path) as TextureImporter;
        if (ti != null)
        {
            ti.textureType = TextureImporterType.Sprite;
            ti.spriteImportMode = SpriteImportMode.Single;
            ti.alphaIsTransparency = true;
            ti.mipmapEnabled = false;
            ti.SaveAndReimport();
        }
        return AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }

    static RectTransform MakeArm(Transform parent, string name, Sprite sprite, bool rightSide, float height)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image));
        go.transform.SetParent(parent, false);
        var img = go.GetComponent<Image>();
        img.sprite = sprite;
        img.preserveAspect = true;
        img.raycastTarget = false;
        var rt = go.GetComponent<RectTransform>();
        float w = height * (sprite.rect.width / sprite.rect.height);
        Vector2 anchor = rightSide ? new Vector2(1f, 0f) : new Vector2(0f, 0f);
        rt.anchorMin = anchor;
        rt.anchorMax = anchor;
        rt.pivot = anchor;
        rt.sizeDelta = new Vector2(w, height);
        rt.anchoredPosition = new Vector2(rightSide ? -20f : 20f, -40f);
        return rt;
    }

    public static string Main()
    {
        var left = ImportSprite("Assets/Art/Arms/LeftArm.png");
        var right = ImportSprite("Assets/Art/Arms/RightArm.png");
        if (left == null || right == null) return "sprite import failed";

        // Remove any previous HUD so re-runs are clean.
        var old = GameObject.Find("HUD");
        if (old != null) Object.DestroyImmediate(old);

        var canvasGo = new GameObject("HUD", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        var canvas = canvasGo.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasGo.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080f, 1920f);
        scaler.matchWidthOrHeight = 0.5f;

        if (Object.FindAnyObjectByType<EventSystem>() == null)
            new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));

        float armHeight = 900f;
        var leftRt = MakeArm(canvasGo.transform, "LeftArm", left, false, armHeight);
        var rightRt = MakeArm(canvasGo.transform, "RightArm", right, true, armHeight);

        var arms = canvasGo.AddComponent<FirstPersonArms>();
        arms.leftArm = leftRt;
        arms.rightArm = rightRt;
        arms.controller = Object.FindAnyObjectByType<FirstPersonController>();

        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(canvasGo.scene);
        UnityEditor.SceneManagement.EditorSceneManager.SaveScene(canvasGo.scene);

        return "ok armWidthL=" + leftRt.sizeDelta.x + " controller=" + (arms.controller != null);
    }
}
