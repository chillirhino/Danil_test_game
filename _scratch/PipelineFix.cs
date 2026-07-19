using UnityEngine;
using UnityEngine.Rendering;
using UnityEditor;

public class PipelineFix
{
    public static string Main()
    {
        string before = "before: current=" + (GraphicsSettings.currentRenderPipeline != null ? GraphicsSettings.currentRenderPipeline.name : "NULL(builtin)")
            + " default=" + (GraphicsSettings.defaultRenderPipeline != null ? GraphicsSettings.defaultRenderPipeline.name : "null")
            + " qualityRP=" + (QualitySettings.renderPipeline != null ? QualitySettings.renderPipeline.name : "null");

        var hdrp = AssetDatabase.LoadAssetAtPath<RenderPipelineAsset>("Assets/Settings/HDRP High Fidelity.asset");
        if (hdrp == null) return before + " | FAILED to load HDRP asset";

        GraphicsSettings.defaultRenderPipeline = hdrp;

        // Clear per-quality overrides so every level uses the HDRP default.
        int levels = QualitySettings.names.Length;
        int current = QualitySettings.GetQualityLevel();
        for (int i = 0; i < levels; i++)
        {
            QualitySettings.SetQualityLevel(i, false);
            QualitySettings.renderPipeline = hdrp;
        }
        QualitySettings.SetQualityLevel(current, false);

        AssetDatabase.SaveAssets();

        string after = "after: current=" + (GraphicsSettings.currentRenderPipeline != null ? GraphicsSettings.currentRenderPipeline.name : "NULL(builtin)")
            + " default=" + (GraphicsSettings.defaultRenderPipeline != null ? GraphicsSettings.defaultRenderPipeline.name : "null");

        return before + " || " + after;
    }
}
