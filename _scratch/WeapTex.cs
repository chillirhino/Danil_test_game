using UnityEngine;
using UnityEditor;
public class WeapTex {
  public static string Main(){
    var m=AssetDatabase.LoadAssetAtPath<Material>("Assets/Polytope Studio/Lowpoly_Weapons/Sources/Materials/PT_Weapons_Material.mat");
    var tex=AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Polytope Studio/Lowpoly_Weapons/Sources/Textures/PT_texture.tga");
    if(m==null||tex==null) return "missing";
    m.SetTexture("_BaseMap",tex); m.SetColor("_BaseColor",Color.white);
    if(m.HasProperty("_Smoothness")) m.SetFloat("_Smoothness",0.5f);
    EditorUtility.SetDirty(m); AssetDatabase.SaveAssets();
    return "weapon texture set "+tex.width+"x"+tex.height;
  }
}
