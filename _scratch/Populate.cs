using UnityEngine;
using UnityEditor;

public class Populate
{
    const string P = "Assets/SimpleNaturePack/Prefabs/";

    static GameObject L(string n) => AssetDatabase.LoadAssetAtPath<GameObject>(P + n + ".prefab");

    static float H(GameObject g)
    {
        bool h = false; Bounds b = new Bounds(g.transform.position, Vector3.zero);
        foreach (var r in g.GetComponentsInChildren<Renderer>()) { if (!h) { b = r.bounds; h = true; } else b.Encapsulate(r.bounds); }
        return h ? b.size.y : 1f;
    }

    static Transform Holder(Transform env, string name)
    {
        var old = env.Find(name);
        if (old != null) Object.DestroyImmediate(old.gameObject);
        var t = new GameObject(name).transform; t.SetParent(env, false); return t;
    }

    static void Scatter(Transform holder, GameObject[] prefabs, int count, float targetH,
        float scaleVar, float minAbsX, float xRange, float zMin, float zMax, float randScaleLo, float randScaleHi)
    {
        for (int i = 0; i < count; i++)
        {
            var p = prefabs[Random.Range(0, prefabs.Length)];
            if (p == null) continue;
            float x = Random.Range(minAbsX, xRange) * (Random.value < 0.5f ? -1f : 1f);
            float z = Random.Range(zMin, zMax);
            var go = (GameObject)PrefabUtility.InstantiatePrefab(p, holder);
            go.transform.localPosition = new Vector3(x, 0f, z);
            go.transform.localRotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
            float h = H(go);
            float s = targetH / Mathf.Max(0.01f, h) * Random.Range(randScaleLo, randScaleHi);
            go.transform.localScale = Vector3.one * s;
        }
    }

    public static string Main()
    {
        Random.InitState(12345);
        var env = GameObject.Find("Environment").transform;

        var trees = new[] { L("Tree_01"), L("Tree_02"), L("Tree_03"), L("Tree_04"), L("Tree_05") };
        var bushes = new[] { L("Bush_01"), L("Bush_02"), L("Bush_03") };
        var rocks = new[] { L("Rock_01"), L("Rock_02"), L("Rock_03"), L("Rock_04"), L("Rock_05") };
        var grass = new[] { L("Grass_01"), L("Grass_02") };
        var flowers = new[] { L("Flowers_01"), L("Flowers_02") };
        var mush = new[] { L("Mushroom_01"), L("Mushroom_02"), L("Stump_01") };

        var tH = Holder(env, "Trees");
        var bH = Holder(env, "Bushes");
        var rH = Holder(env, "Rocks");
        var gH = Holder(env, "GroundCover");

        // Dense forest: trees near-to-far, thinning outward. Path kept clear (|x|>2.2 for big stuff).
        Scatter(tH, trees, 55, 6f, 0.3f, 2.6f, 16f, -8f, 48f, 0.75f, 1.35f);
        Scatter(bH, bushes, 40, 1.3f, 0.3f, 2.2f, 15f, -8f, 48f, 0.7f, 1.4f);
        Scatter(rH, rocks, 22, 0.7f, 0.3f, 2.2f, 14f, -8f, 48f, 0.5f, 1.6f);
        // Ground detail close to the path and across the field.
        Scatter(gH, grass, 220, 0.5f, 0.2f, 1.7f, 15f, -8f, 48f, 0.6f, 1.5f);
        Scatter(gH, flowers, 60, 0.4f, 0.2f, 1.9f, 14f, -8f, 48f, 0.7f, 1.3f);
        Scatter(gH, mush, 30, 0.4f, 0.2f, 2.4f, 13f, -8f, 48f, 0.6f, 1.4f);

        var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(scene);
        UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene);
        return "populated: 55 trees, 40 bushes, 22 rocks, 310 ground details";
    }
}
