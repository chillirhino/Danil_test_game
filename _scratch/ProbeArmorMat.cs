using UnityEngine;
using UnityEditor;
public class ProbeArmorMat {
  public static string Main(){
    var m=AssetDatabase.LoadAssetAtPath<Material>("Assets/Polytope Studio/Lowpoly_Characters/Sources/Modular_Armors/Materials/PT_Armors_Material.mat");
    if(m==null) return "no mat";
    var bm=m.HasProperty("_BaseMap")?m.GetTexture("_BaseMap"):null;
    return "shader="+m.shader.name+" BaseMap="+(bm!=null?bm.name:"NULL")+" BaseColor="+m.GetColor("_BaseColor");
  }
}
