using UnityEngine;
using UnityEditor;
using PoK.Player;
public class RevertClean {
  static AnimationClip Clip(string p){ foreach(var a in AssetDatabase.LoadAllAssetsAtPath(p)) if(a is AnimationClip ac&&!ac.name.StartsWith("__")) return ac; return null; }
  static Transform F(Transform r,string n){ if(r.name==n)return r; foreach(Transform c in r){var x=F(c,n);if(x!=null)return x;} return null; }
  public static string Main(){
    var body=GameObject.Find("FPBody"); var cam=GameObject.Find("Main Camera").transform;
    var aiv=body.GetComponent<ArmsInView>(); if(aiv!=null) aiv.enabled=false;
    var an=body.GetComponentInChildren<Animator>(true);
    an.enabled=false;
    var idle=Clip("Assets/Sword And Shield Idle.fbx"); idle.SampleAnimation(body,0f);
    var head=F(body.transform,"PT_Head");
    Vector3 desired=cam.position - cam.forward*0.08f + cam.up*0.06f;
    body.transform.position+=desired-head.position;
    an.enabled=true; // frozen idle (states speed 0) => static grip, no wiggle
    UnityEditor.SceneManagement.EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
    return "reverted to clean state (animator on, ArmsInView off, head-aligned)";
  }
}
