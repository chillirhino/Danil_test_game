using UnityEngine;
using UnityEditor;
public class BakePose {
  static AnimationClip Clip(string p){ foreach(var a in AssetDatabase.LoadAllAssetsAtPath(p)) if(a is AnimationClip ac&&!ac.name.StartsWith("__")) return ac; return null; }
  public static string Main(){
    var body=GameObject.Find("FPBody");
    var an=body.GetComponentInChildren<Animator>(true);
    // bake grip pose then disable animator so nothing overwrites ArmsInView
    var idle=Clip("Assets/Sword And Shield Idle.fbx"); idle.SampleAnimation(body,0f);
    an.enabled=false;
    UnityEditor.SceneManagement.EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
    return "grip baked, animator OFF";
  }
}
