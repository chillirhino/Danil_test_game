using System.Text;
using UnityEngine;
using UnityEditor;
public class ProbeSNP {
  public static string Main(){
    var sb=new StringBuilder();
    foreach(var n in new[]{"Tree_01","Tree_02","Tree_05","Bush_01"}){
      var go=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/SimpleNaturePack/Prefabs/"+n+".prefab");
      if(go==null){sb.Append(n+" MISSING\n");continue;}
      var mf=go.GetComponentInChildren<MeshFilter>();
      var mr=go.GetComponentInChildren<MeshRenderer>();
      sb.Append(n+": mesh="+(mf&&mf.sharedMesh?mf.sharedMesh.name+"("+mf.sharedMesh.vertexCount+"v)":"NULL"));
      if(mr!=null){sb.Append(" mats=");foreach(var m in mr.sharedMaterials)sb.Append((m?m.name+"["+m.shader.name+"]":"null")+" ");}
      sb.Append("\n");
    }
    // list materials in pack
    sb.Append("materials: ");
    foreach(var g in AssetDatabase.FindAssets("t:Material",new[]{"Assets/SimpleNaturePack"})){
      var m=AssetDatabase.LoadAssetAtPath<Material>(AssetDatabase.GUIDToAssetPath(g));
      sb.Append(m.name+"["+m.shader.name+"] ");
    }
    return sb.ToString();
  }
}
