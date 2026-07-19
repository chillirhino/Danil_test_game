using UnityEngine;
using UnityEditor;
using PoK.Player;
public class AddCombat {
  static AnimationClip Clip(string path){ foreach(var a in AssetDatabase.LoadAllAssetsAtPath(path)) if(a is AnimationClip ac && !ac.name.StartsWith("__")) return ac; return null; }
  public static string Main(){
    var body=GameObject.Find("FPBody");
    if(body.GetComponent<CombatInput>()==null) body.AddComponent<CombatInput>();
    // preview the slash mid-swing (disable animator, sample)
    var an=body.GetComponentInChildren<Animator>(true); an.enabled=false;
    var slash=Clip("Assets/Stable Sword Inward Slash.fbx");
    slash.SampleAnimation(body, slash.length*0.45f);
    return "combat added, previewing slash at 45%";
  }
}
