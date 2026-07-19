using UnityEngine;
using UnityEditor;

public class ReskinTrees
{
    public static string Main()
    {
        var bark = AssetDatabase.LoadAssetAtPath<Material>("Assets/NatureStarterKit2/Materials/bark01.mat");
        var leaf = AssetDatabase.LoadAssetAtPath<Material>("Assets/NatureStarterKit2/Materials/branch01.mat");
        var bushLeaf = AssetDatabase.LoadAssetAtPath<Material>("Assets/NatureStarterKit2/Materials/bush01.mat");
        if (bark == null || leaf == null) return "converted mats missing";

        var env = GameObject.Find("Environment");
        if (env == null) return "no Environment";
        var trees = env.transform.Find("Trees");
        if (trees == null) return "no Trees holder";

        int fixedRenderers = 0;
        foreach (Transform inst in trees)
        {
            bool isBush = inst.name.StartsWith("bush");
            foreach (var r in inst.GetComponentsInChildren<Renderer>())
            {
                var mats = r.sharedMaterials;
                for (int k = 0; k < mats.Length; k++)
                {
                    var sh = mats[k] != null ? mats[k].shader.name : "";
                    if (sh.Contains("Leaves"))
                        mats[k] = isBush ? bushLeaf : leaf;
                    else if (sh.Contains("Bark"))
                        mats[k] = bark;
                }
                r.sharedMaterials = mats;
                fixedRenderers++;
            }
        }

        var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(scene);
        UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene);
        return "reskinned renderers=" + fixedRenderers;
    }
}
