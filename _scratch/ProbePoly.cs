using System.Text;
using UnityEngine;
using UnityEditor;
public class ProbePoly {
  static string Find(string kw){
    foreach(var g in AssetDatabase.FindAssets("t:Prefab")){
      var p=AssetDatabase.GUIDToAssetPath(g);
      if(p.Contains("Polytope")&&p.ToLower().Contains(kw)) return p;
    }
    return null;
  }
  static string Info(string path){
    if(path==null) return "(not found)";
    var go=AssetDatabase.LoadAssetAtPath<GameObject>(path);
    if(go==null) return path+" load fail";
    var sb=new StringBuilder(path.Substring(path.LastIndexOf('/')+1)+": ");
    var mf=go.GetComponentInChildren<MeshFilter>();
    var smr=go.GetComponentInChildren<SkinnedMeshRenderer>();
    sb.Append("mesh="+(mf&&mf.sharedMesh?mf.sharedMesh.name+"("+mf.sharedMesh.vertexCount+"v)":(smr&&smr.sharedMesh?"SKINNED:"+smr.sharedMesh.name:"NULL")));
    var r=go.GetComponentInChildren<Renderer>();
    if(r!=null){sb.Append(" mat=");foreach(var m in r.sharedMaterials)sb.Append((m?m.name+"["+m.shader.name+"]":"null")+" ");}
    return sb.ToString();
  }
  public static string Main(){
    var sb=new StringBuilder();
    sb.Append(Info(Find("male_armor_01_a_gauntlets"))+"\n");
    sb.Append(Info(Find("sword_01_a"))+"\n");
    sb.Append(Info(Find("longsword_01_a"))+"\n");
    // material shaders
    foreach(var mp in new[]{"Assets/Polytope Studio/Lowpoly_Characters/Sources/Modular_Armors/Materials/PT_Armors_Material.mat"}){
      var m=AssetDatabase.LoadAssetAtPath<Material>(mp);
      if(m!=null) sb.Append("ArmorMat shader="+m.shader.name+"\n");
    }
    return sb.ToString();
  }
}
