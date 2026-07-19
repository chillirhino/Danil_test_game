using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEditor;

public class URPSetup
{
    public static string Main()
    {
        const string rendPath = "Assets/Settings/URP-Renderer.asset";
        const string urpPath = "Assets/Settings/URP-Asset.asset";

        var urp = AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(urpPath);
        if (urp == null)
        {
            var rd = ScriptableObject.CreateInstance<UniversalRendererData>();
            AssetDatabase.CreateAsset(rd, rendPath);
            urp = UniversalRenderPipelineAsset.Create(rd);
            AssetDatabase.CreateAsset(urp, urpPath);
            AssetDatabase.SaveAssets();
        }

        GraphicsSettings.defaultRenderPipeline = urp;
        int levels = QualitySettings.names.Length;
        int cur = QualitySettings.GetQualityLevel();
        for (int i = 0; i < levels; i++)
        {
            QualitySettings.SetQualityLevel(i, false);
            QualitySettings.renderPipeline = urp;
        }
        QualitySettings.SetQualityLevel(cur, false);

        // Remove the HDRP-only volume (its overrides don't exist in URP).
        var hv = GameObject.Find("Sky and Fog Volume");
        if (hv != null) Object.DestroyImmediate(hv);

        // Sun tuned for URP's non-physical light units.
        var sun = GameObject.Find("Directional Light");
        if (sun != null)
        {
            var l = sun.GetComponent<Light>();
            l.intensity = 1.2f;
            l.shadows = LightShadows.Soft;
            l.transform.rotation = Quaternion.Euler(45f, 30f, 0f);
        }

        AssetDatabase.SaveAssets();
        var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(scene);
        UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene);

        return "URP active=" + (GraphicsSettings.currentRenderPipeline != null ? GraphicsSettings.currentRenderPipeline.name : "NULL");
    }
}
