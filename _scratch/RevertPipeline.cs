using UnityEngine;
using UnityEngine.Rendering;
using UnityEditor;

public class RevertPipeline
{
    public static string Main()
    {
        GraphicsSettings.defaultRenderPipeline = null;
        int levels = QualitySettings.names.Length;
        int current = QualitySettings.GetQualityLevel();
        for (int i = 0; i < levels; i++)
        {
            QualitySettings.SetQualityLevel(i, false);
            QualitySettings.renderPipeline = null;
        }
        QualitySettings.SetQualityLevel(current, false);
        AssetDatabase.SaveAssets();
        return "reverted to built-in: current=" + (GraphicsSettings.currentRenderPipeline != null ? GraphicsSettings.currentRenderPipeline.name : "NULL(builtin)");
    }
}
