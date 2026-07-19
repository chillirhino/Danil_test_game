using UnityEngine;
using UnityEditor;
using PoK.World;

public class SetupGen
{
    static GameObject Pf(string n) => AssetDatabase.LoadAssetAtPath<GameObject>("Assets/SimpleNaturePack/Prefabs/" + n + ".prefab");

    static void SetArray(SerializedObject so, string prop, GameObject[] items)
    {
        var p = so.FindProperty(prop);
        p.arraySize = items.Length;
        for (int i = 0; i < items.Length; i++)
            p.GetArrayElementAtIndex(i).objectReferenceValue = items[i];
    }

    public static string Main()
    {
        // Remove the static preview world; the generator owns everything now.
        foreach (var n in new[] { "Environment", "Ground" })
        {
            var go = GameObject.Find(n);
            if (go != null) Object.DestroyImmediate(go);
        }

        var old = GameObject.Find("WorldGenerator");
        if (old != null) Object.DestroyImmediate(old);
        var genGo = new GameObject("WorldGenerator");
        var gen = genGo.AddComponent<WorldGenerator>();

        var so = new SerializedObject(gen);
        var player = GameObject.Find("Player");
        so.FindProperty("player").objectReferenceValue = player != null ? player.transform : null;
        so.FindProperty("groundMat").objectReferenceValue = AssetDatabase.LoadAssetAtPath<Material>("Assets/Art/Materials/Grass.mat");
        so.FindProperty("pathMat").objectReferenceValue = AssetDatabase.LoadAssetAtPath<Material>("Assets/Art/Materials/Dirt.mat");
        SetArray(so, "trees", new[] { Pf("Tree_01"), Pf("Tree_02"), Pf("Tree_03"), Pf("Tree_04"), Pf("Tree_05") });
        SetArray(so, "bushes", new[] { Pf("Bush_01"), Pf("Bush_02"), Pf("Bush_03") });
        SetArray(so, "rocks", new[] { Pf("Rock_01"), Pf("Rock_02"), Pf("Rock_03"), Pf("Rock_04"), Pf("Rock_05") });
        SetArray(so, "groundCover", new[] { Pf("Grass_01"), Pf("Grass_02"), Pf("Flowers_01"), Pf("Flowers_02"), Pf("Mushroom_01"), Pf("Mushroom_02"), Pf("Stump_01") });
        so.ApplyModifiedProperties();

        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(genGo.scene);
        UnityEditor.SceneManagement.EditorSceneManager.SaveScene(genGo.scene);
        return "generator set up, static world removed";
    }
}
