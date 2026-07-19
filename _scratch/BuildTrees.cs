using UnityEngine;
using UnityEditor;

public class BuildTrees
{
    static Material bark, leaf;

    static void Prim(PrimitiveType t, Transform parent, Vector3 pos, Vector3 scale, Material m)
    {
        var go = GameObject.CreatePrimitive(t);
        go.transform.SetParent(parent, false);
        go.transform.localPosition = pos;
        go.transform.localScale = scale;
        Object.DestroyImmediate(go.GetComponent<Collider>());
        go.GetComponent<Renderer>().sharedMaterial = m;
    }

    static void MakeTree(Transform parent, Vector3 pos, float yRot, float s)
    {
        var tree = new GameObject("Tree").transform;
        tree.SetParent(parent, false);
        tree.localPosition = pos;
        tree.localRotation = Quaternion.Euler(0f, yRot, 0f);
        tree.localScale = Vector3.one * s;

        float trunkH = 2.2f;
        Prim(PrimitiveType.Cylinder, tree, new Vector3(0f, trunkH, 0f), new Vector3(0.28f, trunkH, 0.28f), bark);

        // Bushy canopy from several overlapping blobs, roughly conical.
        float baseY = trunkH * 2f;
        Prim(PrimitiveType.Sphere, tree, new Vector3(0f, baseY, 0f), new Vector3(2.6f, 2.2f, 2.6f), leaf);
        Prim(PrimitiveType.Sphere, tree, new Vector3(0.7f, baseY + 0.5f, 0.4f), new Vector3(1.8f, 1.7f, 1.8f), leaf);
        Prim(PrimitiveType.Sphere, tree, new Vector3(-0.6f, baseY + 0.7f, -0.5f), new Vector3(1.7f, 1.6f, 1.7f), leaf);
        Prim(PrimitiveType.Sphere, tree, new Vector3(0.1f, baseY + 1.4f, 0.1f), new Vector3(1.7f, 1.7f, 1.7f), leaf);
    }

    public static string Main()
    {
        bark = AssetDatabase.LoadAssetAtPath<Material>("Assets/Art/Materials/Bark.mat");
        leaf = AssetDatabase.LoadAssetAtPath<Material>("Assets/Art/Materials/Leaf.mat");
        if (bark == null || leaf == null) return "materials missing";

        var env = GameObject.Find("Environment");
        if (env == null) return "no Environment";
        var old = env.transform.Find("Trees");
        if (old != null) Object.DestroyImmediate(old.gameObject);
        var holder = new GameObject("Trees").transform;
        holder.SetParent(env.transform, false);

        float[] zs = { -4f, 2f, 9f, 16f, 24f, 33f, 42f, 6f, 20f, 38f, 12f, 30f };
        float[] xs = { -5f, 5.5f, -6f, 5f, -5.5f, 6f, -5f, 7.5f, -8f, 8f, -7f, 7f };
        for (int i = 0; i < zs.Length; i++)
            MakeTree(holder, new Vector3(xs[i], 0f, zs[i]), (i * 47f) % 360f, 0.9f + (i % 4) * 0.15f);

        var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(scene);
        UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene);
        return "built " + zs.Length + " trees";
    }
}
