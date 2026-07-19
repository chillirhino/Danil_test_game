using System.Text;
using UnityEngine;
using UnityEditor;
public class ProbePrefab {
  public static string Main(){
    var sb=new StringBuilder();
    foreach(var n in new[]{"tree01","tree02","tree03","tree04"}){
      var go=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/NatureStarterKit2/Nature/"+n+".prefab");
      if(go==null){sb.Append(n+" MISSING\n");continue;}
      var mf=go.GetComponentInChildren<MeshFilter>();
      var tree=go.GetComponent<Tree>();
      sb.Append(n+": mesh="+(mf!=null&&mf.sharedMesh!=null?mf.sharedMesh.name+"("+mf.sharedMesh.vertexCount+"v)":"NULL")
        +" Tree="+(tree!=null)+" treeData="+(tree!=null&&tree.data!=null?tree.data.name:"null")+"\n");
    }
    return sb.ToString();
  }
}
