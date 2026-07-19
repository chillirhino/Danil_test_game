using UnityEngine;
using UnityEditor;
public class RepositionFP {
  static AnimationClip Clip(string path){ foreach(var a in AssetDatabase.LoadAllAssetsAtPath(path)) if(a is AnimationClip ac && !ac.name.StartsWith("__")) return ac; return null; }
  static Transform F(Transform r,string n){ if(r.name==n)return r; foreach(Transform c in r){var x=F(c,n);if(x!=null)return x;} return null; }
  public static string Main(){
    var body=GameObject.Find("FPBody"); var cam=GameObject.Find("Main Camera").transform;
    var an=body.GetComponentInChildren<Animator>(true); an.enabled=false;
    var idle=Clip("Assets/Sword And Shield Idle.fbx");
    idle.SampleAnimation(body, 0.5f);
    var rHand=F(body.transform,"PT_RightHand");
    Vector3 target=cam.position+cam.forward*0.42f+cam.right*0.10f-cam.up*0.20f;
    Vector3 delta=target-rHand.position;
    body.transform.position+=delta;
    an.enabled=true;
    UnityEditor.SceneManagement.EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
    return "repositioned by "+delta.ToString("0.00");
  }
}
