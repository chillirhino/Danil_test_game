using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

public class EnvironmentSetup
{
    static Material template;

    static Material FindHdrpLitTemplate()
    {
        // 1) HDRP package default material (always valid).
        string[] candidates =
        {
            "Packages/com.unity.render-pipelines.high-definition/Runtime/RenderPipelineResources/Material/DefaultHDMaterial.mat",
        };
        foreach (var p in candidates)
        {
            var m = AssetDatabase.LoadAssetAtPath<Material>(p);
            if (m != null && m.shader != null && m.shader.name == "HDRP/Lit") return m;
        }
        // 2) Any material in the project already using HDRP/Lit.
        foreach (var guid in AssetDatabase.FindAssets("t:Material"))
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            if (path.StartsWith("Assets/Art/Materials/")) continue; // skip our own outputs
            var m = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (m != null && m.shader != null && m.shader.name == "HDRP/Lit") return m;
        }
        return null;
    }

    static Material Mat(string name, Color c, float smooth)
    {
        string path = "Assets/Art/Materials/" + name + ".mat";
        AssetDatabase.DeleteAsset(path); // rebuild clean from the valid HDRP template
        var m = new Material(template);
        AssetDatabase.CreateAsset(m, path);
        m.SetColor("_BaseColor", c);
        if (m.HasProperty("_Smoothness")) m.SetFloat("_Smoothness", smooth);
        HDMaterial.ValidateMaterial(m);
        EditorUtility.SetDirty(m);
        return m;
    }

    static GameObject Prim(PrimitiveType t, string name, Transform parent, Vector3 pos, Vector3 scale, Material m)
    {
        var go = GameObject.CreatePrimitive(t);
        go.name = name;
        go.transform.SetParent(parent, false);
        go.transform.localPosition = pos;
        go.transform.localScale = scale;
        go.GetComponent<Renderer>().sharedMaterial = m;
        return go;
    }

    public static string Main()
    {
        // Find an EXISTING, valid HDRP/Lit material to clone (Shader.Find alone renders magenta).
        template = FindHdrpLitTemplate();
        if (template == null) return "no HDRP/Lit template material found";
        System.IO.Directory.CreateDirectory(Application.dataPath + "/Art/Materials");
        AssetDatabase.Refresh();

        var grass = Mat("Grass", new Color(0.18f, 0.32f, 0.12f), 0.05f);
        var dirt = Mat("Dirt", new Color(0.34f, 0.25f, 0.16f), 0.05f);
        var trunk = Mat("Bark", new Color(0.22f, 0.14f, 0.08f), 0.05f);
        var leaf = Mat("Leaf", new Color(0.13f, 0.28f, 0.10f), 0.05f);
        var rock = Mat("Rock", new Color(0.30f, 0.30f, 0.33f), 0.10f);

        // Ground -> grass
        var ground = GameObject.Find("Ground");
        if (ground != null) ground.GetComponent<Renderer>().sharedMaterial = grass;

        // Clean previous environment root so re-runs are idempotent.
        var oldEnv = GameObject.Find("Environment");
        if (oldEnv != null) Object.DestroyImmediate(oldEnv);
        var env = new GameObject("Environment").transform;

        // Path: a long dirt strip going forward (+Z) from the player.
        Prim(PrimitiveType.Cube, "Path", env, new Vector3(0f, 0.02f, 25f), new Vector3(3.2f, 0.04f, 80f), dirt);

        // Trees along both sides.
        float[] zs = { -4f, 2f, 9f, 16f, 24f, 33f, 42f, 6f, 20f, 38f };
        float[] xs = { -5f, 5.5f, -6f, 5f, -5.5f, 6f, -5f, 7.5f, -8f, 8f };
        for (int i = 0; i < zs.Length; i++)
        {
            var tree = new GameObject("Tree_" + i).transform;
            tree.SetParent(env, false);
            tree.localPosition = new Vector3(xs[i], 0f, zs[i]);
            float h = 1.4f + (i % 3) * 0.4f;
            Prim(PrimitiveType.Cylinder, "Trunk", tree, new Vector3(0f, h, 0f), new Vector3(0.35f, h, 0.35f), trunk);
            Prim(PrimitiveType.Sphere, "Foliage", tree, new Vector3(0f, h * 2f + 0.6f, 0f), new Vector3(2.4f, 2.8f, 2.4f), leaf);
        }

        // A few rocks near the path.
        float[] rz = { 0f, 12f, 28f, 40f, 18f };
        float[] rx = { 2.5f, -2.6f, 2.7f, -2.5f, 2.4f };
        for (int i = 0; i < rz.Length; i++)
        {
            var s = 0.6f + (i % 3) * 0.25f;
            Prim(PrimitiveType.Sphere, "Rock_" + i, env, new Vector3(rx[i], s * 0.4f, rz[i]), new Vector3(s * 1.6f, s, s * 1.3f), rock);
        }

        AssetDatabase.SaveAssets();
        var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(scene);
        UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene);
        return "ok trees=" + zs.Length + " rocks=" + rz.Length;
    }
}
