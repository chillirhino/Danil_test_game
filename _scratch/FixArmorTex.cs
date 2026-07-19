using UnityEngine;
using UnityEditor;
public class FixArmorTex {
  public static string Main(){
    var m=AssetDatabase.LoadAssetAtPath<Material>("Assets/Polytope Studio/Lowpoly_Characters/Sources/Modular_Armors/Materials/PT_Armors_Material.mat");
    if(m==null) return "no mat";
    var tex=AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Polytope Studio/Lowpoly_Characters/Sources/Modular_Armors/Textures/PT_Armors_Base_Texture.png");
    if(tex==null) tex=AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Polytope Studio/Lowpoly_Characters/Sources/Modular_Armors/Textures/PT_texture.tga");
    if(tex==null) return "no base texture found";
    m.SetTexture("_BaseMap",tex);
    m.SetColor("_BaseColor",Color.white);
    if(m.HasProperty("_Smoothness")) m.SetFloat("_Smoothness",0.2f);
    EditorUtility.SetDirty(m); AssetDatabase.SaveAssets();
    return "armor base texture set: "+tex.name;
  }
}
