using UnityEngine;
using UnityEditor;
public class ResetPos {
  static AnimationClip Clip(string p){ foreach(var a in AssetDatabase.LoadAllAssetsAtPath(p)) if(a is AnimationClip ac&&!ac.name.StartsWith("__")) return ac; return null; }
  static Transform F(Transform r,string n){ if(r.name==n)return r; foreach(Transform c in r){var x=F(c,n);if(x!=null)return x;} return null; }
  public static string Main(){
    var body=GameObject.Find("FPBody"); var cam=GameObject.Find("Main Camera").transform;
    var an=body.GetComponentInChildren<Animator>(true); an.enabled=false;
    var idle=Clip("Assets/Sword And Shield Idle.fbx"); idle.SampleAnimation(body,0f);
    var head=F(body.transform,"PT_Head");
    // put head slightly behind+below camera so we look out past it (eyes at camera), chest lower
    Vector3 desired=cam.position - cam.forward*0.08f + cam.up*0.06f;
    body.transform.position+=desired-head.position;
    an.enabled=true;
    UnityEditor.SceneManagement.EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
    return "reset to head-aligned";
  }
}
