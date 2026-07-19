using System.Text;
using UnityEngine;
using UnityEditor;

public class ProbeTrees
{
    public static string Main()
    {
        var sb = new StringBuilder();
        string[] mats = { "bark01", "bark02", "branch01", "branch02", "bush01", "bush02" };
        foreach (var n in mats)
        {
            var m = AssetDatabase.LoadAssetAtPath<Material>("Assets/NatureStarterKit2/Materials/" + n + ".mat");
            sb.Append(n + " -> " + (m != null ? m.shader.name : "MISSING") + "\n");
        }
        return sb.ToString();
    }
}
