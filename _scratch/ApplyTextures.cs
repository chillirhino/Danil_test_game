using UnityEngine;
using UnityEditor;

public class ApplyTextures
{
    const string TEX = "Assets/Art/Textures/";
    const string MAT = "Assets/Art/Materials/";

    static Texture2D LoadTex(string file, bool normal)
    {
        string p = TEX + file;
        var ti = AssetImporter.GetAtPath(p) as TextureImporter;
        if (ti != null)
        {
            var want = normal ? TextureImporterType.NormalMap : TextureImporterType.Default;
            if (ti.textureType != want) { ti.textureType = want; ti.SaveAndReimport(); }
        }
        return AssetDatabase.LoadAssetAtPath<Texture2D>(p);
    }

    static void Apply(string matName, string surf, Vector2 tiling)
    {
        var m = AssetDatabase.LoadAssetAtPath<Material>(MAT + matName + ".mat");
        if (m == null) return;
        var diff = LoadTex(surf + "_diff.jpg", false);
        var nor = LoadTex(surf + "_nor_gl.jpg", true);
        if (diff != null)
        {
            m.SetTexture("_BaseMap", diff);
            m.SetColor("_BaseColor", Color.white);
            m.SetTextureScale("_BaseMap", tiling);
        }
        if (nor != null)
        {
            m.SetTexture("_BumpMap", nor);
            m.SetTextureScale("_BumpMap", tiling);
            m.EnableKeyword("_NORMALMAP");
            if (m.HasProperty("_BumpScale")) m.SetFloat("_BumpScale", 1f);
        }
        if (m.HasProperty("_Smoothness")) m.SetFloat("_Smoothness", 0.1f);
        EditorUtility.SetDirty(m);
    }

    public static string Main()
    {
        AssetDatabase.Refresh();
        Apply("Grass", "grass", new Vector2(40f, 40f));
        Apply("Dirt", "path", new Vector2(3f, 30f));
        Apply("Bark", "bark", new Vector2(1f, 2f));
        Apply("Rock", "rock", new Vector2(2f, 2f));
        AssetDatabase.SaveAssets();
        return "textures applied";
    }
}
