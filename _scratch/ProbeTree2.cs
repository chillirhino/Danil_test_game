using System.Text;
using UnityEngine;
using UnityEditor;

public class ProbeTree2
{
    static string Info(string n)
    {
        var m = AssetDatabase.LoadAssetAtPath<Material>("Assets/NatureStarterKit2/Materials/" + n + ".mat");
        if (m == null) return n + " MISSING";
        var bm = m.HasProperty("_BaseMap") ? m.GetTexture("_BaseMap") : null;
        var bump = m.HasProperty("_BumpMap") ? m.GetTexture("_BumpMap") : null;
        return n + " shader=" + m.shader.name
            + " BaseMap=" + (bm != null ? bm.name : "NULL")
            + " Bump=" + (bump != null ? bump.name : "null")
            + " BaseColor=" + (m.HasProperty("_BaseColor") ? m.GetColor("_BaseColor").ToString() : "-")
            + " AlphaClip=" + (m.HasProperty("_AlphaClip") ? m.GetFloat("_AlphaClip").ToString() : "-")
            + " Cull=" + (m.HasProperty("_Cull") ? m.GetFloat("_Cull").ToString() : "-");
    }

    public static string Main()
    {
        var sb = new StringBuilder();
        foreach (var n in new[] { "bark01", "branch01", "bush01" })
            sb.Append(Info(n) + "\n");
        // Also inspect what a tree prefab's renderers use.
        var tree = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/NatureStarterKit2/Nature/tree01.prefab");
        if (tree != null)
        {
            var rs = tree.GetComponentsInChildren<Renderer>();
            sb.Append("tree01 renderers=" + rs.Length + ": ");
            foreach (var r in rs)
                foreach (var mm in r.sharedMaterials)
                    sb.Append((mm != null ? mm.name + "/" + mm.shader.name : "null") + ", ");
        }
        return sb.ToString();
    }
}
