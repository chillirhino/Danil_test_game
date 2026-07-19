using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using Sirvival;

namespace Sirvival.EditorTools
{
    /// <summary>Builds animated enemy prefabs (fat/biz/karen) from the sliced walk frames
    /// and wires them into the spawner. Menu: Sirvival ▸ Build Enemies.</summary>
    public static class SirvivalEnemyBuilder
    {
        const string Dir = "Assets/Art/Sprites/Sirvival/";

        struct Type { public string tag; public float hpMult, speedMult, contact; public int xp; public float radius; }

        [MenuItem("Sirvival/Build Enemies")]
        public static void BuildEnemies()
        {
            var types = new[]
            {
                new Type{ tag="fat",   hpMult=2.6f, speedMult=0.55f, contact=12f, xp=3, radius=0.55f },
                new Type{ tag="biz",   hpMult=0.7f, speedMult=1.6f,  contact=6f,  xp=1, radius=0.42f },
                new Type{ tag="karen", hpMult=1.0f, speedMult=1.0f,  contact=8f,  xp=1, radius=0.45f },
            };

            var prefabs = new List<Enemy>();
            foreach (var t in types) prefabs.Add(MakePrefab(t));

            var spawner = GameObject.Find("Systems").GetComponent<EnemySpawner>();
            var so = new SerializedObject(spawner);
            var arr = so.FindProperty("enemyPrefabs");
            arr.arraySize = prefabs.Count;
            for (int i = 0; i < prefabs.Count; i++) arr.GetArrayElementAtIndex(i).objectReferenceValue = prefabs[i];
            so.ApplyModifiedPropertiesWithoutUndo();

            var scene = SceneManager.GetActiveScene();
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log("SIRVIVAL_ENEMIES_BUILT " + prefabs.Count);
        }

        static Enemy MakePrefab(Type t)
        {
            var frames = new Sprite[6];
            for (int i = 0; i < 6; i++)
            {
                string p = Dir + "enemy_" + t.tag + "_" + i + ".png";
                AssetDatabase.ImportAsset(p, ImportAssetOptions.ForceUpdate);
                var ti = (TextureImporter)AssetImporter.GetAtPath(p);
                ti.textureType = TextureImporterType.Sprite;
                ti.spritePixelsPerUnit = 300f;
                ti.alphaIsTransparency = true;
                ti.SaveAndReimport();
                frames[i] = AssetDatabase.LoadAssetAtPath<Sprite>(p);
            }

            var go = new GameObject("Enemy_" + t.tag);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = frames[0]; sr.sortingOrder = 0;
            var rb = go.AddComponent<Rigidbody2D>(); rb.gravityScale = 0f; rb.freezeRotation = true;
            go.AddComponent<CircleCollider2D>().radius = t.radius;

            var enemy = go.AddComponent<Enemy>();
            var es = new SerializedObject(enemy);
            es.FindProperty("hpMult").floatValue = t.hpMult;
            es.FindProperty("speedMult").floatValue = t.speedMult;
            es.FindProperty("contactDamage").floatValue = t.contact;
            es.FindProperty("xpValue").intValue = t.xp;
            es.ApplyModifiedPropertiesWithoutUndo();

            var anim = go.AddComponent<ChefAnim>();
            var aso = new SerializedObject(anim);
            var af = aso.FindProperty("frames"); af.arraySize = 6;
            for (int i = 0; i < 6; i++) af.GetArrayElementAtIndex(i).objectReferenceValue = frames[i];
            aso.FindProperty("fps").floatValue = 10f;
            aso.ApplyModifiedPropertiesWithoutUndo();

            string path = Dir + "Enemy_" + t.tag + ".prefab";
            var prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
            Object.DestroyImmediate(go);
            return prefab.GetComponent<Enemy>();
        }
    }
}
