using System.Text;
using UnityEngine;
using UnityEditor;
public class ProbeChar {
  static void Walk(Transform t, int d, StringBuilder sb, System.Collections.Generic.HashSet<string> want){
    string n=t.name;
    if(want.Count==0 || System.Array.Exists(new[]{"arm","hand","shoulder","clavicle","fore","spine","head","neck"}, k=>n.ToLower().Contains(k)))
      sb.Append(new string(' ',d)+n+"\n");
    foreach(Transform c in t) Walk(c,d+1,sb,want);
  }
  public static string Main(){
    var sb=new StringBuilder();
    var p=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Polytope Studio/Lowpoly_Characters/Prefabs/Modular_Armors/PT_Lowpoly_Armors_Male_Moduar_Free.prefab");
    if(p==null) return "no char prefab";
    sb.Append("Animator="+(p.GetComponentInChildren<Animator>()!=null)+" skinnedMeshes="+p.GetComponentsInChildren<SkinnedMeshRenderer>().Length+"\n");
    sb.Append("--- bones (arm/hand/spine/head) ---\n");
    Walk(p.transform,0,sb,new System.Collections.Generic.HashSet<string>());
    // pose fbx clips
    sb.Append("--- PT_Pose_01 clips ---\n");
    foreach(var a in AssetDatabase.LoadAllAssetsAtPath("Assets/Polytope Studio/Lowpoly_Characters/Animations/PT_Pose_01.fbx"))
      if(a is AnimationClip ac) sb.Append("clip: "+ac.name+" len="+ac.length.ToString("0.00")+"\n");
    return sb.ToString();
  }
}
