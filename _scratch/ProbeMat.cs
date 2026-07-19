using UnityEngine;
using UnityEditor;

public class ProbeMat
{
    public static string Main()
    {
        var m = AssetDatabase.LoadAssetAtPath<Material>("Assets/Art/Materials/Grass.mat");
        string s = "Grass ";
        if (m == null) s += "MISSING";
        else
        {
            s += "shader=" + m.shader.name;
            s += " hasBaseColor=" + m.HasProperty("_BaseColor");
            if (m.HasProperty("_BaseColor")) s += " _BaseColor=" + m.GetColor("_BaseColor");
            if (m.HasProperty("_Color")) s += " _Color=" + m.GetColor("_Color");
            s += " map=" + (m.HasProperty("_BaseColorMap") && m.GetTexture("_BaseColorMap") != null);
        }
        var ground = GameObject.Find("Ground");
        if (ground != null)
        {
            var gm = ground.GetComponent<Renderer>().sharedMaterial;
            s += " | Ground.mat=" + (gm != null ? gm.name : "null");
            if (gm != null && gm.HasProperty("_BaseColor")) s += " groundColor=" + gm.GetColor("_BaseColor");
        }
        return s;
    }
}
