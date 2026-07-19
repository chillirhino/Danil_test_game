using UnityEngine;
using UnityEditor;
public class WeapSteel {
  public static string Main(){
    var m=AssetDatabase.LoadAssetAtPath<Material>("Assets/Polytope Studio/Lowpoly_Weapons/Sources/Materials/PT_Weapons_Material.mat");
    var tex=AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Polytope Studio/Lowpoly_Weapons/Sources/Textures/PT_Weapons_Texture_01_base.png");
    m.SetTexture("_BaseMap",tex);
    m.SetColor("_BaseColor",new Color(0.62f,0.65f,0.72f));
    if(m.HasProperty("_Metallic")) m.SetFloat("_Metallic",0.8f);
    if(m.HasProperty("_Smoothness")) m.SetFloat("_Smoothness",0.6f);
    EditorUtility.SetDirty(m); AssetDatabase.SaveAssets();
    return "weapon steel look set";
  }
}
