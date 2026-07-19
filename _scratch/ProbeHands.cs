using System.Text;
using UnityEngine;
using UnityEditor;
public class ProbeHands {
  static string Info(string path){
    var go=AssetDatabase.LoadAssetAtPath<GameObject>(path);
    if(go==null) return path+" MISSING";
    var sb=new StringBuilder(path.Substring(path.LastIndexOf('/')+1)+": ");
    var mf=go.GetComponentInChildren<MeshFilter>();
    var smr=go.GetComponentInChildren<SkinnedMeshRenderer>();
    if(mf&&mf.sharedMesh) sb.Append("static mesh="+mf.sharedMesh.name+" v="+mf.sharedMesh.vertexCount+" size="+mf.sharedMesh.bounds.size.ToString("0.00"));
    else if(smr&&smr.sharedMesh) sb.Append("SKINNED mesh="+smr.sharedMesh.name+" v="+smr.sharedMesh.vertexCount+" size="+smr.sharedMesh.bounds.size.ToString("0.00")+" bones="+smr.bones.Length);
    else sb.Append("no mesh");
    var r=go.GetComponentInChildren<Renderer>();
    if(r) sb.Append(" mat="+(r.sharedMaterial?r.sharedMaterial.name+"["+r.sharedMaterial.shader.name+"]":"null"));
    // bone names if skinned
    if(smr){ sb.Append(" bones:"); int i=0; foreach(var b in smr.bones){ if(i++>8)break; sb.Append(b.name+","); } }
    return sb.ToString();
  }
  public static string Main(){
    return Info("Assets/SimpleHands/Prefabs/WhiteHand.prefab")+"\n"+Info("Assets/SimpleHands/Prefabs/BlackHand.prefab");
  }
}
