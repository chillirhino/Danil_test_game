using UnityEngine;
using UnityEditor;
public class ProbeWeap {
  public static string Main(){
    var m=AssetDatabase.LoadAssetAtPath<Material>("Assets/Polytope Studio/Lowpoly_Weapons/Sources/Materials/PT_Weapons_Material.mat");
    if(m==null){ foreach(var g in AssetDatabase.FindAssets("PT_Weapons_Material t:Material")){m=AssetDatabase.LoadAssetAtPath<Material>(AssetDatabase.GUIDToAssetPath(g));break;} }
    if(m==null) return "no weapon mat";
    var bm=m.HasProperty("_BaseMap")?m.GetTexture("_BaseMap"):null;
    return "path="+AssetDatabase.GetAssetPath(m)+" shader="+m.shader.name+" BaseMap="+(bm!=null?bm.name:"NULL");
  }
}
