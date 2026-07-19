using UnityEngine;
using UnityEditor;
public class TryTga {
  public static string Main(){
    var m=AssetDatabase.LoadAssetAtPath<Material>("Assets/Polytope Studio/Lowpoly_Characters/Sources/Modular_Armors/Materials/PT_Armors_Material.mat");
    var tex=AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Polytope Studio/Lowpoly_Characters/Sources/Modular_Armors/Textures/PT_texture.tga");
    if(tex==null) return "no tga";
    m.SetTexture("_BaseMap",tex); m.SetColor("_BaseColor",Color.white);
    EditorUtility.SetDirty(m); AssetDatabase.SaveAssets();
    return "set PT_texture.tga size="+tex.width+"x"+tex.height;
  }
}
