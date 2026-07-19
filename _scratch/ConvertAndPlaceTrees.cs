using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class ConvertAndPlaceTrees
{
    static void ConvertTreeMaterials()
    {
        var urpLit = Shader.Find("Universal Render Pipeline/Lit");
        foreach (var guid in AssetDatabase.FindAssets("t:Material", new[] { "Assets/NatureStarterKit2" }))
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            var m = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (m == null || m.shader == null) continue;
            string sh = m.shader.name;
            if (!sh.StartsWith("Nature/Tree Creator")) continue;

            bool isLeaf = sh.Contains("Leaves");
            Texture main = m.HasProperty("_MainTex") ? m.GetTexture("_MainTex") : null;
            Texture bump = null;
            if (m.HasProperty("_BumpMap")) bump = m.GetTexture("_BumpMap");
            if (bump == null && m.HasProperty("_BumpSpecMap")) bump = m.GetTexture("_BumpSpecMap");

            m.shader = urpLit;
            if (main != null) { m.SetTexture("_BaseMap", main); m.SetColor("_BaseColor", Color.white); }
            if (bump != null) { m.SetTexture("_BumpMap", bump); m.EnableKeyword("_NORMALMAP"); }
            m.SetFloat("_Smoothness", 0.05f);

            if (isLeaf)
            {
                m.SetFloat("_AlphaClip", 1f);
                m.EnableKeyword("_ALPHATEST_ON");
                m.SetFloat("_Cutoff", 0.5f);
                m.SetFloat("_Cull", 0f); // draw both sides so leaves aren't one-sided
            }
            EditorUtility.SetDirty(m);
        }
        AssetDatabase.SaveAssets();
    }

    public static string Main()
    {
        ConvertTreeMaterials();

        var trees = new[]
        {
            AssetDatabase.LoadAssetAtPath<GameObject>("Assets/NatureStarterKit2/Nature/tree01.prefab"),
            AssetDatabase.LoadAssetAtPath<GameObject>("Assets/NatureStarterKit2/Nature/tree02.prefab"),
            AssetDatabase.LoadAssetAtPath<GameObject>("Assets/NatureStarterKit2/Nature/tree03.prefab"),
            AssetDatabase.LoadAssetAtPath<GameObject>("Assets/NatureStarterKit2/Nature/tree04.prefab"),
        };
        var bushes = new[]
        {
            AssetDatabase.LoadAssetAtPath<GameObject>("Assets/NatureStarterKit2/Nature/bush01.prefab"),
            AssetDatabase.LoadAssetAtPath<GameObject>("Assets/NatureStarterKit2/Nature/bush03.prefab"),
        };

        var envGo = GameObject.Find("Environment");
        if (envGo == null) return "no Environment";
        var env = envGo.transform;

        // Remove placeholder primitive trees.
        var kill = new List<GameObject>();
        foreach (Transform c in env) if (c.name.StartsWith("Tree_")) kill.Add(c.gameObject);
        foreach (var g in kill) Object.DestroyImmediate(g);

        var holder = env.Find("Trees");
        if (holder != null) Object.DestroyImmediate(holder.gameObject);
        holder = new GameObject("Trees").transform;
        holder.SetParent(env, false);

        float[] zs = { -4f, 2f, 9f, 16f, 24f, 33f, 42f, 6f, 20f, 38f };
        float[] xs = { -5f, 5.5f, -6f, 5f, -5.5f, 6f, -5f, 7.5f, -8f, 8f };
        for (int i = 0; i < zs.Length; i++)
        {
            var prefab = trees[i % trees.Length];
            if (prefab == null) continue;
            var t = (GameObject)PrefabUtility.InstantiatePrefab(prefab, holder);
            t.transform.localPosition = new Vector3(xs[i], 0f, zs[i]);
            t.transform.localRotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
            t.transform.localScale = Vector3.one * Random.Range(0.8f, 1.3f);
        }

        // A few bushes near the path.
        float[] bz = { 3f, 11f, 19f, 29f, 37f };
        float[] bx = { 2.4f, -2.5f, 2.6f, -2.4f, 2.5f };
        for (int i = 0; i < bz.Length && bushes[0] != null; i++)
        {
            var prefab = bushes[i % bushes.Length];
            if (prefab == null) continue;
            var b = (GameObject)PrefabUtility.InstantiatePrefab(prefab, holder);
            b.transform.localPosition = new Vector3(bx[i], 0f, bz[i]);
            b.transform.localRotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
            b.transform.localScale = Vector3.one * Random.Range(0.7f, 1.1f);
        }

        var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(scene);
        UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene);
        return "trees=" + zs.Length + " bushes=" + bz.Length;
    }
}
