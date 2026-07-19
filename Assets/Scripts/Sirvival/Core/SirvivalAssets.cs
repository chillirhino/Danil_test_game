using UnityEngine;

namespace Sirvival
{
    /// <summary>Cached loader for the placeholder sprites (editor). Runtime spawners
    /// should prefer prefab references; this is a convenience for editor/play testing.</summary>
    public static class SirvivalAssets
    {
        private static Sprite _circle, _px;
        public static Sprite Circle() => _circle != null ? _circle : (_circle = Load("_circle"));
        public static Sprite Px() => _px != null ? _px : (_px = Load("_px"));

        private static Sprite Load(string n)
        {
#if UNITY_EDITOR
            return UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Sprites/Sirvival/" + n + ".png");
#else
            return null;
#endif
        }
    }
}
