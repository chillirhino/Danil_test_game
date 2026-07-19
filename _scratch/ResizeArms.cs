using UnityEngine;
using UnityEditor;

public class ResizeArms
{
    static void Set(string path, float height)
    {
        var go = GameObject.Find(path);
        if (go == null) return;
        var rt = go.GetComponent<RectTransform>();
        float aspect = 1086f / 1448f;
        rt.sizeDelta = new Vector2(height * aspect, height);
    }

    public static string Main()
    {
        float h = 620f;
        Set("HUD/LeftArm", h);
        Set("HUD/RightArm", h);
        var hud = GameObject.Find("HUD");
        if (hud != null)
        {
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(hud.scene);
            UnityEditor.SceneManagement.EditorSceneManager.SaveScene(hud.scene);
        }
        return "resized to " + h;
    }
}
