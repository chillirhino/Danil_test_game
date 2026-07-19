using UnityEngine;
using UnityEditor;

public class PlaceSNP
{
    static void ConvertMats()
    {
        var urp = Shader.Find("Universal Render Pipeline/Lit");
        foreach (var g in AssetDatabase.FindAssets("t:Material", new[] { "Assets/SimpleNaturePack" }))
        {
            var m = AssetDatabase.LoadAssetAtPath<Material>(AssetDatabase.GUIDToAssetPath(g));
            if (m == null || m.shader == null || m.shader.name != "Standard") continue;
            var tex = m.HasProperty("_MainTex") ? m.GetTexture("_MainTex") : null;
            var col = m.HasProperty("_Color") ? m.GetColor("_Color") : Color.white;
            m.shader = urp;
            if (tex != null) m.SetTexture("_BaseMap", tex);
            m.SetColor("_BaseColor", col);
            m.SetFloat("_Smoothness", 0.1f);
            EditorUtility.SetDirty(m);
        }
        AssetDatabase.SaveAssets();
    }

    static float HeightOf(GameObject inst)
    {
        bool has = false;
        Bounds b = new Bounds(inst.transform.position, Vector3.zero);
        foreach (var r in inst.GetComponentsInChildren<Renderer>())
        {
            if (!has) { b = r.bounds; has = true; }
            else b.Encapsulate(r.bounds);
        }
        return has ? b.size.y : 1f;
    }

    public static string Main()
    {
        ConvertMats();

        var trees = new GameObject[5];
        for (int i = 0; i < 5; i++)
            trees[i] = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/SimpleNaturePack/Prefabs/Tree_0" + (i + 1) + ".prefab");
        var bushes = new[]
        {
            AssetDatabase.LoadAssetAtPath<GameObject>("Assets/SimpleNaturePack/Prefabs/Bush_01.prefab"),
            AssetDatabase.LoadAssetAtPath<GameObject>("Assets/SimpleNaturePack/Prefabs/Bush_02.prefab"),
        };

        var env = GameObject.Find("Environment");
        var old = env.transform.Find("Trees");
        if (old != null) Object.DestroyImmediate(old.gameObject);
        var holder = new GameObject("Trees").transform;
        holder.SetParent(env.transform, false);

        const float targetH = 6f;
        float[] zs = { -4f, 2f, 9f, 16f, 24f, 33f, 42f, 6f, 20f, 38f, 12f, 30f };
        float[] xs = { -5f, 5.5f, -6f, 5f, -5.5f, 6f, -5f, 7.5f, -8f, 8f, -7f, 7f };
        for (int i = 0; i < zs.Length; i++)
        {
            var prefab = trees[i % 5];
            if (prefab == null) continue;
            var t = (GameObject)PrefabUtility.InstantiatePrefab(prefab, holder);
            t.transform.localPosition = new Vector3(xs[i], 0f, zs[i]);
            t.transform.localRotation = Quaternion.Euler(0f, (i * 53f) % 360f, 0f);
            float h = HeightOf(t);
            float s = (targetH / Mathf.Max(0.01f, h)) * (0.8f + (i % 4) * 0.12f);
            t.transform.localScale = Vector3.one * s;
        }

        float[] bz = { 3f, 11f, 19f, 29f, 37f, 7f };
        float[] bx = { 2.4f, -2.5f, 2.6f, -2.4f, 2.5f, -2.6f };
        for (int i = 0; i < bz.Length && bushes[0] != null; i++)
        {
            var prefab = bushes[i % 2];
            var b = (GameObject)PrefabUtility.InstantiatePrefab(prefab, holder);
            b.transform.localPosition = new Vector3(bx[i], 0f, bz[i]);
            b.transform.localRotation = Quaternion.Euler(0f, (i * 71f) % 360f, 0f);
            float h = HeightOf(b);
            b.transform.localScale = Vector3.one * (1.2f / Mathf.Max(0.01f, h));
        }

        var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(scene);
        UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene);
        return "placed " + zs.Length + " trees + " + bz.Length + " bushes";
    }
}
