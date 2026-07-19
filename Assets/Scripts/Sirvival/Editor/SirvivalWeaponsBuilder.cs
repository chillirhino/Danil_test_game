using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using Sirvival;

namespace Sirvival.EditorTools
{
    /// <summary>Adds the starting weapon roster to the chef. Menu: Sirvival ▸ Build Weapons.</summary>
    public static class SirvivalWeaponsBuilder
    {
        [MenuItem("Sirvival/Build Weapons")]
        public static void BuildWeapons()
        {
            var chef = GameObject.Find("Chef");
            if (chef == null) { Debug.LogError("Chef not found"); return; }

            Ensure<RollingPinWeapon>(chef);
            Ensure<GarlicAuraWeapon>(chef);
            // Spicy Sauce is the existing ChefAutoAttack — leave it as-is.

            var scene = SceneManager.GetActiveScene();
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log("SIRVIVAL_WEAPONS_BUILT");
        }

        static T Ensure<T>(GameObject go) where T : Component
        {
            var c = go.GetComponent<T>();
            if (c == null) c = go.AddComponent<T>();
            return c;
        }
    }
}
