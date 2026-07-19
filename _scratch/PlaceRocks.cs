using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class PlaceRocks
{
    static Material MakeRockMat(string id)
    {
        string dir = "Assets/Art/Models/" + id + "/";
        var norImp = AssetImporter.GetAtPath(dir + id + "_nor_gl.jpg") as TextureImporter;
        if (norImp != null && norImp.textureType != TextureImporterType.NormalMap)
        { norImp.textureType = TextureImporterType.NormalMap; norImp.SaveAndReimport(); }

        var diff = AssetDatabase.LoadAssetAtPath<Texture2D>(dir + id + "_diff.jpg");
        var nor = AssetDatabase.LoadAssetAtPath<Texture2D>(dir + id + "_nor_gl.jpg");

        string mp = "Assets/Art/Materials/Rock_" + id + ".mat";
        var m = AssetDatabase.LoadAssetAtPath<Material>(mp);
        if (m == null)
        {
            m = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            AssetDatabase.CreateAsset(m, mp);
        }
        if (diff != null) { m.SetTexture("_BaseMap", diff); m.SetColor("_BaseColor", Color.white); }
        if (nor != null) { m.SetTexture("_BumpMap", nor); m.EnableKeyword("_NORMALMAP"); }
        m.SetFloat("_Smoothness", 0.15f);
        EditorUtility.SetDirty(m);
        return m;
    }

    static GameObject Place(GameObject model, Material mat, Transform parent, Vector3 pos, float yRot, float scale)
    {
        var go = (GameObject)PrefabUtility.InstantiatePrefab(model, parent);
        go.transform.localPosition = pos;
        go.transform.localRotation = Quaternion.Euler(0f, yRot, 0f);
        go.transform.localScale = Vector3.one * scale;
        foreach (var r in go.GetComponentsInChildren<MeshRenderer>())
            r.sharedMaterial = mat;
        return go;
    }

    public static string Main()
    {
        var boulder = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Art/Models/boulder_01/boulder_01_1k.fbx");
        var moon = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Art/Models/moon_rock_01/moon_rock_01_1k.fbx");
        if (boulder == null || moon == null) return "model load failed b=" + (boulder != null) + " m=" + (moon != null);

        var mBoulder = MakeRockMat("boulder_01");
        var mMoon = MakeRockMat("moon_rock_01");

        var envGo = GameObject.Find("Environment");
        if (envGo == null) return "no Environment";
        var env = envGo.transform;

        // Remove placeholder primitive rocks.
        var toKill = new List<GameObject>();
        foreach (Transform c in env) if (c.name.StartsWith("Rock_")) toKill.Add(c.gameObject);
        foreach (var g in toKill) Object.DestroyImmediate(g);

        // Scatter real rocks near the path.
        float[] rz = { 1f, 8f, 15f, 23f, 31f, 39f, 5f, 27f };
        float[] rx = { 2.6f, -2.7f, 2.8f, -2.6f, 2.7f, -2.8f, -3.2f, 3.1f };
        var rocks = new GameObject("Rocks").transform;
        rocks.SetParent(env, false);
        for (int i = 0; i < rz.Length; i++)
        {
            bool useBoulder = (i % 2 == 0);
            var model = useBoulder ? boulder : moon;
            var mat = useBoulder ? mBoulder : mMoon;
            float scale = useBoulder ? Random.Range(0.5f, 0.9f) : Random.Range(0.8f, 1.4f);
            Place(model, mat, rocks, new Vector3(rx[i], 0f, rz[i]), Random.Range(0f, 360f), scale);
        }

        var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(scene);
        UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene);
        return "placed " + rz.Length + " rocks";
    }
}
