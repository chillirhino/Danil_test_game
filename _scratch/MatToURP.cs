using UnityEngine;
using UnityEditor;

public class MatToURP
{
    public static string Main()
    {
        var urpLit = Shader.Find("Universal Render Pipeline/Lit");
        if (urpLit == null) return "no URP/Lit shader";

        string[] names = { "Grass", "Dirt", "Bark", "Leaf", "Rock" };
        Color[] cols =
        {
            new Color(0.18f, 0.32f, 0.12f),
            new Color(0.34f, 0.25f, 0.16f),
            new Color(0.22f, 0.14f, 0.08f),
            new Color(0.13f, 0.28f, 0.10f),
            new Color(0.30f, 0.30f, 0.33f),
        };

        int done = 0;
        for (int i = 0; i < names.Length; i++)
        {
            var m = AssetDatabase.LoadAssetAtPath<Material>("Assets/Art/Materials/" + names[i] + ".mat");
            if (m == null) continue;
            m.shader = urpLit;
            if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", cols[i]);
            if (m.HasProperty("_Smoothness")) m.SetFloat("_Smoothness", 0.05f);
            EditorUtility.SetDirty(m);
            done++;
        }
        AssetDatabase.SaveAssets();
        return "converted " + done + " materials to URP/Lit";
    }
}
